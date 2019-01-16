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
using System.Collections;
using UnityEngine;
using System;
using System.Runtime.CompilerServices;
using Meta.Plugin;
using ConnectionStatus = Meta.Plugin.DeviceStatusSnapshot.ConnectionStatus;
using SensorStream = Meta.Plugin.DeviceStatusSnapshot.SensorStream;
using DeviceStatus = Meta.Plugin.DeviceStatusSnapshot.DeviceStatus;


[assembly: InternalsVisibleTo("Meta-Editor")]

namespace Meta.SensorMessages
{
    /// <summary>
    /// Creates Sensor Failure messages at predefined intervals.
    /// </summary>
    internal class MetaSensorFailureMessages
    {
        public const string SensorMessage = "Please exit, restart your device and\nlaunch the application again.";
        public const string HeadsetDisconnectedMessage = "Meta 2 has been disconnected.\nPlease exit, restart your device and\nlaunch the application again.";
        public const string DepthSensorNotWorkingMessage = "We've encountered issues starting sensors. Hands might not track.\nExit the application and run Headset Diagnostics.";
        private const int RecoveryAttempts = 10;

        protected IMetaSensorUiController _controller;
        private bool _recoverySequenceFailed = false;

        private Func<DeviceStatusSnapshot> _deviceStatusSnapshotRetrievalStrategy = () => { return Plugin.SystemApi.GetDeviceStatus(); };

        protected void CheckSensors()
        {
            var manager = GameObject.FindObjectOfType<MetaManager>();
            if (!manager)
            {
                Debug.LogError("Could not get MetaManager");
                return;
            }

            manager.StartCoroutine(CheckSensorsContinually());
        }

        public IEnumerator CheckSensorsContinually()
        {
            yield return InitializationSequence();

            for (; ; )
            {
                yield return new WaitForSeconds(IntervalToCheckSensorsSeconds);

                IEnumerator ie = CheckSensorsAtIntervals();
                while (ie.MoveNext())
                {
                    yield return ie.Current;
                }

                if (!_recoverySequenceFailed)
                {
                    continue;
                }

                yield break;
            }
        }

        private IEnumerator InitializationSequence()
        {
            DeviceStatusSnapshot snapshot;

            _controller.ChangeMinorMessage("Connecting");
            while (true)
            {
                snapshot = _deviceStatusSnapshotRetrievalStrategy();

                if (snapshot.StatusOfConnection == ConnectionStatus.NOT_SUPERSPEED_USB)
                {
                    _controller.ChangeMinorMessage("Connect to a different USB port");
                    yield return new WaitForSeconds(IntervalToCheckSensorsSeconds);
                    continue;
                }

                if (snapshot.StatusOfConnection != ConnectionStatus.CONNECTED)
                {
                    _controller.ChangeMinorMessage("Please plug in the headset.\n\nIf content is not visible through the headset,\nplease restart the application.");
                    yield return new WaitForSeconds(IntervalToCheckSensorsSeconds);
                    continue;
                }

                break;
            }

            float timeInitStarted = Time.time;
            const float secondsToWaitForCalibrationMessage = 10f;

            _controller.ChangeMinorMessage("Initializing");
            _controller.SetLoadingFeedback(true); 
            while (true)
            {
                snapshot = _deviceStatusSnapshotRetrievalStrategy();

                if ( snapshot.StatusOfDevice != DeviceStatus.RUNNING )
                {
                    bool readingCalibration = Time.time > timeInitStarted + secondsToWaitForCalibrationMessage;
                    if (readingCalibration)
                    {
                        _controller.ChangeMinorMessage("Reading calibration data from headset.\nThis may take a few minutes.");
                    }
                    yield return new WaitForSeconds(IntervalToCheckSensorsSeconds);
                    continue;
                }

                break;
            }

            _controller.SetLoadingFeedback(false);
            _controller.ChangeMinorMessage(string.Empty);
            yield return null;
        }


        /// <summary>
        /// Check the sensors at the intervals defined.
        /// </summary>
        /// <returns></returns>
        private IEnumerator CheckSensorsAtIntervals()
        {
            if (_deviceStatusSnapshotRetrievalStrategy().DeviceStreamingProperly())
            {
                yield break;
            }

            yield return CheckSensorFrequently(RecoveryAttempts, IntervalToCheckSensorsSeconds);

            if (ProcessRecovery())
            {
                yield break;
            }
            else
            {
                _controller.ChangeMessage(GetSpecificSensorFailureMessage());
                _controller.SetTitleVisibility(true);
            }

            _recoverySequenceFailed = true;
        }

        private string GetSpecificSensorFailureMessage()
        {
            DeviceStatusSnapshot snapshot = _deviceStatusSnapshotRetrievalStrategy();
            string error = string.Format("Error: ( {0} | {1} | {2} )", snapshot.StreamStatusMask, snapshot.StatusOfConnection, snapshot.StatusOfDevice);

            return SensorMessage + "\n" + error;
        }

        /// <summary>
        /// Updates the UI controller based on the device status.
        /// </summary>
        /// <returns>Whether the action was completed.</returns>
        private bool ProcessRecovery()
        {
            DeviceStatusSnapshot snapshot = _deviceStatusSnapshotRetrievalStrategy();

            if (snapshot.DeviceStreamingProperly())
            {
                _controller.ChangeMessage(string.Empty);
                _controller.SetTitleVisibility(false);
                return true;
            }

            if(snapshot.StatusOfConnection != ConnectionStatus.CONNECTED)
            {
                _controller.ChangeMessage(HeadsetDisconnectedMessage);
                _controller.SetTitleVisibility(true);
                _recoverySequenceFailed = true;
                return true;
            }

            bool allImportantSensorsExceptDepthWorking = (snapshot.StreamStatusMask ^ (~(int)SensorStream.DEPTH) 
                & DeviceStatusSnapshot.ImportantStreamMask) == DeviceStatusSnapshot.ImportantStreamMask;

            if (allImportantSensorsExceptDepthWorking)
            {
                _controller.ChangeMessage(DepthSensorNotWorkingMessage);
                _controller.SetTitleVisibility(true);
                _recoverySequenceFailed = true;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Check the sensors for a number of times at a given interval. 
        /// </summary>
        /// <param name="numberOfChecks">number of times to check</param>
        /// <param name="checkIntervalSeconds">interval to wait in seconds.</param>
        /// <returns></returns>
        private IEnumerator CheckSensorFrequently(int numberOfChecks, float checkIntervalSeconds)
        {

            for (int i = 0; i < numberOfChecks; ++i)
            {

                DeviceStatusSnapshot snapshot = _deviceStatusSnapshotRetrievalStrategy();
                if (snapshot.DeviceStreamingProperly() && snapshot.StatusOfConnection == ConnectionStatus.CONNECTED)
                {
                    yield break;
                }

                yield return new WaitForSeconds(checkIntervalSeconds);
            }
        }

        /// <summary>
        /// Shows messages on the UI for various sensor failure issues.
        /// </summary>
        public MetaSensorFailureMessages()
        {
            _controller = new MetaSensorUiController();
            _controller.SetTitleVisibility(false);
            IntervalToCheckSensorsSeconds = 1f;
        }

        /// <summary>
        /// Shows messages on the UI for various sensor failure issues with user specified Device status snapshot retrieval strategy and UI controller.
        /// </summary>
        /// <param name="deviceStatusSnapshotRetrievalStrategy">The function which provides device status snapshots.</param>
        /// <param name="controller">The UI controller.</param>
        public MetaSensorFailureMessages(Func<DeviceStatusSnapshot> deviceStatusSnapshotRetrievalStrategy, IMetaSensorUiController controller)
        {
            _deviceStatusSnapshotRetrievalStrategy = deviceStatusSnapshotRetrievalStrategy;
            _controller = controller;
            _controller.SetTitleVisibility(false);
            IntervalToCheckSensorsSeconds = 1f;
        }

        /// <summary>
        /// The interval in seconds between subsequent sensor checks.
        /// </summary>
        public float IntervalToCheckSensorsSeconds
        {
            get; set;
        }

        /// <summary>
        /// Whether the recovery sequence was unable to recover.
        /// </summary>
        public bool RecoverySequenceFailed
        {
            get { return _recoverySequenceFailed; }
        }
    }
}
