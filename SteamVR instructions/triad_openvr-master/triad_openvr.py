import time
import sys
import openvr
import math
import json

# Function to print out text but instead of starting a new line it will overwrite the existing line
def update_text(txt):
    sys.stdout.write('\r'+txt)
    sys.stdout.flush()

#Convert the standard 3x4 position/rotation matrix to a x,y,z location and the appropriate Euler angles (in degrees)
def convert_to_euler(pose_mat):
    yaw = 180 / math.pi * math.atan2(pose_mat[1][0], pose_mat[0][0])
    pitch = 180 / math.pi * math.atan2(pose_mat[2][0], pose_mat[0][0])    
    roll = 180 / math.pi * math.atan2(pose_mat[2][1], pose_mat[2][2])
    x = pose_mat[0][3]
    y = pose_mat[1][3]
    z = pose_mat[2][3]
    return [x,y,z,yaw,pitch,roll]

#Convert the standard 3x4 position/rotation matrix to a x,y,z location and the appropriate Quaternion
def convert_to_quaternion(pose_mat):
    r_w = math.sqrt(max(0, 1 + pose_mat[0][0] + pose_mat[1][1] + pose_mat[2][2])) * 0.5;
    r_x = math.sqrt(max(0, 1 + pose_mat[0][0] - pose_mat[1][1] - pose_mat[2][2])) * 0.5;
    r_y = math.sqrt(max(0, 1 - pose_mat[0][0] + pose_mat[1][1] - pose_mat[2][2])) * 0.5;
    r_z = math.sqrt(max(0, 1 - pose_mat[0][0] - pose_mat[1][1] + pose_mat[2][2])) * 0.5;
    r_x *= 1 if ((r_x * (pose_mat[2][1] - pose_mat[1][2])) >= 0) else -1;
    r_y *= 1 if ((r_y * (pose_mat[0][2] - pose_mat[2][0])) >= 0) else -1;
    r_z *= 1 if ((r_z * (pose_mat[1][0] - pose_mat[0][1])) >= 0) else -1;
    
    ## Per issue #2, adding a abs() so that sqrt only results in real numbers
    #r_w = math.sqrt(abs(1+pose_mat[0][0]+pose_mat[1][1]+pose_mat[2][2]))/2
    #r_x = (pose_mat[2][1]-pose_mat[1][2])/(4*r_w)
    #r_y = (pose_mat[0][2]-pose_mat[2][0])/(4*r_w)
    #r_z = (pose_mat[1][0]-pose_mat[0][1])/(4*r_w)

    x = pose_mat[0][3]
    y = pose_mat[1][3]
    z = pose_mat[2][3]
    return [x,y,z,r_w,r_x,r_y,r_z]

#Define a class to make it easy to append pose matricies and convert to both Euler and Quaternion for plotting
class pose_sample_buffer():
    def __init__(self):
        self.i = 0
        self.index = []
        self.time = []
        self.x = []
        self.y = []
        self.z = []
        self.yaw = []
        self.pitch = []
        self.roll = []
        self.r_w = []
        self.r_x = []
        self.r_y = []
        self.r_z = []

    def append(self,pose_mat,t):
        self.time.append(t)
        self.x.append(pose_mat[0][3])
        self.y.append(pose_mat[1][3])
        self.z.append(pose_mat[2][3])
        self.yaw.append(180 / math.pi * math.atan(pose_mat[1][0] /pose_mat[0][0]))
        self.pitch.append(180 / math.pi * math.atan(-1 * pose_mat[2][0] / math.sqrt(pow(pose_mat[2][1], 2) + math.pow(pose_mat[2][2], 2))))
        self.roll.append(180 / math.pi * math.atan(pose_mat[2][1] /pose_mat[2][2]))
        r_w = math.sqrt(abs(1+pose_mat[0][0]+pose_mat[1][1]+pose_mat[2][2]))/2
        self.r_w.append(r_w)
        self.r_x.append((pose_mat[2][1]-pose_mat[1][2])/(4*r_w))
        self.r_y.append((pose_mat[0][2]-pose_mat[2][0])/(4*r_w))
        self.r_z.append((pose_mat[1][0]-pose_mat[0][1])/(4*r_w))

class vr_tracked_device():
    def __init__(self,vr_obj,index,device_class):
        self.device_class = device_class
        self.index = index
        self.vr = vr_obj

    def get_serial(self):
        return self.vr.getStringTrackedDeviceProperty(self.index,openvr.Prop_SerialNumber_String).decode('utf-8')

    def get_model(self):
        return self.vr.getStringTrackedDeviceProperty(self.index,openvr.Prop_ModelNumber_String).decode('utf-8')

    def sample(self,num_samples,sample_rate):
        interval = 1/sample_rate
        rtn = pose_sample_buffer()
        sample_start = time.time()
        for i in range(num_samples):
            start = time.time()
            pose = self.vr.getDeviceToAbsoluteTrackingPose(openvr.TrackingUniverseStanding, 0,openvr.k_unMaxTrackedDeviceCount)
            rtn.append(pose[self.index].mDeviceToAbsoluteTracking,time.time()-sample_start)
            sleep_time = interval- (time.time()-start)
            if sleep_time>0:
                time.sleep(sleep_time)
        return rtn

    def get_pose_euler(self):
        pose = self.vr.getDeviceToAbsoluteTrackingPose(openvr.TrackingUniverseStanding, 0,openvr.k_unMaxTrackedDeviceCount)
        return convert_to_euler(pose[self.index].mDeviceToAbsoluteTracking)

    def get_pose_quaternion(self):
        pose = self.vr.getDeviceToAbsoluteTrackingPose(openvr.TrackingUniverseStanding, 0,openvr.k_unMaxTrackedDeviceCount)
        return convert_to_quaternion(pose[self.index].mDeviceToAbsoluteTracking)
    
    def controller_state_to_dict(self, pControllerState):
        # This function is graciously borrowed from https://gist.github.com/awesomebytes/75daab3adb62b331f21ecf3a03b3ab46
        # docs: https://github.com/ValveSoftware/openvr/wiki/IVRSystem::GetControllerState
        d = {}
        d['unPacketNum'] = pControllerState.unPacketNum
        # on trigger .y is always 0.0 says the docs
        d['trigger'] = pControllerState.rAxis[1].x
        # 0.0 on trigger is fully released
        # -1.0 to 1.0 on joystick and trackpads
        d['trackpad_x'] = pControllerState.rAxis[0].x
        d['trackpad_y'] = pControllerState.rAxis[0].y
        # These are published and always 0.0
        # for i in range(2, 5):
        #     d['unknowns_' + str(i) + '_x'] = pControllerState.rAxis[i].x
        #     d['unknowns_' + str(i) + '_y'] = pControllerState.rAxis[i].y
        d['ulButtonPressed'] = pControllerState.ulButtonPressed
        d['ulButtonTouched'] = pControllerState.ulButtonTouched
        # To make easier to understand what is going on
        # Second bit marks menu button
        d['menu_button'] = bool(pControllerState.ulButtonPressed >> 1 & 1)
        # 32 bit marks trackpad
        d['trackpad_pressed'] = bool(pControllerState.ulButtonPressed >> 32 & 1)
        d['trackpad_touched'] = bool(pControllerState.ulButtonTouched >> 32 & 1)
        # third bit marks grip button
        d['grip_button'] = bool(pControllerState.ulButtonPressed >> 2 & 1)
        # System button can't be read, if you press it
        # the controllers stop reporting
        return d
    
    def get_controller_inputs(self):
        result, state = self.vr.getControllerState(self.index)
        return self.controller_state_to_dict(state)

class vr_tracking_reference(vr_tracked_device):
    def get_mode(self):
        return self.vr.getStringTrackedDeviceProperty(self.index,openvr.Prop_ModeLabel_String).decode('utf-8').upper()
    def sample(self,num_samples,sample_rate):
        print("Warning: Tracking References do not move, sample isn't much use...")

class triad_openvr():
    def __init__(self):
        # Initialize OpenVR in the
        self.vr = openvr.init(openvr.VRApplication_Other)

        # Initializing object to hold indexes for various tracked objects
        self.object_names = {"Tracking Reference":[],"HMD":[],"Controller":[],"Tracker":[]}
        self.devices = {}
        poses = self.vr.getDeviceToAbsoluteTrackingPose(openvr.TrackingUniverseStanding, 0,
                                                               openvr.k_unMaxTrackedDeviceCount)

        # Loading config file
        self.config = None
        try:
            with open('config.json') as json_data:
                self.config = json.load(json_data)
        except EnvironmentError: # parent of IOError, OSError *and* WindowsError where available
            print('config.json not found, arbitrary id will be chosen.')

        if self.config != None:
            # Iterate through the pose list to find the active devices and determine their type
            for i in range(openvr.k_unMaxTrackedDeviceCount):
                if poses[i].bPoseIsValid:
                    device_serial = self.vr.getStringTrackedDeviceProperty(i,openvr.Prop_SerialNumber_String).decode('utf-8')
                    for device in self.config['devices']:
                        if device_serial == device['serial']:
                            device_name = device['name']
                            self.object_names[device['type']].append(device_name)
                            self.devices[device_name] = vr_tracked_device(self.vr,i,device['type'])
        else:
            # Iterate through the pose list to find the active devices and determine their type
            for i in range(openvr.k_unMaxTrackedDeviceCount):
                if poses[i].bPoseIsValid:
                    device_class = self.vr.getTrackedDeviceClass(i)
                    if (device_class == openvr.TrackedDeviceClass_Controller):
                        device_name = "controller_"+str(len(self.object_names["Controller"])+1)
                        self.object_names["Controller"].append(device_name)
                        self.devices[device_name] = vr_tracked_device(self.vr,i,"Controller")
                    elif (device_class == openvr.TrackedDeviceClass_HMD):
                        device_name = "hmd_"+str(len(self.object_names["HMD"])+1)
                        self.object_names["HMD"].append(device_name)
                        self.devices[device_name] = vr_tracked_device(self.vr,i,"HMD")
                    elif (device_class == openvr.TrackedDeviceClass_GenericTracker):
                        device_name = "tracker_"+str(len(self.object_names["Tracker"])+1)
                        self.object_names["Tracker"].append(device_name)
                        self.devices[device_name] = vr_tracked_device(self.vr,i,"Tracker")
                    elif (device_class == openvr.TrackedDeviceClass_TrackingReference):
                        device_name = "tracking_reference_"+str(len(self.object_names["Tracking Reference"])+1)
                        self.object_names["Tracking Reference"].append(device_name)
                        self.devices[device_name] = vr_tracking_reference(self.vr,i,"Tracking Reference")

    def rename_device(self,old_device_name,new_device_name):
        self.devices[new_device_name] = self.devices.pop(old_device_name)
        for i in range(len(self.object_names[self.devices[new_device_name].device_class])):
            if self.object_names[self.devices[new_device_name].device_class][i] == old_device_name:
                self.object_names[self.devices[new_device_name].device_class][i] = new_device_name

    def print_discovered_objects(self):
        for device_type in self.object_names:
            plural = device_type
            if len(self.object_names[device_type])!=1:
                plural+="s"
            print("Found "+str(len(self.object_names[device_type]))+" "+plural)
            for device in self.object_names[device_type]:
                if device_type == "Tracking Reference":
                    print("  "+device+" ("+self.devices[device].get_serial()+
                          ", Mode "+self.devices[device].get_model()+
                          ", "+self.devices[device].get_model()+
                          ")")
                else:
                    print("  "+device+" ("+self.devices[device].get_serial()+
                          ", "+self.devices[device].get_model()+")")
