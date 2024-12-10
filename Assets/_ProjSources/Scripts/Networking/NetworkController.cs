using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Addons.Physics;
using Fusion.Sockets;
using UnityEngine;

namespace RacingOnline.Networking
{
    public class NetworkController : MonoBehaviour, INetworkRunnerCallbacks
    {
        #region Variables & Properties

        [Header("Main")]
        [Space(10)]
        [SerializeField] Settings settings;
        [SerializeField] NetworkPlayer networkPlayerPrefab;
        private NetworkRunner networkRunner;
        internal static Action<LobbyInfo> OnLocalPlayerJoined;
        public static NetworkController Instance { get; private set; }

        #endregion

        #region Monobehaviour Methods

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this.gameObject);
            }
            else
            {
                Instance = this;
            }

            DontDestroyOnLoad(this.gameObject);
        }

        #endregion

        #region Custom Methods

        /// <summary>
        /// Creates the network runner on Networking scene load completed event
        /// </summary>
        internal void JoinOrCreateLobby()
        {
            GameObject go = new GameObject("RaceSession");
            DontDestroyOnLoad(go);

            networkRunner = go.AddComponent<NetworkRunner>();
            var sim3D = go.AddComponent<RunnerSimulatePhysics3D>();
            sim3D.ClientPhysicsSimulation = ClientPhysicsSimulation.SimulateAlways;

            networkRunner.ProvideInput = true;
            networkRunner.AddCallbacks(this);
#if DEV_LOGS
            Debug.Log($"Created gameobject {go.name} - starting game");
#endif

            // Based on the player type the Start Game Arguments differ
            if (settings.playerType == PlayerType.HOST)
            {
                networkRunner.StartGame(HostStartGameArgs());
            }
            else
            {
                networkRunner.StartGame(ClientStartGameArgs());
            }
        }
        /// <summary>
        /// Called to close the Fusion Network
        /// </summary>
        internal void ExitSession()
        {
            networkRunner.Shutdown();
        }
        /// <summary>
        /// Remove a Network Object from the Network Runner
        /// Only the Server (Host) side can execute this code inside this function
        /// </summary>
        /// <param name="networkObject"></param>
        internal void RemoveNetworkObject(NetworkObject networkObject)
        {
            if (networkRunner.IsServer)
            {
                networkRunner.Despawn(networkObject);
            }
        }
        /// <summary>
        /// Simple helper functions to Get Network Object using the PlayerRef provide by Fusion SDK
        /// </summary>
        /// <param name="playerRef"></param>
        /// <returns></returns>
        internal NetworkObject GetNetworkObject(PlayerRef playerRef)
        {
            for (int i = 0; i < networkRunner.GetAllNetworkObjects().Count; i++)
            {
                var networkObject = networkRunner.GetAllNetworkObjects()[i];
                if (networkObject.InputAuthority == playerRef)
                {
                    return networkObject;
                }
            }
            return null;
        }
        internal NetworkObject GetLocalPlayer()
        {
            return GetNetworkObject(networkRunner.LocalPlayer);
        }
        /// <summary>
        /// Locks the room after the Host clicks Start Game button
        /// No one user can join with the session code once the race mode has been entered
        /// </summary>
        internal void LockRoom()
        {
            if (networkRunner != null && networkRunner.SessionInfo.IsValid)
            {
                networkRunner.SessionInfo.IsOpen = false;
            }
        }
        /// <summary>
        /// Creates a very unique session name
        /// Derived from the universal constant TIME
        /// </summary>
        /// <returns></returns>
        private string CreateUniqueSessionName()
        {
            var time = System.DateTime.Now;

            return $"{time.Second}{time.Hour}{time.Minute}";
        }
        private StartGameArgs HostStartGameArgs()
        {
            settings.sessionName = CreateUniqueSessionName();

            return new StartGameArgs
            {
                GameMode = GameMode.Host,
                SessionName = settings.sessionName,
                SceneManager = LevelManager.Instance,
                PlayerCount = settings.maxPlayers,
                IsVisible = false
            };
        }
        private StartGameArgs ClientStartGameArgs()
        {
            return new StartGameArgs
            {
                GameMode = GameMode.Client,
                SessionName = settings.sessionName,
                SceneManager = LevelManager.Instance,
                EnableClientSessionCreation = false
            };
        }

        #endregion

        #region Fusion Network Callback Methods

        public void OnConnectedToServer(NetworkRunner runner) { }
        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
        public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
        public void OnInput(NetworkRunner runner, NetworkInput input) { }
        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
        public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
        public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            // Assigning the PlayersManager.Instance.thisRunner after the user joins
            PlayersManager.Instance.thisRunner = runner;

            // The server side can only spawn new network objects on player joined event
            if (runner.IsServer)
            {
                var newPlayer = runner.Spawn(networkPlayerPrefab, Vector3.zero, Quaternion.identity, player);
                newPlayer.playerId = player.AsIndex;
            }
        }
        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
            // The server side performs the exit procedure for a network player
            if (runner.IsServer)
            {
                var networkObject = GetNetworkObject(player);
                var networkPlayer = networkObject.GetComponent<NetworkPlayer>();
#if DEV_LOGS
                Debug.Log(networkPlayer.gameObject.name);
#endif
                // Async procedure to wait a frame after a step is executed
                StartCoroutine(PlayersManager.Instance.PerformNetworkExit(networkPlayer));
            }
        }
        public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
        public void OnSceneLoadDone(NetworkRunner runner) { }
        public void OnSceneLoadStart(NetworkRunner runner) { }
        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
        {
            // A simple setup to show error handling while the user fails to join a session
            switch (shutdownReason)
            {
                case ShutdownReason.GameClosed:
                case ShutdownReason.GameIsFull:
                case ShutdownReason.GameNotFound:
                    {
                        UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(AppConstants.NETWORKING);

                        if (runner)
                            Destroy(runner.gameObject);

                        Lobby.LobbyController.ChangeUIStage?.Invoke(Lobby.UIStage.ROOM_NOT_FOUND);
                    }
                    break;
                // Apart of the above three cases, all the other cases will execute this code
                default:
                    {
                        UnityEngine.SceneManagement.SceneManager.LoadScene(AppConstants.LOBBY);

                        if (runner)
                            Destroy(runner.gameObject);
                    }
                    break;
            }
        }
        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }

        #endregion
    }
}
