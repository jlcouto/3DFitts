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
using System.Runtime.InteropServices;
using IntPtr = System.IntPtr;

namespace Meta.Interop
{
    public static class MetaCompositorInterop
    {
        /// <summary>
        /// Enable/disable last minute warp in the compositor.  This is for debugging latewarp.
        /// </summary>
        /// <param name="enableLateWarp">bool for enable/disable</param>
        [DllImport(DllReferences.MetaUnity, EntryPoint = "EnableLateWarp")]
        public static extern void EnableLateWarp(bool enableLateWarp);

        /// <summary>
        /// Enable/disable last minute warp in the compositor.  This is for debugging latewarp.
        /// </summary>
        /// <param name="enableLateWarp">bool for enable/disable</param>
        [DllImport(DllReferences.MetaUnity, EntryPoint = "EnableAsyncDynamicLatewarp")]
        public static extern void EnableAsyncDynamicLateWarp(bool enableDynamicLatewarp);

        /// <summary>
        /// Late warp threshold within the compositor.  This is for debugging the latewarp threshold.
        /// </summary>
        /// <param name="lateWarpThreshold">float for threshold value</param>
        [DllImport(DllReferences.MetaUnity, EntryPoint = "SetThresholdForLateWarp")]
        public static extern void SetThresholdForLateWarp(float lateWarpThreshold);

        /// <summary>
        /// Initialize the compositor
        /// The compositor will determine whether direct mode is enabled or not.
        /// Enabling the AsyncLateWarp creates a separate thread for post processing (Unwarping)
        /// meanwhile allowing the Unity render thread to continue Rendering asynchronously.
        /// </summary>
        /// <param name="enableAsyncLateWarp">bool for enable/disable</param>
        [DllImport(DllReferences.MetaUnity, EntryPoint = "InitCompositor")]
        public static extern void InitCompositor(bool enableAsyncLateWarp);

        /// <summary>
        /// Shutdown compositor
        /// The plugin handles this itself, not sure if this is needed
        /// </summary>
        [DllImport(DllReferences.MetaUnity, EntryPoint = "ShutdownCompositor")]
        public static extern void ShutdownCompositor();

        /// <summary>
        /// Enable/disable 2D warp within the compositor
        /// </summary>
        /// <param name="enable2Dwarp">bool for enable/disable</param>
        [DllImport(DllReferences.MetaUnity, EntryPoint = "Enable2DWarp")]
        public static extern void Enable2DWarp(int enable2Dwarp);


        /// <summary>
        /// Enable/disable 3D Warp within the compositor
        /// </summary>
        /// <param name="enable3DWarp">bool for enable/disable</param>
        [DllImport(DllReferences.MetaUnity, EntryPoint = "Enable3DWarp")]
        public static extern void Enable3DWarp(int enable3DWarp);


        /// <summary>
        /// Reloads 3D warp shaders on the fly for debugging purposes.
        /// </summary>
        [DllImport(DllReferences.MetaUnity, EntryPoint = "DebugReload3DWarp")]
        public static extern void DebugReload3DWarp();

        
        /// <summary>
        /// Enable/disable hand occlusion in compositor
        /// </summary>
        /// <param name="enablehandocclusion">bool for enable/disable</param>
        [DllImport(DllReferences.MetaUnity, EntryPoint = "EnableHandOcclusion")]
        public static extern void EnableHandOcclusion(bool state);


        /// <summary>
        /// Sets temporal filtering momentum of hand occlusion
        /// </summary>
        /// <param name="enablehandocclusion">float for setting temporal filtering momentum of hand occlusion</param>
        [DllImport(DllReferences.MetaUnity, EntryPoint = "SetHandOcclusionTemporalMomentum")]
        public static extern void SetHandOcclusionTemporalMomentum(float momentum);


        /// <summary>
        /// Sets temporal filtering momentum of hand occlusion
        /// </summary>
        /// <param name="enablehandocclusion">float for setting temporal filtering momentum of hand occlusion</param>
        [DllImport(DllReferences.MetaUnity, EntryPoint = "SetHandOcclusionFeatherSize")]
        public static extern void SetHandOcclusionFeatherSize(int size);


        /// <summary>
        /// Sets how fast the opacity of the hand occlusion falls off
        /// </summary>
        /// <param name="enablehandocclusion">float; sets how fast the opacity of the hand occlusion falls off</param>
        [DllImport(DllReferences.MetaUnity, EntryPoint = "SetHandOcclusionFeatherOpacityFalloff")]
        public static extern void SetHandOcclusionFeatherOpacityFalloff(float exponent);

        /// <summary>
        /// Sets a opacity threshold of handocclusion below which pixels would be thrown out.
        /// </summary>
        /// <param name="sethandocclusionfeatheropacitycutoff">float; sets a opacity threshold of handocclusion below which pixels would be thrown out.</param>
        [DllImport(DllReferences.MetaUnity, EntryPoint = "SetHandOcclusionFeatherOpacityCutoff")]
        public static extern void SetHandOcclusionFeatherOpacityCutoff(float exponent);


        /// <summary>
        /// Set the amount of prediction used for 2D warp correction at end of frame.
        /// </summary>
        /// <param name="dt">time in seconds</param>
        [DllImport(DllReferences.MetaUnity, EntryPoint = "SetSystemLatency")]
        public static extern void SetSystemLatency(float dt);

        /// <summary>
        /// Enable/disable the Virtual Webcam.
        /// </summary>
        /// <param name="enable">bool for enable/disable</param>
        [DllImport(DllReferences.MetaUnity, EntryPoint = "EnableWebcam")]
        public static extern void EnableWebcam(bool enable);

        /// <summary>
        /// Enable/disable the Virtual Webcam.
        /// </summary>
        /// <param name="enable">bool for enable/disable</param>
        [DllImport(DllReferences.MetaUnity, EntryPoint = "SetWebcamFovDegrees")]
        public static extern void SetWebcamFovDegrees(float fovDegrees);

        [DllImport(DllReferences.MetaUnity, EntryPoint = "GetEyeRenderTargetWidth")]
        public static extern int GetEyeRenderTargetWidth();

        [DllImport(DllReferences.MetaUnity, EntryPoint = "GetEyeRenderTargetHeight")]
        public static extern int GetEyeRenderTargetHeight();


        /// <summary>
        /// Set the render targets and depth buffers from an external engine
        /// </summary>
        /// <param name="leftRT"></param>
        /// <param name="rightRT"></param>
        /// <param name="leftEyeDepth"></param>
        /// <param name="rightEyeDepth"></param>
        [DllImport(DllReferences.MetaUnity, EntryPoint = "SetEyeRenderTargets")]
        public static extern void SetEyeRenderTargets(
            IntPtr leftRT, IntPtr rightRT, IntPtr leftEyeDepth, IntPtr rightEyeDepth);

        /// <summary>
        /// Set the eye calibration data during runtime.
        /// </summary>
        /// <param name="leftEyeUnWarpTexture">Left eye unwarp texture</param>
        /// <param name="rightEyeUnWarpTexture">Right eye unwarp texture</param>
        /// <param name="leftEyeUnWarpMaskTexture">Left eye unwarp mask texture</param>
        /// <param name="rightEyeUnWarpMaskTexture">Right eye unwarp mask texture</param>
        /// <param name="leftEyeOffset">Left eye position</param>
        /// <param name="rightEyeOffset">Right eye position</param>
        [DllImport(DllReferences.MetaUnity, EntryPoint = "SetEyeCalibrationData")]
        public static extern void SetEyeCalibrationData(
            IntPtr leftEyeUnWarpTexture, IntPtr rightEyeUnWarpTexture,
            IntPtr leftEyeUnWarpMaskTexture, IntPtr rightEyeUnWarpMaskTexture,
            [MarshalAs(UnmanagedType.LPArray, SizeConst = 3)] float[] leftEyeOffset,
            [MarshalAs(UnmanagedType.LPArray, SizeConst = 3)] float[] rightEyeOffset
            );

        /// <summary>
        /// Call this prior to rendering a frame
        /// This sets up the pose for 2D warp to calculate delta
        /// </summary>
        /// <returns></returns>
        [DllImport(DllReferences.MetaUnity, EntryPoint = "BeginFrame")]
        public static extern void BeginFrame();

        /// <summary>
        /// Render function
        /// Calls EndFrame within the compositor, renders whatever is in the target texture
        /// </summary>
        /// <returns>Render function</returns>
        [DllImport(DllReferences.MetaUnity, EntryPoint = "GetRenderEventFunc")]
        public static extern IntPtr GetRenderEventFunc();

        /// <summary>
        /// Get view matrix for camera
        /// </summary>
        /// <returns>Render function</returns>
        [DllImport(DllReferences.MetaUnity, EntryPoint = "GetViewMatrix")]
        public static extern void getViewMatrix(int eye, ref IntPtr ptrResultViewMatrix);

        /// <summary>
        /// Get SLAM pose used for this frames rendering
        /// This will return the pose set in begin frame
        /// (Used in MetaSlamInterop)
        /// </summary>
        /// <param name="translation"></param>
        /// <param name="rotation"></param>
        [DllImport(DllReferences.MetaUnity, EntryPoint = "GetRenderPoseToWorld")]
        public static extern void GetRenderPoseToWorld([MarshalAs(UnmanagedType.LPArray, SizeConst = 3)] double[] translation,
                                                 [MarshalAs(UnmanagedType.LPArray, SizeConst = 4)] double[] rotation);

        /// <summary>
        /// Get projection matrix for camera
        /// </summary>
        /// <returns>Render function</returns>
        [DllImport(DllReferences.MetaUnity, EntryPoint = "GetProjectionMatrix")]
        public static extern void getProjectionMatrix(int eye, ref IntPtr ptrResultProjectionMatrix);

        /// <summary>
        /// Enable/disable latency addition within the compositor.  This is for debugging 2D warp.
        /// </summary>
        /// <param name="enableposelatency">bool for enable/disable</param>
        [DllImport(DllReferences.MetaUnity, EntryPoint = "DebugEnablePoseLatency")]
        public static extern void DebugEnablePoseLatency(bool enableLatency);

        /// <summary>
        /// Enable / Disable Rate limiting on the async application rendering thread.
        /// </summary>
        /// <param name="enableposelatency">bool for enable/disable</param>
        [DllImport(DllReferences.MetaUnity, EntryPoint = "EnableRateLimitAsyncRendering")]
        public static extern void EnableRateLimitAsyncRendering(bool enable);
        
        /// <summary>
        /// Enable/disable latency addition within the compositor.  This is for debugging 2D warp.
        /// </summary>
        /// <param name="enableposelatency">bool for enable/disable</param>
        [DllImport(DllReferences.MetaUnity, EntryPoint = "DebugEnableAsyncRenderingLatency")]
        public static extern void DebugEnableAsyncRenderingLatency(bool enableLatency);
        
        /// <summary>
        /// Enable/disable 3D warp's wireframe within the compositor.
        /// </summary>
        /// <param name="enable3dwarpwireframe">bool for enable/disable</param>
        [DllImport(DllReferences.MetaUnity, EntryPoint = "DebugEnable3DWarpWireframe")]
        public static extern void DebugEnable3DWarpWireframe(bool enable3dwarpwireframe);

        /// <summary>
        /// Set camera near plane distance setting within the compositor.
        /// </summary>
        /// <param name="nearPlane">Near plane distance</param>
        /// <returns>Whether the operation succeeded</returns>
        [DllImport(DllReferences.MetaUnity, EntryPoint = "SetCameraNearPlane")]
        public static extern bool SetCameraNearPlane(float nearPlane);

        /// <summary>
        /// Set camera far plane distance setting within the compositor.
        /// </summary>
        /// <param name="farPlane">Far plane distance</param>
        /// <returns>Whether the operation succeeded</returns>
        [DllImport(DllReferences.MetaUnity, EntryPoint = "SetCameraFarPlane")]
        public static extern bool SetCameraFarPlane(float farPlane);

        /// <summary>
        /// Get view matrix for camera
        /// </summary>
        /// <returns>Render function</returns>
        [DllImport(DllReferences.MetaUnity, EntryPoint = "GetWebcamViewMatrix")]
        public static extern void getWebcamViewMatrix(ref IntPtr ptrResultViewMatrix);


        /// <summary>
        /// Set the webcam render target from an external engine
        /// </summary>
        /// <param name="texturePtr"></param>
        [DllImport(DllReferences.MetaUnity, EntryPoint = "SetWebcamHologramTargetTexture")]
        public static extern void SetWebcamRenderTarget(
            IntPtr texturePtr, IntPtr depthTexturePtr);
    }
}
