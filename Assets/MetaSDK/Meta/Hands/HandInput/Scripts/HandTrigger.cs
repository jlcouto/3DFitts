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
using System.Collections.Generic;
using System;

namespace Meta.HandInput
{
    /// <summary>
    /// Will gather Meta HandFeature GameObjects when they enter and exit the Trigger on this GameObject.
    /// Use whenever you need to determine the entry and exit of the HandFeature into a particular area.
    /// </summary>
    public class HandTrigger : MonoBehaviour
    {
        [Tooltip("Should hand trigger events still fire when this component is disabled?")]
        [SerializeField]
        private bool _triggerWhenDisabled = false;

        [SerializeField]
        private HandTriggerEvent _twoHandEnterEvent = new HandTriggerEvent();

        [SerializeField]
        private HandFeatureEvent _handFeatureEnterEvent = new HandFeatureEvent();

        [SerializeField]
        private HandFeatureEvent _firstHandFeatureEnterEvent = new HandFeatureEvent();

        [SerializeField]
        private HandTriggerEvent _twoHandExitEvent = new HandTriggerEvent();

        [SerializeField]
        private HandFeatureEvent _handFeatureExitEvent = new HandFeatureEvent();

        [SerializeField]
        private HandFeatureEvent _lastHandFeatureExitEvent = new HandFeatureEvent();

        /*TODO Write UI for this to not need these strings typed in
        "Meta.HandInput.TopHandFeature, Assembly-CSharp",
        "Meta.HandInput.CenterHandFeature, Assembly-CSharp",
        "Meta.HandInput.LinkHandFeature, Assembly-CSharp",
        */
        [Header("An empty list allows all Types")]
        [SerializeField]
        HandFeatureType _allowedFeatureType = HandFeatureType.Any;
        
        [SerializeField]
        private Vector3 _expandOnEntry = new Vector3(1.1f, 1.1f, 1.1f);

        private readonly List<HandFeature> _handFeatureList = new List<HandFeature>();

        private bool _twoHandsEntered;
        private Vector3 _initialScale;
        private Collider _collider;

        /// <summary>
        /// Number of unique HandTypes (left, right) in Trigger.
        /// </summary>
        public int HandCount
        {
            get
            {
                bool foundLeft = false, foundRight = false;

                foreach (HandFeature handFeature in _handFeatureList)
                {
                    switch (handFeature.Hand.HandType)
                    {
                        case HandType.Left:
                            foundLeft = true;
                            break;

                        case HandType.Right:
                            foundRight = true;
                            break;
                    }

                    if (foundLeft && foundRight)
                    {
                        break;
                    }
                }

                return Convert.ToInt32(foundLeft) + Convert.ToInt32(foundRight);
            }
        }

        /// <summary>
        /// Called first time two HandFeatures on different hands enter HandVolume.
        /// </summary>
        public HandTriggerEvent TwoHandEnterEvent
        {
            get { return _twoHandEnterEvent; }
        }

        /// <summary>
        /// Called when any HandFeature enters HandVolume.
        /// </summary>
        public HandFeatureEvent HandFeatureEnterEvent
        {
            get { return _handFeatureEnterEvent; }
        }

        /// <summary>
        /// Called when first HandFeature enters HandVolume.
        /// </summary>
        public HandFeatureEvent FirstHandFeatureEnterEvent
        {
            get { return _firstHandFeatureEnterEvent; }
        }

        /// <summary>
        /// Called when two HandFeatures from different hands are no longer in HandVolume.
        /// </summary>
        public HandTriggerEvent TwoHandExitEvent
        {
            get { return _twoHandExitEvent; }
        }

        /// <summary>
        /// Called when any HandFeature exists HandVolume.
        /// </summary>
        public HandFeatureEvent HandFeatureExitEvent
        {
            get { return _handFeatureExitEvent; }
        }

        /// <summary>
        /// Called when all HandFeatures have exited HandVolume.
        /// </summary>
        public HandFeatureEvent LastHandFeatureExitEvent
        {
            get { return _lastHandFeatureExitEvent; }
        }

        /// <summary>
        /// All HandFeatures currently in volume.
        /// </summary>
        public List<HandFeature> HandFeatureList
        {
            get { return _handFeatureList; }
        }

        private void Awake()
        {
            _collider = GetComponent<Collider>();
            SetColliderIsTrigger();

            _initialScale = transform.localScale;

            AddSceneExitCheck();
        }
        
        private void Update()
        {
            for (int i = _handFeatureList.Count - 1; i > -1; --i)
            {
                if (_handFeatureList[i] == null)
                {
                    _handFeatureList.RemoveAt(i);
                    OnHandExit(null);
                }
            }
        }
        
        private void OnTriggerEnter(Collider collider)
        {
            if (!_triggerWhenDisabled && !enabled)
            {
                return;
            }

            HandFeature handFeature = collider.GetComponent<HandFeature>();

            if (handFeature != null && IsAllowedType(handFeature) && handFeature.gameObject.activeSelf)
            {
                if (_firstHandFeatureEnterEvent != null && _handFeatureList.Count == 0)
                {
                    transform.localScale = Vector3.Scale(transform.localScale, _expandOnEntry);
                    _firstHandFeatureEnterEvent.Invoke(handFeature);
                }
                if (_handFeatureEnterEvent != null)
                {
                    _handFeatureEnterEvent.Invoke(handFeature);
                }

                _handFeatureList.Add(handFeature);

                if (!_twoHandsEntered && HandCount > 1)
                {
                    _twoHandsEntered = true;
                    if (_twoHandEnterEvent != null)
                    {
                        _twoHandEnterEvent.Invoke(this);
                    }
                }
            }
        }

        private void OnTriggerExit(Collider collider)
        {
            if (!_triggerWhenDisabled && !enabled)
            {
                return;
            }

            HandFeature handFeature = collider.GetComponent<HandFeature>();

            if (handFeature != null && IsAllowedType(handFeature))
            {
                OnHandExit(handFeature);
            }
        }

        private void AddSceneExitCheck()
        {
            HandsProvider handsProvider = GameObject.Find("MetaHands").GetComponent<HandsProvider>();
            if (handsProvider != null)
            {
                handsProvider.events.OnHandExit.AddListener(OnHandSensorExit);
            }
        }

        private void OnHandSensorExit(Hand h)
        {
            if (!_triggerWhenDisabled && !enabled)
            {
                return;
            }

            HandFeature[] childHandFeatures = h.gameObject.GetComponentsInChildren<HandFeature>();
            for (int i = 0; i < childHandFeatures.Length; i++)
            {
                if (IsAllowedType(childHandFeatures[i]))
                {
                    OnHandExit(childHandFeatures[i]);
                }
            }
        }

        /// <summary>
        /// Determines if HandTrigger contains hand.
        /// </summary>
        /// <param name="handFeature"> HandFeature to find. </param>
        /// <returns> HandFeature if found, otherwise null. </returns>
        public bool ContainsHand(HandFeature handFeature)
        {
            for (int i = 0; i < _handFeatureList.Count; ++i)
            {
                if (_handFeatureList[i] == handFeature)
                {
                    return true;
                }
            }

            return false;
        }

        private void OnHandExit(HandFeature handFeature)
        {
            bool wasInTrigger = false;

            if (handFeature != null)
            {
                wasInTrigger = _handFeatureList.Remove(handFeature);
            }

            if (_lastHandFeatureExitEvent != null && HandCount == 0 && wasInTrigger)
            {
                transform.localScale = _initialScale;
                _lastHandFeatureExitEvent.Invoke(handFeature);
            }

            if (_handFeatureExitEvent != null)
            {
                _handFeatureExitEvent.Invoke(handFeature);
            }

            if (_twoHandsEntered && HandCount < 2)
            {
                _twoHandsEntered = false;
                if (_twoHandExitEvent != null)
                {
                    _twoHandExitEvent.Invoke(this);
                }
            }
        }

        private bool IsAllowedType(HandFeature handFeature)
        {
            switch (_allowedFeatureType)
            {
                case HandFeatureType.Any:
                    return true;
                case HandFeatureType.PalmFeature:
                    return handFeature is CenterHandFeature;
                case HandFeatureType.TopFeature:
                    return handFeature is TopHandFeature;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }


        private void OnDrawGizmos()
        {
            // Enforce proper setup in editor
            SetColliderIsTrigger();
        }

        private void SetColliderIsTrigger()
        {
            if (_collider != null && !_collider.isTrigger)
            {
                _collider.isTrigger = true;
            }
        }
    }
}
