import gym_electric_motor as gem
import numpy as np
from gym_electric_motor.reference_generators import LaplaceProcessReferenceGenerator, SinusoidalReferenceGenerator
import matplotlib.pyplot as plt

state_variables = [
'omega', 'torque', 'i_sa', 'i_sb', 'i_sc', 'i_sd', 'i_sq', 'u_sa', 'u_sb', 'u_sc', 'u_sd', 'u_sq', 'i_ra', 'i_rb', 'i_rc',
'i_rd', 'i_rq', 'u_ra', 'u_rb', 'u_rc', 'u_rd', 'u_rq', 'epsilon', 'u_sup'
]

def env(motor_parameters, i):
    # Select a different ode_solver with default parameters by passing a keystring
    my_overridden_solver = 'scipy.solve_ivp'

    # Update the default arguments to the voltage supply by passing a parameter dict
    my_changed_voltage_supply_args = {'u_nominal': 400.0}

    motor_params = {
        'r_s': 12.42,  # Stator resistance (Ohm)
        'r_r': 3.51,  # Rotor resistance (Ohm)
        'l_m': 297.5e-3,  # Main inductance (Henry)
        'l_sigs': 25.71e-3,  # Stator-side stray inductance (Henry)
        'l_sigr': 25.71e-3,  # Rotor-side stray inductance (Henry)
        'p': 2,  # Pole pair number
        'j_rotor': 13.695e-4  # Moment of inertia of the rotor (kg·m²)
    }

    if len(motor_parameters) > i and motor_parameters[i] is not None:
        motor_params = motor_parameters[i]

    motor = dict(motor_parameter=motor_params)

    env = gem.make(
        'Cont-SC-DFIM-v0',
        voltage_supply=my_changed_voltage_supply_args,
        ode_solver=my_overridden_solver,
        motor=motor
    )

    return env

# 6 voltages
def action(step, time_step=0.001, amplitude=1.0, frequency=5.0):
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
    return [u_a, u_b, u_c, u_a, u_b, u_c]