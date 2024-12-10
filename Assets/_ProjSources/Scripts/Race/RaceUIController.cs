using System.Collections;
using System.Collections.Generic;
using RacingOnline.Networking;
using UnityEngine;
using UnityEngine.UI;

namespace RacingOnline.Race
{
    [System.Serializable]
    public class RaceCompletionDetails
    {
        public string driverName;
        public float timeTaken;
    }

    public class RaceUIController : MonoBehaviour
    {
        #region Variables & Properties

        [SerializeField] Animator countdownAnimator;
        internal bool startCountdown;
        [SerializeField] GameObject raceCompletedPanel;
        [SerializeField] GameObject raceCompletedInfoText;
        [SerializeField] GameObject finishTablePanel;
        [SerializeField] Transform finishTableHolder;
        [SerializeField] LeaderboardItemHelper leaderboardItemHelperPrefab;
        [SerializeField] Button backToMainMenuButton;
        [SerializeField] GameObject fadeOutPanel;
        [SerializeField] GameObject fadeInPanel;
        internal System.Action LocalPlayerCompletedRace;
        private List<float> timeTakenList = new List<float>();

        #endregion

        #region Monobehaviour Methods

        private void Awake()
        {
            LocalPlayerCompletedRace = OnLocalPlayerCompletedRace;
            backToMainMenuButton.onClick.AddListener(OnBackToMainMenuClicked);
            StartCoroutine(ActivateFadeOut());
        }
        private void Update()
        {
            if (startCountdown && RaceTrackManager.Instance != null && RaceTrackManager.Instance.StartRaceTimer.IsRunning)
            {
                var remainingTime = RaceTrackManager.Instance.StartRaceTimer.RemainingTime(PlayersManager.Instance.thisRunner);
                if (remainingTime != null && remainingTime <= 3.0f)
                {
                    startCountdown = false;
#if DEV_LOGS
                    Debug.Log($"Begin Countdown Remaining = {remainingTime}");
#endif
                    countdownAnimator.enabled = true;
                }
            }
        }
        private void OnDestroy()
        {
            LocalPlayerCompletedRace = null;
        }

        #endregion

        #region Custom Methods

        private IEnumerator ActivateFadeOut()
        {
            fadeOutPanel.SetActive(true);

            yield return new WaitForSeconds(1f);

            fadeOutPanel.SetActive(false);
        }
        /// <summary>
        /// Called when the car crosses the finish line collider
        /// This method creates a leaderboard item to be displayed in the finished table
        /// </summary>
        /// <param name="raceCompletionDetails"></param>
        internal void OnPlayerCompletedRace(RaceCompletionDetails raceCompletionDetails)
        {
            raceCompletedInfoText.SetActive(false);
            finishTablePanel.SetActive(true);

            var helper = Instantiate(leaderboardItemHelperPrefab, finishTableHolder);
            helper.SetDetails(raceCompletionDetails.driverName, raceCompletionDetails.timeTaken);
            timeTakenList.Add(raceCompletionDetails.timeTaken);
            AssignPositions();
        }
        /// <summary>
        /// After every time a new item is added, the table's position is updated
        /// </summary>
        private void AssignPositions()
        {
            timeTakenList.Sort();

            for (int i = 0; i < timeTakenList.Count; i++)
            {
                for (int j = 0; j < finishTableHolder.childCount; j++)
                {
                    var helper = finishTableHolder.GetChild(j).GetComponent<LeaderboardItemHelper>();

                    if (helper.timeTaken == timeTakenList[i])
                    {
                        finishTableHolder.GetChild(j).SetSiblingIndex(i);
                        helper.ChangePosition(i, timeTakenList[0]);
                        break;
                    }
                }
            }
        }
        /// <summary>
        /// As soon as the car cross the finish line, this method is invoked to display the race completed panel
        /// </summary>
        private void OnLocalPlayerCompletedRace()
        {
            raceCompletedPanel.SetActive(true);
            // An info text is shown until the table contains atleast one item
            raceCompletedInfoText.SetActive(finishTableHolder.childCount == 0);
            // The finish table is activated only after the first item is created
            finishTablePanel.SetActive(finishTableHolder.childCount > 0);
        }
        /// <summary>
        /// Called when the user clicks the Back to Main Menu button
        /// This method triggers the entire refreshing of the network session until shutting down the network runner
        /// </summary>
        private void OnBackToMainMenuClicked()
        {
            fadeInPanel.SetActive(true);
            var networkPlayer = PlayersManager.Instance.GetNetworkPlayer();
            // Init the exit procedure
            StartCoroutine(PlayersManager.Instance.PerformNetworkExit(networkPlayer));
        }

        #endregion
    }
}
