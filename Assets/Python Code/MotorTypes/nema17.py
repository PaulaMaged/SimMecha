import gym_electric_motor as gem
import numpy as np
from gym_electric_motor.physical_systems.electric_motors import DcMotor
from gym_electric_motor.reference_generators import LaplaceProcessReferenceGenerator
from scipy.integrate import odeint
import matplotlib.pyplot as plt


dc_motor_params = {
    'r_a': 16e-3,      # Armature circuit resistance (Ohm)
    'r_e': 16e-2,      # Exciting circuit resistance (Ohm)
    'l_a': 19e-6,      # Armature circuit inductance (Henry)
    'l_e': 5.4e-3,     # Exciting circuit inductance (Henry)
    'l_e_prime': 1.7e-3,  # Effective excitation inductance (Henry)
    'j_rotor': 0.025,  # Moment of inertia of the rotor (kg*m^2)
}
nominal_values = {
    'i_a': 1.0,        # Nominal armature current (A)
    'i_e': 0.5,        # Nominal exciting current (A)
    'omega': 75.0,    # Nominal angular velocity (rad/s)
    'torque': 0.5,     # Nominal motor-generated torque (Nm)
    'u_a': 12.0,       # Nominal armature voltage (V)
    'u_e': 5.0         # Nominal exciting voltage (V)
}

motor = dict(motor_parameter=dc_motor_params, nominal_values=nominal_values)



# Select a different converter with default parameters by passing a keystring
my_overridden_converter = 'Cont-2QC'


env = gem.make(
    'Cont-SC-SeriesDc-v0',
    #load=const_speed_load,
    #voltage_supply=my_changed_voltage_supply_args,
    converter=my_overridden_converter,
    motor=motor,
    #reference_generator=my_new_ref_gen_instance
    )


env_limits = env.limits
state_variables = ['omega', 'torque', 'i', 'u', 'u_sup']

# Simulation setup
terminated = True
for step in range(400):
    if terminated:
        state, reference = env.reset()

    random_action = env.action_space.sample()
    print('random action: ', random_action)

    action = 1
    (state, reference), reward, terminated, truncated, _ = env.step(random_action)

    # Denormalize state variables using limits
    real_state = state * env_limits

    # Print state variables
    state_values = ", ".join(f"{name}: {value}" for name, value in zip(state_variables, real_state))
    print(f"Step {step}: {state_values}")
    print('reference:', reference)
