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
using UnityEditor;

namespace Meta.Settings
{
    /// <summary>
    /// Important project configurations to use the Meta headset after importing the MetaSDK.unitypackage
    /// </summary>
    internal class MetaProjectSettings
    {
        // GPU Skinning = true
        public const bool GpuSkinning = true;
        // Run in background = true
        public const bool RunInBackground = true;
        // VSync = Don't Sync
        public const int VSync = 0;
        // Anti-aliasing = Disabled
        public const int AntiAliasing = 0;
        // API compatibility = .NET 2.0
        public const ApiCompatibilityLevel ApiCompatibility = ApiCompatibilityLevel.NET_2_0;
        // Correct aspect ratio = 16x9
        public const AspectRatio MetaAspectRatio = AspectRatio.Aspect16by9;
        // Speaker mode = Quad
        public const AudioSpeakerMode MetaSpeakerMode = AudioSpeakerMode.Quad;
        // Build architecture = x86_64
        public const BuildTarget BuildArchitecture = BuildTarget.StandaloneWindows64;
        // Build target = Standalone
        public const BuildTargetGroup BuildPlatform = BuildTargetGroup.Standalone;
        // Hide resolution dialog
        public const ResolutionDialogSetting DialogSetting = ResolutionDialogSetting.HiddenByDefault;

        public static AudioSpeakerMode GetCurrentSpeakerMode()
        {
            return AudioSettings.GetConfiguration().speakerMode;
        }

        public static int GetCurrentAntiAliasing()
        {
            return QualitySettings.antiAliasing;
        }

        public static BuildTarget GetCurrentBuildArchitecture()
        {
            return EditorUserBuildSettings.selectedStandaloneTarget;
        }

        public static bool GetCurrentRunInBackground()
        {
            return Application.runInBackground;
        }

        public static ApiCompatibilityLevel GetCurrentApiCompatibility()
        {
            return PlayerSettings.GetApiCompatibilityLevel(BuildPlatform);
        }

        public static int GetCurrentVSync()
        {
            return QualitySettings.vSyncCount;
        }

        public static ResolutionDialogSetting GetCurrentResolutionDialog()
        {
            return PlayerSettings.displayResolutionDialog;
        }

        public static bool IsCurrentAspectRatioSetup()
        {
            if (!PlayerSettings.HasAspectRatio(MetaAspectRatio))
            {
                return false;
            }
            foreach (AspectRatio aspectRatio in Enum.GetValues(typeof(AspectRatio)))
            {
                if (aspectRatio != MetaAspectRatio && PlayerSettings.HasAspectRatio(aspectRatio))
                {
                    return false;
                }
            }
            return true;
        }

        public static BuildTargetGroup GetCurrentBuildPlatform()
        {
            return EditorUserBuildSettings.selectedBuildTargetGroup;
        }

        public static bool GetCurrentGpuSkinning()
        {
            return PlayerSettings.gpuSkinning;
        }

        public static void SetRecommendedAudioSettings()
        {
            AudioConfiguration audioConfiguration = AudioSettings.GetConfiguration();
            audioConfiguration.dspBufferSize = 0;
            audioConfiguration.sampleRate = 0;
            audioConfiguration.speakerMode = MetaSpeakerMode;
            AudioSettings.Reset(audioConfiguration);
        }

        public static void SetRecommendedBuildArchitecture()
        {
            EditorUserBuildSettings.selectedStandaloneTarget = BuildArchitecture;
        }

        public static void SetRecommendedRunInBackground()
        {
            Application.runInBackground = RunInBackground;
        }

        public static void SetRecommendedApiCompatibility()
        {
            PlayerSettings.SetApiCompatibilityLevel(BuildPlatform, ApiCompatibility);
        }

        public static void SetRecommendedDialogSetting()
        {
            PlayerSettings.displayResolutionDialog = DialogSetting;
        }

        public static void SetRecommendedAspectRatios()
        {
            foreach (AspectRatio aspectRatio in Enum.GetValues(typeof(AspectRatio)))
            {
                PlayerSettings.SetAspectRatio(aspectRatio, false);
            }
            PlayerSettings.SetAspectRatio(MetaAspectRatio, true);
        }

        public static void SetRecommendedVSync()
        {
            QualitySettings.vSyncCount = VSync;
        }

        public static void SetRecommendedAntiAliasing()
        {
            QualitySettings.antiAliasing = AntiAliasing;
        }

        public static void SetRecommendedBuildPlatform()
        {
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildPlatform, BuildArchitecture);
        }

        public static void SetRecommendedGpuSkinning()
        {
            PlayerSettings.gpuSkinning = GpuSkinning;
        }

        public static void ApplyAllRecommendedSettings()
        {
            SetRecommendedBuildPlatform();
            SetRecommendedApiCompatibility();
            SetRecommendedAspectRatios();
            SetRecommendedAudioSettings();
            SetRecommendedBuildArchitecture();
            SetRecommendedDialogSetting();
            SetRecommendedRunInBackground();
            SetRecommendedVSync();
            SetRecommendedGpuSkinning();
        }
    }
}
