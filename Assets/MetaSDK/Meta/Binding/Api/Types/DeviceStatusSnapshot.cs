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

namespace Meta.Plugin
{

    /// <summary>
    /// The Device Status at the time of request
    /// </summary>
    public struct DeviceStatusSnapshot
    {
        /// <summary>
        /// The enumerable states for the device
        /// </summary>
        public enum DeviceStatus
        {
            ERROR = 0,
            OFF = 1,
            INITIALIZING = 2,
            READY = 3,
            RUNNING = 4,
            PAUSED = 5,
            DEVICE_ERROR = 6
        }

        /// <summary>
        /// The enumerable states for the connection
        /// </summary>
        public enum ConnectionStatus
        {
            ERROR = 0,
            CONNECTED = 1,
            DISCONNECTED = 2,
            CONNECTION_TIMED_OUT = 3,
            CONNECTION_ERROR = 4,
            NOT_SUPERSPEED_USB = 5,
            UNKNOWN = 6
        }

        /// <summary>
        /// The enumerable streams for which the data rate is known
        /// </summary>
        public enum SensorStream
        {
            IMU_0_ACCEL = 1<<0,
            IMU_0_GYRO = 1<<1,
            IMU_1_ACCEL = 1<<2,
            IMU_1_GYRO = 1<<3,
            MONO_0 = 1<<4,
            MONO_1 = 1<<5,
            RGB = 1<<6,
            POINT_CLOUD = 1<<7,
            DEPTH = 1<<8,
            IR = 1<<9,
            NOISE = 1<<10,
            CONFIDENCE = 1<<11,
            AUDIO = 1<<12
        }

        /// <summary>
        /// This is used to filter any bits which do not contribute to the stream status.
        /// </summary>
        private const int SensorStreamStatusMask = ((int)SensorStream.AUDIO)*2 - 1;

        /// <summary>
        /// A bitmask for the Sensor Status for sensors which are important for the headset to operate properly.
        /// </summary>
        public const int ImportantStreamMask =
                (int)SensorStream.IMU_0_ACCEL |
                (int)SensorStream.IMU_0_GYRO |
                (int)SensorStream.IMU_1_ACCEL |
                (int)SensorStream.IMU_1_GYRO |
                (int)SensorStream.MONO_0 |
                (int)SensorStream.MONO_1 |
                (int)SensorStream.RGB |
                (int)SensorStream.POINT_CLOUD |
                (int)SensorStream.DEPTH |
                (int)SensorStream.IR |
                (int)SensorStream.NOISE |
                (int)SensorStream.CONFIDENCE;


        private DeviceStatus _deviceStatus;
        private ConnectionStatus _connectionStatus;
        private int _streamingStatus;

        /// <summary>
        /// A snapshot of the device status.
        /// </summary>
        /// <param name="deviceStatus">the Device Status</param>
        /// <param name="connectionStatus">the status of the connection to the device</param>
        /// <param name="streamingStatus">the bitfield containing individual sensor stream statuses.</param>
        public DeviceStatusSnapshot(DeviceStatus deviceStatus, ConnectionStatus connectionStatus, int streamingStatus)
        {
            _deviceStatus = deviceStatus;
            _connectionStatus = connectionStatus;
            _streamingStatus = streamingStatus & SensorStreamStatusMask;
        }

        /// <summary>
        /// Check if the device stream is operating at an acceptable rate. 
        /// </summary>
        /// <param name="streamIdentifier">The identifier of the stream to check.</param>
        /// <returns>whether the stream identified by the identifier is OK</returns>
        public bool IsStreamHealthy(SensorStream streamIdentifier)
        {
            return (_streamingStatus & (int)streamIdentifier) > 0;
        }

        /// <summary>
        /// The device status
        /// </summary>
        public DeviceStatus StatusOfDevice
        {
            get { return _deviceStatus; }
        }

        /// <summary>
        /// The connection status
        /// </summary>
        public ConnectionStatus StatusOfConnection
        {
            get { return _connectionStatus; }
        }

        /// <summary>
        /// Whether the device is streaming properly for all the critical sensors.
        /// </summary>
        /// <returns></returns>
        public bool DeviceStreamingProperly()
        {
            return (_streamingStatus & ImportantStreamMask) == ImportantStreamMask;
        }

        /// <summary>
        /// The raw stream status bitfield.
        /// </summary>
        public int StreamStatusMask
        {
            get { return _streamingStatus; }
        }

        public override bool Equals(object obj)
        {
            if (obj is DeviceStatusSnapshot)
            {
                DeviceStatusSnapshot otherStatus = (DeviceStatusSnapshot)obj;
                return otherStatus.StatusOfConnection == StatusOfConnection
                    && otherStatus.StatusOfDevice == StatusOfDevice
                    && otherStatus.StreamStatusMask == StreamStatusMask;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return new Tuple<DeviceStatus, ConnectionStatus, int>
                (StatusOfDevice, StatusOfConnection, StreamStatusMask).GetHashCode();
        }
    }

}

