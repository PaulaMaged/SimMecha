import os

import pybullet as p
import pybullet_data
import time
import numpy as np
from Simulation import TCP
from scipy.spatial.transform import Rotation as R
from MotorTypes import MotorMain as mot


def connect_to_pybullet():
    physicsClient = p.connect(p.GUI)
    p.setAdditionalSearchPath(pybullet_data.getDataPath())
    return physicsClient


def load_environment():
    planeId = p.loadURDF("plane.urdf")
    return planeId


def get_joint_names(robot_id):
    joint_num = p.getNumJoints(robot_id)
    joint_names = []
    for i in range(joint_num):
        joint_info = p.getJointInfo(robot_id, i)
        joint_name = joint_info[12]
        joint_name = joint_name.decode('utf-8')
        joint_names.append(joint_name)
    return joint_names

# axes orientation in unity are different than pybullet
def convert_coords(unity_coords):
    x, y, z = unity_coords
    pybullet_coords = (x, z, y)
    return pybullet_coords


def transform_quaternion(q):
    # Convert the input quaternion to a rotation matrix
    original_rotation = R.from_quat(q)
    R_matrix = original_rotation.as_matrix()

    # Permutation matrix for flipping (x, y, z) to (x, z, y)
    P = np.array([
        [1, 0, 0],
        [0, 0, 1],
        [0, 1, 0]
    ])

    # Transform the rotation matrix
    R_transformed = P.T @ R_matrix @ P

    # Convert the new rotation matrix back to quaternion
    new_rotation = R.from_matrix(R_transformed)
    new_quaternion = new_rotation.as_quat()

    return new_quaternion


def load_robot(urdf_path, position_vector, orientation_vector, scaling):
    robotId = p.loadURDF(urdf_path, convert_coords(position_vector),
                         baseOrientation=transform_quaternion(orientation_vector), globalScaling=scaling)
    return robotId


def set_gravity(gravity_vector):
    p.setGravity(*gravity_vector)


def get_joint_axis(robot_id, joint_index):
    joint_info = p.getLinkState(robot_id, joint_index)
    joint_axis = joint_info[5]  # Extract the joint axis (local frame)
    return joint_axis


def create_joint_constraint(first_obj, first_joint, second_obj, second_joint, joint_type):
    p_joint_type = None
    joint_axis = None

    if joint_type == 'prismatic':
        p_joint_type = p.JOINT_PRISMATIC
    elif joint_type == 'revolute':
        p_joint_type = p.JOINT_REVOLUTE
    else:
        p_joint_type = p.JOINT_FIXED

    if first_obj == second_obj:
        joint_info = p.getJointInfo(second_obj, second_joint)
        joint_axis = joint_info[13]  # Joint axis in the local frame

    p.createConstraint(
        parentBodyUniqueId=first_obj,
        parentLinkIndex=first_joint,
        childBodyUniqueId=second_obj,
        childLinkIndex=second_joint,
        jointType=p_joint_type,
        jointAxis=joint_axis,
        parentFramePosition=[0, 0, 0],
        childFramePosition=[0, 0, 0]
    )


def apply_external_force(robot_id, link_index, force, position, duration):
    for i in range(duration):
        p.applyExternalForce(robot_id, link_index, force, position, p.WORLD_FRAME)
        p.stepSimulation()
        time.sleep(1. / 240.)


def apply_torque(robot_id, link_index, torque, duration):
    for i in range(duration):
        p.applyExternalTorque(robot_id, link_index, torque, p.LINK_FRAME)
        p.stepSimulation()
        time.sleep(1. / 240.)


def apply_joint_torque(robot_id, joint_index, torque):
    p.setJointMotorControl2(
        bodyUniqueId=robot_id,
        jointIndex=joint_index,
        controlMode=p.TORQUE_CONTROL,
        force=torque
    )

def set_joint_speed(body_id, joint_index, target_speed, max_force):
    p.setJointMotorControl2(
        bodyUniqueId=body_id,
        jointIndex=joint_index,
        controlMode=p.VELOCITY_CONTROL,
        targetVelocity=target_speed,
        force=max_force
    )


def get_joint_position(robot_id, joint_index):
    joint_state = p.getJointState(robot_id, joint_index)
    joint_position = joint_state[0]  # The position of the joint, which is the orientation (theta)
    return joint_position


def lock_link(robot_id, joint_index, maxForce):
    # Get the current joint position (theta)
    joint_position = get_joint_position(robot_id, joint_index)

    # Set the joint motor control to lock the joint in its current position
    p.setJointMotorControl2(
        bodyUniqueId=robot_id,
        jointIndex=joint_index,
        controlMode=p.POSITION_CONTROL,
        targetPosition=joint_position,
        force=maxForce
    )


def unlock_link(robot_id, joint_index):
    p.setJointMotorControl2(
        bodyUniqueId=robot_id,
        jointIndex=joint_index,
        controlMode=p.VELOCITY_CONTROL,
        targetVelocity=0,
        force=0
    )


def py_main():
    while TCP.starting_message == "":
        continue
    print("data received")
    urls, positions, orientations, scalings = TCP.parse_robot_message(TCP.starting_message)

    physicsClient = connect_to_pybullet()
    plane_id = load_environment()

    num_joints = []

    for i in range(len(urls)):
        urdf_path = urls[i]
        robot_id = load_robot(urdf_path, positions[i], orientations[i], 3)
        num_joints.append(p.getNumJoints(robot_id))

    set_gravity([0, 0, -9.81])

    while True:
        p.stepSimulation()
        time.sleep(1. / 240.)
        if p.getConnectionInfo(physicsClient)['isConnected'] == 0:
            print("PyBullet window closed.")
            TCP.sock.close()
            print("socket should have been closed")
            os._exit(0)

def main_test():
    connect_to_pybullet()
    robot_id = p.loadURDF("C:/Users/Ayman Tarek/Desktop/pubullet_data/pybullet_data/franka_panda/panda.urdf", globalScaling=3)
    x = get_joint_names(robot_id)
    print(x)
    while True:
        p.stepSimulation()
        time.sleep(1. / 240.)


if __name__ == '__main__':
    main_test()
