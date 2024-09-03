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
    global all_volts_list, step
    step += 1

    if step < 1000:
        x = 60
    else:
        if ((step - 1000) // 2000) % 2 == 0:
            x = -60
        else:
            x = 60
    volts0 = [x, 60]

    all_volts_list = [volts0]