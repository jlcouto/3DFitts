// Copyright © 2018, Meta Company.  All rights reserved.
// 
// Redistribution and use of this software (the "Software") in source and binary forms, with or 
// without modification, is permitted provided that the following conditions are met:
// 
// 1.      Redistributions in source code must retain the above copyright notice, this list of 
//         conditions and the following disclaimer.
// 2.      Redistributions in binary form must reproduce the above copyright notice, this list of 
//         conditions and the following disclaimer in the documentation and/or other materials 
//         provided with the distribution.
// 3.      The name of Meta Company (“Meta”) may not be used to endorse or promote products derived 
//         from this software without specific prior written permission from Meta.
// 4.      LIMITATION TO META PLATFORM: Use of the Software and of any and all libraries (or other 
//         software) incorporating the Software (in source or binary form) is limited to use on or 
//         in connection with Meta-branded devices or Meta-branded software development kits.  For 
//         example, a bona fide recipient of the Software may modify and incorporate the Software 
//         into an application limited to use on or in connection with a Meta-branded device, while 
//         he or she may not incorporate the Software into an application designed or offered for use 
//         on a non-Meta-branded device.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDER "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, 
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A 
// PARTICULAR PURPOSE ARE DISCLAIMED.  IN NO EVENT SHALL META COMPANY BE LIABLE FOR ANY DIRECT, 
// INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED 
// TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA OR PROFITS; OR BUSINESS 
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT 
// LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS 
// SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
using UnityEngine;
using Meta.HandInput.SwipeInteraction;

namespace Meta.Examples
{
    public class SwipeIntensityLightControl : MonoBehaviour
    {
        [SerializeField]
        private SwipeIntensityInteraction _swipeIntensity;

        [SerializeField]
        private Renderer _targetRenderer;

        private Color _maxColor = new Color(1f, 0.3f, 0.3f);

        private Color _minColor = new Color(0.3f, 0.3f, 1f);

        private Color _neutralColor = new Color(0.8f, 0.8f, 0.8f);

        private void Update()
        {
            if (_swipeIntensity.SwipeOn)
            {
                if (_swipeIntensity.Intensity < 0f)
                {
                    _targetRenderer.material.color = Color.Lerp(_neutralColor, _minColor, -_swipeIntensity.Intensity);
                }
                else
                {
                    _targetRenderer.material.color = Color.Lerp(_neutralColor, _maxColor, _swipeIntensity.Intensity);
                }
            }
        }
    }
}
