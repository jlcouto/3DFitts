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
using NUnit.Framework;
using System;
using MetaVariable = Meta.Interop.MetaCoreInterop.MetaVariable;
using InitStatus = Meta.Interop.MetaCoreInterop.InitStatus;
using Matrix4x4 = UnityEngine.Matrix4x4;

namespace Meta.Tests.SystemApi
{
    [TestFixture]
    [Ignore("Hanging on meta core stop, need to investigate.")]
    public class CalibrationSystemApiTests
    {
        bool systemStarted = false;

        public CalibrationSystemApiTests()
        {
            DllTools.AddPathVariable(Environment.ExpandEnvironmentVariables("%META_CORE%"));
            DllTools.AddPathVariable(Environment.ExpandEnvironmentVariables("%META_INTERNAL_UNITY_SDK%")
                                      + DllTools.DirSep()
                                      + "Plugins"
                                      + DllTools.DirSep()
                                      + "x86_64");
        }

        [SetUp]
        public void SetUpSystemTest()
        {
            systemStarted = true;
            string loadCalibDataAndSerialData =
                Environment.ExpandEnvironmentVariables("%META_INTERNAL_CONFIG%") +
                DllTools.DirSep() + "core_api" + DllTools.DirSep() + "integration_test.json";
            var status = Interop.MetaCoreInterop.meta_init(loadCalibDataAndSerialData, true);
            // Abort test if system didn't start.
            if (status != InitStatus.SUCCESS)
            {
                Assert.Fail("System failed to start. ");
                return;
            }

            Interop.MetaCoreInterop.meta_start_application(false);
            Interop.MetaCoreInterop.meta_wait_start_complete();
        }


        [TearDown]
        public void TearDownSystemTest()
        {
            if (systemStarted)
            {
                Plugin.SystemApi.Stop();
            }
        }


        [Test]
        public void CanStartSuccessfully()
        {
            Assert.IsTrue(systemStarted);
        }


        [Test]
        public void GetSerialNumberData()
        {
            var serialNumber = Plugin.SystemApi.GetSerialNumber();
            string expectedSerialNumberData = "META2354916001071";
            Assert.AreEqual(expectedSerialNumberData, serialNumber);
        }


        [Test]
        public void GetTransform()
        {
            var matrix = new Interop.MetaCoreInterop.MetaMatrix44();
            var transform = Matrix4x4.identity;

            int maxNumberAttempts = 100;
            int numberOfAttempts = 0;
            bool returnStatus = false;

            while (!returnStatus && numberOfAttempts < maxNumberAttempts)
            {
                //returnStatus = Plugin.SystemApi.GetTransform(Interop.MetaCoreInterop.MetaCoordinateFrame.DEPTH,
                //                                             Interop.MetaCoreInterop.MetaCoordinateFrame.RGB,
                //                                             ref transform);

                /// Need to use interop here as the values we are comparing are for the Right handed matrix
                /// TODO: decide if we should just change expected
                returnStatus = Interop.MetaCoreInterop.meta_get_transform(Interop.MetaCoreInterop.MetaCoordinateFrame.DEPTH,
                                                                          Interop.MetaCoreInterop.MetaCoordinateFrame.RGB,
                                                                          ref matrix);

                numberOfAttempts++;
            }

            if ( !returnStatus )
            {
                UnityEngine.Debug.Log("failed to get transform");
                Assert.Fail();
            }

            UnityEngine.Debug.Log(transform);

            float expectedPositionX = -0.02749644f;
            float expectedPositionY = 0.001773831f;
            float expectedPositionZ = 0.001428126f;


            double delta = 0.00001;

            Assert.AreEqual(expectedPositionX, matrix.m03, delta);
            Assert.AreEqual(expectedPositionY, matrix.m13, delta);
            Assert.AreEqual(expectedPositionZ, matrix.m23, delta);
        }


        [Test]
        public void GetTransformBadFrames()
        {
            Matrix4x4 transform = Matrix4x4.identity;

            int maxNumberAttempts = 100;
            int numberOfAttempts = 0;
            bool returnStatus = false;

            while (!returnStatus && numberOfAttempts < maxNumberAttempts)
            {
                returnStatus = Plugin.SystemApi.GetTransform((Interop.MetaCoreInterop.MetaCoordinateFrame)(-1),
                                                             (Interop.MetaCoreInterop.MetaCoordinateFrame)5,
                                                             ref transform);
                numberOfAttempts++;
            }

            if ( returnStatus )
            {
                Assert.Pass();
            }

            var position = transform.GetPosition();

            double expectedPositionX = 0.0;
            double expectedPositionY = 0.0;
            double expectedPositionZ = 0.0;

            Assert.AreEqual(expectedPositionX, position.x);
            Assert.AreEqual(expectedPositionY, position.y);
            Assert.AreEqual(expectedPositionZ, position.z);
        }
    }

    [TestFixture]
    [Ignore("Interaction with CoCo at this point is broken in Unit Test")]
    public class PathApiTests
    {
        static string loadCalibDataAndSerialData;
        public PathApiTests()
        {
            DllTools.AddPathVariable(Environment.ExpandEnvironmentVariables("%META_CORE%"));
            DllTools.AddPathVariable(Environment.ExpandEnvironmentVariables("%META_INTERNAL_UNITY_SDK%")
                                      + DllTools.DirSep()
                                      + "Plugins"
                                      + DllTools.DirSep()
                                      + "x86_64");
            loadCalibDataAndSerialData =
                Environment.ExpandEnvironmentVariables("%META_INTERNAL_CONFIG%") +
                DllTools.DirSep() + "device" + DllTools.DirSep() + "calibration_data_integration_test.json";
        }


        [TearDown]
        public void TearDownSystemTest()
        {
            Plugin.SystemApi.Stop();
        }


        [Test]
        [Ignore("Ignoring production test till we find out how to test this on CI")]
        public void GetPathProduction()
        {
            GetPathTest(false);
        }


        [Test]
        public void GetPathDevelopment()
        {
            GetPathTest(true);
        }


        private static void GetPathTest(bool is_development_environment)
        {
            var status = Interop.MetaCoreInterop.meta_init(loadCalibDataAndSerialData, is_development_environment);
            // Abort test if system didn't start.
            if ( status != InitStatus.SUCCESS)
            {
                Assert.Fail();
                return;
            }

            Interop.MetaCoreInterop.meta_start_application(false);

            MetaVariable[] variables =
                new MetaVariable[] { MetaVariable.META_3RDPARTY, MetaVariable.META_APP_DATA, MetaVariable.META_BUILD,
                                     MetaVariable.META_CACHE, MetaVariable.META_CACHE_DEBUG,
                                     MetaVariable.META_CACHE_RELEASE,
                                     MetaVariable.META_CONFIG, MetaVariable.META_CORE, MetaVariable.META_DRIVER,
                                     MetaVariable.META_INSTALL,
                                     MetaVariable.META_RECORDING, MetaVariable.META_TESTING_DATA,
                                     MetaVariable.META_TOOLS,
                                     MetaVariable.META_USB, MetaVariable.META_USER_DATA };
            foreach (var v in variables)
            {
                string result = string.Empty;
                if (Plugin.SystemApi.GetPath(v, out result))
                {
                    // UnityEngine.Debug.Log( v.ToString() + ": " + result );
                    Assert.IsNotEmpty(result, v + " : should not be empty.");
                }
                else
                {
                    Assert.IsEmpty(result, v + " : should be empty.");
                }
            }
        }
    }
}
