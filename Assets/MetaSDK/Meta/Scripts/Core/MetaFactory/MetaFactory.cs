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
using Meta.Mouse;
using Meta.Buttons;
using Meta.Reconstruction;
using Meta.Plugin;
using Meta.SensorMessages;

namespace Meta
{
    internal class MetaFactory
    {
        private readonly bool _isHeadsetConnected = true;

        internal MetaFactory(bool isHeadsetConnected) 
        {
            _isHeadsetConnected = isHeadsetConnected;
        }

        public MetaFactoryPackage ConstructAll()
        {
            var package = new MetaFactoryPackage();

            ConstructEnvironmentServices(package);  // Environment variables
            ConstructLocalization(package);         // Head tracking
            ConstructHands(package);                // Hand tracking
            ConstructAlignmentHandler(package);     // Eye alignment ?

            ConstructButtonEventProvider(package);
            ConstructGaze(package);
            ConstructLocking(package);
            ConstructUserSettings(package);
            ConstructMetaSdkAnalytics(package);
            ConstructInputWrapper(package);

            return package;
        }

        private void ConstructInputWrapper(MetaFactoryPackage package)
        {
            UnityInputWrapper inputWrapper = new UnityInputWrapper();
            UnityKeyboardWrapper keyboardWrapper = new UnityKeyboardWrapper();
            package.MetaContext.Add<IInputWrapper>(inputWrapper);
            package.MetaContext.Add<IKeyboardWrapper>(keyboardWrapper);
            package.EventReceivers.Add(inputWrapper);
        }

        /// <summary>
        /// Constructs the AlignmentHandler. This will load an 
        /// </summary>
        private void ConstructAlignmentHandler(MetaFactoryPackage package)
        {
            AlignmentHandler alignmentHandler = new AlignmentHandler();
            package.EventReceivers.Add(alignmentHandler);
            package.MetaContext.Add(alignmentHandler);
        }

        private void ConstructMetaSdkAnalytics(MetaFactoryPackage package)
        {
            MetaSdkAnalytics handler = new MetaSdkAnalytics();
            package.EventReceivers.Add(handler);
        }
        
        private void ConstructUserSettings(MetaFactoryPackage package)
        {
            //This will be how the username is passed around
            Credentials creds = new Credentials("default", null);
            package.MetaContext.Add(creds);

            var userSettings = new EventReceivingUserSettings(creds);
            package.EventReceivers.Add(userSettings);
            package.MetaContext.Add((IUserSettings)userSettings);
        }

        private void ConstructButtonEventProvider(MetaFactoryPackage package)
        {
            var provider = new MetaButtonEventProvider();
            package.EventReceivers.Add(provider);
            package.MetaContext.Add(provider);
        }

        private void ConstructLocalization(MetaFactoryPackage package)
        {
            // TODO: deprecate Localizer interface in favor of native tracking implemenation interfaces
            // (allow external developers to implement a tracking interface for the compositor)
            var metaLocalization = new MetaLocalization();
            package.EventReceivers.Add(metaLocalization);
            package.MetaContext.Add(metaLocalization);

            metaLocalization.SetLocalizer(_isHeadsetConnected ? typeof(SlamLocalizer) : typeof(MouseLocalizer));

            //Sensor Messages
            MetaEventReceivingSensorFailureMessages messages = new MetaEventReceivingSensorFailureMessages();
            package.EventReceivers.Add(messages);
            package.MetaContext.Add(messages);

        }

        private void ConstructGaze(MetaFactoryPackage package)
        {
            var gaze = new Gaze();
            package.EventReceivers.Add(gaze);
            package.MetaContext.Add(gaze);
        }

        private void ConstructLocking(MetaFactoryPackage package)
        {
            var hudLock = new HudLock(package.MetaContext);
            var orbitalLock = new OrbitalLock();
            package.EventReceivers.Add(hudLock);
            package.EventReceivers.Add(orbitalLock);

            // Add to context
            package.MetaContext.Add(hudLock);
            package.MetaContext.Add(orbitalLock);
        }

        private void ConstructHands(MetaFactoryPackage package)
        {
            var handsModule = new HandsModule();
            package.EventReceivers.Add(handsModule);
            package.MetaContext.Add(handsModule);
            HandObjectReferences references = new HandObjectReferences();
            package.MetaContext.Add(references);
            InteractionObjectOutlineFactory outlineFactory = new InteractionObjectOutlineFactory();
            outlineFactory.SubscribeToHandObjectReferences(references);
            package.MetaContext.Add(outlineFactory);
        }

        private void ConstructEnvironmentServices(MetaFactoryPackage package)
        {
            string envPath = string.Format("{0}\\{1}\\", System.Environment.GetEnvironmentVariable("meta_root"), EnvironmentConstants.EnvironmentFolderName);
            IEnvironmentProfileRepository profileRepository = new EnvironmentProfileRepository(new EnvironmentProfileFileIOStream(envPath + "EnvironmentProfiles.json"),
                                                                                               new EnvironmentProfileJsonParser(),
                                                                                               new EnvironmentProfileVerifier(), 
                                                                                               envPath);
            package.MetaContext.Add(profileRepository);
        }
    }
}
