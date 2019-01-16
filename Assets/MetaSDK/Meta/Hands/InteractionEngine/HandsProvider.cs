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
using Meta.HandInput;
using UnityEngine;

namespace Meta
{
    /// <summary>
    /// This class holds all information regarding the hands (including variables, thresholds, statistics)
    /// as well as being the main application's entry point for hand references.
    /// </summary>
    public class HandsProvider : MetaBehaviour
    {
        [SerializeField]
        private Hand _rightHand = null, _leftHand = null;
        [SerializeField]
        private Settings _settings = new Settings();
        [SerializeField]
        private Events _events = new Events();

        private readonly List<Hand> _activeHands = new List<Hand>();

        /// <summary>
        /// Class containing all settings for the hand.
        /// </summary>
        public Settings settings
        {
            get { return _settings; }
        }

        /// <summary>
        /// Class containing events related to the hand.
        /// </summary>
        [SerializeField]
        public Events events
        {
            get { return _events; }
        }

        /// <summary>
        /// List of the current active hands.
        /// </summary>
        internal List<Hand> ActiveHands
        {
            get { return _activeHands; }
        }

        private void Start()
        {
            HandsModule handManager = FindObjectOfType<BaseMetaContextBridge>().CurrentContext.Get<HandsModule>();

            handManager.OnHandEnterFrame += OnHandDataAppear;
            handManager.OnHandExitFrame += OnHandDataDisappear;
        }

        private void OnHandDataAppear(HandData handData)
        {
            Hand hand = LookupHandForHandData(handData);

            hand.InitializeHandData(handData);
            hand.gameObject.SetActive(true);
            hand.transform.SetParent(transform);

            _activeHands.Add(hand);

            events.OnHandEnter.Invoke(hand);
        }

        private void OnHandDataDisappear(HandData handData)
        {
            Hand hand = LookupHandForHandData(handData);

            events.OnHandExit.Invoke(hand);

            hand.MarkInvalid();

            _activeHands.Remove(hand);

            hand.gameObject.SetActive(false);
        }

        private Hand LookupHandForHandData(HandData handData)
        {
            switch (handData.HandType)
            {
                case HandType.Right:
                    return _rightHand;
                case HandType.Left:
                    return _leftHand;
                default:
                    throw new Exception("Invalid hand type: " + handData.HandType);
            }
        }

        /// <summary>
        /// Class containing events related to the hand.
        /// </summary>
        [Serializable]
        public class Events
        {
            /// <summary> Event fired on the first frame the hand is visible. </summary>
            public OnNewHandData OnHandEnter;
            /// <summary> Event fired on the last frame the hand is visible, before the hand GameObject is made inactive. </summary>
            public OnNewHandData OnHandExit;


            /// <summary> Event fired on the first frame a hand goes from open to closed. </summary>
            public OnNewHandData OnGrab;
            /// <summary> Event fired on the first frame a hand goes from closed to open. </summary>
            public OnNewHandData OnRelease;
        }

        /// <summary>
        /// Class containing all settings for the hand.
        /// </summary>
        [Serializable]
        public class Settings
        {
            private const int IgnoreRaycast = 2;
            private const int Everything = -1;

            [Header("CenterHandFeature Search Settings")]
            [Range(0, 0.15f)]
            [SerializeField]
            private float _palmRadiusNear = 0.04f;

            [Range(0, 0.15f)]
            [SerializeField]
            private float _palmRadiusFar = 0.065f;

            [Range(0, 0.025f)]
            [SerializeField]
            private float _closestObjectDebounce = 0.01f;

            [SerializeField]
            private QueryTriggerInteraction _queryTriggers = QueryTriggerInteraction.Collide;

            [SerializeField]
            private LayerMask _queryLayerMask = (1 << -Everything) | ~(1 << IgnoreRaycast);

            [SerializeField]
            private int _handFeatureLayer;

            [Tooltip("Turn on or off post processing of the hand trajectory.")]
            [SerializeField]
            private bool _enablePostProcessing = true;

            public float PalmRadiusNear
            {
                get { return _palmRadiusNear; }
            }

            public float PalmRadiusFar
            {
                get { return _palmRadiusFar; }
            }

            public QueryTriggerInteraction QueryTriggers
            {
                get { return _queryTriggers; }
            }

            public LayerMask QueryLayerMask
            {
                get { return _queryLayerMask; }
            }

            public float ClosestObjectDebounce
            {
                get { return _closestObjectDebounce; }
            }

            public int HandFeatureLayer
            {
                get { return _handFeatureLayer; }
            }

            /// <summary>
            /// Turn on or off post processing of the hand trajectory.
            /// </summary>
            public bool EnablePostProcessing
            {
                get { return _enablePostProcessing; }
                set { _enablePostProcessing = value; }
            }
        }
    }
}
