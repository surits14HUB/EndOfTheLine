using UnityEngine;

namespace RacingOnline.Race
{
    /// <summary>
    /// This entire script is referenced from the Fusion SDK's Karts example scene
    /// </summary>
    public class CountdownTriggers : MonoBehaviour
    {
        #region Variables & Properties

        [SerializeField] AudioSource audioSource;
        [SerializeField] AudioClip countdownClip;
        [SerializeField] AudioClip goClip;

        #endregion

        #region Monobehaviour Methods

        #endregion

        #region Custom Methods

        /// <summary>
        /// This method invoked by the animation clip to play SFX during countdown
        /// </summary>
        public void PlayCountdownSound()
        {
            audioSource.clip = countdownClip;
            audioSource.Play();
        }
        /// <summary>
        /// This method invoked by the animation clip to play SFX during countdown
        /// </summary>
        public void FinishCountdown()
        {
            audioSource.clip = goClip;
            audioSource.Play();
        }

        #endregion
    }
}
