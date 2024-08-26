import os

import pybullet as p
import time

from Simulation import bullett as b
from Simulation import TCP
from MotorTypes import MotorMain as mot

robot_id = []
link_names = []

physicsClient = None

correspond_robot_num, correspond_joint_num = [], []

def convert_link_names(links_received):
    for i, link in enumerate(links_received):
        robot_index = correspond_robot_num[i]
        links = link_names[robot_index]
        try:
            joint_ind = links.index(link)
            correspond_joint_num.append(joint_ind)
        except ValueError:
            print(f'Link name "{link}" not found!')

    print('correspond_joint_num:', correspond_joint_num)

def init():
    global robot_id, correspond_robot_num, correspond_joint_num, physicsClient, link_names
    while len(TCP.messages) == 0:
        continue
    time.sleep(1)
    TCP.split_message()
    TCP.starting_flag = False

    urls, positions, orientations, scalings = TCP.parse_robot_message(TCP.messages)

    physicsClient = b.connect_to_pybullet()
    plane_id = b.load_environment()
    b.set_gravity([0, 0, -9.80665])

    for i in range(len(urls)):
        robot_id.append(b.load_robot(urls[i], positions[i], orientations[i], scalings[i] * 3))
        link_names.append(b.get_joint_names(robot_id[i]))
        b.create_joint_constraint(robot_id[i], -1, plane_id, -1, p.JOINT_FIXED, 0)
        #for j in range(p.getNumJoints(robot_id[i])):
            #b.lock_link(robot_id[i], j, 1000)

    mot.motorNames, correspond_robot_num, links_received, mot.motor_params = TCP.parse_motor_message(TCP.messages)
    convert_link_names(links_received)

    mot.motor_process = True


def update():
    global robot_id, correspond_robot_num, correspond_joint_num

    for i in range(len(mot.motorClasses)):
        robot = robot_id[correspond_robot_num[i]]
        b.set_joint_speed(robot, correspond_joint_num[i], mot.omega[i], mot.torque[i])
        #b.unlock_link(robot, correspond_joint_num[i])
        p.stepSimulation()
        time.sleep(1. / 240.)

        if p.getConnectionInfo(physicsClient)['isConnected'] == 0:
            print("PyBullet window closed.")
            TCP.sock.close()
            print("socket should have been closed")
            os._exit(0)

def main():
    init()
    while True:
        update()



if __name__ == '__main__':
    main()