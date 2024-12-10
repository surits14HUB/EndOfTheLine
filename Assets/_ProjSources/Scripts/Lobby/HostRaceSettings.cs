using UnityEngine;
using UnityEngine.UI;

namespace RacingOnline.Lobby
{
    public class HostRaceSettings : RaceSettings
    {
        #region Variables & Properties

        [Header ("Host Side Settings")]
        [Space (10)]
        [SerializeField] Slider playerCountSlider;
        [SerializeField] TMPro.TMP_Text playerCountText;

        #endregion

        #region Monobehaviour Methods

        protected override void Awake()
        {
            base.Awake();

            playerCountSlider.onValueChanged.AddListener(OnPlayerCountValueChanged);
            playerCountText.text = playerCountSlider.value.ToString();
            nameInputField.onValueChanged.AddListener(OnNameValueChanged);
        }
        protected override void OnEnable()
        {
            base.OnEnable();

            // By default, the slide value is set to two players
            // In the event the host does not modify the slider value, the settings.maxPlayers will be zero
            // Hence assign the variable OnEnable
            settings.maxPlayers = (int)playerCountSlider.value;
        }

        #endregion

        #region Custom Methods

        private void OnPlayerCountValueChanged(float value)
        {
            playerCountText.text = value.ToString();
            settings.maxPlayers = (int)value;
        }
        protected override void OnNameValueChanged(string value)
        {
            confirmButton.interactable = value.Length > 0;
        }

        #endregion
    }
}