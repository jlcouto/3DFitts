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
using Quaternion = UnityEngine.Quaternion;

namespace Meta
{
    public static class TypeUtilities
    {
        /// <summary>
        /// Convertion util. 
        /// From: types.fbs.Vec3T (flatbuffers type)
        /// To: UnityEngine.Vector3
        /// </summary>
        /// <param name="vec">types.fbs.Vec3T (flatbuffers type) input </param>
        /// <returns>Converted UnityEngine.Vector3</returns>
        public static Vector3 ToVector3(this types.fbs.Vector3f vec)
        {
            return new Vector3((float)vec.X, (float)vec.Y, (float)vec.Z);
        }

        /// <summary>
        /// Convertion util. 
        /// From: types.fbs.Quaternion (flatbuffers type)
        /// To: UnityEngine.Quaternion
        /// </summary>
        /// <param name="vec">types.fbs.Quaternion (flatbuffers type) input </param>
        /// <returns>Converted UnityEngine.Quaternion</returns>
        public static Quaternion ToQuaternion(this types.fbs.Quaterniond quat)
        {
            return new Quaternion((float)quat.X, (float)quat.Y, (float)quat.Z, (float)quat.W);
        }

        /// <summary>
        /// Convert specified array of 4 to quaternion.
        /// </summary>
        /// <param name="iQuat">array of 4 doubles.</param>
        /// <returns>quaternion.</returns>
        public static Quaternion QuaternionFromDouble(double[] iQuat)
        {
            Quaternion ret;
            ret.x = (float)iQuat[0];
            ret.y = (float)iQuat[1];
            ret.z = (float)iQuat[2];
            ret.w = (float)iQuat[3];
            return ret;
        }

        /// <summary> Float to vector 3.</summary>
        ///
        /// <param name="data">   The data.</param>
        /// <param name="vector"> The vector.</param>
        public static void FloatToVector3(float[] data, ref Vector3 vector)
        {
            vector.Set(data[0], data[1], data[2]);
        }

        /// <summary> Float to vector 3.</summary>
        ///
        /// <param name="data"> The data.</param>
        ///
        /// <returns> A Vector3.</returns>
        public static Vector3 FloatToVector3(float[] data)
        {
            return new Vector3(data[0], data[1], data[2]);
        }

        public static void FloatToVector2(float[] data, ref Vector2 vector)
        {
            vector.Set(data[0], data[1]);
        }


        public static Vector2 FloatToVector2(float[] data)
        {
            return new Vector2(data[0], data[1]);
        }

        public static bool CppBoolToCsBool(byte val)
        {
            return val > 0;
        }

        public static byte CsBoolToCppBool(bool val)
        {
            return val ? (byte)1 : (byte)0;
        }

        
        public static Matrix4x4 MatrixFromArray(double[] vals)
        {
            var poseMat = new Matrix4x4();
            if (vals != null && vals.Length >= 12)
            {
                poseMat.SetRow(0, new Vector4((float) vals[0], (float) vals[1], (float) vals[2], (float) vals[3]));
                poseMat.SetRow(1, new Vector4((float) vals[4], (float) vals[5], (float) vals[6], (float) vals[7]));
                poseMat.SetRow(2, new Vector4((float) vals[8], (float) vals[9], (float) vals[10], (float) vals[11]));
                poseMat.SetRow(3, new Vector4(0, 0, 0, 1));
            }
            else
            {
                Debug.LogError(String.Format("CalibrationParameters.MatrixFromArray: the array '{0}' was insufficient for a matrix4x4.", vals));
            }

            return poseMat;
        }

        /// <summary>
        /// Convert specified array of 4 to quaternion.
        /// </summary>
        /// <param name="iQuat">array of 4 doubles.</param>
        /// <returns>quaternion.</returns>
        public static Quaternion FromDouble(double[] iQuat)
        {
            Quaternion ret;
            ret.x = (float)iQuat[0];
            ret.y = (float)iQuat[1];
            ret.z = (float)iQuat[2];
            ret.w = (float)iQuat[3];
            return ret;
        }
    }
}
