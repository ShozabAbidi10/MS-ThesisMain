#!/usr/bin/env python

import rospy
from geometry_msgs.msg import Twist
from nav_msgs.msg import Odometry
from std_msgs.msg import Empty
import time
import math

class TelloDrone:
    def __init__(self, square_size):
        rospy.init_node('tello_drone')
        self.pub_takeoff = rospy.Publisher('/tello/takeoff', Empty, queue_size=10)
        self.pub_land = rospy.Publisher('/tello/land', Empty, queue_size=10)
        self.pub_cmd_vel = rospy.Publisher('/tello/cmd_vel', Twist, queue_size=10)
        rospy.Subscriber('/tello/odom', Odometry, self.odom_callback)
        self.current_position = [0, 0, 0]  # X, Y, Z
        self.square_size = square_size

    def odom_callback(self, msg):
        self.current_position[0] = msg.pose.pose.position.x
        self.current_position[1] = msg.pose.pose.position.y
        self.current_position[2] = msg.pose.pose.position.z

    def takeoff(self):
        msg = Empty()
        self.pub_takeoff.publish(msg)
        rospy.loginfo("Drone Taking off")

    def land(self):
        msg = Empty()
        self.pub_land.publish(msg)
        rospy.loginfo("Drone Landing")

    def fly_square(self):
        msg = Twist()
        initial_position = self.current_position[:]
        for i in range(4):
            print("initial_position: ", initial_position)
            time_counter = rospy.Time.now()
            while True:
                msg.linear.x = 0.5
                self.pub_cmd_vel.publish(msg)
                rospy.loginfo("Moving forward")
                print("current_position: ", self.current_position)
                distance_travelled = math.sqrt((self.current_position[0] - initial_position[0])**2)
                print("distance_travelled x-axis: ", distance_travelled)
                time_epoch = rospy.Time.now() - time_counter
                print("time counter: ", time_epoch.to_sec())
                
                if distance_travelled >= self.square_size or time_epoch.to_sec() >= 3:
                    break
            msg.linear.x = 0.0
            self.pub_cmd_vel.publish(msg)
            rospy.sleep(2)
            initial_position = self.current_position[:]
            
            msg.linear.x = 0.0  # stop moving forward
            msg.angular.z = 1.0  # start rotating
            self.pub_cmd_vel.publish(msg)
            rospy.loginfo("Rotating 90 degrees")
            rospy.sleep(3)
            
            msg.angular.z = 0.0  # stop rotating
            self.pub_cmd_vel.publish(msg)
            rospy.sleep(2)
        rospy.loginfo("Stopping")

if __name__ == '__main__':
    drone = TelloDrone(3)  # Provide your square size here
    rospy.sleep(5)
    drone.takeoff()
    rospy.sleep(3)
    drone.fly_square()
    rospy.sleep(2)
    drone.land()

