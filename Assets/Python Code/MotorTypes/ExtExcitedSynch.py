import gym_electric_motor as gem
import numpy as np
from gym_electric_motor.reference_generators import LaplaceProcessReferenceGenerator, SinusoidalReferenceGenerator
import matplotlib.pyplot as plt

action_factor = 200

state_variables = ['omega', 'torque', 'i_sd', 'i_sq', 'i_a', 'i_b', 'i_c', 'i_e', 'u_sd', 'u_sq', 'u_a', 'u_b', 'u_c', 'u_e', 'u_sup']

def env(motor_parameters, i):

    # Select a different ode_solver with default parameters by passing a keystring
    my_overridden_solver = 'scipy.solve_ivp'

    # Update the default arguments to the voltage supply by passing a parameter dict
    my_changed_voltage_supply_args = {'u_nominal': 400.0}

    motor_params = {
        'r_s': 15.55e-1,  # Stator resistance (mOhm)
        'r_e': 7.2e-1,  # Excitation resistance (mOhm)
        'l_d': 1.66e-1,  # Direct axis inductance (mH)
        'l_q': 0.35e-1,  # Quadrature axis inductance (mH)
        'l_m': 1.589e-1,  # Mutual inductance (mH)
        'l_e': 1.74e-1,  # Excitation inductance (mH)
        'p': 3,  # Pole pair number
        'j_rotor': 0.03883  # Moment of inertia of the rotor (kg·m²)
    }

    if len(motor_parameters) > i and motor_parameters[i] is not None:
        motor_params = motor_parameters[i]

    motor = dict(motor_parameter=motor_params)


    env = gem.make(
        'Cont-SC-EESM-v0',
        voltage_supply=my_changed_voltage_supply_args,
        ode_solver=my_overridden_solver,
        motor=motor
    )

    return env

# 4 voltages
def action(step):
    actions = [1, 0, -0.5, 1]
    return actions