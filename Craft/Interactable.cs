using System;
using System.Collections;
using Scriptable_Objects;
using Scriptable_Objects.Items;
using UnityEngine;
using UnityEngine.UI;

namespace Craft
{
    [RequireComponent(typeof(Collider2D))]
    public abstract class Interactable : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField] protected InputReader input;

        [Header("UI")]
        [SerializeField] GameObject interactableUI;
        Image _fillImage;

        IEnumerator _fillImageRoutine;

        void Awake()
        {
            _fillImageRoutine = FillImage();
        }

        void OnDisable()
        {
            StopListenToInteractionEvents();
        }

        void OnInteractActionCanceled()
        {
            _fillImage.fillAmount = 0;
            
            StopCoroutine(_fillImageRoutine);
        }

        protected abstract void OnInteractActionCompleted();

        void OnInteractActionStarted()
        {
            _fillImageRoutine = FillImage();
            StartCoroutine(_fillImageRoutine);
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            // Show interactable UI
            GameObject go = Instantiate(interactableUI, transform);

            _fillImage = go.GetComponentInChildren<Image>();
            
            // Activate interactable button
            ListenToInteractionEvents();
        }

        void OnTriggerExit2D(Collider2D other)
        {
            Destroy(_fillImage.transform.parent.gameObject);
            
            StopListenToInteractionEvents();
        }

        IEnumerator FillImage()
        {
            const float frameTime = 0.05f;
            float numberFrames = InputReader.InteractionDuration / frameTime;
            float amount = 1 / numberFrames;
            
            while (_fillImage.fillAmount < 1)
            {
                _fillImage.fillAmount += amount; 
                yield return new WaitForSeconds(frameTime);
            }

            _fillImage.fillAmount = 0;
        }

        void ListenToInteractionEvents()
        {
            input.OnInteractActionStarted += OnInteractActionStarted;
            input.OnInteractActionCompleted += OnInteractActionCompleted;  
            input.OnInteractActionCanceled += OnInteractActionCanceled; 
        }
        
        void StopListenToInteractionEvents()
        {
            input.OnInteractActionStarted -= OnInteractActionStarted;
            input.OnInteractActionCompleted -= OnInteractActionCompleted;  
            input.OnInteractActionCanceled -= OnInteractActionCanceled; 
        }

        protected void EnableInteractionUI(bool enable)
        {
            _fillImage.gameObject.SetActive(enable);
            
            if (enable)
                ListenToInteractionEvents();
            else
                StopListenToInteractionEvents();
        }

        void OnValidate()
        {
            GetComponent<Collider2D>().isTrigger = true;
        }
    }
}
