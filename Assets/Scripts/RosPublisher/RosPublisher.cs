using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.UnityRoboticsDemo;
using JetBrains.Annotations;
using StatusColor = RosMessageTypes.UnityRoboticsDemo.StatusColorMsg;



namespace HoloTracking
{
    public class RosPublisher : MonoBehaviour
    {
        ROSConnection ros;
        public string TargetPoseTopicName = "target_pose";
        public string CurrentDronePoseTopicName = "drone_pose";
        public string StatuscolorTopicName = "/tello/statuscolor";
        [NotNull] public GameObject drone_status_object;

        // Publish the cube's position and rotation every N seconds
        public float publishMessageFrequency = 0.5f;

        // Used to determine how much time has elapsed since the last message was published
        private float timeElapsed;

        void Start()
        {
            // start the ROS connection
            ros = ROSConnection.GetOrCreateInstance();
            ros.RegisterPublisher<CurrentDronePoseMsg>(CurrentDronePoseTopicName);
            ros.RegisterPublisher<TargetPoseMsg>(TargetPoseTopicName);
            ros.RegisterPublisher<StatusColor>(StatuscolorTopicName);
        }

        public void PublishTargetMsg(CurrentDronePoseMsg CurrentDronePosition, TargetPoseMsg CurrentTargetPosition)
        {
            timeElapsed += Time.deltaTime;

            if (CurrentTargetPosition.pos_x < -1.0f || CurrentTargetPosition.pos_x > 1.0f ||
                CurrentTargetPosition.pos_y < -0.8f || CurrentTargetPosition.pos_y > 0.3f ||
                CurrentTargetPosition.pos_z <  2.0f || CurrentTargetPosition.pos_z > 2.2f)
            {
                Debug.Log("Changing drone color: red");
                Color color_update = new Color(255, 0, 0, 1);
                drone_status_object.GetComponent<MeshRenderer>().material.color = color_update;
            }
                
            if (timeElapsed > publishMessageFrequency)
            {
                if (CurrentTargetPosition.pos_x > -1.0f && CurrentTargetPosition.pos_x < 1.0f &&
                    CurrentTargetPosition.pos_y > -0.8f && CurrentTargetPosition.pos_y < 0.3f &&
                    CurrentTargetPosition.pos_z > 2.0f && CurrentTargetPosition.pos_z <= 2.2f)
                {
                    TargetPoseMsg goalPos = new TargetPoseMsg(
                        CurrentTargetPosition.pos_x,
                        CurrentTargetPosition.pos_y,
                        CurrentTargetPosition.pos_z,
                        CurrentTargetPosition.rot_x,
                        CurrentTargetPosition.rot_y,
                        CurrentTargetPosition.rot_z,
                        CurrentTargetPosition.rot_w
                    );

                    Debug.Log("Changing drone color: gray");
                    Color color_update = new Color(20, 15, 15, 1);
                    drone_status_object.GetComponent<MeshRenderer>().material.color = color_update;
                }

                if (CurrentDronePosition.pos_x > -2.0f && CurrentDronePosition.pos_x < 2.0f &&
                    CurrentDronePosition.pos_y > -0.1f && CurrentDronePosition.pos_y < 0.7f)
                {

                    CurrentDronePoseMsg currentDronePos = new CurrentDronePoseMsg(
                        CurrentDronePosition.pos_x,
                        CurrentDronePosition.pos_y,
                        CurrentDronePosition.pos_z,
                        CurrentDronePosition.rot_x,
                        CurrentDronePosition.rot_y,
                        CurrentDronePosition.rot_z,
                        CurrentDronePosition.rot_w
                    );

                    ros.Publish(CurrentDronePoseTopicName, currentDronePos);
                }

                timeElapsed = 0;
            }
        }
    }
}