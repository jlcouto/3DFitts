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
using Meta.Interop;
using Texture = UnityEngine.Texture2D;
using TextureFormat = UnityEngine.TextureFormat;
using Marshal = System.Runtime.InteropServices.Marshal;
using IntPtr = System.IntPtr;
using MetaCoreInterop = Meta.Interop.MetaCoreInterop;

namespace Meta.Plugin
{
    /// <summary>
    /// Provides internal functions for accessing raw data from some cameras. 
    /// This API is not intended for public use.
    /// </summary>
    internal static class CameraApi
    {
        // Constants
        private const int TextureWidth = 1280;
        private const int TextureHeight = 720;
        private const TextureFormat TextureFormat = UnityEngine.TextureFormat.RGB24;
        private const int BitsPerPixel = 32;
        private const bool EnableMipmap = false;

        // Data
        private static readonly Texture _rgbTexture = null;
        private static readonly IntPtr RawPixelBuffer;
        private static readonly int _totalBufferSize = 0;

        // Virtual camera pose correction
        public static double[] _translation = new double[3];
        public static double[] _rotation = new double[4];
        private static double[] _new_rotation = new double[4];

        public static Texture GetRgbFrame()
        {
            return _rgbTexture;
        }

        /// <summary>
        /// Returns false if error.
        /// </summary>
        /// <returns></returns>
        public static void UpdateRgbFrame()
        {
            MetaCoreInterop.meta_get_rgb_frame( RawPixelBuffer, _translation, _new_rotation);  // The buffer is pre-allocated by constructor.

            // Check for a difference
            bool isEqual = true;

            // Check for a difference in pose (should change with each new RGB frame).
            for(int i = 0; i < _new_rotation.Length; ++i)
            {
                isEqual = _rotation[i] == _new_rotation[i];

                if (!isEqual) break;
            }

            // If the two rotations are not equal, we have a new rgb frame. 
            if (!isEqual)
            {
                // Copy new rotation if it's different.
                for (int i = 0; i < _new_rotation.Length; ++i)
                {
                    _rotation[i] = _new_rotation[i];
                }

                _rgbTexture.LoadRawTextureData(RawPixelBuffer, _totalBufferSize);
                _rgbTexture.Apply();
            }
        }

        static CameraApi()
        {
            _totalBufferSize = TextureWidth * TextureHeight * ( BitsPerPixel / 8 );
            RawPixelBuffer = Marshal.AllocHGlobal( _totalBufferSize );
            _rgbTexture = new Texture( TextureWidth, TextureHeight, TextureFormat, EnableMipmap );
        }
    }
}
