using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RacingOnline.Lobby
{
    public enum UIStage
    {
        LANDING,
        HOST_RACE_SETTINGS,
        CLLIENT_RACE_SETTINGS,
        NETWORKING,
        ROOM_NOT_FOUND,
        ROOM_JOINING_ISSUE,
        WAIT_FOR_OTHERS
    }
    public class LobbyController : MonoBehaviour
    {
        #region Variables & Properties

        [Header ("ALL UI Panels")]
        [Space (10)]
        [SerializeField] GameObject landingPanel;
        [SerializeField] GameObject hostRaceSettingsPanel;
        [SerializeField] GameObject clientRaceSettingsPanel;
        [SerializeField] GameObject roomNotFoundPanel;
        [SerializeField] GameObject roomJoiningIssuePanel;
        [SerializeField] GameObject waitForOthersPanel;
        public static Action<UIStage> ChangeUIStage;

        #endregion

        #region Monobehaviour Methods

        private void Awake()
        {
            // Subscribing the method to ChangeUIStage action
            ChangeUIStage = OnUIStageChanged;
        }
        private void Start()
        {
            // Invoking the landing page UI Stage in the first frame
            ChangeUIStage?.Invoke(UIStage.LANDING);
        }
        private void OnDestroy()
        {
            ChangeUIStage = null;
        }

        #endregion

        #region Custom Methods

        /// <summary>
        /// Whenever the ChangeUIStage action is invoked, this method will be called
        /// </summary>
        /// <param name="stage">The UI Stage to be displayed in the screen</param>
        private void OnUIStageChanged(UIStage stage)
        {
            landingPanel.SetActive(stage == UIStage.LANDING);
            hostRaceSettingsPanel.SetActive(stage == UIStage.HOST_RACE_SETTINGS);
            // After the client enters all the details and clicks confirm button,
            // The Photo fusion network runner will try to join the session,
            // In the event the Room was not found and any other issue, the CLient Race Settings page will be opened by default
            clientRaceSettingsPanel.SetActive(stage == UIStage.CLLIENT_RACE_SETTINGS || stage == UIStage.ROOM_NOT_FOUND || stage == UIStage.ROOM_JOINING_ISSUE);
            roomNotFoundPanel.SetActive(stage == UIStage.ROOM_NOT_FOUND);
            roomJoiningIssuePanel.SetActive(stage == UIStage.ROOM_JOINING_ISSUE);
            waitForOthersPanel.SetActive(stage == UIStage.WAIT_FOR_OTHERS);

            if (stage == UIStage.NETWORKING)
            {
                LoadNetworkingScene();
            }
        }
        private void LoadNetworkingScene()
        {
            // Subscribing to the OnSceneLoaded event
            SceneManager.sceneLoaded += OnSceneLoaded;
            // Loading the networking scene as an additive scene
            // The lobby and networking scenes go hand in hand for error handling,
            // hence loaded additively
            SceneManager.LoadSceneAsync(AppConstants.NETWORKING, LoadSceneMode.Additive);
        }
        private void OnSceneLoaded(Scene scene, LoadSceneMode arg1)
        {
            // If the loaded scene is Networking
            if (scene.name == AppConstants.NETWORKING)
            {
                // Unsubscribing OnSceneLoaded event
                SceneManager.sceneLoaded -= OnSceneLoaded;
                // Once the scene is loaded, the Network Runner Join Session function is called
                Networking.NetworkController.Instance.JoinOrCreateLobby();
            }
        }

        #endregion
    }
}
