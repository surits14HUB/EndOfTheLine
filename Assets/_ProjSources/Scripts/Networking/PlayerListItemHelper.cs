using System;
using UnityEngine;
using UnityEngine.UI;

namespace RacingOnline.Networking
{
    public class PlayerListItemHelper : MonoBehaviour
    {
        #region Variables & Properties

        [SerializeField] internal NetworkPlayer networkPlayer;
        [SerializeField] TMPro.TMP_Text playerNameText;
        [SerializeField] Image playerColor;
        [SerializeField] TMPro.TMP_Text playerIndexText;

        #endregion

        #region Monobehaviour Methods

        #endregion

        #region Custom Methods

        /// <summary>
        /// Inits the player list item with the username and color hex value
        /// </summary>
        /// <param name="index"></param>
        internal void Init(int index)
        {
            playerNameText.text = networkPlayer.username.ToString();
            playerIndexText.text = index.ToString();

            if (!string.IsNullOrEmpty(networkPlayer.colorHex.ToString()))
            {
                var color = Color.white;
                ColorUtility.TryParseHtmlString(networkPlayer.colorHex.ToString(), out color);
                playerColor.color = color;
            }

            // Subscribing to this event to make changes to the player list item's data whenever the player's data changes are received through RPC
            networkPlayer.OnPlayerDetailsUpdated = OnDetailsUpdated;
        }
        /// <summary>
        /// Subscribed to and invoked whenever the updated username and color hex values are received through RPC by the Network Player
        /// </summary>
        /// <param name="playerName"></param>
        /// <param name="colorHex"></param>
        private void OnDetailsUpdated(string playerName, string colorHex)
        {
            playerNameText.text = playerName;

            var color = Color.white;
            ColorUtility.TryParseHtmlString(colorHex, out color);
            playerColor.color = color;
        }

        #endregion
    }
}
