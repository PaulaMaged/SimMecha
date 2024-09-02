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

def convert_link_names(links_received, robot_nums):
    joint_nums = []
    for i, link in enumerate(links_received):
        if link == 'plane':
            joint_nums.append(-1)
            continue
        robot_index = robot_nums[i]
        links = link_names[robot_index]
        try:
            joint_ind = links.index(link)
            joint_nums.append(joint_ind - 1)
        except ValueError:
            print(f'Link name "{link}" not found!')

    return joint_nums

def init():
    global robot_id, correspond_robot_num, correspond_joint_num, physicsClient, link_names
    while len(TCP.messages) == 0:
        continue
    time.sleep(3)
    TCP.split_message()
    TCP.starting_flag = False

    urls, positions, orientations, scalings = TCP.parse_robot_message(TCP.messages)

    physicsClient = b.connect_to_pybullet()
    plane_id = b.load_environment()
    b.set_gravity([0, 0, -9.80665])

    for i in range(len(urls)):
        robot_id.append(b.load_robot(urls[i], positions[i], orientations[i], scalings[i]))
        link_names.append(b.get_link_names(robot_id[i]))

    mot.motorNames, correspond_robot_num, links_received, mot.motor_params = TCP.parse_motor_message(TCP.messages)

    correspond_joint_num = convert_link_names(links_received, correspond_robot_num)
    robot1_nums, robot2_nums, links1, links2, constraint_types = TCP.parse_constraint_message(TCP.messages)

    joint1_nums = convert_link_names(links1, robot1_nums)
    joint2_nums = convert_link_names(links2, robot2_nums)

    # convert_link_names
    for i in range(len(robot1_nums)):
        robot1 = plane_id if robot1_nums[i] == -1 else robot_id[robot1_nums[i]]
        robot2 = plane_id if robot2_nums[i] == -1 else robot_id[robot2_nums[i]]
        b.create_joint_constraint(robot1, joint1_nums[i], robot2, joint2_nums[i], constraint_types[i])

    if len(mot.motorNames) > 0:
        mot.motor_process = True
        print('motor process started')


def update():
    global robot_id, correspond_robot_num, correspond_joint_num
    if len(mot.motorClasses) > 0:
        for i in range(len(mot.motorClasses)):
            robot = robot_id[correspond_robot_num[i]]
            b.set_joint_speed(robot, correspond_joint_num[i], mot.omega[i], mot.torque[i])

    p.stepSimulation()
    time.sleep(1. / 240.)

    if p.getConnectionInfo(physicsClient)['isConnected'] == 0:
        TCP.sock.close()
        os._exit(0)


def main():
    init()
    while True:
        update()



if __name__ == '__main__':
    main()