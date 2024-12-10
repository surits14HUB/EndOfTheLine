using UnityEngine;
using UnityEngine.UI;

namespace RacingOnline.Lobby
{
    /// <summary>
    /// The base class for the race creation and joining settings.
    /// </summary>
    public abstract class RaceSettings : MonoBehaviour
    {
        #region Variables & Properties

        [Header ("Common Settings")]
        [Space (10)]
        [SerializeField] protected Settings settings;
        [SerializeField] protected TMPro.TMP_InputField nameInputField;
        [SerializeField] protected Button confirmButton;

        #endregion

        #region Monobehaviour Methods

        protected virtual void Awake()
        {
            confirmButton.onClick.AddListener(OnSettingsConfirmed);
        }
        protected virtual void OnEnable()
        {
            GetSavedName();
        }

        #endregion

        #region Custom Methods

        /// <summary>
        /// OnEnable Gets the name entered in the previous session
        /// </summary>
        private void GetSavedName()
        {
            if (PlayerPrefs.HasKey(AppConstants.SAVED_NAME))
            {
                nameInputField.text = PlayerPrefs.GetString(AppConstants.SAVED_NAME);
            }
        }
        /// <summary>
        /// Abstract method that should be implemented by derived class since both the host and client side deal with Name Field
        /// It is overrided by both because confirm button's interactive state depends on the name field as well as the session code in the case of a client
        /// </summary>
        /// <param name="value"></param>
        protected abstract void OnNameValueChanged(string value);
        /// <summary>
        /// Inovked when the confirm button is interactable and clicked by the player
        /// </summary>
        protected void OnSettingsConfirmed()
        {
            // Invokes the UI stage change to display the next panel
            LobbyController.ChangeUIStage?.Invoke(UIStage.NETWORKING);

            SavePlayerName();
        }
        /// <summary>
        /// Saves the player name to the Player Prefs as well as to the settings scriptable object
        /// </summary>
        private void SavePlayerName()
        {
            settings.playerName = nameInputField.text;
            PlayerPrefs.SetString(AppConstants.SAVED_NAME, nameInputField.text);
            PlayerPrefs.Save();
        }

        #endregion
    }
}
