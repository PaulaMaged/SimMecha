import gym_electric_motor as gem
import numpy as np
from gym_electric_motor.reference_generators import LaplaceProcessReferenceGenerator
from gym_electric_motor.physical_systems import DcMotorSystem, DcPermanentlyExcitedMotor, ConstantSpeedLoad, \
    PolynomialStaticLoad, ExternalSpeedLoad, OrnsteinUhlenbeckLoad

state_variables = ['omega', 'torque', 'i', 'u', 'u_sup']

def env(motor_parameters, i):
    # Select a different converter with default parameters by passing a keystring
    my_overridden_converter = 'Cont-2QC'

    # Update the default arguments to the voltage supply by passing a parameter dict
    my_changed_voltage_supply_args = {'u_nominal': 400.0}

    # Replace the reference generator by passing a new instance
    my_new_ref_gen_instance = LaplaceProcessReferenceGenerator(
        reference_state='omega',
        sigma_range=(10, 100)
    )

    motor_params = {
        'r_a': 1.5,  # Armature resistance (Ohm)
        'l_a': 0.8e-3,  # Armature inductance (Henry)
        'j_rotor': 0.015,  # Rotor inertia (kg·m²)
    }

    if len(motor_parameters) > i and motor_parameters[i] is not None:
        motor_params = motor_parameters[i]

    motor = dict(motor_parameter=motor_params)


    env = gem.make(
        'Cont-SC-PermExDc-v0',
        voltage_supply=my_changed_voltage_supply_args,
        converter=my_overridden_converter,
        motor=motor,
    )
    return env

def action(step):
    return [12/60]