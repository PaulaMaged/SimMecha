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
                messages.append(curr_message)
                print(f"current message: {curr_message}")
                #split_message()


def split_message():
    global messages
    i = 0
    while i < len(messages):
        # Check if there's a '\n' in the current message
        while '\n' in messages[i]:
            # Split the message at the first occurrence of '\n'
            split_parts = messages[i].split('\n', 1)  # Split into exactly 2 parts

            # Update the current message to the first part
            messages[i] = split_parts[0]

            # Insert the second part in the next index, and shift the rest
            messages.insert(i + 1, split_parts[1])

        # Move to the next message
        i += 1

    for msg in messages:
        if msg == '':
            messages.remove(msg)

    print(f"Number of Parts: {len(messages)}")
    print("Split Parts:")

    max_length = max(len(msg) for msg in messages)
    for msg in messages:
        print(f"{msg.ljust(max_length)}")


def parse_robot_message(messages):
    # Regular expressions to find numbers and patterns in the message
    url_pattern = r'[A-Za-z]:\\(?:[\w\s]+\\)*[\w\s]+\.[\w]+'
    position_pattern = r'\(([\d\.\-]+),\s*([\d\.\-]+),\s*([\d\.\-]+)\)'
    orientation_pattern = r'\(([\d\.\-]+),\s*([\d\.\-]+),\s*([\d\.\-]+),\s*([\d\.\-]+)\)'
    scaling_pattern = r',\s*([\d\.\-]+)\s*$'  # Pattern to match the last number (scaling value, can be float)

    # Initialize lists to hold the parsed data
    urls = []
    positions = []
    orientations = []
    scalings = []

    i = 0
    while i < len(messages):
        line = messages[i]

        # Check if the line matches the expected format
        if re.search(url_pattern, line) or re.search(position_pattern, line) or re.search(orientation_pattern, line):
            # Extract matches for each pattern
            url_matches = re.findall(url_pattern, line)
            position_matches = re.findall(position_pattern, line)
            orientation_matches = re.findall(orientation_pattern, line)
            scaling_matches = re.findall(scaling_pattern, line)

            # Convert matched strings to appropriate types and store them in lists
            urls.extend(url_matches)
            positions.extend([list(map(float, match)) for match in position_matches])
            orientations.extend([list(map(float, match)) for match in orientation_matches])
            scalings.extend([float(match) for match in scaling_matches])

            # Remove the processed line from the messages list
            del messages[i]
        else:
            # Move to the next line if the current one does not match the expected format
            i += 1

    # Print or return the parsed data
    print("Urls:", urls)
    print("Positions:", positions)
    print("Orientations:", orientations)
    print("Scalings:", scalings)

    return urls, positions, orientations, scalings


def parse_motor_message(messages):
    # Define regular expressions for each component
    motor_name_pattern = r'^\((\w+)'  # Matches motor name at the start of the line
    robot_num_pattern = r',\s*(\d+)'  # Matches robot number after a comma
    link_pattern = r',\s*(\w+_link\d+)'  # Matches link names like panda_link1
    params_pattern = r'\{(.+)\}'  # Matches the dictionary of motor parameters

    # Initialize lists to hold the parsed data
    motorNames = []
    correspond_robot_num = []
    correspond_links = []
    motor_params = []

    i = 0
    while i < len(messages):
        line = messages[i]

        # Extract motor name
        motor_name_match = re.search(motor_name_pattern, line)
        robot_num_match = re.search(robot_num_pattern, line)
        link_match = re.search(link_pattern, line)
        params_match = re.search(params_pattern, line)

        if motor_name_match and robot_num_match and link_match and params_match:
            # Store the extracted data
            motorNames.append(motor_name_match.group(1))
            correspond_robot_num.append(int(robot_num_match.group(1)))
            correspond_links.append(link_match.group(1))

            # Parse motor parameters as a dictionary
            param_str = '{' + params_match.group(1) + '}'
            try:
                param_dict = ast.literal_eval(param_str)
                motor_params.append(param_dict)
            except (ValueError, SyntaxError) as e:
                print(f"Error parsing motor parameters: {e}")
                # Skip this message if parsing fails, move to the next line
                i += 1
                continue

            # Remove the processed line from the messages list
            del messages[i]
        else:
            # Move to the next line if the current one does not match the expected format
            i += 1

    # Print final parsed results
    print("Motor Names:", motorNames)
    print("Corresponding Robot Numbers:", correspond_robot_num)
    print("Corresponding Links:", correspond_links)
    print("Motor Parameters:", motor_params)

    return motorNames, correspond_robot_num, correspond_links, motor_params


def parse_constraint_message(messages):
    # Initialize lists for each data type
    robot1_nums = []
    robot2_nums = []
    robot1_links = []
    robot2_links = []
    constraint_types = []

    # Updated regular expression to match the constraint pattern, including negative numbers
    pattern = r'\((-?\d+),\s*(-?\d+),\s*\'?([^\']+?)\'?,\s*\'?([^\']+?)\'?,\s*\'?([^\']+?)\'?\)'

    # Iterate over all the messages and extract constraints
    messages_to_remove = []
    for message in messages:
        match = re.match(pattern, message)
        if match:
            robot1_num, robot2_num, robot1_link, robot2_link, constraint_type = match.groups()

            # Append values to corresponding lists
            robot1_nums.append(int(robot1_num))
            robot2_nums.append(int(robot2_num))
            robot1_links.append(robot1_link)
            robot2_links.append(robot2_link)
            constraint_types.append(constraint_type)

            # Mark message for removal after successful processing
            messages_to_remove.append(message)

    # Remove processed messages from the original messages list
    for message in messages_to_remove:
        messages.remove(message)

    # Print the lists inside the function
    print("robot1_nums:", robot1_nums)
    print("robot2_nums:", robot2_nums)
    print("robot1_links:", robot1_links)
    print("robot2_links:", robot2_links)
    print("constraint_types:", constraint_types)

    # Return the lists
    return robot1_nums, robot2_nums, robot1_links, robot2_links, constraint_types


if __name__ == '__main__':
    messages = [
        "C:\\Users\\Ayman Tarek\\Desktop\\pubullet_data\\pybullet_data\\franka_panda\\panda.urdf, (0.00, 5.00, 0.00), (0.00000, 0.00000, 0.00000, 1.00000), 1",
        "C:\\Users\\Ayman Tarek\\Desktop\\pubullet_data\\pybullet_data\\franka_panda\\panda.urdf, (0.00, 5.00, 0.00), (0.00000, 0.00000, 0.00000, 1.00000), 1",
        "(ExtExcitedDc, 0, panda_link1, {'r_a': 1, 'r_e': 1, 'l_a': 1.9000000000000001E-05, 'l_e': 0.0054000000000000003, 'l_e_prime': 0.0016999999999999999, 'j_rotor': 0.025000000000000001})",
        "(ExtExcitedDc, 0, panda_link2, {'r_a': 1, 'r_e': 1, 'l_a': 1.9000000000000001E-05, 'l_e': 0.0054000000000000003, 'l_e_prime': 0.0016999999999999999, 'j_rotor': 0.025000000000000001})",
        "(1, -1, link1, link2, p.FIXEDCONSTRAINT)",
        "(-4, 4, link3, link4, revolute)",
        "(5, 6, link5, 'link6, prismatic)",
        "(7, 8, link5, 'link6, prismatic)"
    ]

    parse_constraint_message(messages)