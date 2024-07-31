import threading

import TCP
import bullett


def main():
    # Create and start threads
    pybullet_thread = threading.Thread(target=bullett.main)
    tcp_thread = threading.Thread(target=TCP.start_server)

    pybullet_thread.start()
    tcp_thread.start()

    # Join threads to ensure they complete
    pybullet_thread.join()
    tcp_thread.join()


if __name__ == '__main__':
    main()
