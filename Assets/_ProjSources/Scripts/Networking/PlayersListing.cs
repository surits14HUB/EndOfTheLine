using System;
using ExitGames.Client.Photon.StructWrapping;
using Fusion;
using UnityEngine;
using UnityEngine.UI;

namespace RacingOnline.Networking
{
    public class PlayersListing : MonoBehaviour
    {
        #region Variables & Properties

        [SerializeField] Settings settings;
        [SerializeField] Transform playersListHolder;
        [SerializeField] PlayerListItemHelper playerListItemHelperPrefab;
        [SerializeField] Toggle markReadyToggle;
        [SerializeField] Button startGameButton;
        [SerializeField] TMPro.TMP_Text sessionCodeText;
        [SerializeField] GameObject playerListPanel;
        [SerializeField] GameObject hostInfoText;
        [SerializeField] GameObject clientInfoText;

        #endregion

        #region Monobehaviour Methods

        private void Awake()
        {
            // Only the host will have access to the Start Game Button
            if (settings.playerType == PlayerType.HOST)
            {
                startGameButton.onClick.AddListener(OnStartGamePressed);
                hostInfoText.SetActive(true);
            }
            else
            {
                sessionCodeText.text = $"Session Code : {settings.sessionName}";
                clientInfoText.SetActive(true);
            }

            markReadyToggle.onValueChanged.AddListener(OnMarkReady);

            PlayersManager.Instance.OnPlayerJoined += AddPlayer;
            PlayersManager.Instance.OnPlayerJoined += UpdateUI;
            PlayersManager.Instance.OnPlayerLeft = RemovePlayer;
        }

        void OnDestroy()
        {
            PlayersManager.Instance.OnPlayerJoined = null;
            PlayersManager.Instance.OnPlayerLeft = null;
        }

        #endregion

        #region Custom Methods

        /// <summary>
        /// Invoked when the Start Game Button is pressed
        /// </summary>
        private void OnStartGamePressed()
        {
            this.gameObject.SetActive(true);
            PlayersManager.Instance.OnStartGamePressed();
        }
        /// <summary>
        /// Called the Mark Ready toggle value changes
        /// </summary>
        /// <param name="value"></param>
        private void OnMarkReady(bool value)
        {
            if (settings.playerType == PlayerType.HOST)
            {
                startGameButton.interactable = value;
            }

            PlayersManager.Instance.OnMarkReady(value);
        }
        /// <summary>
        /// Invoked when a new network player is spawned in the server by the PlayersManager
        /// </summary>
        /// <param name="player"></param>
        private void UpdateUI(NetworkPlayer player)
        {
            // Only UI updates are done here
            if (player.Object.HasInputAuthority)
            {
                markReadyToggle.gameObject.SetActive(true);
                sessionCodeText.gameObject.SetActive(true);
                playerListPanel.SetActive(true);
                hostInfoText.SetActive(false);
                clientInfoText.SetActive(false);
                startGameButton.gameObject.SetActive(settings.playerType == PlayerType.HOST);
                PlayersManager.Instance.OnPlayerJoined -= UpdateUI;
            }
        }
        /// <summary>
        /// Invoked after a new network player is spawned
        /// Adds a player list item to the listing to show all the players who have joined the race session
        /// </summary>
        /// <param name="networkPlayer"></param>
        internal void AddPlayer(NetworkPlayer networkPlayer)
        {
#if DEV_LOGS
            Debug.Log($"Adding new player");
#endif
            sessionCodeText.text = $"Session Code : {networkPlayer.Runner.SessionInfo.Name}";

            // Adding a list item to the panel
            var helper = Instantiate(playerListItemHelperPrefab, playersListHolder);
            helper.networkPlayer = networkPlayer;
            // Assigning a colorhex value based on the player joined index
            if (PlayersManager.Instance.thisRunner.IsServer)
            {
                networkPlayer.colorHex = settings.colorHexs[playersListHolder.childCount - 1];
            }
            // Caching the variables for later use
            if (networkPlayer.Object.HasInputAuthority)
            {
                PlayersManager.Instance.thisPlayer = networkPlayer;
            }
            helper.Init(playersListHolder.childCount);
            // Newly spawned network player is added to the list
            PlayersManager.Instance.spawnedPlayers.Add(networkPlayer);
        }
        /// <summary>
        /// Invoked when a network player is despawned from the server
        /// </summary>
        /// <param name="networkPlayer"></param>
        private void RemovePlayer(NetworkPlayer networkPlayer)
        {
            for (int i = 0; i < playersListHolder.childCount; i++)
            {
                var helper = playersListHolder.GetChild(i).GetComponent<PlayerListItemHelper>();
#if DEV_LOGS
                Debug.Log($"helper = {helper.networkPlayer}");
#endif

                if (helper.networkPlayer == networkPlayer)
                {
                    Destroy(playersListHolder.GetChild(i).gameObject);
                    break;
                }
            }

            PlayersManager.Instance.spawnedPlayers.Remove(networkPlayer);
        }

        #endregion
    }
}
