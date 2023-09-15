using System;
using UnityEngine;

namespace Player
{
    [RequireComponent(typeof(AudioSource))]
    [RequireComponent(typeof(Animator))]
    public class AnimationController : MonoBehaviour
    {
        [Header("SFX")]
        [SerializeField] AudioClip attackSound;
        [SerializeField] AudioClip hitSound;
        [SerializeField] AudioClip dieSound;
               
        AudioSource _audioSource;
        
        Animator _animator;
        static readonly int Speed = Animator.StringToHash("speed");
        static readonly int AttackTrigger = Animator.StringToHash("attack");
        static readonly int Hit = Animator.StringToHash("hit");
        static readonly int Die = Animator.StringToHash("death");

        void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            _animator = GetComponent<Animator>();
        }

        public void SetSpeed(float speed)
        {
            _animator.SetFloat(Speed, speed);
        }

        public void Attack()
        {
            _animator.SetTrigger(AttackTrigger);
            PlaySound(attackSound);
        }

        public void OnHit()
        {
            _animator.SetTrigger(Hit);
            PlaySound(hitSound);
        }

        public void OnDeath()
        {
            _animator.SetTrigger(Die);
            PlaySound(dieSound);
        }

        public void SetTrigger(string animation, AudioClip clip = null)
        {
            SetTrigger(Animator.StringToHash(animation), clip);
        }

        public void SetTrigger(int hash, AudioClip clip = null)
        {
            _animator.SetTrigger(hash);
            
            if (clip)
                PlaySound(clip);
        }

        public static void Flip(Transform rig)
        {
            Vector3 scale = rig.localScale;
            rig.localScale = new Vector3(-scale.x, scale.y, scale.z);
        }
        
        void PlaySound(AudioClip clip)
        {
            _audioSource.PlayOneShot(clip);
        }

        public void Disable() => _animator.enabled = false;
        public void Enable() => _animator.enabled = true;
    }
}
