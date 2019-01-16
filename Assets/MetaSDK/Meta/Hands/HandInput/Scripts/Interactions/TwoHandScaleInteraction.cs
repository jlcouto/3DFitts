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
using Meta.HandInput;
using UnityEngine;

namespace Meta
{
    /// <summary>
    /// Interaction to scale model by placing two hands into the model and moving them together/apart.
    /// </summary>
    [AddComponentMenu("Meta/Interaction/Two Hand Scale Interaction")]
    public class TwoHandScaleInteraction : Interaction
    {
        private const float LimitResizeDamp = .1f;

        /// <summary>
        /// Minimum scale
        /// </summary>
        [SerializeField]
        private float _minSize = .2f;

        /// <summary>
        /// Maximum scale
        /// </summary>
        [SerializeField]
        private float _maxSize = 2f;

        [Tooltip("Make objects scale exponentially using a gamma scale power rather than linearly")]
        [SerializeField]
        private bool _exponentialScaling = false;

        private float _priorDistance;
        private Vector3 _limitResizeVelocity;
        private const float _gammaScalePower = 1.4f;

        /// <summary>
        /// Maximum scale
        /// </summary>
        public float MaxSize
        {
            get { return _maxSize; }
        }

        /// <summary>
        /// Minimum scale
        /// </summary>
        public float MinSize
        {
            get { return _minSize; }
        }

        private HandFeature FirstHand
        {
            get { return HoveringHands[0].Hand.Palm; }
        }

        private HandFeature SecondHand
        {
            get { return HoveringHands[1].Hand.Palm; }
        }

        protected override void Update()
        {
            base.Update();

            //Resize if past limits
            if (TargetTransform.localScale.x > _maxSize)
            {
                TargetTransform.localScale = new Vector3(_maxSize, _maxSize, _maxSize);
            }
            if (State != InteractionState.On && TargetTransform.localScale.x < _minSize)
            {
                TargetTransform.localScale = Vector3.SmoothDamp(TargetTransform.localScale,
                                                                new Vector3(_minSize, _minSize, _minSize), ref _limitResizeVelocity, LimitResizeDamp);
            }
        }

        protected override bool CanEngage(Hand hand)
        {
            //Check if two hands are in the collider for the object.
            return HoveringHands.Count == 2;
        }

        protected override void Engage()
        {
            _priorDistance = Vector3.Distance(FirstHand.transform.position,
                                              SecondHand.transform.position);
        }

        protected override bool CanDisengage(Hand hand)
        {
            return HoveringHands.Count > 1 && HoveringHands.Contains(hand.Palm);
        }

        protected override void Disengage()
        { }

        protected override void Manipulate()
        {
            float currentDistance = Vector3.Distance(FirstHand.transform.position,
                                                     SecondHand.transform.position);
            float multiplier = currentDistance / _priorDistance;
            if (multiplier < 1.5f && multiplier > .5f)
            {
                if(_exponentialScaling)
                {
                    TargetTransform.localScale *= Mathf.Pow(multiplier, _gammaScalePower);
                }
                else
                {
                    TargetTransform.localScale *= multiplier;
                }
                
            }
            _priorDistance = currentDistance;
        }
    }
}
