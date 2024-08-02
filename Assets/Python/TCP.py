import socket
import re

import bullett

sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)

# handles communication between unity and python
def start_server(host='127.0.0.1', port=65432):
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
                print('Received:', data.decode())
                bullett.received_data = data.decode()


def parse_message(message):
    # Regular expressions to find numbers in the message
    position_pattern = r'\(([\d\.\-]+), ([\d\.\-]+), ([\d\.\-]+)\)'
    orientation_pattern = r'\(([\d\.\-]+), ([\d\.\-]+), ([\d\.\-]+), ([\d\.\-]+)\)'

    # Find all matches for position and orientation
    position_matches = re.findall(position_pattern, message)
    orientation_matches = re.findall(orientation_pattern, message)

    positions = []
    orientations = []

    # Convert matched strings to floats and store them in lists
    for match in position_matches:
        positions.append([float(num) for num in match])

    for match in orientation_matches:
        orientations.append([float(num) for num in match])

    print("Positions:", positions)
    print("Orientations:", orientations)
    return positions, orientations
