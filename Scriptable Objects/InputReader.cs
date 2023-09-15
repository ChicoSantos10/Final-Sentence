using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Users;

namespace Scriptable_Objects
{
    [CreateAssetMenu(menuName = nameof(InputReader))]
    public class InputReader : ScriptableObject, GameInput.IGameplayActions, GameInput.IMenuActions
    {
        GameInput _input;
        InputUser _user;

        public InputControlScheme? ControlScheme => _user.controlScheme;

        // Gameplay
        public event UnityAction<Vector2> OnMovementAction = delegate { };        
        public event UnityAction<Vector2> OnMovementStartedAction = delegate { };
        public event UnityAction OnAttackAction = delegate { };
        public event UnityAction OnSprintStarted = delegate { };
        public event UnityAction OnSprintCanceled = delegate { };
        public event UnityAction OnOpenCraftMenuAction = delegate { };
        public event UnityAction OnInteractActionStarted = delegate { };
        public event UnityAction OnInteractActionCompleted = delegate { };
        public event UnityAction OnInteractActionCanceled = delegate { };
        public event UnityAction OnOpenInventoryAction = delegate { };
        public event UnityAction<int> OnConsumeAction = delegate { };
        public event UnityAction OnPauseAction = delegate { };
        
        public static float InteractionDuration => InputSystem.settings.defaultHoldTime;

        // Menu
        public event UnityAction OnPreviousAction = delegate { };
        public event UnityAction<Vector2> OnNavigationAction = delegate {  };
        public event UnityAction OnSubmitAction = delegate { };
        public event UnityAction OnAddToHandAction = delegate { };

        List<InputAction> _gameplayActions; // Actions to enable/disable on gameplay enable/disable

        void OnEnable()
        {
            if (_input == null)
            {
                _input = new GameInput();
                _input.Gameplay.SetCallbacks(this);
                _input.Menu.SetCallbacks(this);

                _gameplayActions = new List<InputAction>()
                {
                    _input.Gameplay.Movement,
                    _input.Gameplay.Pause,
                    _input.Gameplay.Attack,
                    _input.Gameplay.Consumable1,
                    _input.Gameplay.Consumable2,
                    _input.Gameplay.Consumable3,
                    _input.Gameplay.Interact
                };
            }

            // _user = InputUser.CreateUserWithoutPairedDevices();
            // InputUser.listenForUnpairedDeviceActivity = 1;
            //
            // InputControlList<InputDevice> availableDevices = InputUser.GetUnpairedInputDevices();
            //
            // Debug.Log(availableDevices);
            
            // if (InputControlScheme.FindControlSchemeForDevices(availableDevices, _input.controlSchemes,
            //         out InputControlScheme controlScheme, out InputControlScheme.MatchResult matchResult))
            // {
            //     
            //     matchResult.Dispose();
            // }

            // foreach (InputDevice availableDevice in availableDevices)
            // {
            //     _user = InputUser.PerformPairingWithDevice(availableDevice, _user);
            // }

            // foreach (InputDevice device in _user.pairedDevices)
            // {
            //     Debug.Log(device);
            // }
            //
            // _user.AssociateActionsWithUser(_input);
            // InputUser.onUnpairedDeviceUsed += InputUserOnonUnpairedDeviceUsed;
            // InputUser.onChange += InputUserOnChange;

            //InputSystem.onActionChange += InputSystemOnActionChange;
            
            EnableAllInput();
        }

        void InputSystemOnActionChange(object obj, InputActionChange change)
        {
            if (change == InputActionChange.ActionPerformed)
            {
                InputControl lastDevice = ((InputAction)obj).activeControl;
                
                if (InputControlScheme.FindControlSchemeForDevices(new InputControlList<InputDevice>{lastDevice.device}, _input.controlSchemes,
                        out InputControlScheme controlScheme, out InputControlScheme.MatchResult matchResult))
                {
                    
                    matchResult.Dispose();
                }
            }
        }

        void InputUserOnonUnpairedDeviceUsed(InputControl arg1, InputEventPtr arg2)
        {
            Debug.Log("Used a unpaired device");
        }

        void InputUserOnChange(InputUser user, InputUserChange changeType, InputDevice device)
        {
            Debug.Log("Changed");
            
            if (changeType == InputUserChange.ControlSchemeChanged)
                Debug.Log($"New Scheme = {user.controlScheme}");
        }

        void OnDisable()
        {
            InputUser.onChange -= InputUserOnChange;
            InputUser.onUnpairedDeviceUsed -= InputUserOnonUnpairedDeviceUsed;
        
            DisableAllInput();
        }

        public void OnMovement(InputAction.CallbackContext context)
        {
            OnMovementAction.Invoke(context.ReadValue<Vector2>());
            
            if (context.phase == InputActionPhase.Performed)
                OnMovementStartedAction.Invoke(context.ReadValue<Vector2>());
        }

        public void OnAttack(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Performed)
            {
                OnAttackAction.Invoke();
            }
        }

        public void OnSprint(InputAction.CallbackContext context)
        {
            if(context.phase == InputActionPhase.Performed)
                OnSprintStarted.Invoke();
            if(context.phase == InputActionPhase.Canceled)
                OnSprintCanceled.Invoke();
        }

        public void OnOpenCraftMenu(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Performed)
            {
                OnOpenCraftMenuAction.Invoke();
            }
        }
    
        public void OnInteract(InputAction.CallbackContext context)
        {
            switch (context.phase)
            {
                case InputActionPhase.Disabled:
                    break;
                case InputActionPhase.Waiting:
                    break;
                case InputActionPhase.Started:
                    OnInteractActionStarted();
                    break;
                case InputActionPhase.Performed:
                    OnInteractActionCompleted();
                    break;
                case InputActionPhase.Canceled:
                    OnInteractActionCanceled();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }


        public void OnOpenInventory(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Performed)
                OnOpenInventoryAction.Invoke();
        }

        public void OnConsumable1(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Performed)
                OnConsumeAction.Invoke(0);
        }

        public void OnConsumable2(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Performed)
                OnConsumeAction.Invoke(1);
        }

        public void OnConsumable3(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Performed)
                OnConsumeAction.Invoke(2);
        }

        public void OnPause(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Performed)
                OnPauseAction.Invoke();
        }

        #region Menu

        public void OnPrevious(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Performed)
            {
                OnPreviousAction.Invoke();
            }
        }

        public void OnNavigate(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Performed)
            {
                OnNavigationAction(context.ReadValue<Vector2>());
            }
        }

        public void OnSubmit(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Performed)
            {
                OnSubmitAction.Invoke();
            }
        }

        public void OnAddToHand(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Performed)
                OnAddToHandAction.Invoke();
        }

        #endregion
    
        public void EnableGameplayInput()
        {
            DisableAllInput();
            _input.Gameplay.Enable();
        }

        public void EnableMenuInput()
        {
            DisableAllInput();
            _input.Menu.Enable();
        }
    
        public void EnableAllInput() => _input.Enable();

        public void DisableAllInput() => _input.Disable();


        public void DisableGameplay()
        {
            // _input.Gameplay.Movement.Disable();
            // _input.Gameplay.Pause.Disable();

            foreach (InputAction action in _gameplayActions)
            {
                action.Disable();
            }
        }

        public void EnableGameplay()
        {
            foreach (InputAction action in _gameplayActions)
            {
                action.Enable();
            }
        }
    }
}
