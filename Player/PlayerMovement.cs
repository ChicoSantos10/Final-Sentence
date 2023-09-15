using System;
using System.Collections;
using Scriptable_Objects;
using Scriptable_Objects.Event_Channels;
using UnityEngine;

namespace Player
{
    [RequireComponent(typeof(AnimationController))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerMovement : MonoBehaviour
    {
        // Start is called before the first frame update

        [SerializeField] InputReader inputReader;
        [SerializeField] PlayerInfo stats;

        [SerializeField] float speed = 8f;
        [SerializeField] float sprintBonus = 2f;
        bool _sprinting;

        [Header("Stats")]
        [SerializeField] float sprintStaminaCost = 1;
        [SerializeField] float staminaTick = 0.2f;
        [SerializeField] float hungerPerSecond = 0.2f;
        [SerializeField] float staminaRegenSecond = 0.1f;

        [SerializeField] Transform rig;
        AnimationController _animator;

        Rigidbody2D _rb;
        Vector2 _dir;
        
        void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _animator = GetComponent<AnimationController>();
            
            stats.Player = transform;
            
            stats.Reset();
        }

        void Update()
        {
            stats.Stats[VariableStat.StatType.Hunger].Reduce(hungerPerSecond * Time.deltaTime);

            if (!_sprinting || _dir == Vector2.zero)
                stats.Stats[VariableStat.StatType.Stamina].Recover(staminaRegenSecond * Time.deltaTime);
            
            _animator.SetSpeed(_dir.magnitude);
        }

        void FixedUpdate()
        {     
            _rb.MovePosition(_rb.position + _dir.normalized * speed * Time.fixedDeltaTime);
        }

        void OnEnable()
        {
            inputReader.OnMovementAction += PlayerMovement_OnMovementAction;
            inputReader.OnSprintStarted += PlayerMovement_OnSprintStarted;
            inputReader.OnSprintCanceled += PlayerMovement_OnSprintCanceled;
        }

        void OnDisable()
        {
            inputReader.OnMovementAction -= PlayerMovement_OnMovementAction;
            inputReader.OnSprintStarted -= PlayerMovement_OnSprintStarted;
            inputReader.OnSprintCanceled -= PlayerMovement_OnSprintCanceled;
        }

        void PlayerMovement_OnMovementAction(Vector2 dir)
        {
            if (dir.x > 0 && rig.localScale.x > 0)
                AnimationController.Flip(rig);
            else if (dir.x < 0 && rig.localScale.x < 0)
                AnimationController.Flip(rig);
            
            _dir = dir;
        }

        void PlayerMovement_OnSprintStarted()
        {
            _sprinting = true;
            StartCoroutine(StartSprinting());
        }

        void PlayerMovement_OnSprintCanceled()
        {
            _sprinting = false;
        }

        IEnumerator StartSprinting()
        {
            speed += sprintBonus;
            
            while (stats.Stats[VariableStat.StatType.Stamina] > sprintStaminaCost && _sprinting)
            {
                if (_dir != Vector2.zero)
                {
                    stats.Stats[VariableStat.StatType.Stamina].Reduce(sprintStaminaCost);
                }
                
                yield return new WaitForSeconds(staminaTick);
            }
            
            speed -= sprintBonus;
        }

    }
}
