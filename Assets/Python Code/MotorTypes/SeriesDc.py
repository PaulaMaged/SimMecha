import gym_electric_motor as gem
from gym_electric_motor.physical_systems import ConstantSpeedLoad
from gym_electric_motor.reference_generators import LaplaceProcessReferenceGenerator

action_factor = 60

state_variables = ['omega', 'torque', 'i', 'u', 'u_sup']

def env(motor_parameters, i):
    # Select a different converter with default parameters by passing a keystring
    my_overridden_converter = 'Finite-2QC'

    # Update the default arguments to the voltage supply by passing a parameter dict
    my_changed_voltage_supply_args = {'u_nominal': 400.0}


    motor_params = {
        'r_a': 16e-3,  # Armature circuit resistance (Ohm)
        'r_e': 48e-3,  # Exciting circuit resistance (Ohm)
        'l_a': 19e-6,  # Armature circuit inductance (Henry)
        'l_e': 5.4e-3,  # Exciting circuit inductance (Henry)
        'l_e_prime': 1.7e-3,  # Effective excitation inductance (Henry)
        'j_rotor': 0.025  # Moment of inertia of the rotor (kg·m²)
    }

    if motor_parameters[i] is not None:
        motor_params = motor_parameters[i]

    motor = dict(motor_parameter=motor_params)

    env = gem.make(
        'Cont-SC-SeriesDc-v0',
        #load=const_speed_load,
        voltage_supply=my_changed_voltage_supply_args,
        converter=my_overridden_converter,
        motor=motor
    )
    return env

def action(step):
    return 1