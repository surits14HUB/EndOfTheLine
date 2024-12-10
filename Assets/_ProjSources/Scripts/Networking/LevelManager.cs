using System.Collections;
using Fusion;
using UnityEngine;

namespace RacingOnline.Networking
{
    /// <summary>
    /// Networking scene manager
    /// Any scene load request made by the host will affect all the clients as this is assigned as the scene manager during the session creation
    /// This Class is referenced from the Photo Fusion SDK's Karts Sample
    /// </summary>
    public class LevelManager : NetworkSceneManagerDefault
    {
        #region Variables & Properties

        public static LevelManager Instance { get; private set; }

        #endregion

        #region Monobehaviour Methods

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this.gameObject);
            }
            else
            {
                Instance = this;
            }

            DontDestroyOnLoad(this.gameObject);
        }

        #endregion

        #region Custom Methods

        /// <summary>
        /// calls the network runner's loadscene function
        /// </summary>
        /// <param name="sceneIndex">Index of the scene listed in the build settings scenes list</param>
        public void LoadRaceScene(int sceneIndex)
        {
            Runner.LoadScene(SceneRef.FromIndex(sceneIndex));
        }
        /// <summary>
        /// An overrided method of LoadSceneCoroutine
        /// This extends the base class to add more functions after the scene is loaded async
        /// This function is referenced from the Photo Fusion SDK's Karts Sample
        /// </summary>
        /// <param name="sceneRef"></param>
        /// <param name="sceneParams"></param>
        /// <returns></returns>
        protected override IEnumerator LoadSceneCoroutine(SceneRef sceneRef, NetworkLoadSceneParameters sceneParams)
        {
#if DEV_LOGS
            Debug.Log($"LoadSceneCoroutine {sceneRef}");
#endif
            yield return base.LoadSceneCoroutine(sceneRef, sceneParams);

            // Delay one frame, so we're sure level objects has spawned locally
            yield return null;

            // Now we can safely spawn the cars
            PlayersManager.Instance.SpawnCars();
        }

        #endregion
    }
}
