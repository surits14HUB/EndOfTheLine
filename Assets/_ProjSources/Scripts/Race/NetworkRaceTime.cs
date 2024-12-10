using Fusion;

namespace RacingOnline.Race
{
    /// <summary>
    /// This entire script is referenced from the Fusion SDK's KartLapController
    /// Only the required functionality is added for simplicity purposes and minor modifications are made to suit this racing project
    /// </summary>
    public class NetworkRaceTime : NetworkBehaviour
    {
        internal System.Action<NetworkRaceTime> OnRaceCompleted;
        [Networked] public int StartRaceTick { get; set; }
        [Networked] public int EndRaceTick { get; set; }
        private ChangeDetector _changeDetector;
        private bool hasEnded;

        public override void Spawned()
        {
            base.Spawned();

            _changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);
        }
        /// <summary>
        /// Invoked to assign the start tick value
        /// </summary>
        public void OnRaceStart()
        {
            StartRaceTick = Runner.Tick;
        }
        /// <summary>
        /// Invoked by the OnTriggerStay function when the car crosses the finish line collider
        /// This method is called as long as the car is in the collider
        /// Therefor a bool check is made sure to call it only once
        /// Marks the end time tick for each car in the race (Calculated only in the host's simulation)
        /// </summary>
        public void ProcessFinishLine()
        {
            if (!hasEnded)
            {
                hasEnded = true;
                EndRaceTick = Runner.Tick;
                OnRaceCompleted?.Invoke(this);
            }
        }
        /// <summary>
        /// Calculates the time taken to complete the race after starting
        /// </summary>
        /// <returns></returns>
        public float GetTotalRaceTime()
        {
            if (!Runner.IsRunning || StartRaceTick == 0)
                return 0f;

            var endTick = EndRaceTick == 0 ? Runner.Tick.Raw : EndRaceTick;
            return TickHelper.TickToSeconds(Runner, endTick - StartRaceTick);
        }
    }
}