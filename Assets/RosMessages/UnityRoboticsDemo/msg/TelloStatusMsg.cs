//Do not edit! This file was generated by Unity-ROS MessageGeneration.
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;

namespace RosMessageTypes.UnityRoboticsDemo
{
    [Serializable]
    public class TelloStatusMsg : Message
    {
        public const string k_RosMessageName = "unity_robotics_demo_msgs/TelloStatus";
        public override string RosMessageName => k_RosMessageName;

        //  Non-negative; calibrated to takeoff altitude; auto-calib if falls below takeoff height; inaccurate near ground
        public float height_m;
        public float speed_northing_mps;
        public float speed_easting_mps;
        public float speed_horizontal_mps;
        public float speed_vertical_mps;
        public float flight_time_sec;
        public bool imu_state;
        public bool pressure_state;
        public bool down_visual_state;
        public bool power_state;
        public bool battery_state;
        public bool gravity_state;
        public bool wind_state;
        public byte imu_calibration_state;
        public byte battery_percentage;
        public float drone_fly_time_left_sec;
        public float drone_battery_left_sec;
        public bool is_flying;
        public bool is_on_ground;
        //  is_em_open True in flight, False when landed
        public bool is_em_open;
        public bool is_drone_hover;
        public bool is_outage_recording;
        public bool is_battery_low;
        public bool is_battery_lower;
        public bool is_factory_mode;
        //  flymode=1: landed; =6: flying
        public byte fly_mode;
        public float throw_takeoff_timer_sec;
        public byte camera_state;
        public byte electrical_machinery_state;
        public bool front_in;
        public bool front_out;
        public bool front_lsc;
        public float temperature_height_m;
        public float cmd_roll_ratio;
        public float cmd_pitch_ratio;
        public float cmd_yaw_ratio;
        public float cmd_vspeed_ratio;
        public bool cmd_fast_mode;

        public TelloStatusMsg()
        {
            this.height_m = 0.0f;
            this.speed_northing_mps = 0.0f;
            this.speed_easting_mps = 0.0f;
            this.speed_horizontal_mps = 0.0f;
            this.speed_vertical_mps = 0.0f;
            this.flight_time_sec = 0.0f;
            this.imu_state = false;
            this.pressure_state = false;
            this.down_visual_state = false;
            this.power_state = false;
            this.battery_state = false;
            this.gravity_state = false;
            this.wind_state = false;
            this.imu_calibration_state = 0;
            this.battery_percentage = 0;
            this.drone_fly_time_left_sec = 0.0f;
            this.drone_battery_left_sec = 0.0f;
            this.is_flying = false;
            this.is_on_ground = false;
            this.is_em_open = false;
            this.is_drone_hover = false;
            this.is_outage_recording = false;
            this.is_battery_low = false;
            this.is_battery_lower = false;
            this.is_factory_mode = false;
            this.fly_mode = 0;
            this.throw_takeoff_timer_sec = 0.0f;
            this.camera_state = 0;
            this.electrical_machinery_state = 0;
            this.front_in = false;
            this.front_out = false;
            this.front_lsc = false;
            this.temperature_height_m = 0.0f;
            this.cmd_roll_ratio = 0.0f;
            this.cmd_pitch_ratio = 0.0f;
            this.cmd_yaw_ratio = 0.0f;
            this.cmd_vspeed_ratio = 0.0f;
            this.cmd_fast_mode = false;
        }

        public TelloStatusMsg(float height_m, float speed_northing_mps, float speed_easting_mps, float speed_horizontal_mps, float speed_vertical_mps, float flight_time_sec, bool imu_state, bool pressure_state, bool down_visual_state, bool power_state, bool battery_state, bool gravity_state, bool wind_state, byte imu_calibration_state, byte battery_percentage, float drone_fly_time_left_sec, float drone_battery_left_sec, bool is_flying, bool is_on_ground, bool is_em_open, bool is_drone_hover, bool is_outage_recording, bool is_battery_low, bool is_battery_lower, bool is_factory_mode, byte fly_mode, float throw_takeoff_timer_sec, byte camera_state, byte electrical_machinery_state, bool front_in, bool front_out, bool front_lsc, float temperature_height_m, float cmd_roll_ratio, float cmd_pitch_ratio, float cmd_yaw_ratio, float cmd_vspeed_ratio, bool cmd_fast_mode)
        {
            this.height_m = height_m;
            this.speed_northing_mps = speed_northing_mps;
            this.speed_easting_mps = speed_easting_mps;
            this.speed_horizontal_mps = speed_horizontal_mps;
            this.speed_vertical_mps = speed_vertical_mps;
            this.flight_time_sec = flight_time_sec;
            this.imu_state = imu_state;
            this.pressure_state = pressure_state;
            this.down_visual_state = down_visual_state;
            this.power_state = power_state;
            this.battery_state = battery_state;
            this.gravity_state = gravity_state;
            this.wind_state = wind_state;
            this.imu_calibration_state = imu_calibration_state;
            this.battery_percentage = battery_percentage;
            this.drone_fly_time_left_sec = drone_fly_time_left_sec;
            this.drone_battery_left_sec = drone_battery_left_sec;
            this.is_flying = is_flying;
            this.is_on_ground = is_on_ground;
            this.is_em_open = is_em_open;
            this.is_drone_hover = is_drone_hover;
            this.is_outage_recording = is_outage_recording;
            this.is_battery_low = is_battery_low;
            this.is_battery_lower = is_battery_lower;
            this.is_factory_mode = is_factory_mode;
            this.fly_mode = fly_mode;
            this.throw_takeoff_timer_sec = throw_takeoff_timer_sec;
            this.camera_state = camera_state;
            this.electrical_machinery_state = electrical_machinery_state;
            this.front_in = front_in;
            this.front_out = front_out;
            this.front_lsc = front_lsc;
            this.temperature_height_m = temperature_height_m;
            this.cmd_roll_ratio = cmd_roll_ratio;
            this.cmd_pitch_ratio = cmd_pitch_ratio;
            this.cmd_yaw_ratio = cmd_yaw_ratio;
            this.cmd_vspeed_ratio = cmd_vspeed_ratio;
            this.cmd_fast_mode = cmd_fast_mode;
        }

        public static TelloStatusMsg Deserialize(MessageDeserializer deserializer) => new TelloStatusMsg(deserializer);

        private TelloStatusMsg(MessageDeserializer deserializer)
        {
            deserializer.Read(out this.height_m);
            deserializer.Read(out this.speed_northing_mps);
            deserializer.Read(out this.speed_easting_mps);
            deserializer.Read(out this.speed_horizontal_mps);
            deserializer.Read(out this.speed_vertical_mps);
            deserializer.Read(out this.flight_time_sec);
            deserializer.Read(out this.imu_state);
            deserializer.Read(out this.pressure_state);
            deserializer.Read(out this.down_visual_state);
            deserializer.Read(out this.power_state);
            deserializer.Read(out this.battery_state);
            deserializer.Read(out this.gravity_state);
            deserializer.Read(out this.wind_state);
            deserializer.Read(out this.imu_calibration_state);
            deserializer.Read(out this.battery_percentage);
            deserializer.Read(out this.drone_fly_time_left_sec);
            deserializer.Read(out this.drone_battery_left_sec);
            deserializer.Read(out this.is_flying);
            deserializer.Read(out this.is_on_ground);
            deserializer.Read(out this.is_em_open);
            deserializer.Read(out this.is_drone_hover);
            deserializer.Read(out this.is_outage_recording);
            deserializer.Read(out this.is_battery_low);
            deserializer.Read(out this.is_battery_lower);
            deserializer.Read(out this.is_factory_mode);
            deserializer.Read(out this.fly_mode);
            deserializer.Read(out this.throw_takeoff_timer_sec);
            deserializer.Read(out this.camera_state);
            deserializer.Read(out this.electrical_machinery_state);
            deserializer.Read(out this.front_in);
            deserializer.Read(out this.front_out);
            deserializer.Read(out this.front_lsc);
            deserializer.Read(out this.temperature_height_m);
            deserializer.Read(out this.cmd_roll_ratio);
            deserializer.Read(out this.cmd_pitch_ratio);
            deserializer.Read(out this.cmd_yaw_ratio);
            deserializer.Read(out this.cmd_vspeed_ratio);
            deserializer.Read(out this.cmd_fast_mode);
        }

        public override void SerializeTo(MessageSerializer serializer)
        {
            serializer.Write(this.height_m);
            serializer.Write(this.speed_northing_mps);
            serializer.Write(this.speed_easting_mps);
            serializer.Write(this.speed_horizontal_mps);
            serializer.Write(this.speed_vertical_mps);
            serializer.Write(this.flight_time_sec);
            serializer.Write(this.imu_state);
            serializer.Write(this.pressure_state);
            serializer.Write(this.down_visual_state);
            serializer.Write(this.power_state);
            serializer.Write(this.battery_state);
            serializer.Write(this.gravity_state);
            serializer.Write(this.wind_state);
            serializer.Write(this.imu_calibration_state);
            serializer.Write(this.battery_percentage);
            serializer.Write(this.drone_fly_time_left_sec);
            serializer.Write(this.drone_battery_left_sec);
            serializer.Write(this.is_flying);
            serializer.Write(this.is_on_ground);
            serializer.Write(this.is_em_open);
            serializer.Write(this.is_drone_hover);
            serializer.Write(this.is_outage_recording);
            serializer.Write(this.is_battery_low);
            serializer.Write(this.is_battery_lower);
            serializer.Write(this.is_factory_mode);
            serializer.Write(this.fly_mode);
            serializer.Write(this.throw_takeoff_timer_sec);
            serializer.Write(this.camera_state);
            serializer.Write(this.electrical_machinery_state);
            serializer.Write(this.front_in);
            serializer.Write(this.front_out);
            serializer.Write(this.front_lsc);
            serializer.Write(this.temperature_height_m);
            serializer.Write(this.cmd_roll_ratio);
            serializer.Write(this.cmd_pitch_ratio);
            serializer.Write(this.cmd_yaw_ratio);
            serializer.Write(this.cmd_vspeed_ratio);
            serializer.Write(this.cmd_fast_mode);
        }

        public override string ToString()
        {
            return "TelloStatusMsg: " +
            "\nheight_m: " + height_m.ToString() +
            "\nspeed_northing_mps: " + speed_northing_mps.ToString() +
            "\nspeed_easting_mps: " + speed_easting_mps.ToString() +
            "\nspeed_horizontal_mps: " + speed_horizontal_mps.ToString() +
            "\nspeed_vertical_mps: " + speed_vertical_mps.ToString() +
            "\nflight_time_sec: " + flight_time_sec.ToString() +
            "\nimu_state: " + imu_state.ToString() +
            "\npressure_state: " + pressure_state.ToString() +
            "\ndown_visual_state: " + down_visual_state.ToString() +
            "\npower_state: " + power_state.ToString() +
            "\nbattery_state: " + battery_state.ToString() +
            "\ngravity_state: " + gravity_state.ToString() +
            "\nwind_state: " + wind_state.ToString() +
            "\nimu_calibration_state: " + imu_calibration_state.ToString() +
            "\nbattery_percentage: " + battery_percentage.ToString() +
            "\ndrone_fly_time_left_sec: " + drone_fly_time_left_sec.ToString() +
            "\ndrone_battery_left_sec: " + drone_battery_left_sec.ToString() +
            "\nis_flying: " + is_flying.ToString() +
            "\nis_on_ground: " + is_on_ground.ToString() +
            "\nis_em_open: " + is_em_open.ToString() +
            "\nis_drone_hover: " + is_drone_hover.ToString() +
            "\nis_outage_recording: " + is_outage_recording.ToString() +
            "\nis_battery_low: " + is_battery_low.ToString() +
            "\nis_battery_lower: " + is_battery_lower.ToString() +
            "\nis_factory_mode: " + is_factory_mode.ToString() +
            "\nfly_mode: " + fly_mode.ToString() +
            "\nthrow_takeoff_timer_sec: " + throw_takeoff_timer_sec.ToString() +
            "\ncamera_state: " + camera_state.ToString() +
            "\nelectrical_machinery_state: " + electrical_machinery_state.ToString() +
            "\nfront_in: " + front_in.ToString() +
            "\nfront_out: " + front_out.ToString() +
            "\nfront_lsc: " + front_lsc.ToString() +
            "\ntemperature_height_m: " + temperature_height_m.ToString() +
            "\ncmd_roll_ratio: " + cmd_roll_ratio.ToString() +
            "\ncmd_pitch_ratio: " + cmd_pitch_ratio.ToString() +
            "\ncmd_yaw_ratio: " + cmd_yaw_ratio.ToString() +
            "\ncmd_vspeed_ratio: " + cmd_vspeed_ratio.ToString() +
            "\ncmd_fast_mode: " + cmd_fast_mode.ToString();
        }

#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
#else
        [UnityEngine.RuntimeInitializeOnLoadMethod]
#endif
        public static void Register()
        {
            MessageRegistry.Register(k_RosMessageName, Deserialize);
        }
    }
}
