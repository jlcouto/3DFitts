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
using Meta.Plugin;
using Meta.Utility;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
namespace Meta
{
    internal class WebcamOffCanvasHandler : MonoBehaviour
    {
        [SerializeField]
        private GameObject _logoCanvasPrefab;

        private bool _isSlamSensorReady = false;
        private MetaSensorMessageController _sensorController;
        private SlamLocalizer _slamLocalizer;
        private MetaCompositor _compositor;
        private MetaLogoCanvasController _controller;
        private Text _slamContent;

        /// <summary>
        /// Coroutine for setting the title message.
        /// </summary>
        /// <returns></returns>
        private IEnumerator SetMessage()
        {
            // Loop until we're notified that the sensors are ready.
            while (!_isSlamSensorReady)
            {
                SetSlamMessage();
                yield return null;
            }

            SetVirtualWebcamMessage();
        }

        /// <summary>
        /// Handles assignment/removal of the canvas shown in place of the Webcam.
        /// </summary>
        /// <param name="changedToMode"></param>
        private void Start()
        {
            _sensorController = GetComponentInChildren<MetaSensorMessageController>();

            _slamLocalizer = GetComponent<SlamLocalizer>();
            _slamLocalizer.onSlamSensorsReady.AddListener(SlamInitCallback);

            Screen.SetResolution(1280, 720, false);
            _controller = ConstructCanvas();

            _compositor = GetComponent<MetaCompositor>();

            StartCoroutine(SetMessage());
        }

        /// <summary>
        /// Call back to get the state of slam.
        /// </summary>
        private void SlamInitCallback()
        {
            _isSlamSensorReady = true;
        }

        private MetaLogoCanvasController ConstructCanvas()
        {
            if(_logoCanvasPrefab == null)
            {
                GameObject UiResourceRef = (GameObject)Resources.Load("Prefabs/MetaLogoCanvas");
                var go = GameObject.Instantiate(UiResourceRef);
                return go.GetComponent<MetaLogoCanvasController>();
            }
            else
            {
                var go = GameObject.Instantiate(_logoCanvasPrefab);
                return go.GetComponent<MetaLogoCanvasController>();
            }

        }

        /// <summary>
        /// Set the title text to messages about the virtual webcam. 
        /// </summary>
        private void SetVirtualWebcamMessage()
        {
            if (_compositor && _compositor.WebcamEnabled)
            {
                _controller.SetMessage("The Virtual Webcam has been enabled\nand is visible in a separate window.");

            }
            else
            {
                if (Application.isEditor)
                {
                    _controller.SetMessage("Enable the Virtual Webcam on the MetaCameraRig\nto show the Virtual Webcam window.");
                }
            }
        }

        /// <summary>
        /// Set the title text to the current slam message.
        /// </summary>
        private void SetSlamMessage()
        {
            if (_sensorController == null)
            {
                _sensorController = GetComponentInChildren<MetaSensorMessageController>();
            }
            else
            {
                if (_slamContent == null)
                {
                    _slamContent = _sensorController.GetComponentInChildren<Text>();
                }
                else
                {
                    _controller.SetMessage(_slamContent.text);
                }

            }
        }


    }
}
