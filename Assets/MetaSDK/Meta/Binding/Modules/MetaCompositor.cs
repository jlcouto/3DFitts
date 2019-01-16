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

using CompositorApi = Meta.Plugin.CompositorApi;
using CompositorInterop = Meta.Interop.MetaCompositorInterop;
using MetaUIContent = Meta.Rendering.MetaUIContent;

namespace Meta.Plugin
{
    /// <summary>
    /// Meta Compositor Script
    /// This class controls the render to the Meta Compositor.
    /// </summary>
    public class MetaCompositor : MonoBehaviour
    {
        // Clipping plane constants
        public const float DefaultNearClippingPlane = 0.015f,
                           MinNearClippingPlane = 0.001f,
                           MaxNearClippingPlane = 1f,
                           DefaultFarClippingPlane = 10f,
                           MinFarClippingPlane = 10f,   // Note: webcam renders at a distance of 9
                           MaxFarClippingPlane = 100f;

        public const int TEXTURE_SIZE = 2048;
        public const int TEXTURE_DEPTH = 24;
        public const int TEXTURE_ANTIALIASING_FACTOR = 8;
        public const RenderTextureFormat TEXTURE_FORMAT = RenderTextureFormat.ARGB32;

        #region Serialized Fields
        // Stereo cameras
        [SerializeField]
        [HideInInspector]
        [Tooltip("Left Eye Camera")]
        private UnityEngine.Camera _leftCam;
        [SerializeField]
        [HideInInspector]
        [Tooltip("Right Eye Camera")]
        private UnityEngine.Camera _rightCam;

        [SerializeField]
        [HideInInspector]
        private bool _enableWebcam;

        // Debug values
        [Header("Common")]
        [SerializeField]
        [HideInInspector]
        private bool _debugAddPoseLatency = false;

        [SerializeField]
        [HideInInspector]
        private bool _debugAsyncRenderingLatency = false;

        // Debug values
        [Header("Stabilization and Latency Reduction")]
        [SerializeField]
        [HideInInspector]
        private bool _enable2DWarp = true;

        // Debug values
        [SerializeField]
        [HideInInspector]
        private bool _enable3DWarp = false;

        [SerializeField]
        [Range(0.000f, 0.060f)]
        [HideInInspector]
        private float _trackingPrediction = 0.045f;

        // Async Rendering Parameters
        [SerializeField]
        [HideInInspector]
        [Tooltip("Enable or disable Late Warp")]
        private bool _enableLateWarp = false;

        [SerializeField]
        [HideInInspector]
        [Tooltip("Enable or disable dynamic late warp timing")]
        private bool _enableAsyncDynamicLateWarp = false;

        [Header("Asynchronous Rendering")]
        [SerializeField]
        [HideInInspector]
        [Tooltip("Asynchronous rendering delivers frames in a manner which is synchronous with the display. It is a prerequisite for some advanced prediction functionality. This cannot be configured after the scene has started.")]
        private bool _enableAsynchronousRendering = false;

        [SerializeField]
        [Range(1, 9)]
        [HideInInspector]
        private float _lateWarpThreshold = 4.0f;

        [SerializeField]
        [HideInInspector]
        [Tooltip("Draw 3D warp's wireframe mesh")]
        private bool _debugEnable3DWarpWireframe = false;

        [Header("Depth Occlusion")]
        [SerializeField]
        [HideInInspector]
        [Tooltip("Enable or disable rendering of the hand occlusion mesh")]
        private bool _enableDepthOcclusion = true;

        [SerializeField]
        [Range(0, 1)]
        [HideInInspector]
        [Tooltip("Strength of temporal filter, which acts as momentum")]
        private float _temporalMomentum = 0.60f;

        [SerializeField]
        [Range(1, 5)]
        [HideInInspector]
        [Tooltip("Filter size of the feather on the edge of hand occlusion")]
        private int _featherSize = 3;

        [SerializeField]
        [Range(1, 32)]
        [HideInInspector]
        [Tooltip("How fast the opacity falls off at the edge of the feather")]
        private float _featherFalloffExponent = 8;

        [SerializeField]
        [Range(0, 1)]
        [HideInInspector]
        [Tooltip("Cutoff feather opacity, below which pixels are thrown out")]
        private float _featherCutoff = 0.8f;

        [Header("Clipping Planes")]
        [SerializeField]
        [Range(MinNearClippingPlane, MaxNearClippingPlane)]
        [HideInInspector]
        [Tooltip("Near clipping plane for all cameras (default: 0.015)")]   // Ensure this matches DefaultNearClippingPlane
        private float _nearClippingPlane = DefaultNearClippingPlane;

        [SerializeField]
        [Range(MinFarClippingPlane, MaxFarClippingPlane)]
        [HideInInspector]
        [Tooltip("Far clipping plane for all cameras (default: 10)")]       // Ensure this matches DefaultFarClippingPlane
        private float _farClippingPlane = DefaultFarClippingPlane;

        /// <summary>
        /// Reference to the ContentCamera used to coordinate that camera's clipping planes with the Compositor.
        /// </summary>
        [SerializeField]
        private Camera _contentCamera;

        #endregion

        private Coroutine _endOfFrameLoop;
        private bool _started = false;

        /// <summary>
        /// Indicate if the Render Textures need to be created using AntiAliasing or not.
        /// </summary>
        private bool _enableAntiAliasing = false;

        /// <summary>
        /// Flag used to check if we have already disabled the 2D warp or 3D warp when displaying 
        /// HUD locked UI
        /// </summary>
        private bool _warpDisabledForUI = false;

        /// <summary>
        /// Initialize the Compositor
        /// </summary>
        private void Awake()
        {
            CompositorInterop.InitCompositor(_enableAsynchronousRendering);
            _warpDisabledForUI = _enable3DWarp || _enable2DWarp;

            if (!_contentCamera)
            {
                Debug.LogError("Unable to find ContentCamera; please reimport MetaCameraRig prefab.");
            }
        }

        /// <summary>
        /// Initialize this class on Start
        /// </summary>
        private void Start()
        {
            // Setup rendertargets for stereo cameras
            var rt_left = CreateRenderTexture();
            var rt_right = CreateRenderTexture();

            _rightCam.targetTexture = rt_right;
            _leftCam.targetTexture = rt_left;

            Interop.MetaCompositorInterop.SetEyeRenderTargets(
                rt_left.GetNativeTexturePtr(),
                rt_right.GetNativeTexturePtr(),
                rt_left.GetNativeDepthBufferPtr(),
                rt_right.GetNativeDepthBufferPtr()
            );

            // register the callback used before camera renders.  Unfortunately, Unity sets this for ALL cameras,
            //so we can't register a callback for a single camera only.
            UnityEngine.Camera.onPreRender += OnPreRenderEvent;

            //ensure that the right camera renders after the left camera.  We need the right camera
            //to render last since we call EndFrame on the Compositor via the right camera and the left camera to render first
            _rightCam.depth = _leftCam.depth + 1;

            // Enable/Disable 2D warp or 3D warp on start
            Interop.MetaCompositorInterop.Enable2DWarp(_enable2DWarp ? 1 : 0);
            Interop.MetaCompositorInterop.Enable3DWarp(_enable3DWarp ? 1 : 0);
            Interop.MetaCompositorInterop.SetSystemLatency(_trackingPrediction);
            Interop.MetaCompositorInterop.DebugEnablePoseLatency(_debugAddPoseLatency);

            // Always enable dynamic latewarp and rate limiting when asynchronously rendering
            Interop.MetaCompositorInterop.EnableAsyncDynamicLateWarp(_enableAsynchronousRendering);
            Interop.MetaCompositorInterop.EnableRateLimitAsyncRendering(_enableAsynchronousRendering);
            Interop.MetaCompositorInterop.EnableLateWarp(_enableAsynchronousRendering);

            rt_left.autoGenerateMips = false;
            rt_right.autoGenerateMips = false;
            rt_left.filterMode = rt_right.filterMode = FilterMode.Bilinear;

            var rt_webcam = new RenderTexture(1280, 720, 24, RenderTextureFormat.ARGB32);
            rt_webcam.antiAliasing = 1;
            rt_webcam.Create();

            _contentCamera.targetTexture = rt_webcam;
            Interop.MetaCompositorInterop.SetWebcamRenderTarget(rt_webcam.GetNativeTexturePtr(),
                rt_webcam.GetNativeDepthBufferPtr());


            // Set defaults for clipping planes
            NearClippingPlane = _nearClippingPlane;
            FarClippingPlane = _farClippingPlane;

            // Enabling/setting up all the defaults
            EnableHandOcclusion = _enableDepthOcclusion;
            TemporalMomentum = _temporalMomentum;
            FeatherSize = _featherSize;
            FeatherCutoff = _featherCutoff;
            FeatherFalloffExponent = _featherFalloffExponent;

            // Occlusion is disabled during the SLAM init process. Record the initial state of occlusion so it can be restored after SLAM init is dismissed.
            OcclusionEnabledAtStart = EnableHandOcclusion;

            WebcamEnabled = _enableWebcam;

            // Start the End Of Frame Loop
            _started = true;
        }

        /// <summary>
        /// Create the textures for render targets for stereo cameras
        /// </summary>
        /// <returns>RenderTexture configured for compositor cameras</returns>
        private RenderTexture CreateRenderTexture()
        {
            var texture = new RenderTexture(TEXTURE_SIZE, TEXTURE_SIZE, TEXTURE_DEPTH, TEXTURE_FORMAT);
            texture.autoGenerateMips = false;
            texture.filterMode = FilterMode.Point;
            if (_enableAntiAliasing)
            {
                texture.antiAliasing = TEXTURE_ANTIALIASING_FACTOR;
            }

            texture.Create();
            return texture;
        }

        /// <summary>
        /// Perform all scene render initialization here
        /// </summary>
        /// <param name="cam">Camera Source</param>
        private void OnPreRenderEvent(UnityEngine.Camera cam)
        {
            Interop.MetaCompositorInterop.SetWebcamFovDegrees(_contentCamera.fieldOfView);
            // perform all scene render initialization here.  The left eye camera gets rendered first,
            //so simply do all compositor setup if this is the OnPreRender call for the left camera.
            if (cam != _leftCam)
            {
                return;
            }

            // We need to call begin frame from SlamApi when we show the SLAM UI. This is to fix jitter
            // in the SLAM UI when we call begin frame from here since the UI works of an old pose
            if ((Enable3DWarp || Enable2DWarp) && !MetaUIContent.IsUIPresent)
            {
                // If we have disabled warping for the UI we need to enable it if the user has chosen this option
                // since we are no longer displaying the UI
                if (!_warpDisabledForUI)
                {
                    Interop.MetaCompositorInterop.Enable2DWarp(_enable2DWarp ? 1 : 0);
                    Interop.MetaCompositorInterop.Enable3DWarp(_enable3DWarp ? 1 : 0);
                    _warpDisabledForUI = true;
                }
                Interop.MetaCompositorInterop.BeginFrame();
            }
            else
            {
                // Since we are displaying the UI we need to disable any warping that might be on currently
                if (_warpDisabledForUI)
                {
                    Interop.MetaCompositorInterop.Enable2DWarp(0);
                    Interop.MetaCompositorInterop.Enable3DWarp(0);
                    _warpDisabledForUI = false;
                }
            }
            // Update view matrices for the cameras
            UpdateCameraMatrices();
        }

        /// <summary>
        /// Update the Camera Matrices for the Compositor
        /// </summary>
        private void UpdateCameraMatrices()
        {
            //-------------- left eye --------------------
            var viewLeftMatrix = Matrix4x4.identity;
            CompositorApi.GetViewMatrix(0, ref viewLeftMatrix);

            //-------------- right eye --------------------
            var viewRightMatrix = Matrix4x4.identity;
            CompositorApi.GetViewMatrix(1, ref viewRightMatrix);

            //-------------- webcam --------------------
            Matrix4x4 webcamViewMatrix = Matrix4x4.identity;
            CompositorApi.GetWebcamViewMatrix(ref webcamViewMatrix);

            if (transform.parent)
            {
                var worldToLocal = transform.parent.worldToLocalMatrix;

                //set the final view matrix for right eye
                _rightCam.worldToCameraMatrix = viewRightMatrix * worldToLocal;

                //set the final view matrix for left eye
                _leftCam.worldToCameraMatrix = viewLeftMatrix * worldToLocal;

                _contentCamera.worldToCameraMatrix = webcamViewMatrix*worldToLocal;
            }
            else
            {
                //set the final view matrix for right eye
                _rightCam.worldToCameraMatrix = viewRightMatrix;

                //set the final view matrix for left eye
                _leftCam.worldToCameraMatrix = viewLeftMatrix;

                _contentCamera.worldToCameraMatrix = webcamViewMatrix;
            }

            //-------------- left eye --------------------
            var projLeftMatrix = Matrix4x4.identity;
            CompositorApi.GetProjectionMatrix(0, ref projLeftMatrix);

            //set the final proj matrix for left eye
            _leftCam.projectionMatrix = projLeftMatrix;

            //-------------- right eye --------------------
            var projRightMatrix = Matrix4x4.identity;
            CompositorApi.GetProjectionMatrix(1, ref projRightMatrix);

            //set the final proj matrix for right eye
            _rightCam.projectionMatrix = projRightMatrix;
        }

        /// <summary>
        /// Starts the end of frame loop
        /// </summary>
        private void OnEnable()
        {
            if (_endOfFrameLoop != null)
            {
                StopCoroutine(_endOfFrameLoop);
            }

            _endOfFrameLoop = StartCoroutine(CallPluginAtEndOfFrames());
        }

        /// <summary>
        /// Stops the end of frame loop if its running
        /// </summary>
        private void OnDisable()
        {
            if (_endOfFrameLoop != null)
            {
                StopCoroutine(_endOfFrameLoop);
            }
            _endOfFrameLoop = null;
        }

        /// <summary>
        /// End of frame loop
        /// </summary>
        /// <returns>IEnumerator Coroutine</returns>
        private IEnumerator CallPluginAtEndOfFrames()
        {
            // Wait for start
            while (!_started)
            {
                yield return new WaitForEndOfFrame();
            }

            // RenderLoop for compositor
            while (true)
            {
                // Wait until all frame rendering is done
                yield return new WaitForEndOfFrame(); // this waits for all cams

                GL.IssuePluginEvent(Interop.MetaCompositorInterop.GetRenderEventFunc(), 1); //calls EndFrame on Compositor.
            }
        }

        /// <summary>
        /// Shuts down the compositor
        /// </summary>
        private void OnDestroy()
        {
            Interop.MetaCompositorInterop.ShutdownCompositor();
            UnityEngine.Camera.onPreRender -= OnPreRenderEvent;
        }

        private void DebugReload3DWarp()
        {
            Interop.MetaCompositorInterop.DebugReload3DWarp();
        }

        #region Properties
        /// <summary>
        /// Gets or sets the left camera reference
        /// </summary>
        public UnityEngine.Camera LeftCamera
        {
            get
            {
                return _leftCam;
            }
            set
            {
                _leftCam = value;
            }
        }

        /// <summary>
        /// Gets or sets the right camera reference
        /// </summary>
        public UnityEngine.Camera RightCamera
        {
            get
            {
                return _rightCam;
            }
            set
            {
                _rightCam = value;
            }
        }

        /// <summary>
        /// Experimental - Enables or disables AntiAliasing of the render textues.
        /// In order to work properly (So far) 2D Warp, 3D Warp, HandOcclusion and AsynchronousRendering need to be turned off.
        /// </summary>
        public bool EnableAntiAliasing
        {
            get
            {
                return _enableAntiAliasing;
            }
            set
            {
                if (_started)
                {
                    Debug.LogWarning("Property can only be set before Start");
                    return;
                }
                if (_enableAsynchronousRendering || _enable2DWarp || _enable3DWarp || _enableDepthOcclusion)
                {
                    Debug.LogWarning("Cannot enable Anti Aliasing when 2D Warp, 3D Warp, HandOcclusion or AsynchronousRendering are enabled");
                    return;
                }
                _enableAntiAliasing = value;
            }
        }

        /// <summary>
        /// Whether occlusion was enabled at launch time. This is used to restore the previous state after the SLAM initialization is dismissed.
        /// </summary>
        public bool OcclusionEnabledAtStart
        {
            get; private set;
        }

        /// <summary>
        /// Indicate if the compositor is ready to display content
        /// </summary>
        public bool IsReady
        {
            get
            {
                return _started;
            }
        }

        /// <summary>
        /// Enables or disables 2D Warp
        /// </summary>
        public bool Enable2DWarp
        {
            get
            {
                return _enable2DWarp;
            }
            set
            {
                if (Enable3DWarp && value)
                {
                    Enable3DWarp = !value;
                }
                _enable2DWarp = value;
                Interop.MetaCompositorInterop.Enable2DWarp(_enable2DWarp ? 1 : 0);
            }
        }

        /// <summary>
        /// Enables or disables 2D Warp
        /// </summary>
        public bool Enable3DWarp
        {
            get
            {
                return _enable3DWarp;
            }
            set
            {
                if (Enable2DWarp && value)
                {
                    Enable2DWarp = !value;
                }
                _enable3DWarp = value;
                Interop.MetaCompositorInterop.Enable3DWarp(_enable3DWarp ? 1 : 0);
            }
        }

        /// <summary>
        /// Gets or sets the 2D warp Prediction Time
        /// </summary>
        public float Warp2DPredictionTime
        {
            get
            {
                return _trackingPrediction;
            }
            set
            {
                _trackingPrediction = value;
                Interop.MetaCompositorInterop.SetSystemLatency(_trackingPrediction);
            }
        }

        /// <summary>
        /// Indicate to add artificial Latency or not.
        /// </summary>
        public bool DebugPoseLatency
        {
            get
            {
                return _debugAddPoseLatency;
            }
            set
            {
                _debugAddPoseLatency = value;
                Interop.MetaCompositorInterop.DebugEnablePoseLatency(_debugAddPoseLatency);
            }
        }

        /// <summary>
        /// Enables or Disables the 3D Wire Frame in Debug option
        /// </summary>
        public bool DebugEnable3DWarpWireframe
        {
            get
            {
                return _debugEnable3DWarpWireframe;
            }
            set
            {
                _debugEnable3DWarpWireframe = value;
                Interop.MetaCompositorInterop.DebugEnable3DWarpWireframe(_debugEnable3DWarpWireframe);
            }
        }

        /// <summary>
        /// Enables or disables the Asynchronous Rendering Latency Debug option
        /// </summary>
        public bool DebugAsyncRenderingLatency
        {
            get
            {
                return _debugAsyncRenderingLatency;
            }
            set
            {
                _debugAsyncRenderingLatency = value;
                Interop.MetaCompositorInterop.DebugEnableAsyncRenderingLatency(_debugAsyncRenderingLatency);
            }
        }

        /// <summary>
        /// Enable or disable Late Warp
        /// </summary>
        public bool EnableLateWarp
        {
            get
            {
                return _enableLateWarp;
            }
            set
            {
                _enableLateWarp = value;
                Interop.MetaCompositorInterop.EnableLateWarp(_enableLateWarp);
            }
        }


        /// <summary>
        /// Enable or disable Dynamic Late Warp
        /// </summary>
        public bool EnableAsyncDynamicLateWarp
        {
            get
            {
                return _enableAsyncDynamicLateWarp;
            }
            set
            {
                _enableAsyncDynamicLateWarp = value;
                Interop.MetaCompositorInterop.EnableAsyncDynamicLateWarp(_enableAsyncDynamicLateWarp);
            }
        }

        /// <summary>
        /// Enables/Disable hand occlusion
        /// </summary>
        public bool EnableHandOcclusion
        {
            get
            {
                return _enableDepthOcclusion;
            }
            set
            {
                _enableDepthOcclusion = value;
#if UNITY_EDITOR
                if (!UnityEditor.EditorApplication.isPlaying)
                {
                    return;
                }
#endif
                CompositorInterop.EnableHandOcclusion(_enableDepthOcclusion);
            }
        }

        /// <summary>
        /// Sets or gets temporal momentum
        /// </summary>
        public float TemporalMomentum
        {
            get
            {
                return _temporalMomentum;
            }
            set
            {
                _temporalMomentum = value;
                CompositorInterop.SetHandOcclusionTemporalMomentum(_temporalMomentum);
            }
        }

        /// <summary>
        /// Sets or gets feather size
        /// </summary>
        public int FeatherSize
        {
            get
            {
                return _featherSize;
            }
            set
            {
                _featherSize = value;
                CompositorInterop.SetHandOcclusionFeatherSize(_featherSize);
            }
        }

        /// <summary>
        /// Sets or gets feather falloff exponent
        /// </summary>
        public float FeatherFalloffExponent
        {
            get
            {
                return _featherFalloffExponent;
            }
            set
            {
                _featherFalloffExponent = value;
                CompositorInterop.SetHandOcclusionFeatherOpacityFalloff(_featherFalloffExponent);
            }
        }

        /// <summary>
        /// Sets or gets feather cutoff
        /// </summary>
        public float FeatherCutoff
        {
            get
            {
                return _featherCutoff;
            }
            set
            {
                _featherCutoff = value;
                CompositorInterop.SetHandOcclusionFeatherOpacityCutoff(_featherCutoff);
            }
        }

        /// <summary>
        /// Indicate if Async Late Warp was enabled at initialization
        /// </summary>
        public bool IsAsyncLateWarpEnabled
        {
            get
            {
                return _enableAsynchronousRendering;
            }
        }

        /// <summary>
        /// Gets or sets the Late Warp Threshold
        /// </summary>
        public float LateWarpThreshold
        {
            get
            {
                return _lateWarpThreshold;
            }
            set
            {
                _lateWarpThreshold = value;
                Interop.MetaCompositorInterop.SetThresholdForLateWarp(_lateWarpThreshold);
            }
        }

        /// <summary>
        /// Gets or sets the Content Camera of the Rig
        /// </summary>
        public Camera ContentCamera
        {
            get
            {
                return _contentCamera;
            }
            set
            {
                _contentCamera = value;
            }
        }

        /// <summary>
        /// Gets or sets the near clipping plane
        /// </summary>
        public float NearClippingPlane
        {
            get
            {
                return _nearClippingPlane;
            }
            set
            {
                _nearClippingPlane = value;

#if UNITY_EDITOR
                if (!UnityEditor.EditorApplication.isPlaying)
                {
                    return;
                }
#endif

                if (!CompositorInterop.SetCameraNearPlane(_nearClippingPlane))
                {
                    Debug.LogError("Unable to set Compositor clipping plane distances!  Headset may not be connected.");
                }

                _contentCamera.nearClipPlane = _leftCam.nearClipPlane = _rightCam.nearClipPlane = value;
            }
        }

        /// <summary>
        /// Gets or sets the far clipping plane
        /// </summary>
        public float FarClippingPlane
        {
            get
            {
                return _farClippingPlane;
            }
            set
            {
                _farClippingPlane = value;

#if UNITY_EDITOR
                if (!UnityEditor.EditorApplication.isPlaying)
                {
                    return;
                }
#endif

                if (!CompositorInterop.SetCameraFarPlane(_farClippingPlane))
                {
                    Debug.LogError("Unable to set Compositor clipping plane distances!  Headset may not be connected.");
                }

                _contentCamera.farClipPlane = _leftCam.farClipPlane = _rightCam.farClipPlane = value;
            }
        }

        /// <summary>
        /// Shows/Hides the virtual webcam window
        /// </summary>
        public bool WebcamEnabled
        {
            get { return _enableWebcam; }
            set
            {
                _enableWebcam = value;
                _contentCamera.enabled = _enableWebcam;
                Interop.MetaCompositorInterop.EnableWebcam(_enableWebcam);
            }
        }
        #endregion
    }
}
