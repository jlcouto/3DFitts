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
using System;
using UnityEngine.Events;
using System.Collections;
using Meta.SlamUI;
using Directory = System.IO.Directory;
using Path      = System.IO.Path;
using SlamApi   = Meta.Plugin.SlamApi;
using SlamInterop   = Meta.Interop.SlamInterop;

namespace Meta
{
    ///<summary>
    /// This module uses MetaSLAM as a localizer.
    /// </summary>
    [Serializable]
    internal class SlamLocalizer : MetaBehaviour, ILocalizer, ISlamEventProvider
    {
        private enum SlamInitializationState
        {
            WaitingForInitialization,
            InitialMapping,
            Mapping,
            Finished
        }

        #region Public Events

        [System.Serializable]
        public class SLAMSensorsReadyEvent : UnityEvent { }
        [System.Serializable]
        public class SLAMMappingInProgressEvent : UnityEvent<float> { }
        [System.Serializable]
        public class SLAMMappingCompleteEvent : UnityEvent { }
        [System.Serializable]
        public class SLAMMapLoadingFailedEvent : UnityEvent { }
        [System.Serializable]
        public class SLAMTrackingLostEvent : UnityEvent { }
        [System.Serializable]
        public class SLAMTrackingRelocalizedEvent : UnityEvent { }
        [System.Serializable]
        public class SLAMInitializationFailedEvent : UnityEvent { }
        [System.Serializable]
        public class SLAMLocalizerResetEvent : UnityEvent { }

        public SLAMSensorsReadyEvent onSlamSensorsReady = null;
        public SLAMMappingInProgressEvent onSlamMappingInProgress = null;
        public SLAMMappingCompleteEvent onSlamMappingComplete = null;
        public SLAMMapLoadingFailedEvent onSlamMapLoadingFailedEvent = null;
        public SLAMTrackingLostEvent onSlamTrackingLost = null;
        public SLAMTrackingRelocalizedEvent onSlamTrackingRelocalized = null;
        public SLAMInitializationFailedEvent onSlamInitializationFailed = null;
        public SLAMLocalizerResetEvent onSlamLocalizerResetEvent = null;
        #endregion

        /// <summary> 
        /// Whether or not to load a map on initialization
        /// </summary> 
        [SerializeField]
        private float _loadingMapWaitTime = 10f;
        
        [SerializeField]
        private bool _showCalibrationUI = true;

        [SerializeField]
        private bool _rotationOnlyTracking = false;
        private bool _rotationOnlyTrackingPrevious = false;

        /// <summary>
        /// The SLAM interop; this wraps implementations of ISlam interface.
        /// </summary>
        private SlamApi _slamInterop;

        private SlamInitializationState _initializationState = SlamInitializationState.WaitingForInitialization;
        private SlamInterop.TrackingStatus _status;
        private SlamInterop.TrackingStatus _lastStatus;
        private string _saveMapName;
        private string _loadMapName;
        private bool _loadMapAtInitRequested = false;
        private bool _slamInitializedFromLoadedMap = false;
        private Coroutine _waitForSlamLoadingCoroutine;
        private GameObject _targetGO;
        private GameObject _slamUiPrefab;
        private GameObject _slamUI;
        private Coroutine _slamUICoroutine;
        private const string SlamUIPrefabName = "Prefabs/SLAM_UI";
        /// <summary>
        /// Indicates if the Meta Compositor script is in the scene.
        /// </summary>
        private bool _fromCompositor;

        /// <summary>
        /// Whether SLAM was last initialized by loading a saved map. 
        /// </summary>
        public bool SlamInitializedFromLoadedMap
        {
            get { return _slamInitializedFromLoadedMap; }
        }

        /// <summary>
        /// Occurs when Slam Mapping is completed.
        /// </summary>
        public UnityEvent SlamMappingCompleted
        {
            get { return onSlamMappingComplete; }
        }

        /// <summary>
        /// Occurs when Slam Map Loading failed.
        /// </summary>
        public UnityEvent SlamMapLoadingFailed
        {
            get { return onSlamMapLoadingFailedEvent; }
        }

        /// <summary>
        /// Indicate to show or not the Calibration UI
        /// </summary>
        public bool ShowCalibrationUI
        {
            get { return _showCalibrationUI; }
            set { _showCalibrationUI = value; }
        }

        /// <summary>
        /// Whether the slam process is finished or not.
        /// </summary>
        public bool IsFinished
        {
            get { return _initializationState == SlamInitializationState.Finished; }
        }

        /// <summary>
        /// Is rotation only tracking enabled
        /// </summary>
        public bool RotationOnlyTrackingEnabled
        {
            get { return _rotationOnlyTracking; }
        }

        private void Start()
        {
            _slamUiPrefab = (GameObject)Resources.Load(SlamUIPrefabName);
            _fromCompositor = (FindObjectOfType<Plugin.MetaCompositor>() != null);
            gameObject.AddComponent<SlamTrackingUILoader>();
            InitSlam(null);
        }
        
        private void OnApplicationQuit()
        {
            /// Save map at end. Must be called before meta_stop() (called in OnDestroy())
            if (!string.IsNullOrEmpty(_saveMapName))
            {
                _slamInterop.SaveSlamMap(_saveMapName);
            }
        }

        /// <summary>
        /// Enables the slam UI.
        /// </summary>
        [Obsolete("This method is obsolete. Please use SlamLocalizer.ShowCalibrationUI property")]
        public void EnableSlamUI()
        {
            ShowCalibrationUI = true;
        }

        /// <summary>
        /// Initializes the slam map creation process.
        /// </summary>
        public void CreateSlamMap()
        {
            SaveSlamMap(null);
            InitSlam(null);
        }

        /// <summary>
        /// Initializes the slam map loading process.
        /// If cameras are not initialized we cache the map to load later.
        /// Handles UI if load succeeds or fails
        /// </summary>
        /// <param name="mapName">The slam map name.</param>
        public void LoadSlamMap(string mapName)
        {
            if (!SensorsReady())
            {
                _loadMapName = mapName;
                _loadMapAtInitRequested = true;
            }
            else
            {
                bool loaded = _slamInterop.LoadSlamMap(mapName);

                if (loaded)
                {
                    StopInitialization();
                    _slamUICoroutine = StartCoroutine(ShowUI(SlamInitializationType.LoadingMap));
                    _slamInitializedFromLoadedMap = true;
                }
                else
                {
                    onSlamMapLoadingFailedEvent.Invoke();
                    ResetLocalizer();
                }

            }
        }

        /// <summary>
        /// Saves an slam map defined by mapName.
        /// Only caches map name, map will be saved on exit.
        /// </summary>
        /// <param name="mapName">The slam map name.</param>
        public void SaveSlamMap(string mapName)
        {
            if (!string.IsNullOrEmpty(mapName))
            {
                string folder = Path.GetDirectoryName(mapName);
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }
            }
            _saveMapName = mapName;
        }

        /// <summary> 
        /// Override of ILocalizer. 
        /// </summary>
        public void SetTargetGameObject(GameObject targetGO)
        {
            _targetGO = targetGO;
        }

        /// <summary>
        /// Override of ILocalizer. 
        /// </summary>
        public void ResetLocalizer()
        {
            _loadMapAtInitRequested = false;
            _slamInitializedFromLoadedMap = false;
            _loadMapName = "";
            _status.state = SlamInterop.TrackingStatus.State.NOT_READY;
            _lastStatus.state = SlamInterop.TrackingStatus.State.NOT_READY;

            SetState(SlamInitializationState.InitialMapping);
            StopInitialization();
            _slamInterop.ResetLocalizer();
            if (!_slamInterop.IsRotationOnlyTracking())
            {
                _slamUICoroutine = StartCoroutine(ShowUI(SlamInitializationType.NewMap));
            }
            onSlamLocalizerResetEvent.Invoke();
        }

        public void ToggleRotationOnlyTracking()
        {
            _slamInterop.ToggleRotationOnlyTracking();
        }

        /// <summary> 
        /// Override of ILocalizer.
        /// </summary>
        public void UpdateLocalizer()
        {
            if (_slamInterop != null)
            {
                _slamInterop.TargetGO = _targetGO;
                _slamInterop.Update(_fromCompositor);

                if (_loadMapAtInitRequested && SensorsReady() )
                {
                    LoadSlamMap(_loadMapName);
                    _loadMapAtInitRequested = false;
                    _waitForSlamLoadingCoroutine = StartCoroutine(WaitForSlamLoading());
                }

                _slamInterop.GetTrackingStatus(out _status);
                ProcessSlamFeedback(_status, _lastStatus);
                _lastStatus = _status;

                if (_rotationOnlyTracking != _rotationOnlyTrackingPrevious)
                {
                    if (_rotationOnlyTracking)
                    {
                        onSlamTrackingLost.Invoke();
                    }
                    else
                    {
                        onSlamTrackingRelocalized.Invoke();
                    }

                    ToggleRotationOnlyTracking();
                    _rotationOnlyTrackingPrevious = _rotationOnlyTracking;
                }
            }
        }

        private void SetState(SlamInitializationState slamInitializationState)
        {
            _initializationState = slamInitializationState;
        }

        private void InitSlam(string mapName)
        {
            if (_initializationState != SlamInitializationState.WaitingForInitialization)
            {
                return;
            }

            StopInitialization();
            SetState(SlamInitializationState.InitialMapping);

            _slamInterop = new SlamApi();

            if (_rotationOnlyTracking)
            {
                SetState(SlamInitializationState.Finished);
            }
            else
            {
                _slamUICoroutine = StartCoroutine(ShowUI(SlamInitializationType.NewMap));
            }
        }

        private void StopInitialization()
        {
            if (_slamUI != null)
            {
                DestroyImmediate(_slamUI);
                _slamUI = null;
            }
            if (_slamUICoroutine != null)
            {
                StopCoroutine(_slamUICoroutine);
            }

            StopWaitingForSlamLoading();
        }

        private void StopWaitingForSlamLoading()
        {
            if (_waitForSlamLoadingCoroutine != null)
            {
                StopCoroutine(_waitForSlamLoadingCoroutine);
            }
        }

        /// <summary>
        /// Wait _loadingMapWaitTime before making slam fails.
        /// </summary>
        /// <returns></returns>
        private IEnumerator WaitForSlamLoading()
        {
            // Waiting for camera ready before initializing the timer.
            // Also waiting for UI, so the timer will start when the user knows what to do to relocalize.
            yield return new WaitUntil(() => SensorsReady() && _showCalibrationUI);

            float time = 0;
            while (time < _loadingMapWaitTime)
            {
                if (_initializationState != SlamInitializationState.InitialMapping)
                {
                    yield break;
                }
                time += Time.deltaTime;
                yield return 0;
            }

            onSlamMapLoadingFailedEvent.Invoke();
            ResetLocalizer();
        }

        private IEnumerator ShowUI(SlamInitializationType initializationType)
        {
            //Waiting for the interop to refresh slam data after reset.
            yield return new WaitForSeconds(3f);
            yield return new WaitUntil(() => _showCalibrationUI);

            if (IsFinished)
            {
                yield break;
            }

            if (_slamUiPrefab != null)
            {
                _slamUI = Instantiate(_slamUiPrefab);
                BaseSlamGuide slamGuide = _slamUI.GetComponent<BaseSlamGuide>();
                slamGuide.StartTrackCalibrationSteps(initializationType);
            }
            else
            {
                Debug.LogError("Could not locate SLAM UI resource.");
            }
        }

        public bool SensorsReady()
        {
            return _status.state != SlamInterop.TrackingStatus.State.NOT_READY;
        }

        public bool ShouldHoldStill()
        {
            return _status.state == SlamInterop.TrackingStatus.State.INITIALIZING
                && _status.reason == SlamInterop.TrackingStatus.Reason.IMU_INIT;
        }

        public bool IsTracking()
        {
            return _status.state == SlamInterop.TrackingStatus.State.TRACKING;
        }

        private void ProcessSlamFeedback(SlamInterop.TrackingStatus thisFrame, SlamInterop.TrackingStatus previousFrame)
        {
            if (thisFrame.state == SlamInterop.TrackingStatus.State.INITIALIZING
                && previousFrame.state == SlamInterop.TrackingStatus.State.NOT_READY)
            {
                onSlamSensorsReady.Invoke();
            }

            if (thisFrame.state == SlamInterop.TrackingStatus.State.INITIALIZING
                && thisFrame.reason == SlamInterop.TrackingStatus.Reason.ESTIMATING_SCALE
                && previousFrame.state == SlamInterop.TrackingStatus.State.INITIALIZING
                && previousFrame.reason == SlamInterop.TrackingStatus.Reason.VISUAL_INIT)
            {
                onSlamMappingInProgress.Invoke(1.0f);
                SetState(SlamInitializationState.Mapping);
            }


            if (thisFrame.state == SlamInterop.TrackingStatus.State.TRACKING
                && previousFrame.state == SlamInterop.TrackingStatus.State.INITIALIZING)
            {
                SetState(SlamInitializationState.Finished);
                onSlamMappingComplete.Invoke();
            }

            if (thisFrame.state == SlamInterop.TrackingStatus.State.LIMITED_TRACKING
                && thisFrame.reason == SlamInterop.TrackingStatus.Reason.LOST
                && previousFrame.state == SlamInterop.TrackingStatus.State.TRACKING)
            {
                onSlamTrackingLost.Invoke();
            }

            if (thisFrame.state == SlamInterop.TrackingStatus.State.TRACKING
                && previousFrame.state == SlamInterop.TrackingStatus.State.LIMITED_TRACKING
                && previousFrame.reason == SlamInterop.TrackingStatus.Reason.LOST)
            {
                onSlamTrackingRelocalized.Invoke();
            }
        }
    }
}
