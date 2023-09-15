using UnityEngine;

namespace Audio
{
    [RequireComponent(typeof(AudioSource))]
    public class AudioManager : MonoBehaviour
    {
        [SerializeField] AudioChannelSO channel;
    
        AudioSource _source;

        void Awake()
        {
            _source = GetComponent<AudioSource>();
        }

        void OnEnable()
        {
            channel.OnPlayAudio += OnPlayAudio;
        }
        
        void OnDisable()
        {
            channel.OnPlayAudio -= OnPlayAudio;
        }

        void OnPlayAudio(AudioClip clip)
        {
            if (clip == _source.clip)
                return;
            
            _source.Stop();
            _source.clip = clip;
            _source.Play();
        }
    }
}
