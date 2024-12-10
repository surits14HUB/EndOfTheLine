using UnityEngine;

namespace RacingOnline.Race
{
    /// <summary>
    /// This entire script is referenced from the Fusion SDK's Karts example scene
    /// </summary>
    public class FinishLine : MonoBehaviour
    {
        /// <summary>
        /// When the car crosses the finish line collider, this method is invoked
        /// </summary>
        /// <param name="other"></param>
        private void OnTriggerStay(Collider other)
        {
            if (other.TryGetComponent(out NetworkRaceTime networkRaceTime))
            {
                networkRaceTime.ProcessFinishLine();
            }
        }
    }
}