using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

namespace RacingOnline.Networking
{
    /// <summary>
    /// Manages all the network players in the game session
    /// </summary>
    public class PlayersManager : MonoBehaviour
    {
        #region Variables & Properties

        internal NetworkRunner thisRunner;
        internal NetworkPlayer thisPlayer;
        internal System.Action<NetworkPlayer> OnPlayerJoined;
        internal System.Action<NetworkPlayer> OnPlayerLeft;
        internal List<NetworkPlayer> spawnedPlayers = new List<NetworkPlayer>();
        public static PlayersManager Instance { get; private set; }

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
        /// Executed When the host presses the Start Game Button
        /// Checks if all the joined network players are ready (isReady toggled)
        /// </summary>
        internal void OnStartGamePressed()
        {
            var count = 0;

            for (int i = 0; i < spawnedPlayers.Count; i++)
            {
                var networkPlayer = spawnedPlayers[i];

                if (networkPlayer.isReady)
                {
                    count++;
                }
            }

            if (spawnedPlayers.Count == count)
            {
#if DEV_LOGS
                Debug.Log($"Begin Game");
#endif
                NetworkController.Instance.LockRoom();
                LevelManager.Instance.LoadRaceScene(AppConstants.RACE);
            }
            else
            {
#if DEV_LOGS
                Debug.LogError($"Few Players are not Ready");
#endif
                Lobby.LobbyController.ChangeUIStage?.Invoke(Lobby.UIStage.WAIT_FOR_OTHERS);
            }
        }
        /// <summary>
        /// Invoked everytime the user toggles the isReady toggle
        /// Sends a RPC message to the network so that the proxy in the other local simulations of the other users get status changed
        /// </summary>
        /// <param name="value"></param>
        internal void OnMarkReady(bool value)
        {
            for (int i = 0; i < spawnedPlayers.Count; i++)
            {
                var networkPlayer = spawnedPlayers[i];

                if (networkPlayer.HasInputAuthority)
                {
                    // Send a RPC message to change the status of isReady
                    networkPlayer.RPC_ChangeReadyState(value);
                }
            }
        }
        /// <summary>
        /// Spawns the race car as soon as the Race scene is loaded
        /// </summary>
        internal void SpawnCars()
        {
            StartCoroutine(GetTrackAndSpawnPlayers());
        }
        /// <summary>
        /// Spawns a new car for every NetworkPlayer connected in the session before the Host presses the Start Game Button
        /// </summary>
        /// <returns></returns>
        private IEnumerator GetTrackAndSpawnPlayers()
        {
            yield return new WaitUntil(() => Race.RaceTrackManager.Instance != null);

            if (thisRunner.IsServer)
            {
#if DEV_LOGS
                Debug.Log($"Spawning Cars");
#endif
                for (int i = 0; i < spawnedPlayers.Count; i++)
                {
                    Race.RaceTrackManager.Instance.SpawnCar(i, spawnedPlayers[i]);
                }
            }

            //Wait for 1 second after the cars are spawned before the countdown begins
            yield return new WaitForSeconds(1.0f);

            Race.RaceTrackManager.Instance.StartRaceCountdown();
        }
        internal NetworkPlayer GetNetworkPlayer()
        {
            var networkObj = NetworkController.Instance.GetLocalPlayer();
            return networkObj.GetComponent<NetworkPlayer>();
        }
        /// <summary>
        /// Executes the Exit procedure for the host and clients and all proxies
        /// </summary>
        /// <param name="networkPlayer"></param>
        /// <returns></returns>
        public IEnumerator PerformNetworkExit(NetworkPlayer networkPlayer)
        {
            yield return new WaitForSeconds(0.5f);
            // Remove the network player subscription to the OnRace Completed event
            if (Race.RaceTrackManager.Instance != null)
            {
                Race.RaceTrackManager.Instance.RemoveRelatedItems(networkPlayer);
            }
            // Invokes the player left action
            // This is executed only in the Networking scene
            // To remove the player list item from the list of players joined
            OnPlayerLeft?.Invoke(networkPlayer);
            // Skip a frame
            yield return null;
            // Caching the input authority state of the network object to be used after the network obbject is despawned
            var isInput = networkPlayer.Object.HasInputAuthority;
            // All despawning and removing network objects is done only if the network runner is server (HOST)
            if (networkPlayer.Runner.IsServer)
            {
                if (networkPlayer.car != null)
                {
                    NetworkController.Instance.RemoveNetworkObject(networkPlayer.car.Object);
                    yield return null;
                }
                spawnedPlayers.Remove(networkPlayer);
                NetworkController.Instance.RemoveNetworkObject(networkPlayer.Object);
                yield return null;
            }
            #if DEV_LOGS
            Debug.Log(isInput);
            #endif

            if (isInput)
            {
                // Only if the network player has input authority the exit session is called to shutdown the network runner
                NetworkController.Instance.ExitSession();
            }
        }

        #endregion
    }
}
