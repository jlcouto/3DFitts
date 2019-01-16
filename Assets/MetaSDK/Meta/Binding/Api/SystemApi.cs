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
using Environment = System.Environment;
using File = System.IO.File;
using MetaCoreInterop = Meta.Interop.MetaCoreInterop;
using MetaVariable = Meta.Interop.MetaCoreInterop.MetaVariable;
using InitStatus = Meta.Interop.MetaCoreInterop.InitStatus;
using FrameHands = types.fbs.FrameHands; // Flatbuffers
using Debug = UnityEngine.Debug;
using Transform = UnityEngine.Transform;
using Matrix4x4 = UnityEngine.Matrix4x4;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;
using System;
using System.Collections;


namespace Meta.Plugin
{
    public static class SystemApi
    {
        /// <summary>
        /// Initializes the system (sensors, algorithms, etc). Returns false on failure, true on success.
        /// </summary>
        /// <param name="json_config_file">Configuration file with which to initialize coco</param>
        /// <param name="json_config_file">boolean to specify if we are running in development environment or production
        // environment</param>
        /// <param name="initialize_web_server">Specify weather to initialize stats web server.</param>
        public static bool Start()
        {
            InitStatus result = MetaCoreInterop.meta_start();
            if (result != InitStatus.SUCCESS)
            {
                Debug.LogError("Meta initialization failed with result: " + result);
                return false;
            }
            return true;
        }


        /// <summary>
        /// Coroutine which waits for Meta Configuration to complete
        /// then calls an Action
        /// </summary>
        /// <param name="action">Action to call on Meta Ready</param>
        /// <returns></returns>
        public static IEnumerator MetaReady(Action action)
        {
            MetaCoreInterop.meta_wait_start_complete();
            action();
            yield return null;
        }


        /// <summary>
        /// Stops currently running coco instance.
        /// </summary>
        public static void Stop()
        {
            MetaCoreInterop.meta_stop();
        }


        /// <summary>
        /// Returns latest frame's hands.
        /// </summary>
        /// <param name="buffer">Byte buffer to use for deserialization.</param>
        /// <param name="frameHands">FrameHands datastructure to populate.</param>
        /// <returns></returns>
        public static bool GetFrameHandsFlatbufferObject(ref byte[] buffer, out FrameHands frameHands)
        {
            if (MetaCoreInterop.meta_get_frame_hands(buffer) == 0)
            {
                frameHands = new FrameHands();

                return false;
            }

            var byteBuffer = new FlatBuffers.ByteBuffer(buffer);

            frameHands = FrameHands.GetRootAsFrameHands(byteBuffer);
            return true;
        }


        public static bool GetTransform(MetaCoreInterop.MetaCoordinateFrame destination, MetaCoreInterop.MetaCoordinateFrame source, ref Matrix4x4 matrix)
        {
            MetaCoreInterop.MetaMatrix44 mat = new MetaCoreInterop.MetaMatrix44();
            if (!MetaCoreInterop.meta_get_transform(destination, source, ref mat))
            {
                return false;
            }

            matrix[0, 0] = mat.m00;
			matrix[0, 1] = mat.m01;
			matrix[0, 2] = mat.m02;
			matrix[0, 3] = mat.m03;

			matrix[1, 0] = mat.m10;
			matrix[1, 1] = mat.m11;
			matrix[1, 2] = mat.m12;
			matrix[1, 3] = mat.m13;

			matrix[2, 0] = mat.m20;
			matrix[2, 1] = mat.m21;
			matrix[2, 2] = mat.m22;
			matrix[2, 3] = mat.m23;

			matrix[3, 0] = mat.m30;
			matrix[3, 1] = mat.m31;
			matrix[3, 2] = mat.m32;
			matrix[3, 3] = mat.m33;

            // conver from right to left handed coordinate system
            Matrix4x4 m_right_to_left = Matrix4x4.identity;
            m_right_to_left[1, 1] *= -1;
            matrix = m_right_to_left * matrix * m_right_to_left.inverse;

            return true;
        }


        public static string GetSerialNumber()
        {
            string data = null;
            MetaCoreInterop.meta_get_serial_number(ref data);
            return data;
        }

        /// <summary>
        /// Gets a snapshot of the device status.
        /// </summary>
        /// <returns>The device status snapshot</returns>
        public static DeviceStatusSnapshot GetDeviceStatus()
        {
            int deviceStatus, connectionStatus, streamingStatus = 0;
            MetaCoreInterop.meta_get_device_status(out deviceStatus, out connectionStatus, out streamingStatus);
            return new DeviceStatusSnapshot((DeviceStatusSnapshot.DeviceStatus)deviceStatus, 
                                            (DeviceStatusSnapshot.ConnectionStatus)connectionStatus, 
                                            streamingStatus);
        }

        /// <summary>
        /// Applies latest head pose, if available to referenced transform
        /// </summary>
        /// <param name="transformToApply">Transform to apply head pose to.</param>
        public static void ApplyHeadPose(ref Transform transformToApply)
        {
            var pose = MetaCoreInterop.meta_get_latest_head_pose();

            transformToApply.localPosition = new Vector3(pose.position.x,
                                                          pose.position.y,
                                                          pose.position.z);

            transformToApply.localRotation = new Quaternion(pose.rotation.x,
                                                             pose.rotation.y,
                                                             pose.rotation.z,
                                                             pose.rotation.w);
        }


        /// <summary>
        /// Updated a coco attribute.
        /// </summary>
        /// <param name="blockName">Name of block to update.</param>
        /// <param name="attributeName">Name of paramiter to update.</param>
        /// <param name="attributeValue">Target string value for specified attribute.</param>
        /// <returns></returns>
        public static bool SetAttribute(string blockName, string attributeName, string attributeValue)
        {
            if (!MetaCoreInterop.meta_update_attribute(blockName, attributeName, attributeValue))
            {
                Debug.Log("Failed to update attribute: " + blockName + " " + attributeName);
                return false;
            }

            return true;
        }


        internal static void ToggleDebugDrawing(bool targetState)
        {
            var targetStateString = targetState ? "true" : "false";
            var attribute = "draw";

            MetaCoreInterop.meta_update_attribute("HandsDataPreprocessingBlock", attribute, targetStateString);
            MetaCoreInterop.meta_update_attribute("HandSegmentationBlock", attribute, targetStateString);
            MetaCoreInterop.meta_update_attribute("HandTrackingBlock", attribute, targetStateString);
            MetaCoreInterop.meta_update_attribute("HandFeatureExtractionBlock", attribute, targetStateString);
        }


        public static bool GetPath(MetaVariable variable, out string result)
        {
            return MetaCoreInterop.get_meta_variable(variable, out result);
        }
    }
}
