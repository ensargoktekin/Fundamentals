using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Input
{
    public class PlayerController : MonoBehaviour
    {
        #region Events
        public static Action PointerDown;
        public static Action<Block> BlockPointed; //indicate which Block is pointed, type of argument can be changed for different games
        public static Action<Vector2, float> TouchStart;
        public static Action<Vector2, float> TouchEnd;
        #endregion

        #region Variables
        public Vector2 PointerPos
        {
            get => _inputActions.Player.PointerPosition.ReadValue<Vector2>();
        }
        public bool CanPlay
        {
            get
            {
                return _canPlay;
            }
            set
            {
                _canPlay = value;
            }
        }
        public string TargetLayer
        {
            get
            {
                return _targetLayer;
            }
            set
            {
                _targetLayer = value;
            }
        }
        private string _targetLayer = "targetLayerName";
        private InputActions _inputActions;
        private Camera _mainCam;
        private bool _checkPointer = false;
        private bool _canPlay;
        #endregion

        #region Initialization
        private void Start()
        {
            try
            {
                _mainCam = Camera.main;
                _canPlay = true;
                _inputActions = new InputActions();
                _inputActions.Player.Enable();
                _inputActions.Player.PointerDown.started += OnPointerDown;
                _inputActions.Player.PointerDown.canceled += OnPointerUp;
                _inputActions.Player.Contact.started += OnStartTouch;
                _inputActions.Player.Contact.canceled += OnEndTouch;
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
            }
        }

        private void OnDisable()
        {
            try
            {
                _inputActions.Player.PointerDown.started -= OnPointerDown;
                _inputActions.Player.PointerDown.canceled -= OnPointerUp;
                _inputActions.Player.Contact.started -= OnStartTouch;
                _inputActions.Player.Contact.canceled -= OnEndTouch;
                _inputActions.Player.Disable();
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
            }
        }

        private void Update()
        {
            if (!_checkPointer)
                return;

            if (!_canPlay)
                return;

            Vector2 pos = _inputActions.Player.PointerPosition.ReadValue<Vector2>();
            GetPointedObject(pos);
        }
        #endregion

        #region Methods
        private void OnPointerDown(InputAction.CallbackContext context)
        {
            _checkPointer = true;
        }

        private void OnPointerUp(InputAction.CallbackContext context)
        {
            _checkPointer = false;
            PointerDown?.Invoke();
        }

        private void OnStartTouch(InputAction.CallbackContext context)
        {
            Vector2 pos = _inputActions.Player.PointerPosition.ReadValue<Vector2>();
            TouchStart?.Invoke(pos, (float)context.startTime);
        }

        private void OnEndTouch(InputAction.CallbackContext context)
        {
            Vector2 pos = _inputActions.Player.PointerPosition.ReadValue<Vector2>();
            TouchEnd?.Invoke(pos, (float)context.time);
        }

        /// <summary>
        /// Check whether an object is pointed via screen touch. If touched, send corresponding event
        /// </summary>
        /// <param name="pointerPos">position to check whether there is an object</param>
        private void GetPointedObject(Vector2 pointerPos)
        {
            Ray ray = _mainCam.ScreenPointToRay(pointerPos);
            LayerMask mask = LayerMask.GetMask(_targetLayer);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, ray.direction.magnitude, mask);
            //Debug.DrawRay(ray.origin, ray.direction*100);
            if (hit)
            {
                //References.HintManager.OnInput(); uncomment if you are planning to add hint mechanism
                Block block = hit.transform.GetComponent<Block>();
                BlockPointed?.Invoke(block);
            }
        }
        #endregion
    }
}

