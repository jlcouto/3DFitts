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
using UnityEditor;

[CustomEditor(typeof(MetaPointCloudExample))]
[CanEditMultipleObjects]
class MetaPointCloudExampleEditor : Editor
{
    private MetaPointCloudExample _component;

    public override void OnInspectorGUI()
    {
        if (_component == null)
            _component = target as MetaPointCloudExample;

        EditorGUILayout.HelpBox("MetaPointCloudExample is not meant for production use!", MessageType.Warning);

        DrawEnableMeshRender();
    }

    private void DrawEnableMeshRender()
    {
        var RenderPointCloud = serializedObject.FindProperty("RenderPointCloud");
        var oldValue = _component.RenderPointCloud;

        EditorGUILayout.PropertyField(RenderPointCloud);

        var newValue = RenderPointCloud.boolValue;
        if (oldValue != newValue)
        {
            _component.RenderPointCloud = newValue;
        }
    }
} 
