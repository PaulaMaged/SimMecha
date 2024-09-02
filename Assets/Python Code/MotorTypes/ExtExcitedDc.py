import gym_electric_motor as gem
from gym_electric_motor.physical_systems import ConstantSpeedLoad
from gym_electric_motor.reference_generators import LaplaceProcessReferenceGenerator

action_factor = 60

state_variables = ['omega', 'torque', 'i_a', 'i_e', 'u_a', 'u_e', 'u_sup']


def env(motor_parameters, i):

    # Select a different ode_solver with default parameters by passing a keystring
    my_overridden_solver = 'scipy.solve_ivp'

    # Update the default arguments to the voltage supply by passing a parameter dict
    my_changed_voltage_supply_args = {'u_nominal': 400.0}

    # Replace the reference generator by passing a new instance
    my_new_ref_gen_instance = LaplaceProcessReferenceGenerator(
        reference_state='omega',
        sigma_range=(1e-3, 1e-2)
    )

    motor_params = {
        'r_a': 1,  # Armature circuit resistance (Ohm)
        'r_e': 1,  # Exciting circuit resistance (Ohm)
        'l_a': 19e-6,  # Armature circuit inductance (Henry)
        'l_e': 5.4e-3,  # Exciting circuit inductance (Henry)
        'l_e_prime': 1.7e-3,  # Effective excitation inductance (Henry)
        'j_rotor': 0.025  # Moment of inertia of the rotor (kg·m²)
    }

    if len(motor_parameters) > i and motor_parameters[i] is not None:
        motor_params = motor_parameters[i]

    motor = dict(motor_parameter=motor_params)

    env = gem.make(
        'Cont-SC-ExtExDc-v0',
        voltage_supply=my_changed_voltage_supply_args,
        ode_solver=my_overridden_solver,
        reference_generator=my_new_ref_gen_instance,
        motor=motor
    )
    return env


x = 60

# 2 voltages
def action(step):
    global x

    # First 1000 steps positive
    if step < 1000:
        x = 60
    else:
        # After 1000 steps, toggle sign every 2000 steps
        if ((step - 1000) // 2000) % 2 == 0:
            x = -60  # Negative for these 2000 steps
        else:
            x = 60   # Positive for the next 2000 steps

    actions = [x / 60, 60 / 60]
    return actions
