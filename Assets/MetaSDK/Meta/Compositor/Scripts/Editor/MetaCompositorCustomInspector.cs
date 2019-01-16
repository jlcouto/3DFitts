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
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Meta
{
    /// <summary>
    /// Custom Inspector for Meta Compositor
    /// </summary>
    [CustomEditor(typeof(Plugin.MetaCompositor))]
    public class MetaCompositorCustomInspector : Editor
    {
        private Plugin.MetaCompositor _component;
        private Dictionary<string, SerializedProperty> _fields = new Dictionary<string, SerializedProperty>();

        /// <summary>
        /// Overrides the default Inspector GUI
        /// </summary>
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            CheckComponent();

            // Cameras
            DrawMember("_leftCam");
            DrawMember("_rightCam");

            DrawEnableWebcam();

            // Clipping Planes
            DrawNearClippingPlane();
            DrawFarClippingPlane();
            MaybeDisplayClippingPlanesWarningAndResetButton();

            DrawEnableHandOcclusion();

            DisplayDirectModeMessage();

            // Draw Warp Menus
            DrawEnable2DWarp();
            DrawEnable3DWarp();

            //Common warp related attributes
            DrawWarpPredictionTime();                      

            // Asynchronous Rendering
            DrawEnableAsynchronousRendering();

            // Do not apply changes to serialized fields if we are in Play Mode
            if (Application.isPlaying)
                return;

            // Save and update
            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();
        }

        private void DrawEnableWebcam()
        {
            var property = GetProperty("_enableWebcam");
            bool oldValue = property.boolValue;
            EditorGUILayout.PropertyField(property);
            bool newValue = property.boolValue;

            if (!Application.isPlaying)
                return;

            if (oldValue != newValue)
            {
                _component.WebcamEnabled = newValue;
            }
        }

        #region Warps
        /// <summary>
        /// Draw the Enable 2D Warp field
        /// </summary>
        private void DrawEnable2DWarp()
        {
            var warp2DEnabled = GetProperty("_enable2DWarp");
            var oldValue = _component.Enable2DWarp;

            var warp3DEnabled = GetProperty("_enable3DWarp");

            if (warp3DEnabled.boolValue)
            {
                warp3DEnabled.boolValue = true;
                warp2DEnabled.boolValue = false;
            }

            EditorGUILayout.PropertyField(warp2DEnabled);

            if (!Application.isPlaying)
                return;

            // Check for new value
            var newValue = warp2DEnabled.boolValue;
            if (oldValue != newValue)
                _component.Enable2DWarp = newValue;
        }


        private void DrawEnable3DWarp()
        {
            var warp3DEnabled = GetProperty("_enable3DWarp");
            var oldValue = _component.Enable3DWarp;

            var warp2DEnabled = GetProperty("_enable2DWarp");

            if (warp2DEnabled.boolValue)
            {
                warp3DEnabled.boolValue = false;
                warp2DEnabled.boolValue = true;
            }

            EditorGUILayout.PropertyField(warp3DEnabled);

            if (!Application.isPlaying)
                return;

            // Check for new value
            var newValue = warp3DEnabled.boolValue;
            if (oldValue != newValue)
            {
                _component.Enable3DWarp = newValue;
            }
        }


        /// <summary>
        /// Draw the Warp Prediction Time field
        /// </summary>
        private void DrawWarpPredictionTime()
        {
            var property = GetProperty("_trackingPrediction");
            var oldValue = _component.Warp2DPredictionTime;

            EditorGUILayout.PropertyField(property);

            if (!Application.isPlaying)
                return;

            // Check for new value
            var newValue = property.floatValue;
            if (oldValue != newValue)
                _component.Warp2DPredictionTime = newValue;
        }
        #endregion

        #region AsynchronousRendering
        /// <summary>
        /// Draw the Enable Async Late Warp field
        /// </summary>
        private void DrawEnableAsynchronousRendering()
        {
            var enableAsync = GetProperty("_enableAsynchronousRendering");
            if (!Application.isPlaying)
            {
                EditorGUILayout.PropertyField(enableAsync);
            }
            else
            {
                var content = new GUIContent("Enable Async Rendering", "Async Rendering should be set before start of scene");
                EditorGUILayout.Toggle(content, enableAsync.boolValue);
            }
        }

        #endregion

        #region HandOcclusion
        /// <summary>
        /// Draw field for enabling hand occlusion
        /// </summary>
        private void DrawEnableHandOcclusion()
        {
            var property = GetProperty("_enableDepthOcclusion");
            var oldValue = _component.EnableHandOcclusion;

            EditorGUILayout.PropertyField(property);

            if (!Application.isPlaying)
                return;

            // Check for new value
            var newValue = property.boolValue;
            if (oldValue != newValue)
                _component.EnableHandOcclusion = newValue;
        }
        #endregion

        #region Clipping Planes
        /// <summary>
        /// Draw the near clipping plane field
        /// </summary>
        private void DrawNearClippingPlane()
        {
            var property = GetProperty("_nearClippingPlane");
            var oldValue = _component.NearClippingPlane;

            EditorGUILayout.PropertyField(property);

            if (!Application.isPlaying)
                return;

            // Check for new value
            var newValue = property.floatValue;
            if (oldValue != newValue)
                _component.NearClippingPlane = newValue;
        }

        /// <summary>
        /// Draw the far clipping plane field
        /// </summary>
        private void DrawFarClippingPlane()
        {
            var property = GetProperty("_farClippingPlane");
            var oldValue = _component.FarClippingPlane;

            EditorGUILayout.PropertyField(property);

            if (!Application.isPlaying)
                return;

            // Check for new value
            var newValue = property.floatValue;
            if (oldValue != newValue)
                _component.FarClippingPlane = newValue;
        }

        private void MaybeDisplayClippingPlanesWarningAndResetButton()
        {
            if (_component.NearClippingPlane != Plugin.MetaCompositor.DefaultNearClippingPlane ||
                _component.FarClippingPlane != Plugin.MetaCompositor.DefaultFarClippingPlane)
            {
                EditorGUILayout.HelpBox("Default clipping plane settings overriden. Visual artifacts may occur due to the focal length of the Meta 2's optics.",
                    MessageType.Warning);

                if (GUILayout.Button("Reset Default Clipping Planes"))
                {
                    _component.NearClippingPlane = Plugin.MetaCompositor.DefaultNearClippingPlane;
                    _component.FarClippingPlane = Plugin.MetaCompositor.DefaultFarClippingPlane;
                }
            }
        }
        #endregion

        #region Utility
        /// <summary>
        /// Check that we have a reference of the Compositor script
        /// </summary>
        private void CheckComponent()
        {
            if (_component != null)
                return;
            _component = target as Plugin.MetaCompositor;
        }

        /// <summary>
        /// Displays an Information Messager regarding Extended modes limited support with stabilization warps.
        /// </summary>
        private void DisplayDirectModeMessage()
        {
            EditorGUILayout.HelpBox("The following features have limited support in Extended Mode", MessageType.Info);
        }

        /// <summary>
        /// Draw the given member of the class in the inspector
        /// </summary>
        /// <param name="name">Member Name</param>
        private void DrawMember(string name)
        {
            SerializedProperty field = GetProperty(name);
            // Nothing?
            if (field == null)
                return;

            EditorGUILayout.PropertyField(field);
        }

        /// <summary>
        /// Get the given Property by name
        /// </summary>
        /// <param name="name">Property name</param>
        /// <returns>Serialized Property</returns>
        private SerializedProperty GetProperty(string name)
        {
            // Look for the property
            if (!_fields.ContainsKey(name))
            {
                var field = serializedObject.FindProperty(name);
                _fields.Add(name, field);
            }

            // Look for the property
            return _fields[name];
        }
        #endregion
    }
}
