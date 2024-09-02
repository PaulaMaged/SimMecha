# This script can be edited by the user

# do not change this section

import time
motor_classes = []


print("Hello, I am the user script!")

all_volts_list = []

def init():
    global all_volts_list
    for i in range(len(motor_classes)):
        all_volts_list.append([])


step = -1

# you should define all motor voltages here and put each motor volts in a list inside all_volts_list

def update_motor_voltages():
    step += 1
    
    # put your voltage logic here
    # example, if you have 2 motors, a Permanently Magnetic Synchronous motor that takes 3 volts (3 phase) and Externally Excited Dc motor that takes 2 volts
    # you should return something in this form
    # all_volts_list = [[float1, float2, float3], [float4, float5]]