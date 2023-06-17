#nullable enable
using Players.Bullets;
using Players.ChangeColor;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using Utils;


namespace Players
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(NetworkTransform))]
    public class Player : NetworkBehaviour
    {
        [Header("Shooting")]
        [SerializeField] private Bullet bulletPrefab = null!;
        [SerializeField] private Bullet[] bullets = null!;
        [SerializeField] private Transform shootPoint = null!;
        [Space] [SerializeField] private SpriteRenderer spriteRenderer = null!;
        [SerializeField] private NetworkVariable<Color> currentColor = new(
            readPerm: NetworkVariableReadPermission.Everyone,
            writePerm: NetworkVariableWritePermission.Owner
        );
        [SerializeField] private float speed = 10;
        [SerializeField] private float jumpPower = 20;
        private new Rigidbody2D rigidbody2D = null!;
        [SerializeField] private NetworkVariable<float> health = new(
            value: 100,
            readPerm: NetworkVariableReadPermission.Everyone,
            writePerm: NetworkVariableWritePermission.Server
        );
        [SerializeField] private NetworkVariable<float> inputDirection = new(
            0,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner
        );
        [SerializeField] private NetworkVariable<bool> inputJump = new(
            false,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner
        );
        private readonly Quaternion lookAdditional = Quaternion.Euler(0, -90, 0);
        private IInput input = new PlugInput();
        private readonly IChangeColor changeColor = new KeyboardChangeColor();


        public NetworkVariable<float> Health => health;


        public NetworkVariable<Color> Color => currentColor;


        private void Awake()
        {
            rigidbody2D = GetComponent<Rigidbody2D>()!;
            spriteRenderer = GetComponent<SpriteRenderer>()!;

            bulletPrefab.EnsureNotNull();
            shootPoint.EnsureNotNull();
        }


        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            currentColor.OnValueChanged += OnColorChanged;
            if (IsOwner)
            {
                input = new KeyboardInput();
                currentColor.Value = Random.ColorHSV();
            }

            spriteRenderer.color = currentColor.Value;
        }


        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            currentColor.OnValueChanged -= OnColorChanged;
        }


        private void OnColorChanged(Color previousColor, Color newColor)
        {
            spriteRenderer.color = newColor;
        }


        [ServerRpc]
        private void ShootServerRpc()
        {
            var randomPrefab = Random.Range(0f, 1f) < 0.8 ? bullets[0] : bullets[1];

            var bullet = Instantiate(randomPrefab, shootPoint.position, Quaternion.identity)!;
            bullet.Spawn(shootPoint.right);
        }


        private void Update()
        {
            if (IsOwner)
            {
                inputDirection.Value = input.Direction();
                inputJump.Value = input.Jump();
                currentColor.Value = changeColor.Change(currentColor.Value);
                if (input.Shoot())
                {
                    ShootServerRpc();
                }
            }

            if (IsServer)
            {
                var direction = Vector2.right * inputDirection.Value;
                if (direction.sqrMagnitude > 0)
                {
                    rigidbody2D.AddForce(direction * (speed * Time.deltaTime));
                    transform.rotation = Quaternion.LookRotation(direction) * lookAdditional;
                }

                if (inputJump.Value && rigidbody2D.IsTouchingLayers())
                {
                    inputJump.Value = false;
                    rigidbody2D.AddForce(Vector2.up * jumpPower);
                }
            }
        }


        public void ApplyDamage(float amount)
        {
            health.Value -= amount;
        }
    }
}
