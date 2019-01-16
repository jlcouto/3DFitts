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
using System.IO;
using System;
using System.Collections;
using System.Reflection;
using System.Runtime.InteropServices;


namespace Meta.Tests
{

    ///// <summary>
    ///// MetaPlugin adds dll path to the programs path.
    ///// </summary>
    ///// <remarks>
    ///// It adds Assets/Plugins/x86 to the path in the editor, and ApplicationDataFolder\Plugins to the build path.
    ///// *NOTE*The static constructor for this class needs to be loaded before the assembly tris to load the dlls. therfore, changing the MetaWorld script exxecution order will create problems for builds.*NOTE*
    ///// </remarks>
    public static class DllTools
    {
        public static void AddPathVariable(string dllPath)
        {
            String currentPath = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Process);

            // Check that we haven't added it already.
            if (currentPath.Contains(dllPath))
            {
                return;
            }

            Environment.SetEnvironmentVariable("PATH", dllPath + Path.PathSeparator + currentPath, EnvironmentVariableTarget.Process);
            Console.WriteLine("Added to PATH: " + dllPath);
        }

        [DllImport("kernel32", SetLastError = true)]
        static extern public IntPtr LoadLibrary(string lpFileName);

        
        [DllImport("kernel32", SetLastError = true)]
        static extern public IntPtr FreeLibrary(string lpFileName);

       
        /// <summary>
        ///  Returns true if the fileName library is found. No *.dll extension is needed.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static bool CheckLibrary(string fileName)
        {
            IntPtr libraryHandle = LoadLibrary(fileName);
            Console.WriteLine(fileName + " handle: " + libraryHandle);
            return libraryHandle != IntPtr.Zero;
        }


        public static void CheckAllLibraries()
        {
            // Print path
            // Console.WriteLine(Environment.GetEnvironmentVariable("PATH"));

            System.Reflection.FieldInfo[] fieldInfo;
            var constantsType = typeof(Meta.Interop.DllReferences);
            fieldInfo = constantsType.GetFields( BindingFlags.NonPublic | BindingFlags.Static);

            foreach (_FieldInfo field in fieldInfo)
            {
                if (field.FieldType == typeof(String))
                {
                    String dllName = (String)field.GetValue(null);
                    bool dllFound = CheckLibrary(dllName + ".dll");

                    Console.WriteLine("DLL " + (dllFound ? "Found    " : "NOT FOUND") + " --> " + dllName );
                }
                
            }
        }

        public static IEnumerable EnumerateDllNames()
        {
            System.Reflection.FieldInfo[] fieldInfo;
            var constantsType = typeof(Meta.Interop.DllReferences);
            fieldInfo = constantsType.GetFields(BindingFlags.NonPublic | BindingFlags.Static);

            foreach (_FieldInfo field in fieldInfo)
            {
                if (field.FieldType == typeof(String))
                {
                    String dllName = (String)field.GetValue(null);
                    yield return dllName;

                    // Console.WriteLine("DLL " + (dllFound? "Found    " : "NOT FOUND") + " --> " + dllName);
                }

            }
        }

        public static String DirSep()
        {
            return System.IO.Path.DirectorySeparatorChar.ToString();
        }


    }

    //[DllImport("kernel32", SetLastError = true)]
    //static extern IntPtr LoadLibrary(string lpFileName);

    //[DllImport("kernel32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
    //static extern UIntPtr GetProcAddress(IntPtr hModule, string procName);

}
