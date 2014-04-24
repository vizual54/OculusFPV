using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MavLinkTest
{
    public class CurrentState
    {
        object lockObject = new object();

        private DateTime lastUpdate;

        const float rad2deg = (float)(180 / Math.PI);
        const float deg2rad = (float)(1.0 / rad2deg);

        public Dictionary<int, string> modeList = new Dictionary<int, string>();
        public Dictionary<string, DateTime> alertList = new Dictionary<string, DateTime>();

        private ushort gcsLinkQuality;
        private DateTime gpsTime;
        private float boardVoltage;
        private float i2cErrors;
        private bool armed;
        private bool landed;
        private string flightMode;
        private uint mode;
        private float systemLoad; // load of the autopilot system in %
        private float batteryVoltage; // Battery voltage in V
        private float batteryCurrent; // Battery current in A
        private int batteryRemaining; // Battery remaining in %
        private int packetDropsRemote; // Dropped mavlink packages on remote side in %
        private float pressureAbs; // Absolute pressure in hectopasacal
        private float temperature; // Temprerature in degrees celsius
        private float attitudeRoll;
        private float attitudePitch;
        private float attitudeYaw;
        private int gpsStatus;
        private float gpsHdop;
        private int gpsSatCount;
        private float gpsGroundSpeed;
        private float gpsGroundCourse;
        private float radioRssi;
        private float radioRemoteRssi;
        private byte radioTxBuffer;
        private ushort radioRxErrors;
        private float radioNoise;
        private float radioRemoteNoise;
        private float radioFixedPackages;
        private float vfrAirspeed; // Current airspeed in m/s
        private float vfrGroundspeeed; // Current ground speed in m/s
        private float vfrAltitude; // Current altitude (MSL), in meters
        private float vfrClimbRate; // Current climb rate in meters/second
        private int vfrHeading; // Current heading in degrees, in compass units (0..360, 0=north)
        private int vfrThrottle; // Current throttle setting in integer percent, 0 to 100

        public CurrentState()
        {
            // This must be set up in some config
            modeList.Add(1, "Stabilize");
            modeList.Add(2, "Altitude Hold");
            modeList.Add(3, "Loiter");
            modeList.Add(4, "Auto");
            modeList.Add(5, "Land");
            modeList.Add(6, "RTL");

            Initialize();
        }

        public ushort GcsLinkQuality { get { return gcsLinkQuality; } private set { gcsLinkQuality = value; }}
        
        public DateTime GpsTime
        {
            get { return gpsTime; }
            private set { gpsTime = value; }
        }
        
        public float BoardVoltage
        {
            get { return boardVoltage; }
            set { boardVoltage = value; }
        }

        public float I2cErrors
        {
            get { return i2cErrors; }
            set { i2cErrors = value; }
        }

        public bool Armed
        {
            get { return armed; }
            set { armed = value; }
        }

        public bool Landed
        {
            get { return landed; }
            set { landed = value; }
        }

        public string FlightMode
        {
            get { return flightMode; }
            set { flightMode = value; }
        }

        public float SystemLoad
        {
            get { return systemLoad; }
            set { systemLoad = value; }
        }

        public float BatteryVoltage
        {
            get { return batteryVoltage; }
            set { batteryVoltage = value; }
        }

        public float BatteryCurrent
        {
            get { return batteryCurrent; }
            set { batteryCurrent = value; }
        }

        public int BatteryRemaining
        {
            get { return batteryRemaining; }
            set { batteryRemaining = value; }
        }

        public int PacketDropsRemote
        {
            get { return packetDropsRemote; }
            set { packetDropsRemote = value; }
        }

        public float PressureAbs
        {
            get { return pressureAbs; }
            set { pressureAbs = value; }
        }

        public float Temperature
        {
            get { return temperature; }
            set { temperature = value; }
        }

        public float AttitudeRoll
        {
            get { return attitudeRoll; }
            set { attitudeRoll = value; }
        }

        public float AttitudePitch
        {
            get { return attitudePitch; }
            set { attitudePitch = value; }
        }
        
        public float AttitudeYaw
        {
            get { return attitudeYaw; }
            set { attitudeYaw = value; }
        }

        public int GpsStatus
        {
            get { return gpsStatus; }
            set { gpsStatus = value; }
        }

        public float GpsHdop
        {
            get { return gpsHdop; }
            set { gpsHdop = value; }
        }

        public int GpsSatCount
        {
            get { return gpsSatCount; }
            set { gpsSatCount = value; }
        }

        public float GpsGroundSpeed
        {
            get { return gpsGroundSpeed; }
            set { gpsGroundSpeed = value; }
        }

        public float GpsGroundCourse
        {
            get { return gpsGroundCourse; }
            set { gpsGroundCourse = value; }
        }
 
        public float RadioRssi
        {
            get { return radioRssi; }
            set { radioRssi = value; }
        }

        public float RadioRemoteRssi
        {
            get { return radioRemoteRssi; }
            set { radioRemoteRssi = value; }
        }

        public byte RadioTxBuffer
        {
            get { return radioTxBuffer; }
            set { radioTxBuffer = value; }
        }
 
        public ushort RadioRxErrors
        {
            get { return radioRxErrors; }
            set { radioRxErrors = value; }
        }

        public float RadioNoise
        {
            get { return radioNoise; }
            set { radioNoise = value; }
        }

        public float RadioRemoteNoise
        {
            get { return radioRemoteNoise; }
            set { radioRemoteNoise = value; }
        }

        public float RadioFixedPackages
        {
            get { return radioFixedPackages; }
            set { radioFixedPackages = value; }
        }

        public float VfrAirspeed
        {
            get { return vfrAirspeed; }
            set { vfrAirspeed = value; }
        }

        public float VfrGroundspeeed
        {
            get { return vfrGroundspeeed; }
            set { vfrGroundspeeed = value; }
        }

        public float VfrAltitude
        {
            get { return vfrAltitude; }
            set { vfrAltitude = value; }
        }

        public float VfrClimbRate
        {
            get { return vfrClimbRate; }
            set { vfrClimbRate = value; }
        }

        public int VfrHeading
        {
            get { return vfrHeading; }
            set { vfrHeading = value; }
        }

        public int VfrThrottle
        {
            get { return vfrThrottle; }
            set { vfrThrottle = value; }
        }

        private void Initialize()
        {
            gcsLinkQuality = 0;
            gpsTime = DateTime.MinValue;
            boardVoltage = 0;
            i2cErrors = 0;
            armed = false;
            landed = false;
            flightMode = "";
            mode = 0;
            systemLoad = 0;
            batteryVoltage = 0;
            batteryCurrent = 0;
            batteryRemaining = 0;
            packetDropsRemote = 0;
            pressureAbs = 0;
            temperature = 0;
            attitudeRoll = 0;
            attitudePitch = 0;
            attitudeYaw = 0;
            gpsStatus = 0;
            gpsHdop = 0;
            gpsSatCount = 0;
            gpsGroundSpeed = 0;
            gpsGroundCourse = 0;
            radioRssi = 0;
            radioRemoteRssi = 0;
            radioTxBuffer = 0;
            radioRxErrors = 0;
            radioNoise = 0;
            radioRemoteNoise = 0;
            radioFixedPackages = 0;
            vfrAirspeed = 0;
            vfrGroundspeeed = 0;
            vfrAltitude = 0;
            vfrClimbRate = 0;
            vfrHeading = 0;
            vfrThrottle = 0;
        }

        public void UpdateCurrentSettings(System.Windows.Forms.BindingSource bs, bool forceUpdate, MAVLinkInterface mavInterface)
        {
            //throw new NotImplementedException();
            lock (lockObject)
            {

                if (DateTime.Now > lastUpdate.AddMilliseconds(50) || forceUpdate) // 20 hz
                {
                    lastUpdate = DateTime.Now;

                    if ((DateTime.Now - mavInterface.lastvalidpacket).TotalSeconds > 10)
                    {
                        gcsLinkQuality = 0;
                    }
                    else
                    {
                        gcsLinkQuality = (ushort)((mavInterface.packetsnotlost / (mavInterface.packetsnotlost + mavInterface.packetslost)) * 100);
                    }
                }

                byte[] bytearray;

                bytearray = mavInterface.MAV.packets[(byte)MAVLink.MAVLINK_MSG_ID.SYSTEM_TIME];

                if (bytearray != null)
                {
                    var systime = bytearray.ByteArrayToStructure<MAVLink.mavlink_system_time_t>(6);

                    DateTime date1 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

                    date1 = date1.AddMilliseconds(systime.time_unix_usec / 1000);

                    gpsTime = date1;
                }

                bytearray = mavInterface.MAV.packets[(byte)MAVLink.MAVLINK_MSG_ID.HWSTATUS];

                if (bytearray != null)
                {
                    var hwStatus = bytearray.ByteArrayToStructure<MAVLink.mavlink_hwstatus_t>(6);

                    boardVoltage = hwStatus.Vcc / 1000.0f;
                    i2cErrors = hwStatus.I2Cerr;
                }

                bytearray = mavInterface.MAV.packets[(byte)MAVLink.MAVLINK_MSG_ID.HEARTBEAT];
                if (bytearray != null)
                {
                    var hb = bytearray.ByteArrayToStructure<MAVLink.mavlink_heartbeat_t>(6);

                    if (hb.type == (byte)MAVLink.MAV_TYPE.GCS)
                    {
                        // This should not happen
                    }
                    else
                    {
                        armed = (hb.base_mode & (byte)MAVLink.MAV_MODE_FLAG.SAFETY_ARMED) == (byte)MAVLink.MAV_MODE_FLAG.SAFETY_ARMED;
                        landed = hb.system_status == (byte)MAVLink.MAV_STATE.STANDBY;
                        
                        if ((hb.base_mode & (byte)MAVLink.MAV_MODE_FLAG.CUSTOM_MODE_ENABLED) != 0)
                        {
                            if (mode != hb.custom_mode)
                            {
                                mode = hb.custom_mode;
                                modeList.TryGetValue((int)hb.custom_mode, out flightMode);
                            }
                        }
                    }
                }

                bytearray = mavInterface.MAV.packets[(byte)MAVLink.MAVLINK_MSG_ID.SYS_STATUS];
                if (bytearray != null)
                {
                    var sysstatus = bytearray.ByteArrayToStructure<MAVLink.mavlink_sys_status_t>(6);
                    systemLoad = (float)sysstatus.load / 10.0f;
                    batteryVoltage = (float)sysstatus.voltage_battery / 1000.0f;
                    batteryCurrent = (float)sysstatus.current_battery / 100.0f;
                    batteryRemaining = sysstatus.battery_remaining;
                    packetDropsRemote = sysstatus.drop_rate_comm;

                    MavlinkSensors sensorsEnabled = new MavlinkSensors(sysstatus.onboard_control_sensors_enabled);
                    MavlinkSensors sensorsHealth = new MavlinkSensors(sysstatus.onboard_control_sensors_health);
                    MavlinkSensors sensorsPresent = new MavlinkSensors(sysstatus.onboard_control_sensors_present);

                    if (sensorsHealth.gps != sensorsEnabled.gps)
                    {
                        checkAlertList("Bad GPS Health");
                    }
                    else if (sensorsHealth.gyro != sensorsEnabled.gyro)
                    {
                        checkAlertList("Bad Gyro Health");
                    }
                    else if (sensorsHealth.accelerometer != sensorsEnabled.accelerometer)
                    {
                        checkAlertList("Bad Accel Health");
                    }
                    else if (sensorsHealth.compass != sensorsEnabled.compass)
                    {
                        checkAlertList("Bad Compass Health");
                    }
                    else if (sensorsHealth.barometer != sensorsEnabled.barometer)
                    {
                        checkAlertList("Bad Baro Health");
                    }
                    else if (sensorsHealth.optical_flow != sensorsEnabled.optical_flow)
                    {
                        checkAlertList("Bad OptFlow Health");
                    }
                    else if (sensorsPresent.rc_receiver != sensorsEnabled.rc_receiver)
                    {
                        //checkAlertList("NO RC Receiver");
                    }

                    mavInterface.MAV.packets[(byte)MAVLink.MAVLINK_MSG_ID.SYS_STATUS] = null;
                }

                bytearray = mavInterface.MAV.packets[(byte)MAVLink.MAVLINK_MSG_ID.SCALED_PRESSURE];
                if (bytearray != null)
                {
                    var pres = bytearray.ByteArrayToStructure<MAVLink.mavlink_scaled_pressure_t>(6);
                    pressureAbs = pres.press_abs;
                    temperature = pres.temperature;
                }

                bytearray = mavInterface.MAV.packets[(byte)MAVLink.MAVLINK_MSG_ID.ATTITUDE];
                if (bytearray != null)
                {
                    var att = bytearray.ByteArrayToStructure<MAVLink.mavlink_attitude_t>(6);

                    attitudeRoll = att.roll * rad2deg;
                    attitudePitch = att.pitch * rad2deg;
                    attitudeYaw = att.yaw * rad2deg;
                }

                bytearray = mavInterface.MAV.packets[(byte)MAVLink.MAVLINK_MSG_ID.GPS_RAW_INT];
                if (bytearray != null)
                {
                    var gps = bytearray.ByteArrayToStructure<MAVLink.mavlink_gps_raw_int_t>(6);

                    gpsStatus = gps.fix_type;
                    gpsHdop = (float)Math.Round((double)gps.eph / 100.0, 2);
                    gpsSatCount = gps.satellites_visible;
                    gpsGroundSpeed = gps.vel / 100.0f;
                    gpsGroundCourse = gps.cog / 100.0f;
                }

                bytearray = mavInterface.MAV.packets[(byte)MAVLink.MAVLINK_MSG_ID.GPS_STATUS];
                if (bytearray != null)
                {
                    var gps = bytearray.ByteArrayToStructure<MAVLink.mavlink_gps_status_t>(6);
                    gpsSatCount = gps.satellites_visible;
                }

                bytearray = mavInterface.MAV.packets[(byte)MAVLink.MAVLINK_MSG_ID.RADIO];
                if (bytearray != null)
                {
                    var radio = bytearray.ByteArrayToStructure<MAVLink.mavlink_radio_t>(6);
                    radioRssi = radio.rssi;
                    radioRemoteRssi = radio.remrssi;
                    radioTxBuffer = radio.txbuf;
                    radioRxErrors = radio.rxerrors;
                    radioNoise = radio.noise;
                    radioRemoteNoise = radio.remnoise;
                    radioFixedPackages = radio.@fixed;
                }

                bytearray = mavInterface.MAV.packets[(byte)MAVLink.MAVLINK_MSG_ID.RADIO_STATUS];
                if (bytearray != null)
                {
                    var radio = bytearray.ByteArrayToStructure<MAVLink.mavlink_radio_t>(6);
                    radioRssi = radio.rssi;
                    radioRemoteRssi = radio.remrssi;
                    radioTxBuffer = radio.txbuf;
                    radioRxErrors = radio.rxerrors;
                    radioNoise = radio.noise;
                    radioRemoteNoise = radio.remnoise;
                    radioFixedPackages = radio.@fixed;
                }

                bytearray = mavInterface.MAV.packets[(byte)MAVLink.MAVLINK_MSG_ID.VFR_HUD];
                if (bytearray != null)
                {
                    var vfr = bytearray.ByteArrayToStructure<MAVLink.mavlink_vfr_hud_t>(6);
                    vfrAirspeed = vfr.airspeed;
                    vfrGroundspeeed = vfr.groundspeed;
                    vfrAltitude = vfr.alt;
                    vfrClimbRate = vfr.climb;
                    vfrHeading = vfr.heading;
                    vfrThrottle = vfr.throttle;
                }
            }
        }

        private void checkAlertList(string alertMessage)
        {
            if (!alertList.ContainsKey(alertMessage))
            {
                alertList.Add(alertMessage, DateTime.Now);
            }
            else
            {
                alertList[alertMessage] = DateTime.Now;
            }
        }
    }

    

    public class MavlinkSensors
    {
        BitArray bitArray = new BitArray(32);

        public MavlinkSensors()
        {
        }

        public MavlinkSensors(uint p)
        {
            bitArray = new BitArray(new int[] { (int)p });
        }

        public bool gyro { get { return bitArray[0]; } set { bitArray[0] = value; } }
        public bool accelerometer { get { return bitArray[1]; } set { bitArray[1] = value; } }
        public bool compass { get { return bitArray[2]; } set { bitArray[2] = value; } }
        public bool barometer { get { return bitArray[3]; } set { bitArray[3] = value; } }
        public bool differential_pressure { get { return bitArray[4]; } set { bitArray[4] = value; } }
        public bool gps { get { return bitArray[5]; } set { bitArray[5] = value; } }
        public bool optical_flow { get { return bitArray[6]; } set { bitArray[6] = value; } }
        public bool unused_7 { get { return bitArray[7]; } set { bitArray[7] = value; } }
        public bool unused_8 { get { return bitArray[8]; } set { bitArray[8] = value; } }
        public bool unused_9 { get { return bitArray[9]; } set { bitArray[9] = value; } }
        public bool rate_control { get { return bitArray[10]; } set { bitArray[10] = value; } }
        public bool attitude_stabilization { get { return bitArray[11]; } set { bitArray[11] = value; } }
        public bool yaw_position { get { return bitArray[12]; } set { bitArray[12] = value; } }
        public bool altitude_control { get { return bitArray[13]; } set { bitArray[13] = value; } }
        public bool xy_position_control { get { return bitArray[14]; } set { bitArray[14] = value; } }
        public bool motor_control { get { return bitArray[15]; } set { bitArray[15] = value; } }
        public bool rc_receiver { get { return bitArray[16]; } set { bitArray[16] = value; } }

        public int Value
        {
            get
            {
                int[] array = new int[1];
                bitArray.CopyTo(array, 0);
                return array[0];
            }
            set
            {
                bitArray = new BitArray(value);
            }
        }
    }
}
