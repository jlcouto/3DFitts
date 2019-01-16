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
using System.Runtime.InteropServices;
using IntPtr          = System.IntPtr;
using StringBuilder   = System.Text.StringBuilder;
using VirtualKeyCodes = Meta.Mouse.VirtualKeyCodes;  // This is part of the Interop assembly, but is used in all layers.

namespace Meta.Interop
{
    public static class User32Interop
    {
        [DllImport(DllReferences.WindowsUI)]
        public static extern bool SetCursorPos(int X, int Y);

        [DllImport(DllReferences.WindowsUI)]
        public static extern bool GetCursorPos(out Win32Point pos);

        [DllImport(DllReferences.WindowsUI)]
        public static extern short GetKeyState(VirtualKeyCodes virtualKey);

        [DllImport(DllReferences.WindowsUI)]
        public static extern IntPtr GetActiveWindow();

        [DllImport(DllReferences.WindowsUI)]
        public static extern IntPtr GetForegroundWindow();

        [DllImport(DllReferences.WindowsUI)]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport(DllReferences.WindowsUI)]
        public static extern IntPtr SetActiveWindow(IntPtr hWnd);

        [DllImport(DllReferences.WindowsUI)]
        public static extern bool BringWindowToTop(IntPtr hWnd);

        [DllImport(DllReferences.WindowsUI)]
        public static extern bool GetWindowRect(IntPtr hWnd, out Win32Rect rect);

        [DllImport(DllReferences.WindowsUI)]
        public static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);

        [DllImport(DllReferences.WindowsUI, CharSet = CharSet.Unicode)]
        public static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string lclassName, string windowTitle);

        [DllImport(DllReferences.WindowsUI, SetLastError = true, CharSet = CharSet.Auto)]
        public static extern long GetClassName(IntPtr hwnd, StringBuilder lpClassName, long nMaxCount);

        [DllImport(DllReferences.WindowsUI, SetLastError = true)]
        public static extern System.IntPtr FindWindow(string lpClassName, string lpWindowName);

        /// Used in GameWindow configuration.
        [DllImport(DllReferences.WindowsUI)]
        public static extern IntPtr GetDC(IntPtr hwnd);

        /// Used in GameWindow configuration.
        [DllImport(DllReferences.WindowsUI)]
        public static extern int ReleaseDC(IntPtr hwnd, IntPtr dc);

        /// Used in GameWindow configuration.
        [DllImport(DllReferences.WindowsGDI)]
        public static extern int GetDeviceCaps(IntPtr hdc, int nIndex);


        /// <summary>
        /// Data structure to marshall point data from Win API calls.
        /// </summary>
        public struct Win32Point
        {
            public int X;
            public int Y;

            public Win32Point(int x, int y)
            {
                X = x;
                Y = y;
            }
        }

        /// <summary>
        /// Data structure to marshall data from Win API calls.
        /// </summary>
        public struct Win32Rect
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

    }
}
