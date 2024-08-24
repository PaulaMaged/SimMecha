import multiprocessing
import threading
from Simulation import TCP, bullett
from MotorTypes import MotorMain as mot, GraphPlot as graph
from Simulation import BulletTest as b
import threading


def bullet():
    b.init()
    while True:
        b.update()

def motor():
    mot.init()
    while True:
        mot.update()


def main():
    # Create and start processes
    bullet_thread = threading.Thread(target=bullet)
    motor_thread = threading.Thread(target=motor)
    plot_thread = threading.Thread(target=graph.plot)
    tcp_thread = threading.Thread(target=TCP.start_server)

    bullet_thread.start()
    motor_thread.start()
    plot_thread.start()
    tcp_thread.start()

    # Join processes to ensure they complete
    bullet_thread.join()
    motor_thread.join()
    plot_thread.join()
    #tcp_thread.join()


if __name__ == '__main__':
    main()
