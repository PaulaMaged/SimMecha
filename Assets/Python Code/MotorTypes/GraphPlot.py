import math
import threading
import time

import matplotlib.pyplot as plt

show_graph = False
flag_lock = threading.Lock()

# Initialize plotting function
def init_plotting(state_variables, window_title):
    # Initialize data lists for each state variable dynamically
    data_dict = {var: [] for var in state_variables}
    steps = []

    # Setup the plots (by columns)
    num_plots = len(state_variables)
    cols = math.ceil(num_plots/6)  # Two columns
    rows = (num_plots + cols - 1) // cols  # Calculate number of rows required

    fig, axes = plt.subplots(rows, cols, figsize=(12, 18))
    fig.canvas.manager.set_window_title(window_title)

    # Adjust spacing between plots
    plt.subplots_adjust(wspace=0.4, hspace=0.7)  # Increase wspace and hspace for more spacing

    # Dictionary to store plot lines
    plot_lines = {}

    # Flatten axes for easy indexing and rearrange it by columns
    axes = axes.T.flatten()  # Transpose first to arrange by column

    # Iterate through axes and variables to create plot lines
    for i, var_name in enumerate(state_variables):
        ax = axes[i]
        #ax.set_xlabel('Step')
        ax.set_ylabel(f'{var_name}')
        plot_lines[var_name], = ax.plot([], [], label=var_name)
        #ax.legend()

    # Hide any remaining unused subplots
    for j in range(i + 1, len(axes)):
        fig.delaxes(axes[j])

    # Return necessary components for the update function
    return fig, axes, plot_lines, data_dict, steps


# Plot all data at once
def plot_all_data(fig, axes, state_variables, state_data_per_step):
    # `state_data_per_step` is a 2D array where each row corresponds to a step, and each column to a state variable
    steps = list(range(len(state_data_per_step)))  # Create a steps list based on the number of rows (steps)

    # Transpose state data to get lists of values for each state variable across all steps
    all_states = list(zip(*state_data_per_step))

    # Iterate through each state variable and plot its values
    for i, var_name in enumerate(state_variables):
        ax = axes[i]
        ax.set_ylabel(f'{var_name}')
        ax.plot(steps, all_states[i], label=var_name)
        ax.legend()

    # Display the plot

def plot():
    global show_graph
    while True:
        if show_graph:
            plt.show()
            show_graph = False
        time.sleep(1000)

