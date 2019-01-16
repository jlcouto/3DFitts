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
using System.Collections.Generic;
using UnityEngine;

namespace Meta
{
    /// <summary>
    /// The HudLock class locks GameObjects to Camera space,
    /// making them appear as if they are a part of the HUD
    /// as they won't appear to move when the camera position or rotation changes
    /// </summary>
    internal class HudLock : IEventReceiver
    {
        private readonly IMetaContext _metaContext;

        /// <summary>
        /// List of hud locked MetaBodies
        /// </summary>
        private readonly List<MetaLocking> _hudLockedObjects = new List<MetaLocking>();

        /// <summary>
        /// Initial Positions of locked objects
        /// </summary>
        private readonly Dictionary<MetaLocking, Vector3> _initialPositions = new Dictionary<MetaLocking, Vector3>();

        /// <summary>
        /// Initial Rotations of locked objects
        /// </summary>
        private readonly Dictionary<MetaLocking, Quaternion> _initialRotations = new Dictionary<MetaLocking, Quaternion>();

        private Transform _mainCameraTransform;

        /// <summary>
        /// Creates an instance of <see cref="HudLock"/> class.
        /// </summary>
        /// <param name="metaContext"></param>
        internal HudLock(IMetaContext metaContext)
        {
            if (metaContext == null)
            {
                throw new ArgumentNullException("metaContext");
            }
            _metaContext = metaContext;
        }

        /// <summary>
        /// Adds the IEventReceiver functions to the delegates in order to be called from MetaManager
        /// </summary>
        public void Init(IEventHandlers eventHandlers)
        {
            eventHandlers.SubscribeOnLateUpdate(Update);
        }

        /// <summary>
        /// Adds MetaBodies to the list of lockables
        /// </summary>
        internal void AddHudLockedObject(MetaLocking hudLockedObject)
        {
            SetCamera();
            if (!_hudLockedObjects.Contains(hudLockedObject))
            {
                _hudLockedObjects.Add(hudLockedObject);
                _initialPositions[hudLockedObject] = _mainCameraTransform.InverseTransformPoint(hudLockedObject.transform.position);
                _initialRotations[hudLockedObject] = Quaternion.Inverse(_mainCameraTransform.rotation) * hudLockedObject.transform.rotation;
            }
        }

        /// <summary>
        /// Removes MetaBodies from the list of lockables
        /// </summary>
        internal void RemoveHudLockedObject(MetaLocking hudLockedObject)
        {
            if (_hudLockedObjects.Contains(hudLockedObject))
            {
                _hudLockedObjects.Remove(hudLockedObject);
                _initialPositions.Remove(hudLockedObject);
                _initialRotations.Remove(hudLockedObject);
            }
        }

        private void SetCamera()
        {
            if (_mainCameraTransform == null)
            {
                _mainCameraTransform = _metaContext.Get<IEventCamera>().EventCameraRef.transform;
            }
        }

        private void Update()
        {
            UpdateHUDLocks();
        }

        /// <summary>
        /// Updates the position and rotation of the MetaBodies so that it remains locked to the HUD
        /// </summary>
        private void UpdateHUDLocks()
        {
            for (int i = 0; i < _hudLockedObjects.Count; i++)
            {
                MetaLocking metaLocking = _hudLockedObjects[i];
                if (metaLocking != null)
                {
                    Vector3 pos = _mainCameraTransform.TransformPoint(_initialPositions[metaLocking]);
                    Vector3 rot = (_mainCameraTransform.rotation * _initialRotations[metaLocking]).eulerAngles;
                    if (metaLocking.useDefaultHUDSettings)
                    {
                        metaLocking.transform.position = pos;
                        metaLocking.transform.rotation = Quaternion.Euler(rot);
                    }
                    else
                    {
                        if (metaLocking.hudLockPosition)
                        {
                            if (!metaLocking.hudLockPositionX)
                            {
                                pos.x = metaLocking.transform.position.x;
                            }
                            if (!metaLocking.hudLockPositionY)
                            {
                                pos.y = metaLocking.transform.position.y;
                            }
                            if (!metaLocking.hudLockPositionZ)
                            {
                                pos.z = metaLocking.transform.position.z;
                            }
                            metaLocking.transform.position = pos;
                        }
                        if (metaLocking.hudLockRotation)
                        {
                            if (!metaLocking.hudLockRotationX)
                            {
                                rot.x = metaLocking.transform.rotation.eulerAngles.x;
                            }
                            if (!metaLocking.hudLockRotationY)
                            {
                                rot.y = metaLocking.transform.rotation.eulerAngles.y;
                            }
                            if (!metaLocking.hudLockRotationZ)
                            {
                                rot.z = metaLocking.transform.rotation.eulerAngles.z;
                            }
                            metaLocking.transform.rotation = Quaternion.Euler(rot);
                        }
                    }
                }
            }
        }
    }
}
