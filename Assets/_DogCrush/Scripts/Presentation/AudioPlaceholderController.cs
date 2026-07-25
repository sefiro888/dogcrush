using UnityEngine;

namespace DogCrush.Presentation
{
    public class AudioPlaceholderController : MonoBehaviour
    {
        public AudioSource sfxSource;
        public AudioSource musicSource;

        [Header("Audio Clips (Optional)")]
        public AudioClip selectClip;
        public AudioClip matchClip;
        public AudioClip comboClip;
        public AudioClip timerWarningClip;
        public AudioClip gameOverClip;

        private void Awake()
        {
            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
                sfxSource.playOnAwake = false;
            }
            if (musicSource == null)
            {
                musicSource = gameObject.AddComponent<AudioSource>();
                musicSource.loop = true;
                musicSource.playOnAwake = false;
            }
        }

        public void PlaySelectSound()
        {
            if (selectClip != null)
                sfxSource.PlayOneShot(selectClip);
        }

        public void PlayMatchSound()
        {
            if (matchClip != null)
                sfxSource.PlayOneShot(matchClip);
        }

        public void PlayComboSound()
        {
            if (comboClip != null)
                sfxSource.PlayOneShot(comboClip);
        }

        public void PlayTimerWarningSound()
        {
            if (timerWarningClip != null)
                sfxSource.PlayOneShot(timerWarningClip);
        }

        public void PlayGameOverSound()
        {
            if (gameOverClip != null)
                sfxSource.PlayOneShot(gameOverClip);
        }
    }
}
