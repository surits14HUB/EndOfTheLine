using UnityEngine;

namespace RacingOnline.Race
{
    public class LeaderboardItemHelper : MonoBehaviour
    {
        #region Variables & Properties

        [SerializeField] TMPro.TMP_Text positionText;
        [SerializeField] TMPro.TMP_Text driverNameText;
        [SerializeField] TMPro.TMP_Text timeTakenText;
        internal float timeTaken;
        [SerializeField] TMPro.TMP_Text timeDifferenceText;

        #endregion

        #region Monobehvaiour Methods

        #endregion

        #region Custom Methods

        /// <summary>
        /// Called whenever a car crosses the finish line to update the leaderboard position and data
        /// </summary>
        /// <param name="position"></param>
        /// <param name="firstPlaceTime"></param>
        internal void ChangePosition(int position, float firstPlaceTime)
        {
            positionText.text = (position + 1).ToString();
            timeDifferenceText.text = "+" + (timeTaken - firstPlaceTime).ToString("0.000") + "s";
        }
        /// <summary>
        /// Called when a leaderboard item is created
        /// </summary>
        /// <param name="name"></param>
        /// <param name="timeTaken"></param>
        internal void SetDetails(string name, float timeTaken)
        {
            driverNameText.text = name;
            timeTakenText.text = timeTaken.ToString("0.000") + "s";
            this.timeTaken = timeTaken;
        }
        /// <summary>
        /// Calculates the elapsed time of the user with respect to the time taken by the first place holder
        /// </summary>
        /// <param name="timeDifference"></param>
        internal void SetTimeDifferenceDetail(float timeDifference)
        {
            driverNameText.text = timeDifference.ToString();
        }

        #endregion
    }
}