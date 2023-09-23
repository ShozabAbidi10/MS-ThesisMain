#!/usr/bin/env python3

import threading 
import socket
import sys
import time
import platform  
import math
import random
import actionlib
import subprocess
import signal
import numpy as np
import roslib
import rospy
from timeit import default_timer as timer
from datetime import timedelta
from geometry_msgs.msg import Twist, Point, Pose
from std_msgs.msg import Empty
from std_msgs.msg import String
from std_msgs.msg import Float64
from nav_msgs.msg import Odometry
from unity_robotics_demo_msgs.msg import CurrentDronePose, TargetPose, StatusColor
from tello_driver.msg import TelloStatus

class DroneController:

    def __init__(self):
        self.hologram = Pose()
        self.tello = Pose()
        self.save_tello = Pose()
        self.goal_pose = Pose()
        self.take_off = Empty()
        self.land = Empty()
        self.drone_vel = Twist()
        self.zero_vel = Twist()
        self.tello_status = TelloStatus()
        self.holo_counter_x = []
        self.holo_counter_y = []
        self.holo_counter_z = []
        self.holo_count = 0
        self.wait_next = 0
        self.msg = ''
        self.speed =1
        self.flag = 1
        self.i = 0
        self.counter = 0
        self.y_integral_sum = 0
        self.current_status_color = StatusColor()

    def hologramPos(self, data):
				self.hologram.position.x = currenthologramPos.pos_x
				self.hologram.position.y = currenthologramPos.pos_y
				self.hologram.position.z = currenthologramPos.pos_z

    def telloPos(self, data):
    		tello.position.x = currenttelloPos.pos_x
				tello.position.y = currenttelloPos.pos_y
				tello.position.z = currenttelloPos.pos_z
				print("\nTello_x: ", tello.position.x, " Tello y: ", tello.position.y, " Tello z: ", tello.position.z)

				if('track' in msg and flag):
					track_thread = threading.Thread(target=hologramTrack)
					flag = 0
					track_thread.start()

    def telloStat(self, data):
				self.telloStatus = newStatus

    def publish_zero_vel(self, publisher):
      	self.zero_vel.linear.x = 0
      	self.zero_vel.linear.y = 0
      	self.zero_vel.linear.z = 0
      	publisher.publish(self.zero_vel)

		def hologramTrack():
		
				global tello, hologram, pub_vel, drone_vel, goalPose, msg, flag, i, counter, waitNext, y_integral_sum, current_status_color, pub_status_color


				if(not rospy.is_shutdown() and 'stop' not in msg):
					goalPose.position.x = hologram.position.x - tello.position.x
					goalPose.position.y = hologram.position.y - tello.position.y
					goalPose.position.z = hologram.position.z - tello.position.z
					#print("\nGoal x: ", goalPose.position.x, " Goal y: ", goalPose.position.y, " Goal z: ", goalPose.position.z)
		
					y_integral_sum = y_integral_sum + goalPose.position.y

					# "holo_counter_x[len(holo_counter_x)-2] == holo_counter_x[len(holo_counter_x)-1]" this just to check if drone stationary at position or
					# not. "((abs(goalPose.position.x) > 0.1) or (abs(goalPose.position.y) > 0.1) or (abs(goalPose.position.z) > 0.1))" This is just to see if 						the difference between target and current position is greater than 0.1. 
		
				# waitNext isflag to indicate that one goal is still
					if((not waitNext) and holo_counter_x[len(holo_counter_x)-2] == holo_counter_x[len(holo_counter_x)-1] and ((abs(goalPose.position.x) > 0.1) 							or (abs(goalPose.position.y) > 0.1) or (abs(goalPose.position.z) > 0.1))):
						counter = timer()
						
						i+=1
				
						st = "Goal ",i,  " Given: ", " x: ", hologram.position.x, "y: ", hologram.position.y, "z: ", hologram.position.z
						print(st , "\n")
						dataFile.write(str(st) + '\n')
						
						current_status_color.r = 255
						current_status_color.g = 0
						current_status_color.b = 0
						current_status_color.a = 1
						pub_status_color.publish(current_status_color)
						
						waitNext = 1
						
					if(waitNext and (abs(goalPose.position.x) <= 0.10) and (abs(goalPose.position.y) <= 0.10) and (abs(goalPose.position.z) <= 0.10)):
						timeElapsed = timer() - counter
			
						st = "Tello Pose: ", "x: ", tello.position.x, " y: ", tello.position.y, " z: ", tello.position.z
						print (st, "\n")
						
						dataFile.write(str(st) + '\n')
						st = "Time taken to reach the Goal: ", timedelta(seconds=timeElapsed)
						print (st, "\n")
						dataFile.write(str(st) + '\n')
						
						current_status_color.r = 0
						current_status_color.g = 255
						current_status_color.b = 0
						current_status_color.a = 1
						pub_status_color.publish(current_status_color)
			
						waitNext = 0
			
			
					# For the bounding box
					if(not (-1 < hologram.position.x < 1)):
						goalPose.position.x = 0
						print ('Goal x outside bounding box')
						
					
					if(not (-0.8 < hologram.position.y < 0.3)):
						goalPose.position.y = 0
						print ('Goal y outside bounding box')
						
					
					if(not (2.0 <= hologram.position.z <= 2.2)):
						goalPose.position.z = 0
						print ('Goal z outside bounding box')
		
					drone_vel.linear.x = -(speed)*goalPose.position.x
					drone_vel.linear.y = -(speed)*goalPose.position.z
					drone_vel.linear.z = (speed+0.5)*goalPose.position.y + 0.03*(y_integral_sum)
					pub_vel.publish(drone_vel)

					time.sleep(1)
					pub_vel.publish(zero_vel)
			
				pub_vel.publish(zero_vel)
				flag = 1
	
				return

def main():
    rospy.init_node('Tello_Server')
    drone_controller = DroneController()
    
    with open('datafile.txt', 'a') as data_file:
        data_file.write('\n')
        data_file.write('New Data ---------------------------------------------------------\n')

        # Subscribed Topics
        sub_hologram = rospy.Subscriber("/target_pose", TargetPose, drone_controller.hologramPos)
        sub_tello = rospy.Subscriber("/drone_pose", CurrentDronePose, drone_controller.telloPos)
        sub_tello_status = rospy.Subscriber("/tello/status", TelloStatus, drone_controller.telloStat)

        # Published Topics
        pub_takeOff = rospy.Publisher("/tello/takeoff", Empty, queue_size=10)
        pub_vel = rospy.Publisher("/tello/cmd_vel", Twist, queue_size=10)
        pub_land = rospy.Publisher("/tello/land", Empty, queue_size=10)
        pub_status_color = rospy.Publisher("/tello/statuscolor", StatusColor, queue_size=10)

        # Loop here...

        drone_controller.publish_zero_vel(pub_vel)


if __name__ == '__main__':
    main()

