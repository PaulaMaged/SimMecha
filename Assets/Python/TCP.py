import socket
import re

import bullett


# handles communication between unity and python
def start_server(host='127.0.0.1', port=65534):
    with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s:
        s.bind((host, port))
        s.listen()
        print(f'Server listening on {host}:{port}')
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