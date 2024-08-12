import multiprocessing
import threading

import TCP
import bullett

def main():

    # Create and start processes
    pybullet_process = threading.Thread(target=bullett.py_main)
    tcp_process = threading.Thread(target=TCP.start_server)

    pybullet_process.start()
    tcp_process.start()

    # Join processes to ensure they complete
    pybullet_process.join()
    # tcp_process.join()


if __name__ == '__main__':
    main()
