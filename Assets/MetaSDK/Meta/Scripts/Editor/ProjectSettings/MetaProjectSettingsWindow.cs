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
using UnityEditor;

namespace Meta.Settings
{
    /// <summary>
    /// UI for managing project settings
    /// </summary>
    [InitializeOnLoad]
    internal class MetaProjectSettingsWindow : EditorWindow
    {
        private static Vector2 _minWindowSize = new Vector2(500, 600);
        private Vector2 _scrollPosition = Vector2.zero;
        private int _spaceBetweenSettings = 10;
        private bool _isInitialized = false;

        private GUIContent _apiCompatibility;
        private GUIContent _architecture;
        private GUIContent _aspectRatios;
        private GUIContent _audio;
        private GUIContent _platform;
        private GUIContent _resolutionDialog;
        private GUIContent _runInBackground;
        private GUIContent _vsync;
        private GUIContent _gpuSkinning;

        [MenuItem("Meta 2/Meta Project Settings")]
        public static void ShowProjectSettingsWindow()
        {
            MetaProjectSettingsWindow window = GetWindow<MetaProjectSettingsWindow>(true);
            window.titleContent = new GUIContent("Meta Project Settings");
            window.minSize = _minWindowSize;
            window.Show();
        }

        private void OnGUI()
        {
            if (!_isInitialized)
            {
                _isInitialized = true;
                InitializeTexts();
            }

            ShowHeader();
            ShowSettingsOptions();
            ShowButtons();
        }

        private void InitializeTexts()
        {
            _apiCompatibility = new GUIContent("Change API Compatibility to .NET 2.0",
                "The Meta SDK requires .NET 2.0 compatibility.");
            _architecture = new GUIContent("Set Build Architecture to x86_64",
                "The Meta SDK only supports 64-bit builds.");
            _aspectRatios = new GUIContent("Disable Unnecessary Aspect Ratios",
                "This option generates a build that only shows the aspect ratio that the Meta 2 supports.");
            _audio = new GUIContent("Enable Quad Audio",
                "Necessary to take advantage of the four speakers in the Meta 2 headset.");
            _platform = new GUIContent("Set Target Platform to Standalone",
                "");
            _resolutionDialog = new GUIContent("Hide Resolution Dialog",
                "Resolution Dialog is generally not relevant to applications for the Meta 2.");
            _runInBackground = new GUIContent("Enable Run in Background",
                "Set this option to avoid pausing the application when its window is out of focus.");
            _vsync = new GUIContent("Disable VSync",
                "Meta's Compositor handles VSync. Because of that, Unity's VSync setting is unnecessary and could limit framerate.");
            _gpuSkinning = new GUIContent("Enable GPU Skinning",
                "Enable GPU skinning for performance improvements.");
        }

        private void ShowHeader()
        {
            EditorGUILayout.HelpBox("Meta recommends you to apply these changes to your project settings.", MessageType.Info);
        }

        private void ShowSettingsOptions()
        {
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);

            ShowSettingsGroup(_platform,
                MetaProjectSettings.GetCurrentBuildPlatform() == MetaProjectSettings.BuildPlatform,
                MetaProjectSettings.GetCurrentBuildPlatform().ToString(),
                MetaProjectSettings.SetRecommendedBuildArchitecture);
            ShowSettingsGroup(_architecture,
                MetaProjectSettings.GetCurrentBuildArchitecture() == MetaProjectSettings.BuildArchitecture,
                MetaProjectSettings.GetCurrentBuildArchitecture().ToString(),
                MetaProjectSettings.SetRecommendedBuildPlatform);
            ShowSettingsGroup(_apiCompatibility,
                MetaProjectSettings.GetCurrentApiCompatibility() == MetaProjectSettings.ApiCompatibility,
                MetaProjectSettings.GetCurrentApiCompatibility().ToString(),
                MetaProjectSettings.SetRecommendedApiCompatibility);
            ShowSettingsGroup(_vsync,
                MetaProjectSettings.GetCurrentVSync() == MetaProjectSettings.VSync,
                MetaProjectSettings.GetCurrentVSync() != MetaProjectSettings.VSync ? "Enabled" : "Disabled",
                MetaProjectSettings.SetRecommendedVSync);
            ShowSettingsGroup(_audio,
                MetaProjectSettings.GetCurrentSpeakerMode() == MetaProjectSettings.MetaSpeakerMode,
                MetaProjectSettings.GetCurrentSpeakerMode().ToString(),
                MetaProjectSettings.SetRecommendedAudioSettings);
            ShowSettingsGroup(_runInBackground,
                MetaProjectSettings.GetCurrentRunInBackground() == MetaProjectSettings.RunInBackground,
                MetaProjectSettings.GetCurrentRunInBackground().ToString(),
                MetaProjectSettings.SetRecommendedRunInBackground);
            ShowSettingsGroup(_gpuSkinning,
                MetaProjectSettings.GetCurrentGpuSkinning() == MetaProjectSettings.GpuSkinning,
                MetaProjectSettings.GetCurrentGpuSkinning().ToString(),
                MetaProjectSettings.SetRecommendedGpuSkinning);
            ShowSettingsGroup(_resolutionDialog,
                MetaProjectSettings.GetCurrentResolutionDialog() == MetaProjectSettings.DialogSetting,
                MetaProjectSettings.GetCurrentResolutionDialog().ToString(),
                MetaProjectSettings.SetRecommendedDialogSetting);
            ShowSettingsGroup(_aspectRatios,
                MetaProjectSettings.IsCurrentAspectRatioSetup(),
                MetaProjectSettings.IsCurrentAspectRatioSetup() ? "Setup" : "Not Setup",
                MetaProjectSettings.SetRecommendedAspectRatios);

            GUILayout.EndScrollView();
        }

        private void ShowButtons()
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Apply all"))
            {
                MetaProjectSettings.ApplyAllRecommendedSettings();
            }
            if (GUILayout.Button("Close"))
            {
                Close();
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(_spaceBetweenSettings);
        }

        private void ShowSettingsGroup(GUIContent guiContent, bool isSetup, string currentSetting, System.Action action)
        {
            EditorGUI.BeginDisabledGroup(isSetup);
            if (GUILayout.Button(guiContent))
            {
                action.Invoke();
            }
            EditorGUI.EndDisabledGroup();

            string currentSettingsMessage = "Current setting: " + currentSetting;
            if (isSetup)
            {
                currentSettingsMessage += " (no action required)";
            }
            GUILayout.Label(currentSettingsMessage, EditorStyles.largeLabel);

            GUILayout.Space(_spaceBetweenSettings);
        }
    }
}
