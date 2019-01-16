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
using System.Linq;
using Assembly    = System.Reflection.Assembly;
using MethodBase  = System.Reflection.MethodBase;
using Application = UnityEngine.Application;


namespace Meta.Plugin
{
    using System;
    using System.IO;

    internal static class MetaUtils
    {
        /// <summary>
        /// The URL of docs homepage, accessed from Meta 2 menu in Unity Editor.
        /// </summary>
        public const string MetaDocsURL = "https://docs.metavision.com/external/doc/latest/unity_sdk_landing_page.html";

        /// <summary>
        /// The path to the Meta release version text file document.
        /// </summary>
        public static readonly string MetaVersionPath = @".\Assets\MetaSDK\Meta\SDK2Version.txt";


        private static string _sdkVersion = null;

        /// <summary>
        /// Get the version of the SDK that is written in the release version text file.
        /// </summary>
        /// <returns></returns>
        public static string SDKVersion
        {
            get
            {
                if (_sdkVersion == null)
                {
                    _sdkVersion = File.ReadAllLines(MetaVersionPath).First();
                }
                return _sdkVersion;
            }
        }

        /// <summary>
        /// Checks if the unitypackage is beta
        /// </summary>
        public static bool IsBeta()
        {
            if (Assembly.GetAssembly(MethodBase.GetCurrentMethod().DeclaringType).GetType("MetaBeta", false, true) != null
                && Assembly.GetAssembly(MethodBase.GetCurrentMethod().DeclaringType).GetType("MetaInternal", false, true) == null)
            {
                return true;
            }
            return false;
        }

        public static string GetPluginsFolderPath()
        {
            string applicationDataFolder, pluginsRelativePath;
            string platformFolder = Is64Bit() ? "x86_64" : "x86";
            if (Application.isEditor)
            {
                applicationDataFolder = Path.Combine(Application.dataPath, "MetaSDK");
                pluginsRelativePath = Path.Combine("Plugins", platformFolder);
            }
            else
            {
                applicationDataFolder = Application.dataPath;
                pluginsRelativePath = "Plugins";
            }
            return Path.Combine(applicationDataFolder, pluginsRelativePath).Replace("/", "\\");
        }

        /// <summary>
        /// Checks if we are in internal
        /// </summary>
        public static bool IsInternal()
        {
            if (Assembly.GetAssembly(MethodBase.GetCurrentMethod().DeclaringType).GetType("MetaInternal", false, true) != null)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Checks if the Unity Editor is 64-bit
        /// </summary>
        public static bool Is64Bit()
        {
            return (System.IntPtr.Size == 8);
        }

        /// <summary>
        /// Opens a URL within the META_SDK directory, where %META_SDK% is replaced by the SDK path.
        /// </summary>
        /// <param name="url">The URL to be parsed.</param>
        public static void OpenURL(string url)
        {
            url = url.Replace("%META_SDK2%", System.Environment.GetEnvironmentVariable("META_SDK2"))
                .Replace("%META_SDK2_BETA%", System.Environment.GetEnvironmentVariable("META_SDK2_BETA"))
                .Replace("\\", "/");
            Application.OpenURL(url);
        }

        public static string GetCurrentSystemTime(string format = "{0:HH:mm:ss.ffff}")
        {
            return string.Format(format, System.DateTime.Now);
        }

    }

}
