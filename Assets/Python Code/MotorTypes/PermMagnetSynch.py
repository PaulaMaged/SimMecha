import gym_electric_motor
import gym_electric_motor as gem
import numpy as np
from gym_electric_motor.reference_generators import LaplaceProcessReferenceGenerator, SinusoidalReferenceGenerator, \
    WienerProcessReferenceGenerator, MultipleReferenceGenerator, ConstReferenceGenerator
import matplotlib.pyplot as plt
from MotorTypes import MotorMain as mot


state_variables = ['omega', 'torque', 'i_sd', 'i_sq', 'i_a', 'i_b', 'i_c', 'u_sd', 'u_sq', 'u_a', 'u_b', 'u_c', 'u_sup']

def env(motor_parameters, i):
    # Select a different ode_solver with default parameters by passing a keystring
    my_overridden_solver = 'scipy.solve_ivp'

    # Update the default arguments to the voltage supply by passing a parameter dict
    my_changed_voltage_supply_args = {'u_nominal': 400}

    voltage = gym_electric_motor.physical_systems.voltage_supplies.IdealVoltageSupply(u_nominal=600.0)

    motor_params = {
        'r_s': 18e-2,  # Stator resistance (Ohm)
        'l_d': 0.37e-2,  # Direct axis inductance (Henry)
        'l_q': 1.2e-2,  # Quadrature axis inductance (Henry)
        'p': 3,  # Pole pair number
        'j_rotor': 0.03883  # Moment of inertia of the rotor (kg·m²)
    }

    if motor_parameters[i] is not None:
        motor_params = motor_parameters[i]

    motor = dict(motor_parameter=motor_params)

    env = gem.make(
        'Cont-SC-PMSM-v0',
        voltage_supply=voltage,
        ode_solver=my_overridden_solver,
        motor=motor,
    )

    return env

x = 1
y = 0
# 3-phase voltages
def action(step, time_step=0.01, amplitude=1, frequency=1.0):
    global x, y
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