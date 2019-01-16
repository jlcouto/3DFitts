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


namespace Meta.HandInput.SwipeInteraction
{
    /// <summary>
    /// Interaction to rotate a GameObject along a single axis by swiping a hand through a volume
    /// </summary>
    public class SwipeOneAxisRotationInteraction : MonoBehaviour
    {
        [Tooltip("The Transform of the GameObject that will be rotated by swipe interactions")]
        [SerializeField]
        private Transform _targetTransform;

        [Tooltip("Multiplier between linear and rotational motion")]
        [SerializeField]
        private float _rotationAmplifier = 240;

        [Tooltip("Component to track hand position along a single axis")]
        [SerializeField]
        private LinearSwipeMonitor _monitor;

        [Tooltip("Axis of rotation")]
        [SerializeField]
        private Vector3 _rotationAxis = Vector3.up;

        /// <summary>
        /// The Transform of the GameObject that will be rotated by swipe interactions
        /// </summary>
        public Transform TargetTransform
        {
            get
            {
                if (_targetTransform == null)
                {
                    return transform;
                }
                return _targetTransform;
            }
            set { _targetTransform = value; }
        }

        /// <summary>
        /// Is a swipe occuring 
        /// </summary>
        public bool SwipeOn
        {
            get { return _monitor.Engaged; }
        }

        private void Update()
        {
            if (_monitor.Engaged)
            {
                _targetTransform.Rotate(_rotationAxis, - _monitor.DeltaX * _rotationAmplifier, Space.World);
            }
        }
    }
}
