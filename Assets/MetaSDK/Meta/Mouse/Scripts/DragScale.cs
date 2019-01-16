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
using Meta.Extensions;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Meta.Mouse
{
    /// <summary>
    /// Handles scaling of an object using the mouse scrollwheel.
    /// </summary>
    public class DragScale : MonoBehaviour, IPointerAction, IDragHandler, IPointerDownHandler, IPointerUpHandler
    {
        /// <summary>
        /// Transform to scale
        /// </summary>
        [SerializeField]
        private Transform _scaleTransform;

        [SerializeField]
        private float _scaleMultiplier = .05f;

        [SerializeField]
        private float _maxScale = 5;

        [SerializeField]
        private float _minScale = .2f;

        /// <summary>
        /// Input Button that activates the interaction
        /// </summary>
        [SerializeField]
        private PointerEventData.InputButton _button = PointerEventData.InputButton.Middle;
        /// <summary>
        /// Input Button that activates the interaction when a modifier key is used
        /// </summary>
        [SerializeField]
        private PointerEventData.InputButton _altButton = PointerEventData.InputButton.Left;
        /// <summary>
        /// Keys that activate the interaction
        /// </summary>
        [SerializeField]
        private KeySet _keyCodes;

        private readonly MetaInteractionDataEvent _pointerDown = new MetaInteractionDataEvent();
        private readonly MetaInteractionDataEvent _pointerUp = new MetaInteractionDataEvent();

        ///<inheritdoc />
        public MetaInteractionDataEvent PointerDown
        {
            get { return _pointerDown; }
        }

        ///<inheritdoc />
        public MetaInteractionDataEvent PointerUp
        {
            get { return _pointerUp; }
        }

        ///<inheritdoc />
        public Transform TargetTransform
        {
            get { return _scaleTransform; }
            set { _scaleTransform = value; }
        }

        /// <summary>
        /// The maximum size an object can be scaled, considering all its dimensions
        /// </summary>
        public float MaximumScale
        {
            get { return _maxScale; }
            set { _maxScale = value; }
        }

        /// <summary>
        /// The minimum size an object can be scaled, considering all its dimensions
        /// </summary>
        public float MinimumScale
        {
            get { return _minScale; }
            set { _minScale = value; }
        }

        private void Awake()
        {
            if (_scaleTransform == null)
            {
                _scaleTransform = transform;
            }

            if (_keyCodes == null)
            {
                Debug.LogWarningFormat("No KeySet specified on {0}! This is fine if you don't want the Alt Button to require a modifier key.", name);
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (eventData.button == _button || (eventData.button == _altButton && (_keyCodes == null || _keyCodes.IsPressed())))
            {
                float scaleValue = eventData.delta.y * _scaleMultiplier;
                Vector3 newScale = _scaleTransform.localScale.Add(scaleValue);
                if (newScale.LargestComponent() < _maxScale && newScale.SmallestComponent() > _minScale)
                {
                    _scaleTransform.localScale = newScale;
                }
            }
        }

        /// <summary>
        /// Pointer button is down.
        /// </summary>
        /// <inheritdoc />
        public void OnPointerDown(PointerEventData eventData)
        {
            if (_pointerDown != null)
            {
                _pointerDown.Invoke(new MetaInteractionData(eventData, null));
            }
        }

        /// <summary>
        /// Pointer button is up.
        /// </summary>
        /// <inheritdoc />
        public void OnPointerUp(PointerEventData eventData)
        {
            if (_pointerUp != null)
            {
                _pointerUp.Invoke(new MetaInteractionData(eventData, null));
            }
        }
    }
}
