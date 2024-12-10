using System;
using UnityEngine;
using UnityEngine.UI;

namespace RacingOnline.Lobby
{
    public class LandingPage : MonoBehaviour
    {
        #region Variables & Properties

        [Header ("Main")]
        [Space (10)]
        [SerializeField] Settings settings;
        [SerializeField] TMPro.TMP_Text versionText;

        [Header ("GUI Elements")]
        [Space (10)]
        [SerializeField] Button joinGameButton;
        [SerializeField] Button createGameButton;

        #endregion

        #region Monobehaviour Methods

        private void Awake()
        {
            settings.Reset();
            joinGameButton.onClick.AddListener(OnJoinGamePressed);
            createGameButton.onClick.AddListener(OnCreateGamePressed);
        }
        private void Start()
        {
            versionText.text = $"v{Application.version}";
        }

        #endregion

        #region Custom Methods

        /// <summary>
        /// The next few steps depends on the player type set in this method
        /// The player who wants to host a new game
        /// </summary>
        private void OnCreateGamePressed()
        {
            // The player who creates the game will be the host
            settings.playerType = PlayerType.HOST;
            // Invoking the UI stage to display the Host Race settings panel
            LobbyController.ChangeUIStage?.Invoke(UIStage.HOST_RACE_SETTINGS);
        }
        /// <summary>
        /// The next few steps depends on the player type set in this method
        /// The player who wants to join an already created team
        /// </summary>
        private void OnJoinGamePressed()
        {
            // The player who creates the game will be the client
            settings.playerType = PlayerType.CLIENT;
            // Invoking the UI stage to display the Client Race settings panel
            LobbyController.ChangeUIStage?.Invoke(UIStage.CLLIENT_RACE_SETTINGS);
        }

        #endregion
    }
}
