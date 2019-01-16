// Copyright © 2018, Meta Company.  All rights reserved.
// 
// Redistribution and use of this software (the "Software") in binary form, without modification, is 
// permitted provided that the following conditions are met:
// 
// 1.      Redistributions of the unmodified Software in binary form must reproduce the above 
//         copyright notice, this list of conditions and the following disclaimer in the 
//         documentation and/or other materials provided with the distribution.
// 2.      The name of Meta Company (“Meta”) may not be used to endorse or promote products derived 
//         from this Software without specific prior written permission from Meta.
// 3.      LIMITATION TO META PLATFORM: Use of the Software is limited to use on or in connection 
//         with Meta-branded devices or Meta-branded software development kits.  For example, a bona 
//         fide recipient of the Software may incorporate an unmodified binary version of the 
//         Software into an application limited to use on or in connection with a Meta-branded 
//         device, while he or she may not incorporate an unmodified binary version of the Software 
//         into an application designed or offered for use on a non-Meta-branded device.
// 
// For the sake of clarity, the Software may not be redistributed under any circumstances in source 
// code form, or in the form of modified binary code – and nothing in this License shall be construed 
// to permit such redistribution.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDER "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, 
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A 
// PARTICULAR PURPOSE ARE DISCLAIMED.  IN NO EVENT SHALL META COMPANY BE LIABLE FOR ANY DIRECT, 
// INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, 
// PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA OR PROFITS; OR BUSINESS 
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT 
// LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS 
// SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
using UnityEngine;

namespace Meta.Mouse
{
    /// <summary>
    /// Base functionality needed for MetaMouse in actual build
    /// </summary>
    internal class MetaMouse : MetaBehaviourInternal
    {
        [SerializeField]
        private BoolEvent _onMouseStart = new BoolEvent();

        [SerializeField]
        private BoolEvent _onMouseEnable = new BoolEvent();

        [SerializeField]
        private Animator _cursorAnimator;

        private const float ClampPadding = 2;
        private const float UserMoveTolerance = 0.0001f;
        private IMetaInputModule _metaInputModule;
        private IInputWrapper _inputWrapper;
        private IEventCamera _eventCamera;
        private IMetaMouseFeedback _metaMouseFeedback;
        private Vector3 _screenPositionDelta;
        private float _cursorDistance;
        private float _cursorDistanceDampVelocity;
        private bool _isShown = false;
        private bool _cursorEnabled = true;

        public BoolEvent OnMouseStart
        {
            get { return _onMouseStart; }
        }

        public BoolEvent OnMouseEnable
        {
            get { return _onMouseEnable; }
        }

        public Animator CursorAnimator
        {
            get { return _cursorAnimator; }
        }

        /// <summary>
        /// Distance of raycast which hit interactable component.
        /// </summary>
        public float RaycastDistance { get; set; }

        /// <summary>
        /// Did this raycast hit something interactable? Set from MetaInputModule
        /// </summary>
        public bool RaycastHit { get; set; }

        /// <summary>
        /// Did this raycast hit a Unity UI component
        /// </summary>
        public bool DidRaycastHitUiComponent { get; set; }

        public Vector3 ScreenPosition { get; private set; }

        /// <summary>
        /// Whether any button of the mouse is pressed or not.
        /// </summary>
        internal bool IsPressed
        {
            get { return _metaInputModule.IsPressed; }
        }

        /// <summary>
        /// Whether the Meta Mouse is currently being shown, and receives mouse input.
        /// </summary>
        internal bool IsShown
        {
            get
            {
                return _isShown;
            }
            private set
            {
                _isShown = value;
                UpdateCursor();
            }
        }

        internal bool ShowingFeedback
        {
            get { return _metaMouseFeedback != null ? _metaMouseFeedback.Animating : false; }
        }

        private void Awake()
        {
            metaContext.Add(this);
            _metaMouseFeedback = GetComponentInChildren<IMetaMouseFeedback>();
        }

        public void Initialize(IEventCamera eventCamera, IInputWrapper inputWrapper, IMetaInputModule metaInputModule)
        {
            _eventCamera = eventCamera;
            _inputWrapper = inputWrapper;
            _metaInputModule = metaInputModule;
            _isShown = metaInputModule.MouseConfig.EnableOnStart;
        }

        private void Start()
        {
            _cursorDistance = _metaInputModule.MouseConfig.FloatDistance;
            ScreenPosition = _eventCamera.EventCameraRef.WorldToScreenPoint(transform.position);
        }

        /// <summary>
        /// Process the calculation of the mouse position if it is necessary
        /// </summary>
        private void Update()
        {
            if (!_cursorEnabled)
            {
                return;
            }

            if (_inputWrapper.LockState == CursorLockMode.Locked)
            {
                //Only clamp when the mouse is moved.
                bool userMoved = ProcessAxisInput();
                MoveCursorScreenPosition();
                if (userMoved)
                {
                    ClampScreenCursorToCamera();
                }
                UpdateCursorWorldPosition();
            }
        }

        private void OnDestroy()
        {
            var context = metaContext;
            if (context == null)
                return;
            context.Remove(this);
        }

        /// <summary>
        /// Start the mouse configuration
        /// </summary>
        /// <param name="visible"></param>
        public void StartMouse(bool visible)
        {
            if (_metaMouseFeedback != null)
            {
                _metaMouseFeedback.HandleStartMouse(visible);
            }

            if (_onMouseStart != null)
            {
                _onMouseStart.Invoke(visible);
            }
        }

        /// <summary>
        /// Turn on the mouse
        /// </summary>
        public void Show()
        {
            EnableCursor(true);
            SetMouseInputEnabled(true);
            IsShown = true;
            MoveMouseToCenter();

            if (_metaMouseFeedback != null)
            {
                _metaMouseFeedback.HandleShowMouse(true);
            }
            if (_onMouseEnable != null)
            {
                _onMouseEnable.Invoke(true);
            }
        }

        /// <summary>
        /// Turn off the mouse
        /// </summary>
        public void Hide()
        {
            EnableCursor(true);
            SetMouseInputEnabled(false);
            IsShown = false;
            MoveMouseToCenter();

            if (_metaMouseFeedback != null)
            {
                _metaMouseFeedback.HandleShowMouse(false);
            }
            if (_onMouseEnable != null)
            {
                _onMouseEnable.Invoke(false);
            }
        }
        
        /// <summary>
        /// Updates the cursor state based on value of <see cref="IsOn"/>.
        /// </summary>
        public void UpdateCursor()
        {
            Cursor.lockState = _isShown ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !_isShown;
        }

        /// <summary>
        /// Enables or disables the meta mouse cursor
        /// </summary>
        /// <param name="enable"></param>
        internal void EnableCursor(bool enable)
        {
            if (_metaMouseFeedback != null && !enable)
            {
                _metaMouseFeedback.EnableMetaMouseFeedback(enable);
            }
            CursorAnimator.gameObject.SetActive(enable);
            _cursorEnabled = enable;
        }

        /// <summary>
        /// Highlights the meta mouse cursor
        /// </summary>
        internal void Highlight()
        {
            MoveMouseToCenter();
            if (IsShown)
            {
                EnableCursor(true);
                SetMouseInputEnabled(true);
            }

            if (_metaMouseFeedback != null)
            {
                _metaMouseFeedback.Highlight(IsShown);
            }
        }

        /// <summary>
        /// Update cursor movement delta.
        /// </summary>
        /// <returns>Did movement occur?</returns>
        private bool ProcessAxisInput()
        {
            _screenPositionDelta = new Vector3(
                _inputWrapper.GetAxis("Mouse X") * _metaInputModule.MouseConfig.Sensitivity,
                _inputWrapper.GetAxis("Mouse Y") * _metaInputModule.MouseConfig.Sensitivity, 0f);
            return _screenPositionDelta.sqrMagnitude > UserMoveTolerance;
        }

        private void MoveCursorScreenPosition()
        {
            Vector3 projectedPosition = _eventCamera.EventCameraRef.WorldToScreenPoint(transform.position);
            ScreenPosition = projectedPosition + _screenPositionDelta;
        }

        /// <summary>
        /// Clamps cursor to constraints of screen size.
        /// </summary>
        /// <returns>Did a clamp occur?</returns>
        private bool ClampScreenCursorToCamera()
        {
            float xMin = ClampPadding;
            float xMax = _inputWrapper.GetScreenRect().width - ClampPadding;
            float yMin = ClampPadding;
            float yMax = _inputWrapper.GetScreenRect().height - ClampPadding;

            bool isClamped = ScreenPosition.x > xMax || ScreenPosition.x < xMin ||
                ScreenPosition.y > yMax || ScreenPosition.y < yMin;

            ScreenPosition = new Vector3(Mathf.Clamp(ScreenPosition.x, xMin, xMax), Mathf.Clamp(ScreenPosition.y, yMin, yMax), 0f);

            return isClamped;
        }

        private void UpdateCursorWorldPosition()
        {
            float targetDistance = _metaInputModule.MouseConfig.FloatDistance;
            Ray inputRay = _eventCamera.EventCameraRef.ScreenPointToRay(ScreenPosition);
            float physicsColliderRayBeginOffset = 0f;
#if UNITY_2017_1_OR_NEWER
            //For Unity 2017.1+, the calculation for the distance for a raycast hit changed.
            //The camera position is now used for the origin, while the ray generated by
            //ScreenPointToRay uses the near clipping plane as the origin. This should move
            //the ray back.
            inputRay.origin = _eventCamera.Position;

            if (!DidRaycastHitUiComponent)
            {
                physicsColliderRayBeginOffset = _eventCamera.EventCameraRef.nearClipPlane;
            }
            
#endif
            if (RaycastHit)
            {
                targetDistance = RaycastDistance + physicsColliderRayBeginOffset;
            }

            _cursorDistance = Mathf.SmoothDamp(_cursorDistance, targetDistance, ref _cursorDistanceDampVelocity, _metaInputModule.MouseConfig.DistanceDamp);

            transform.position = inputRay.GetPoint(_cursorDistance);
            transform.rotation = _eventCamera.EventCameraRef.transform.rotation;
        }

        /// <summary>
        /// Move the mouse to the center of the screen.
        /// </summary>
        private void MoveMouseToCenter()
        {
            Rect screenRect = _inputWrapper.GetScreenRect();
            ScreenPosition = new Vector3(screenRect.center.x, screenRect.center.y, 0f);
            UpdateCursorWorldPosition();
        }

        /// <summary>
        /// Sets whether the Meta Mouse should receive mouse input.
        /// </summary>
        /// <param name="enabled">Whether to receive mouse input.</param>
        private void SetMouseInputEnabled(bool enabled)
        {
            Cursor.lockState = enabled ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !enabled;
        }
    }
}
