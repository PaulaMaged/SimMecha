import time

import matplotlib.pyplot as plt

from MotorTypes.GraphPlot import init_plotting
from Simulation import bullett, BulletTest, TCP
from MotorTypes import (ExtExcitedDc, ExtExcitedSynch, SeriesDc, ShuntDc, SynchReluctance, SquirrelCageInduction, PermExcitedDc, PermMagnetSynch, DoublyFedInduction, GraphPlot)

env, env_limits, state_variables, motorClasses = [], [], [], []

motorNames, motor_params = [], []  # should take them from unity at start

torque, omega, terminated = [], [], []

fig, axes, plot_lines, data_dict, steps, all_states = [], [], [], [], [], []
step = -1

motor_process = False

def init():
    global env, state_variables, env_limits, motorClasses, omega, torque, motorNames, fig, axes, plot_lines, data_dict, steps, all_states
    while not motor_process:
        time.sleep(1)
        continue

    for i, motorName in enumerate(motorNames):
        omega.append(0)
        torque.append(0)
        terminated.append(True)

        class_name = motorName
        motor = globals()[class_name]
        motorClasses.append(motor)

        env.append(motor.env(motor_params, i))

        state_variables.append(motor.state_variables)
        env_limits.append(env[i].limits)
        all_states.append([])

        init_graphs(i, f'{i+1} - {motorName}')


def update():
    global motorClasses, env, state_variables, env_limits, step, terminated, omega, torque, fig, axes, plot_lines, data_dict, steps, all_states
    step += 1

    for i, motorClass in enumerate(motorClasses):
        if terminated[i]:
            state, reference = env[i].reset()

        action = motorClass.action(step)
        (state, reference), reward, terminated[i], truncated, _ = env[i].step(action)

        # Denormalize state variables using limits
        real_state = state * env_limits[i]
        omega[i] = real_state[0]
        torque[i] = real_state[1]
        # Print state variables
        state_values = ", ".join(f"{name}: {value}" for name, value in zip(state_variables[i], real_state))
        print(f"Motor {i+1}, Step {step}: {state_values}")
        update_graphs(i, 5000, real_state)


def init_graphs(i, motorName):
    f, ax, pl, data, st = init_plotting(state_variables[i], motorName)
    fig.append(f)
    axes.append(ax)
    plot_lines.append(pl)
    data_dict.append(data)
    steps.append(st)

def update_graphs(i, max_step, real_state):
    if step <= max_step:
        all_states[i].append(real_state)
    if step == max_step:
        GraphPlot.plot_all_data(fig[i], axes[i], state_variables[i], all_states[i])
        if i == len(motorClasses) - 1:
            plt.show()

def main():
    init()
    while True:
        update()



if __name__ == "__main__":
    main()

