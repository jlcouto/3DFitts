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
using UnityEngine;

namespace Meta.HandInput
{
    public enum ControllerType
    {
        Hand, Controller
    }

    [System.Serializable]
    public class HandData : IInteractionController
    {
        private const float MaxUntrackedTime = 1.0f;
        private const float MaxAnglesFromGaze = 32.5f;

        /// <summary> Unique id for hand </summary>
        public int UniqueId
        {
            get; private set;
        }
        /// <summary> Hand's top point </summary>
        public Vector3 Top
        {
            get; private set;
        }
        /// <summary> Hand's palm anchor </summary>
        public Vector3 Palm
        {
            get; private set;
        }
        /// <summary> Hand's grab anchor </summary>
        public Vector3 GrabAnchor
        {
            get; private set;
        }
        /// <summary> Hand's grab value </summary>
        public bool IsGrasping
        {
            get; private set;
        }
        /// <summary> hand's HandType </summary>
        public HandType HandType
        {
            get; private set;
        }
        /// <summary> Is the hand visible is the cameras view. </summary>
        public bool IsTracked
        {
            get; private set;
        }

        /// <summary> Unique id for hand </summary>
        [Obsolete("Property HandId is deprecated, please use UniqueId")]
        public int HandId
        {
            get
            {
                return UniqueId;
            }
        }

        public event Action OnUpdated = () => { };

        public Vector3 Position
        {
            get
            {
                return Palm;
            }
        }
        public Quaternion Rotation
        {
            get
            {
                return Quaternion.identity;
            }
        }
        public ControllerType ControllerType
        {
            get
            {
                return ControllerType.Hand;
            }
        }



        private bool _wasTracked;
        private bool _untrackedInView;
        private bool _wasTrackedPerFrame;
        private float _timeLostTracking;

        /// <summary> Event to get fired whenever the hand has entered the camera's view. /// </summary>
        public System.Action OnEnterFrame;
        /// <summary> Event to get fired whenever the hand has left the camera's view. /// </summary>
        public System.Action OnExitFrame;
        /// <summary> Event to get fired whenever the tracking of the hand is lost. /// </summary>
        public System.Action OnTrackingLost;
        /// <summary> Event to get fired whenever the tracking of the hand is recovered. /// </summary>
        public System.Action OnTrackingRecovered;

        /// <summary> Returns the angle between the gaze vector and the palm-to-sensor vector. </summary>
        private float AnglesFromGaze
        {
            get
            {
                var palmToSensorDir = (Position - Camera.main.transform.position).normalized;
                return Vector3.Angle(Camera.main.transform.forward, palmToSensorDir);
            }
        }

        public HandData()
        {
        }

        /// <summary>
        /// Applies hand properties from input types.fbs.HandData to current hand.
        /// </summary>
        public void UpdateHand(types.fbs.HandData? cocoHand)
        {
            _wasTracked = IsTracked;
            var hand = cocoHand.Value;

            CalculateTrackingState(hand);

            UniqueId = hand.HandId;
            HandType = hand.HandType == types.fbs.HandType.RIGHT ? HandType.Right : HandType.Left;
            IsGrasping = hand.IsGrabbing;

            GrabAnchor = hand.GrabAnchor.Value.ToVector3();
            Palm = hand.Palm.Value.ToVector3();
            Top = hand.Top.Value.ToVector3();

            OnUpdated.Invoke();
        }


        /// <summary>
        /// Fires all hand related events. 
        /// Called after all hands in view are updated.
        /// </summary>
        public void UpdateEvents()
        {
            if (_wasTracked != IsTracked)
            {
                if (IsTracked)
                {
                    if (OnEnterFrame != null)
                    {
                        OnEnterFrame.Invoke();
                    }
                }
                else
                {
                    if (OnExitFrame != null)
                    {
                        OnExitFrame.Invoke();
                    }
                }
            }
        }

        private void CalculateTrackingState(types.fbs.HandData hand)
        {
            if (hand.IsTracked)
            {
                IsTracked = true;
            }
            else
            {
                if (_wasTrackedPerFrame)
                {
                    _timeLostTracking = Time.time;
                }

                if (AnglesFromGaze < MaxAnglesFromGaze)
                {
                    if (Time.time - _timeLostTracking > MaxUntrackedTime)
                    {
                        IsTracked = false;
                    }
                }
                else
                {
                    IsTracked = false;
                }
            }

            _wasTrackedPerFrame = hand.IsTracked;
        }


        public override string ToString()
        {
            string data;
            data = "Hand Type: " + (HandType == HandType.Right ? "Right" : "Left");
            data += "\nHand Id: " + UniqueId;
            data += "\nIs Grabbed: " + (IsGrasping ? "True" : "False");
            data += "\nIs Tracked: " + (IsTracked ? "True" : "False");
            return data;
        }

    };
}
