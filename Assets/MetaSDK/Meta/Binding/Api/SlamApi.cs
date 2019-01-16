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
using SlamInterop = Meta.Interop.SlamInterop;
using CompositorInterop = Meta.Interop.MetaCompositorInterop;
using GameObject = UnityEngine.GameObject;
using Vector3 = UnityEngine.Vector3;
using MetaUIContent = Meta.Rendering.MetaUIContent;


namespace Meta.Plugin
{
    ///<summary>
    /// This is using a legacy API method that requires this class to be instantiated.
    /// TODO: 
    /// </summary>
    /// <remarks>
    /// <para>Notes</para>
    /// </remarks>
    public class SlamApi
    {
        /// <summary>
        /// Transform as vector data buffer.
        /// </summary>
        private double[] _trans = new double[3];
        /// <summary>
        /// Quaternion as vector data buffer.
        /// </summary>
        private double[] _quat = new double[4];

        private bool _rotationOnlyTracking = false;

        private MetaCompositor _compositor;

        /// <summary>
        /// Game object to apply poses to 
        /// </summary>
        private GameObject targetGO = null; // Target game object

        public GameObject TargetGO
        {
            get
            {
                return targetGO;
            }
            set
            {
                targetGO = value;
            }
        }

        /// <summary>
        /// Internal start method that can be used for all specializations of the SLAM localizer.
        /// </summary>
        public SlamApi()
        {
        }

        /// <summary>Internal update method that can be used for all specializations of the SLAM localizer.</summary>
        virtual public void Update(bool fromCompositor)
        {
            UpdateTargetGOTransform(fromCompositor);
        }

        public void GetTrackingStatus(out SlamInterop.TrackingStatus feedback)
        {
            feedback = SlamInterop.GetTrackingStatus();
        }

        public void SaveSlamMap(string mapname)
        {
            SlamInterop.SaveMap(mapname + ".mmf");
        }

        public bool LoadSlamMap(string mapname)
        {
            return SlamInterop.LoadMap(mapname + ".mmf");
        }

        /// <summary> 
        /// Updates the target game object transform.
        /// SLAM Reports its transfrom aligned with gravity on (0,-9.8,0)
        /// At the origin with the initial rotation at Identity
        /// </summary>
        public void UpdateTargetGOTransform(bool getPoseFromCompositor)
        {
            if (getPoseFromCompositor)
            {
                // We need to call begin frame from SlamApi when we show the SLAM UI. This is to fix jitter
                // in the SLAM UI when we call begin frame from MetaCompositor since the UI works of an old pose
                // It is important to note that this adds a large latency since we call begin frame way before than it should be
                if (_compositor == null)
                {
                    _compositor = UnityEngine.GameObject.FindObjectOfType<MetaCompositor>();
                }
                if (_compositor != null && (!(_compositor.Enable3DWarp || _compositor.Enable2DWarp) || MetaUIContent.IsUIPresent))
                {
                    CompositorInterop.BeginFrame();
                }
                // Update pose for behaviors with rendering pose from compositor
                CompositorInterop.GetRenderPoseToWorld(_trans, _quat);
            }

            if (TargetGO != null)
            {
                TargetGO.transform.localRotation = TypeUtilities.QuaternionFromDouble(_quat);

                TargetGO.transform.localPosition
                    = new Vector3(
                        (float)_trans[0],
                        (float)_trans[1],
                        (float)_trans[2]
                    );
            }
        }

        virtual public void ResetLocalizer()
        {
            SlamInterop.ResetSLAM();
        }

        public void ToggleRotationOnlyTracking()
        {
            _rotationOnlyTracking = !_rotationOnlyTracking;
            SlamInterop.EnableRotationOnlyTracking(_rotationOnlyTracking);
        }

        public bool IsRotationOnlyTracking()
        {
            return _rotationOnlyTracking;
        }
    }
}
