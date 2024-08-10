import multiprocessing
import threading

import TCP
import bullett

def main():

    # Create and start processes
    pybullet_thread = threading.Thread(target=bullett.py_main)
    tcp_thread = threading.Thread(target=TCP.start_server)

    pybullet_thread.start()
    tcp_thread.start()

    # Join processes to ensure they complete
    pybullet_thread.join()
    tcp_thread.join()


if __name__ == '__main__':
    main()
