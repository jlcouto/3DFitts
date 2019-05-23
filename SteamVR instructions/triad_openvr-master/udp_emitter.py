import triad_openvr
import time
import sys
import struct
import socket

desiredController = "controller_1_citi"

sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
server_address = ('127.0.0.1', 8051)

v = triad_openvr.triad_openvr()
v.print_discovered_objects()

if len(sys.argv) == 1:
    interval = 1/250
elif len(sys.argv) == 2:
    interval = 1/float(sys.argv[1])
else:
    print("Invalid number of arguments")
    interval = False
print("Interval ", interval)
print("Sending data...")
    
if interval:
    while(True):
        start = time.time()            
        data = v.devices[desiredController].get_pose_quaternion()
        trigger = v.devices[desiredController].get_controller_inputs()["trigger"];
        txt = "Trigger: %d" % trigger;
        print("\r" + txt, end="")  
        data.append(trigger)
        sent = sock.sendto(struct.pack('d'*len(data), *data), server_address)
        sleep_time = interval-(time.time()-start)
        if sleep_time>0:            
            time.sleep(sleep_time)