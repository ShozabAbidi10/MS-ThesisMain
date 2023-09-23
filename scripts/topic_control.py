#!/usr/bin/env python

# Python libs
import sys
import time
import math
import random
import actionlib
import subprocess
import signal

# numpy and scipy
import numpy as np


# Ros libraries
import roslib
import rospy

# Ros Messages

from geometry_msgs.msg import Twist, Point, Pose
from std_msgs.msg import Empty
from std_msgs.msg import String
from std_msgs.msg import Float64
from nav_msgs.msg import Odometry
from unity_robotics_demo_msgs.msg import PosRot



def droneCurrentPos(currentPos):
    
    current_drone_position.position.x = currentPos.position.x
    current_drone_position.position.y = currentPos.position.y
    current_drone_position.position.z = currentPos.position.z
    #print("Drone x: ", drone.position.x, "Drone y: ", drone.position.y, "Drone z: ", drone.position.z)



def targetPos(currenttargetPos):
    
    target.position.x = currenttargetPos.pos_x
    target.position.y = currenttargetPos.pos_y
    target.position.z = currenttargetPos.pos_z
    #print("Cube x: ", cube.position.x, "Drone y: ", cube.position.y, "Drone z: ", cube.position.z)

def odom_callback(msg):
		odom_drone_position[0] = msg.pose.pose.position.x
		odom_drone_position[1] = msg.pose.pose.position.y
		odom_drone_position[2] = msg.pose.pose.position.z


def main():

	global pub_cmd_vel, pub_takeoff, vel, goal, current_drone_position, target, empty, odom_drone_position
	vel = Twist()
	goal = Pose()
	current_drone_position = Pose()
	target = Pose()
	empty = Empty()
	odom_drone_position = [0,0,0]
	#empty = ""

	rospy.init_node('drone_control_from_hologram')

	sub_odom = rospy.Subscriber("/drone_pose", Pose, droneCurrentPos)
	sub_cube = rospy.Subscriber("/target_pose", PosRot, targetPos)
	rospy.Subscriber('/tello/odom', Odometry, odom_callback)

	pub_cmd_vel = rospy.Publisher("/cmd_vel", Twist, queue_size = 1)
	pub_takeoff = rospy.Publisher("/drone/takeoff", Empty, queue_size = 1)
	time.sleep(2)
	pub_takeoff.publish(empty)
	time.sleep(4)



	while(not rospy.is_shutdown()):
		if(((target.position.x - current_drone_position.position.x) > 0.5) | ((target.position.y - current_drone_position.position.y) > 0.5) | ((target.position.z - current_drone_position.position.z) > 0.5)):
			vel.linear.x = 1*(target.position.x - current_drone_position.position.x)
			print("Vel x: ",vel.linear.x)
			vel.linear.y = 1*(target.position.y - current_drone_position.position.y)
			print("Vel y: ",vel.linear.y)
			vel.linear.z = 1*(target.position.z - current_drone_position.position.z)
			print("Vel z: ",vel.linear.z)

		else:
			print("Zero Vel")
			vel.linear.x = 0
			vel.linear.y = 0
			vel.linear.z = 0
		pub_cmd_vel.publish(vel)

	rospy.spin()


if __name__ == '__main__':
    main()







