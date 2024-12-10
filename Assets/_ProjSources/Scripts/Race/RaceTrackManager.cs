using Fusion;
using RacingOnline.Networking;
using UnityEngine;

namespace RacingOnline.Race
{
    /// <summary>
    /// This class maintains the entire race track details and time
    /// This class is responsible for the spawning of the network cars for each network player
    /// </summary>
    public class RaceTrackManager : NetworkBehaviour
    {
        #region Variables & Properties

        public static RaceTrackManager Instance { get; private set; }
        [SerializeField] float countdownDelayInSeconds;
        [SerializeField] RaceUIController raceUIController;
        [Networked] public TickTimer StartRaceTimer { get; set; }
        [SerializeField] Transform[] startPoints;
        [SerializeField] NetworkCar[] carPrefabs;

        #endregion

        #region Monobehaviour Methods

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
            }
            else
            {
                Instance = this;
            }
        }

        #endregion

        #region Custom Methods

        public override void Spawned()
        {
            base.Spawned();

            // This code is executed only for the host player and the variable is synced with all the other player with input authority
            if (PlayersManager.Instance.thisPlayer.Object.IsValid && PlayersManager.Instance.thisPlayer.Object.HasStateAuthority)
            {
                // Setting the start time tick with a small delay and 4 seconds to show the 3,2,1,GO! countdown animation
                StartRaceTimer = TickTimer.CreateFromSeconds(Runner, countdownDelayInSeconds + 4f);
            }
        }
        /// <summary>
        /// A new network car is spawned for each network player as soon as the Race scene loads
        /// </summary>
        /// <param name="index"></param>
        /// <param name="networkPlayer"></param>
        internal void SpawnCar(int index, NetworkPlayer networkPlayer)
        {
            if (index < startPoints.Length)
            {
                var networkCar = Runner.Spawn(carPrefabs[index], startPoints[index].position, startPoints[index].rotation, networkPlayer.Object.InputAuthority);
                networkPlayer.car = networkCar;
                networkCar.username = networkPlayer.username.ToString();
            }
        }
        /// <summary>
        /// Enables the countdown animation for the network players with input authority
        /// </summary>
        internal void StartRaceCountdown()
        {
            raceUIController.startCountdown = true;
        }

        #endregion

        #region Network Methods

        /// <summary>
        /// Invoked whenever the network car crosses the finish line
        /// </summary>
        /// <param name="networkRaceTime"></param>
        internal void OnCarCompletedRace(NetworkRaceTime networkRaceTime)
        {
            networkRaceTime.OnRaceCompleted = null;
            // Called to show the race completed UI panel
            if (networkRaceTime.Object.HasInputAuthority)
            {
                raceUIController.LocalPlayerCompletedRace?.Invoke();
            }
            // Sharing the car details upon completion to the shown the time taken and driver's name
            var details = new RaceCompletionDetails();
            details.driverName = networkRaceTime.GetComponent<NetworkCar>().username.ToString();
            details.timeTaken = networkRaceTime.GetTotalRaceTime();
            raceUIController.OnPlayerCompletedRace(details);
        }
        internal void RemoveRelatedItems(NetworkPlayer networkPlayer)
        {
            networkPlayer.car.networkRaceTime.OnRaceCompleted = null;
        }

        #endregion
    }
}
