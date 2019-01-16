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
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Meta.Mouse
{
    /// <summary>
    /// Extends the built-in StandAloneInputModule to process events for
    /// MetaMouse and offer ability to play and record through MetaMouseMock.
    /// </summary>
    [RequireComponent(typeof(EventSystem))]
    internal class MetaInputModule : StandaloneInputModule, IMetaInputModule
    {
        [SerializeField]
        private MetaMouseConfig _metaMouseConfig = new MetaMouseConfig();

        private const float StateTransitionTime = .05f;
        private readonly MouseState _mouseStateRecyclable = new MouseState();
        private IInputWrapper _inputWrapper;
        private MetaMouse _metaMouse;
        private int _priorLockKeyState;
        private IMetaContextInternal _metaContext;

        public MetaMouseConfig MouseConfig
        {
            get { return _metaMouseConfig; }
        }

        /// <summary>
        /// Whether any input button is pressed or not.
        /// </summary>
        public bool IsPressed { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            _metaContext = FindObjectOfType<MetaContextBridge>().CurrentContextInternal;

            if (!_metaContext.ContainsModule<IMetaInputModule>())
            {
                _metaContext.Add<IMetaInputModule>(this);
            }

            if (!_metaContext.ContainsModule<IKeyboardWrapper>())
            {
                _metaContext.Add<IKeyboardWrapper>(new UnityKeyboardWrapper());
            }
        }

        protected override void Start()
        {
            base.Start();

            _inputWrapper = _metaContext.Get<IInputWrapper>();
            _inputWrapper.LockState = _metaMouseConfig.EnableOnStart ? CursorLockMode.Locked : CursorLockMode.None;

            GameObject mousePrefab = (GameObject)Resources.Load("MetaMouse");
            GameObject mouseInstance = Instantiate(mousePrefab);
            _metaMouse = mouseInstance.GetComponent<MetaMouse>();
            _metaMouse.Initialize(_metaContext.Get<IEventCamera>(), _inputWrapper, this);
            _metaMouse.StartMouse(_metaMouseConfig.EnableOnStart);
        }

        private void Update()
        {
            if (GetLockKeyUp())
            {
                ToggleCursorLock();
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (_metaContext != null)
            {
                _metaContext.Remove<IMetaInputModule>();
                _metaContext.Remove<IKeyboardWrapper>();
            }
        }

        /// <summary>
        /// Called when the window gains or loses input focus.
        /// </summary>
        /// <param name="focus">Indicates whether the window gained focus.</param>
        public void OnApplicationFocus(bool focus)
        {
            if (_metaMouse != null)
            {
                if (_metaMouse.IsShown)
                {
                    _metaMouse.UpdateCursor();
                }
            }
        }

        /// <summary>
        /// Returns the current MouseState.
        /// </summary>
        /// <remarks>
        /// Copied from PointerInputModule and altered to use _inputWrapper
        /// and forward information to _metaMouse. If issues ever arise in 
        /// this method, check for a new version of this method on Unity's 
        /// UI bitbucket repo. This is called from inside the inherited 
        /// PointerInputModule.
        /// </remarks>
        protected override MouseState GetMousePointerEventData(int id)
        {
            // Populate the left button...
            PointerEventData leftData;
            var created = GetPointerData(kMouseLeftId, out leftData, true);

            leftData.Reset();

            if (created)
            {
                leftData.position = _metaMouse.ScreenPosition;
            }

            Vector2 pos = _metaMouse.ScreenPosition;
            leftData.delta = pos - leftData.position;
            leftData.position = pos;
            leftData.scrollDelta = _inputWrapper.GetMouseScrollDelta();
            leftData.button = PointerEventData.InputButton.Left;
            eventSystem.RaycastAll(leftData, m_RaycastResultCache);

            RaycastResult raycast = FindFirstRaycastResult(m_RaycastResultCache, RaycastComparer);

            TestForMouseAttractor(leftData, ref raycast);

            leftData.pointerCurrentRaycast = raycast;

            // copy the apropriate data into right and middle slots
            PointerEventData rightData;
            GetPointerData(kMouseRightId, out rightData, true);
            CopyFromTo(leftData, rightData);
            rightData.button = PointerEventData.InputButton.Right;

            PointerEventData middleData;
            GetPointerData(kMouseMiddleId, out middleData, true);
            CopyFromTo(leftData, middleData);
            middleData.button = PointerEventData.InputButton.Middle;

            UpdateCursorStates(leftData, rightData, middleData, raycast);

            _metaMouse.RaycastHit = raycast.isValid;
            //The raycast result of an EventSystem Raycast will be missing its world position if the ray
            // collided with a GUI element.
            _metaMouse.DidRaycastHitUiComponent = raycast.worldPosition.Equals(Vector3.zero);
            _metaMouse.RaycastDistance = raycast.distance;

            m_RaycastResultCache.Clear();

            _mouseStateRecyclable.SetButtonState(PointerEventData.InputButton.Left, StateForMouseButton(0), leftData);
            _mouseStateRecyclable.SetButtonState(PointerEventData.InputButton.Right, StateForMouseButton(1), rightData);
            _mouseStateRecyclable.SetButtonState(PointerEventData.InputButton.Middle, StateForMouseButton(2), middleData);

            return _mouseStateRecyclable;
        }

        /// <summary>
        /// Given a mouse button return the current state for the frame.
        /// </summary>
        /// <remarks>
        /// Copied from Unity UI's PointerInputModule.
        /// </remarks>
        protected new PointerEventData.FramePressState StateForMouseButton(int buttonId)
        {
            bool pressed = _inputWrapper.GetMouseButtonDown(buttonId);
            bool released = _inputWrapper.GetMouseButtonUp(buttonId);
            PointerEventData.FramePressState state = PointerEventData.FramePressState.NotChanged;

            if (pressed && released)
            {
                state = PointerEventData.FramePressState.PressedAndReleased;
            }
            else if (pressed)
            {
                state = PointerEventData.FramePressState.Pressed;
            }
            else if (released)
            {
                state = PointerEventData.FramePressState.Released;
            }

            return state;
        }

        /// <summary>
        /// Process a drag for the given frame.
        /// </summary>
        /// <remarks>
        /// Copied from Unity UI's PointerInputModule.
        /// Removed a conditional check for cursor locked from first if clause as MetaMouse locks the cursor.
        /// If issues, check for updates to this in Unity's UI repo.        
        /// </remarks>
        protected override void ProcessMove(PointerEventData pointerEvent)
        {
            var targetGO = pointerEvent.pointerCurrentRaycast.gameObject;
            HandlePointerExitAndEnter(pointerEvent, targetGO);
        }

        /// <summary>
        /// Process a drag for the given frame.
        /// </summary>
        /// <remarks>
        /// Copied from Unity UI's PointerInputModule.
        /// Removed a conditional check for cursor locked from first if clause as MetaMouse locks the cursor.
        /// If issues, check for updates to this in Unity's UI repo.        
        /// </remarks>
        protected override void ProcessDrag(PointerEventData pointerEvent)
        {
            if (!pointerEvent.IsPointerMoving() ||
                pointerEvent.pointerDrag == null)
            {
                return;
            }
            if (!pointerEvent.dragging
                && ShouldStartDrag(pointerEvent.pressPosition, pointerEvent.position, eventSystem.pixelDragThreshold, pointerEvent.useDragThreshold))
            {
                ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.beginDragHandler);
                pointerEvent.dragging = true;
            }
            // Drag notification
            if (pointerEvent.dragging)
            {
                // Before doing drag we should cancel any pointer down state
                // And clear selection!
                if (pointerEvent.pointerPress != pointerEvent.pointerDrag)
                {
                    ExecuteEvents.Execute(pointerEvent.pointerPress, pointerEvent, ExecuteEvents.pointerUpHandler);

                    pointerEvent.eligibleForClick = false;
                    pointerEvent.pointerPress = null;
                    pointerEvent.rawPointerPress = null;
                }
                ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.dragHandler);
            }
        }

        /// <summary>
        /// Apply a sort to raycast results and then get the first result
        /// </summary>
        protected RaycastResult FindFirstRaycastResult(List<RaycastResult> results, Comparison<RaycastResult> comparer)
        {
            results.Sort(comparer);
            return FindFirstRaycast(results);
        }

        /// <summary>
        /// Compare two RaycastResults
        /// </summary>
        /// /// <remarks>
        /// Copied from Unity UI's EventSystem..
        /// If issues, check for updates to this in Unity's UI repo.        
        /// </remarks>
        private static int RaycastComparer(RaycastResult lhs, RaycastResult rhs)
        {
            if (lhs.module != rhs.module)
            {
                if (lhs.module.eventCamera != null && rhs.module.eventCamera != null && lhs.module.eventCamera.depth != rhs.module.eventCamera.depth)
                {
                    // need to reverse the standard compareTo
                    if (lhs.module.eventCamera.depth < rhs.module.eventCamera.depth)
                        return 1;
                    if (lhs.module.eventCamera.depth == rhs.module.eventCamera.depth)
                        return 0;

                    return -1;
                }

                if (lhs.module.sortOrderPriority != rhs.module.sortOrderPriority)
                    return rhs.module.sortOrderPriority.CompareTo(lhs.module.sortOrderPriority);

                if (lhs.module.renderOrderPriority != rhs.module.renderOrderPriority)
                    return rhs.module.renderOrderPriority.CompareTo(lhs.module.renderOrderPriority);
            }

            if (lhs.sortingLayer != rhs.sortingLayer)
            {
                // Uses the layer value to properly compare the relative order of the layers.
                var rid = SortingLayer.GetLayerValueFromID(rhs.sortingLayer);
                var lid = SortingLayer.GetLayerValueFromID(lhs.sortingLayer);
                return rid.CompareTo(lid);
            }

            if (lhs.sortingOrder != rhs.sortingOrder)
                return rhs.sortingOrder.CompareTo(lhs.sortingOrder);

            //Only compare depth if the two have the same raycaster
            //This is the only modification from EventSystem
            if (lhs.module == rhs.module && lhs.depth != rhs.depth)
                return rhs.depth.CompareTo(lhs.depth);

            if (lhs.distance != rhs.distance)
            {
#if UNITY_2017_1_OR_NEWER
                //For Unity 2017.1+, the calculation for the distance for a raycast hit changed.
                //The camera position is now used for the origin for Physics raycasts, while for 
                //GUI raycasts, the ray uses the near clipping plane as the origin.

                //In Unity, a raycast hit is known to be from a Graphics raycast 
                //if it is missing its world position.
                if (lhs.worldPosition.Equals(Vector3.zero))
                {
                    lhs.distance -= lhs.module.eventCamera.nearClipPlane;
                }

                if (rhs.worldPosition.Equals(Vector3.zero))
                {
                    rhs.distance -= rhs.module.eventCamera.nearClipPlane;
                }
#endif
                return lhs.distance.CompareTo(rhs.distance);
            }
                

            return lhs.index.CompareTo(rhs.index);
        }

        private void TestForMouseAttractor(PointerEventData leftData, ref RaycastResult raycast)
        {
            //try to find mouseattractor if dragging
            if (leftData.dragging)
            {
                for (int i = 0; i < m_RaycastResultCache.Count; ++i)
                {
                    if (m_RaycastResultCache[i].gameObject.GetComponent<MouseAttractor>() != null)
                    {
                        raycast = m_RaycastResultCache[i];
                        return;
                    }
                }
            }
        }

        private void UpdateCursorStates(PointerEventData leftData, PointerEventData rightData, PointerEventData middleData, RaycastResult raycast)
        {
            IsPressed = leftData.pointerPress || rightData.pointerPress || middleData.pointerPress;

            //Cursor States
            if (leftData.pointerPress != null)
            {
                string stateName = "";

                //get state from cursorState
                MetaMouseCursorState cursorState = leftData.pointerPress.GetComponent<MetaMouseCursorState>();
                if (cursorState != null)
                {
                    stateName = cursorState.EngagedKeyState();
                }

                //is no state from cursorState use default
                if (string.IsNullOrEmpty(stateName))
                {
                    stateName = "Click";
                }
                _metaMouse.CursorAnimator.CrossFadeToStateIfNotCurrent(stateName, StateTransitionTime);
            }
            else
            {
                string stateName = "";

                //get state from cursorState
                if (leftData.pointerEnter != null)
                {
                    MetaMouseCursorState cursorState = leftData.pointerEnter.GetComponent<MetaMouseCursorState>();
                    if (cursorState != null)
                    {
                        stateName = cursorState.EngagedKeyState();
                    }
                }

                //is no state from cursorState use default
                if (string.IsNullOrEmpty(stateName))
                {
                    stateName = raycast.isValid ? "Hover" : "Idle";
                }
                _metaMouse.CursorAnimator.CrossFadeToStateIfNotCurrent(stateName, StateTransitionTime);
            }
        }

        /// <summary>
        /// Checks whether a cursor drag event should be started.
        /// </summary>
        /// <remarks>
        /// Copied from Unity UI's PointerInputModule.
        /// </remarks>
        private static bool ShouldStartDrag(Vector2 pressPos, Vector2 currentPos, float threshold, bool useDragThreshold)
        {
            if (!useDragThreshold)
                return true;
            return (pressPos - currentPos).sqrMagnitude >= threshold * threshold;
        }

        private bool GetLockKeyUp()
        {
            bool up = false;
            int state = Interop.User32Interop.GetKeyState(VirtualKeyCodes.VK_F8);
            if (state < -1 && _priorLockKeyState > -1)
            {
                //down
            }
            else if (state > -1 && _priorLockKeyState < -1)
            {
                //up
                up = true;
            }
            _priorLockKeyState = state;
            return up;
        }

        private void ToggleCursorLock()
        {
            if (_metaMouse.IsShown)
            {
                HideMouse();
            }
            else
            {
                ShowMouse();
            }
        }

        private void ShowMouse()
        {
            _inputWrapper.LockState = CursorLockMode.Locked;
            _metaMouse.Show();
        }

        private void HideMouse()
        {
            _inputWrapper.LockState = CursorLockMode.None;
            _metaMouse.Hide();
        }
    }
}
