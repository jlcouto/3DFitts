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

namespace Meta.Rendering
{
    /// <summary>
    /// Component that indicates that the object attached to is a hudlocked or headlocked UI.
    /// This tells the MetaCompositor that it should disable 3D warp as long as this script is enabled.
    /// </summary>
    public class MetaUIContent : MonoBehaviour
    {
        /// <summary>
        /// Count the UI elements in display
        /// </summary>
        private static int _uiCount = 0;
        /// <summary>
        /// Mutex object for this class
        /// </summary>
        private static object _classLock = new object();

        private bool _isEnabled = false;

        /// <summary>
        /// Raises the flag that indicates that a Headlocked UI is displaying
        /// </summary>
        /// <param name="raise">Raise the flag if true, lower the flag if false</param>
        public void RaiseUIFlag(bool raise)
        {
            if (raise && _isEnabled)
            {
                return;
            }
            if (!raise && !_isEnabled)
            {
                return;
            }

            lock (_classLock)
            {
                if (!_isEnabled)
                {
                    ++_uiCount;
                    _isEnabled = true;
                }
                else
                {
                    --_uiCount;
                    _isEnabled = false;
                }
            }
        }

        /// <summary>
        /// Add one to the UI Count
        /// </summary>
        private void OnEnable()
        {
            RaiseUIFlag(true);
        }

        /// <summary>
        /// Rest one to the UI Count
        /// </summary>
        private void OnDisable()
        {
            RaiseUIFlag(false);
        }

        /// <summary>
        /// Indicate the number of headlocked UI elements in display now.
        /// </summary>
        public static int CurrentUICount
        {
            get
            {
                return _uiCount;
            }
        }

        /// <summary>
        /// Indicate if there is any headlocked UI present
        /// </summary>
        public static bool IsUIPresent
        {
            get
            {
                return _uiCount > 0;
            }
        }
    }
}
