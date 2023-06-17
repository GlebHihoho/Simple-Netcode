#nullable enable
using Players;
using System;
using System.Collections.Generic;
using UI;
using Unity.Netcode;
using UnityEngine;
using Utils;


namespace DefaultNamespace
{
    public class Session : MonoBehaviour
    {
        [SerializeField] private NetworkManager networkManager = null!;
        [SerializeField] private WinUi winUi;
        [SerializeField] private List<Player> connectedPlayers = new();


        private void Awake()
        {
            winUi.EnsureNotNull();
            networkManager.EnsureNotNull().OnServerStarted += OnServerStarted;
        }


        private void OnDestroy()
        {
            networkManager.OnServerStarted -= OnServerStarted;

            if (networkManager.IsServer)
            {
                networkManager.OnClientConnectedCallback -= NetworkManagerOnOnClientConnectedCallback;
                networkManager.OnClientDisconnectCallback -= NetworkManagerOnOnClientDisconnectCallback;
            }
        }


        private void OnServerStarted()
        {
            foreach (var (id, _) in networkManager.ConnectedClients!)
            {
                NetworkManagerOnOnClientConnectedCallback(id);
            }

            if (networkManager.IsServer)
            {
                networkManager.OnClientConnectedCallback += NetworkManagerOnOnClientConnectedCallback;
                networkManager.OnClientDisconnectCallback += NetworkManagerOnOnClientDisconnectCallback;
            }
        }


        private void NetworkManagerOnOnClientDisconnectCallback(ulong obj)
        {
            var player = networkManager.ConnectedClients![obj]!
                .PlayerObject.GetComponent<Player>().EnsureNotNull();

            player.Health.OnValueChanged -= OnValueChanged;
            connectedPlayers.Remove(player);
        }


        private void NetworkManagerOnOnClientConnectedCallback(ulong obj)
        {
            var player = networkManager.ConnectedClients![obj]!
                .PlayerObject.GetComponent<Player>().EnsureNotNull();

            player.Health.OnValueChanged += OnValueChanged;
            connectedPlayers.Add(player);
        }


        private void OnValueChanged(float previousvalue, float newvalue)
        {
            if (newvalue <= 0)
            {
                Color? winnerColor = null; 
                foreach (var connectedPlayer in connectedPlayers)
                {
                    if (winnerColor == null && connectedPlayer.Health.Value > 0)
                    {
                        winnerColor = connectedPlayer.Color.Value;
                    }
                    connectedPlayer.NetworkObject!.Despawn();
                }
                winUi.Show(winnerColor!.Value);
            }
        }
    }
}