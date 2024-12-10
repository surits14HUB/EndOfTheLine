using Fusion;
using UnityEngine;

namespace RacingOnline.Networking
{
    public class NetworkPlayer : NetworkBehaviour
    {
        #region Variables & Properties

        [Networked] public NetworkBool isReady { get; set; }
        [Networked] public NetworkString<_32> username { get; set; }
        [Networked] public NetworkString<_32> colorHex { get; set; }
        [Networked] public int playerId { get; set; }
        [Networked] public Race.NetworkCar car { get; set; }
        internal System.Action<string, string> OnPlayerDetailsUpdated;

        #endregion

        #region Monobehaviour Methods

        #endregion

        #region Custom Methods

        /// <summary>
        /// Called after this Network Objeect is spawned in the server
        /// </summary>
        public override void Spawned()
        {
            base.Spawned();

            if (Object.HasInputAuthority)
            {
                // Setting the username from the PlayerPrefs
                username = PlayerPrefs.GetString(AppConstants.SAVED_NAME);
                // Sending a RPC message to State Authority
                RPC_SetupPlayer(username, colorHex);

#if DEV_LOGS
                Debug.Log($"username : {username}");
#endif
            }

            DontDestroyOnLoad(gameObject);
            // When this action is invoked, a player list item is added in the network connection scene
            PlayersManager.Instance.OnPlayerJoined?.Invoke(this);
        }

        #endregion

        #region RPC Methods

        /// <summary>
        /// RPC message that receives the username and color hex value assigned to the player's car
        /// </summary>
        /// <param name="username"></param>
        /// <param name="colorHex"></param>
        [Rpc(sources: RpcSources.InputAuthority, targets: RpcTargets.StateAuthority)]
        private void RPC_SetupPlayer(NetworkString<_32> username, NetworkString<_32> colorHex)
        {
            this.username = username;
            this.colorHex = colorHex;
            // Invokes an action that updates the player list item's data with proper username and color hex value
            OnPlayerDetailsUpdated?.Invoke(username.ToString(), colorHex.ToString());
        }
        /// <summary>
        /// Sends a RPC message to change the status of the isReady variable
        /// This state is used by the host to start the game only if all the users are ready
        /// </summary>
        /// <param name="state"></param>
        [Rpc(sources: RpcSources.InputAuthority, targets: RpcTargets.StateAuthority)]
        internal void RPC_ChangeReadyState(NetworkBool state)
        {

#if DEV_LOGS
            Debug.Log($"Setting {Object.Name} ready state to {state}");
#endif
            isReady = state;
        }

        #endregion
    }
}
