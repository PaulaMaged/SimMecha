import ast
import socket
import re
from Simulation import bullett

sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
starting_message = ""
curr_message = ""
messages = []
starting_flag = True
# handles communication between unity and python
def start_server(host="127.0.0.1", port=300):
    global starting_message, curr_message
    with sock as s:
        #This line sets the SO_REUSEADDR option on the socket,
        # which allows the server to bind to the port even if it is in the TIME_WAIT state after being closed.
        s.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)

        s.bind((host, port))
        s.listen()
        conn, addr = s.accept()
        with conn:
            print(f'Connected by {addr}')
            while True:
                data = conn.recv(1024)
                if not data:
                    break
                curr_message = data.decode()
                if starting_flag:
                    starting_message += curr_message

                print(f"starting message: {starting_message}")
                print(f"current message: {curr_message}")


def split_message():
    global messages
    # Split the message by '\n' into as many parts as there are occurrences of '\n'
    messages = starting_message.split('\n')

    # Print or return the parts as an array of strings
    print(f"Number of Parts: {len(messages)}")
    print("Split Parts:", messages)


def parse_robot_message(message):
    # Regular expressions to find numbers in the message
    url_pattern = r'[A-Za-z]:\\(?:[\w\s]+\\)*[\w\s]+\.[\w]+'
    position_pattern = r'\(([\d\.\-]+), ([\d\.\-]+), ([\d\.\-]+)\)'
    orientation_pattern = r'\(([\d\.\-]+), ([\d\.\-]+), ([\d\.\-]+), ([\d\.\-]+)\)'
    scaling_pattern = r',,\s*([\d\.\-]+)'  # Pattern to match the last number (scaling value, can be float)

    # Find all matches for url, position, orientation, and scaling
    url_matches = re.findall(url_pattern, message)
    position_matches = re.findall(position_pattern, message)
    orientation_matches = re.findall(orientation_pattern, message)
    scaling_matches = re.findall(scaling_pattern, message)

    urls = []
    positions = []
    orientations = []
    scalings = []

    # Convert matched strings to appropriate types and store them in lists
    for match in url_matches:
        urls.append(match)

    for match in position_matches:
        positions.append([float(num) for num in match])

    for match in orientation_matches:
        orientations.append([float(num) for num in match])

    for match in scaling_matches:
        scalings.append(float(match))  # Convert scaling value to float

    print("Urls:", urls)
    print("Positions:", positions)
    print("Orientations:", orientations)
    print("Scalings:", scalings)

    return urls, positions, orientations, scalings


def parse_motor_message(message):
    # Regular expression for extracting arrays inside parentheses
    array_pattern = r'\(([^)]+)\)'  # Matches anything inside parentheses

    # Find all arrays in the message
    matches = re.findall(array_pattern, message)

    motorNames = []
    correspond_robot_num = []
    correspond_links = []
    motor_params = []

    if len(matches) == 4:  # Now expecting 4 arrays in the message
        # Extract and split the elements from each matched array
        motorNames = [name.strip() for name in matches[0].split(',')]
        correspond_robot_num = [int(num.strip()) for num in matches[1].split(',')]
        correspond_links = [link.strip() for link in matches[2].split(',')]

        # Parse the motor parameters from the dictionary-like string
        motor_params_str = matches[3].split('},')
        for param_str in motor_params_str:
            # Make sure to re-add the closing brace '}' if it's been split off
            if '}' not in param_str:
                param_str = param_str + '}'

            # Use ast.literal_eval to safely evaluate the dictionary string
            param_dict = ast.literal_eval(param_str.strip())
            motor_params.append(param_dict)

    # Print or return the parsed data
    print("Motor Names:", motorNames)
    print("Corresponding Robot Numbers:", correspond_robot_num)
    print("Corresponding Links:", correspond_links)
    print("Motor Parameters:", motor_params)

    return motorNames, correspond_robot_num, correspond_links, motor_params


if __name__ == '__main__':
    # Modify the message to use a single comma to separate arrays
    message = "(PermMagnetSynch, PermMagnetSynch), (0,0), (panda_link1, panda_link2), ({'r_s': 18e-2, 'l_d': 0.37e-2, 'l_q': 1.2e-2, 'p': 3, 'j_rotor': 0.03883}, {'r_s': 20e-2, 'l_d': 0.4e-2, 'l_q': 1.5e-2, 'p': 4, 'j_rotor': 0.040})"
    parse_motor_message(message)