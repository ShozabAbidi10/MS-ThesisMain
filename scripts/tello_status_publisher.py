#!/usr/bin/env python

# license removed for brevity
import rospy
import rospy
import rosgraph
import time
from std_msgs.msg import String
from tello_driver.msg import TelloStatus

tello_status = TelloStatus()
TOPIC_NAME = 'unity_tello_status'

def main():
	#global tello_status
	rospy.init_node('unity_node', anonymous=True)
	pub = rospy.Publisher(TOPIC_NAME, TelloStatus, queue_size=10)
	rospy.Subscriber("/tello/status", TelloStatus, callback)
	#tello_status.battery_percentage = 55
	#wait_for_connections(pub, TOPIC_NAME)
	rate = rospy.Rate(10)
	while not rospy.is_shutdown():
		#print(tello_status.battery_percentage)
		pub.publish(tello_status)
		rate.sleep()	


def callback(data):
	global tello_status
	tello_status.battery_percentage = data.battery_percentage
	print(tello_status.battery_percentage)
	

def wait_for_connections(pub, topic):
	ros_master = rosgraph.Master('/rostopic')
	topic = rosgraph.names.script_resolve_name('rostopic', topic)
	num_subs = 0
	for sub in ros_master.getSystemState()[1]:
		if sub[0] == topic:
			num_subs+=1
	
	for i in range(10):
		if pub.get_num_connections() == num_subs:
			return
		time.sleep(0.1)
	raise RuntimeError("failed to get publisher")

if __name__ == '__main__':
    try:
        main()
    except rospy.ROSInterruptException:
        pass
