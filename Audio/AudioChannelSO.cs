using UnityEngine;
using UnityEngine.Events;

namespace Audio
{
    [CreateAssetMenu(menuName = "Event Channels/Audio Channel")]
    public class AudioChannelSO : ScriptableObject
    {
        public event UnityAction<AudioClip> OnPlayAudio = delegate {  };
        public event UnityAction OnStopAudio = delegate {  };

        public void PlayAudio(AudioClip clip) => OnPlayAudio.Invoke(clip);

        public void StopAudio() => OnStopAudio.Invoke();
    }
}
