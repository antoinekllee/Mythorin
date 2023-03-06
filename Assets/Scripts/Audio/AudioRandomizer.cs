using UnityEngine; 

namespace RPG.Audio
{
    public class AudioRandomizer : MonoBehaviour
    {
        [SerializeField] AudioClip[] audioClips = default;
        private AudioSource audioSource;
        float defaultPitch, upperRange, lowerRange; 

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            // audioSource.clip = audioClips[0]; // INDEX OUT OF RANGE instead set by default
            defaultPitch = audioSource.pitch; 
            upperRange = defaultPitch * 1.1f;
            lowerRange = defaultPitch * 0.9f;
        }

        public void PlayRandomClip()
        {
            // Screen.SetResolution(2048, 1280, false, 60); // change screen size
            
            if (audioClips.Length > 1)
            {
                AudioClip prevClip = audioSource.clip;

                do
                {
                    audioSource.clip = audioClips[Random.Range(0, audioClips.Length)];
                } while (audioSource.clip == prevClip);
            }

            audioSource.pitch = Random.Range(lowerRange, upperRange);
            audioSource.Play();
        }
    }
}