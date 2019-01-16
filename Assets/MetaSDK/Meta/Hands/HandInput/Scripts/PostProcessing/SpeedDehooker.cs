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

namespace Meta.HandInput
{
    /// <summary>
    /// Removes hooks based on the instantaneous speed of the hand.
    /// When the hand is moving in a certain range of speeds, it is presumed to be in a hook.
    /// </summary>
    public class SpeedDehooker : MonoBehaviour, IHandPostProcess
    {
        [Tooltip("When enabled, helps dehooking work at low speeds.")]
        [SerializeField]
        private bool _enableLowSpeedCheck = true;

        private const float LowerSpeedThreshold = 0.04639265f;
        private const float UpperSpeedThreshold = 1.89408274f;
        private const float LowerMeanRatioThreshold = 0.14561233f;
        private const float UpperMeanRatioThreshold = 1;
        private const float LowSpeedThreshold = 0.02060104f;
        private const float MaxCandidatePositionAge = 0.25f;

        private Vector3 _candidatePosition;
        private Vector3 _lastHandPosition;
        private CenterHandFeature _centerHandFeature;
        private float _candidatePositionTimestamp;
        private float _meanSpeed;
        private int _numSamples;

        /// <inheritdoc />
        public Vector3 CandidatePosition
        {
            get
            {
                return IsOldCandidatePosition() ? _lastHandPosition : _candidatePosition;
            }
            private set
            {
                _candidatePositionTimestamp = Time.time;
                _candidatePosition = value;
            }
        }

        private void Awake()
        {
            _centerHandFeature = GetComponent<CenterHandFeature>();
            CandidatePosition = _centerHandFeature.transform.position;
        }

        /// <inheritdoc />
        public void StartGrab()
        {
            CandidatePosition = _centerHandFeature.transform.position;
        }

        /// <summary>
        /// Checks the speed of the hand to determine if there is a hook and correct if one is detected.
        /// </summary>
        public void UpdateCandidatePosition()
        {
            if (_centerHandFeature.IsNearObject && _centerHandFeature.Hand.IsGrabbing)
            {
                float deltaDistance = Vector3.Distance(transform.position, _lastHandPosition);
                float speed = deltaDistance / Time.unscaledDeltaTime;

                if (!_enableLowSpeedCheck || _meanSpeed > LowSpeedThreshold)
                {
                    CheckMeasurementAgainstThresholds(speed, LowerSpeedThreshold,
                                                      UpperSpeedThreshold);
                }
                else
                {
                    if (_meanSpeed != 0)
                    {
                        float ratioToMeanSpeed = speed / _meanSpeed;
                        CheckMeasurementAgainstThresholds(ratioToMeanSpeed, LowerMeanRatioThreshold,
                                                          UpperMeanRatioThreshold);
                    }
                }

                _meanSpeed = (_meanSpeed * _numSamples + speed) / ++_numSamples;
            }

            _lastHandPosition = _centerHandFeature.transform.position;
        }

        private bool IsOldCandidatePosition()
        {
            return Time.time - _candidatePositionTimestamp > MaxCandidatePositionAge;
        }

        private void CheckMeasurementAgainstThresholds(float measurement, float lowerThreshold, float upperThreshold)
        {
            if (measurement > 0 && measurement < lowerThreshold)
            {
                CandidatePosition = transform.position;
            }
            else if (measurement > upperThreshold)
            {
                CandidatePosition = transform.position;
            }
        }
    }
}
