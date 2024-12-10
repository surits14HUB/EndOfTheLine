using UnityEngine;

namespace RacingOnline.Lobby
{
    public class ClientRaceSettings : RaceSettings
    {
        #region Variables & Properties

        [Header ("Client Side Settings")]
        [Space (10)]
        [SerializeField] TMPro.TMP_InputField sessionCodeInputField;

        #endregion

        #region Monobehaviour Methods

        protected override void Awake()
        {
            base.Awake();

            nameInputField.onValueChanged.AddListener(OnNameValueChanged);
            sessionCodeInputField.onValueChanged.AddListener(OnGameCodeValueChanged);
        }

        #endregion

        #region Custom Methods

        protected override void OnNameValueChanged(string value)
        {
            CheckInteractionStatus();
        }
        /// <summary>
        /// When the value of the session code changes it is saved to the settings scriptable object
        /// Checks the Confirm button's interaction state on every change made
        /// </summary>
        /// <param name="value"></param>
        private void OnGameCodeValueChanged(string value)
        {
            settings.sessionName = value;
            CheckInteractionStatus();
        }
        /// <summary>
        /// This methods checks and enables the confirm button's interactable state if the name and session code field length is greater than zero
        /// </summary>
        private void CheckInteractionStatus()
        {
            confirmButton.interactable = sessionCodeInputField.text.Length > 0 && nameInputField.text.Length > 0;
        }

        #endregion
    }
}