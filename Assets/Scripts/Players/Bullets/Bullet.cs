#nullable enable
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;


namespace Players.Bullets
{
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(NetworkObject))]
    [RequireComponent(typeof(NetworkTransform))]
    public class Bullet : NetworkBehaviour
    {
        [SerializeField] private float damage = 10;
        [SerializeField] private float speed = 10;
        [SerializeField] private Vector2 vector2;
        [SerializeField] private NetworkObject networkObject = null!;


        private void Update()
        {
            transform.Translate(vector2 * (speed * Time.deltaTime));
        }


        public void Spawn(Vector2 direction)
        {
            vector2 = direction.normalized;
            networkObject.Spawn();
        }


        private void OnCollisionEnter2D(Collision2D col)
        {
            if (IsServer && col.collider!.TryGetComponent(out Player player))
            {
                player!.ApplyDamage(damage);
                networkObject.Despawn();
            }
        }
    }
}