using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using TelloStatus = RosMessageTypes.UnityRoboticsDemo.TelloStatusMsg;



namespace HoloTracking
{
    public class RosSubcriber : MonoBehaviour
    {
        private float tello_battery = 0.0f;

        void Start()
        {
            ROSConnection.GetOrCreateInstance().Subscribe<TelloStatus>("unity_tello_status", TelloStatus);
        }

        void TelloStatus(TelloStatus status)
        {
            tello_battery = status.battery_percentage;
            //Debug.Log($"Battery Status: {status.battery_percentage}");
        }

        public float getTelloBattary()
        {
            return tello_battery;
        }
    }
}