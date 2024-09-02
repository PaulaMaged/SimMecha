import gym_electric_motor as gem
import numpy as np
from gym_electric_motor.reference_generators import LaplaceProcessReferenceGenerator, SinusoidalReferenceGenerator
import matplotlib.pyplot as plt

action_factor = 200

state_variables = ['omega', 'torque', 'i_sa', 'i_sb', 'i_sc', 'i_sd', 'i_sq', 'u_sa', 'u_sb', 'u_sc', 'u_sd', 'u_sq', 'epsilon', 'u_sup']

def env(motor_parameters, i):
    # Select a different ode_solver with default parameters by passing a keystring
    my_overridden_solver = 'scipy.solve_ivp'

    # Update the default arguments to the voltage supply by passing a parameter dict
    my_changed_voltage_supply_args = {'u_nominal': 400.0}

    motor_params = {
        'r_s': 0.67,  # Stator resistance (Ohm)
        'l_d': 10.1e-1,  # Direct axis inductance (Henry)
        'l_q': 4.1e-3,  # Quadrature axis inductance (Henry)
        'p': 4,  # Pole pair number
        'j_rotor': 0.8e-4  # Moment of inertia of the rotor (kg·m²)
    }

    if len(motor_parameters) > i and motor_parameters[i] is not None:
        motor_params = motor_parameters[i]

    motor = dict(motor_parameter=motor_params)

    env = gem.make(
        'Cont-SC-SynRM-v0',
        voltage_supply=my_changed_voltage_supply_args,
        ode_solver=my_overridden_solver,
        motor=motor
    )
    return env

# 3-phase voltages
def action(step, time_step=0.001, amplitude=1.0, frequency=50.0):
    # Calculate time based on the current step
    t = step * time_step

    # Angular frequency ω = 2π * frequency
    omega = 2 * np.pi * frequency

    # Phase differences between the three phases (in radians)
    phase_a = 0.0
    phase_b = 2 * np.pi / 3  # 120 degrees
    phase_c = -2 * np.pi / 3  # 120 degrees

    # Generate sinusoidal voltages for each phase
    u_a = amplitude * np.sin(omega * t + phase_a)
    u_b = amplitude * np.sin(omega * t + phase_b)
    u_c = amplitude * np.sin(omega * t + phase_c)
    return [u_a, u_b, u_c]