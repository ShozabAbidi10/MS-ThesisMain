# Mixed Reality Tool for a UAV Pilot
This project contains a Mixed-Reality Interface for controlling UAV or Drone.

The drone pilot will be wearing a Hololens2 that will augment the pilot's vision with a drone hologram and some useful information related to the drone control. The drone pilot will drag and drop the drone hologram to control the drone in the line of sight, and the drone will reach the coordinate of the hologram autonomously.

## Software Specifications
Ubuntu 20.04 running inside a Virtual Box \\
ROS Noetic installed in Ubuntu 20.04
Unity 2020.3.35f1
Visual Studio 2019
MRTK v2.7
OpenCV for Unity

## Hardware Specifications
Hololens 2
DJI Tello Drone
Dell Laptop Latitude 5590 

## Software Architecture
The Robot Operating System (ROS) is used to design the software architecture to provide a communication between the Hololens2 and Ubuntu, and from Ubuntu to the DJI Tello drone. On the other hand, in order to develop the Mixed Reality scene for the Hololens2, a Unity project is created, and a Universal Windows Platform application is built to be deployed on the Hololens through Visual Studio.
