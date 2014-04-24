using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using Comms;

namespace MavLinkTest
{
    
    

    public partial class Form1 : Form
    {
        private object lockObject = new object();
        delegate void SetTextCallback(string text);
        SetTextCallback setTextCallback;
        delegate void ClearTextCallback();
        ClearTextCallback clearTextCallback;
        public static MAVLinkInterface comPort;
        private string[] baudRates = new string[] {"57600", "115200"};
        Thread mountControlThread;
        Thread serialReaderThread;
        private bool mountControlThreadRun = false;
        private bool serialReaderThreadRun = false;
        private int cameraRoll = 0;
        private int cameraPitch = 0;
        private int cameraYaw = 0;

        private DateTime heartBeatSend;

        public Form1()
        {
            InitializeComponent();
            setTextCallback = new SetTextCallback(mavLinkTextBoxSetText);
            //setTextCallback = new SetTextCallback(SetText);
            string[] theSerialPortNames = System.IO.Ports.SerialPort.GetPortNames();
            comPortComboBox.Items.AddRange(theSerialPortNames);
            comPortComboBox.Items.Add("TCP");
            comPortComboBox.Items.Add("UDP");
            baudRateComboBox.Items.AddRange(baudRates);

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
            
        }

        public void UpdateTextBox(string text)
        {
            this.Invoke(setTextCallback, new object[] { text });
        }

        public void ClearTextBox()
        {
            this.Invoke(clearTextCallback, new object[] { });
        }

        private void mavLinkTextBoxSetText(string text)
        {
            mavLinkTextBox.AppendText(text);
        }

        private void connectButton_Click(object sender, EventArgs e)
        {
            if (comPortComboBox.Text != null && comPortComboBox.Text != "")
            {
                disconnectButton.Enabled = true;
                connectButton.Enabled = false;

                switch (comPortComboBox.Text)
                {
                    case "TCP":
                        {
                            comPort = new MAVLinkInterface(this);
                            comPort.BaseStream = new TcpSerial();
                            comPort.Open();
                            break;
                        }
                    case "UDP":
                        {
                            comPort = new MAVLinkInterface(this);
                            comPort.BaseStream = new UdpSerial();
                            comPort.Open();
                            break;
                        }
                    default:
                        {
                            comPort = new MAVLinkInterface(this);
                            comPort.BaseStream = new SerialPort();
                            comPort.BaseStream.DtrEnable = false;
                            comPort.BaseStream.RtsEnable = false;
                            comPort.BaseStream.toggleDTR();
                            comPort.Open(comPortComboBox.Text, baudRateComboBox.Text);
                            break;
                        }
                }


                if (comPort != null && comPort.BaseStream.IsOpen)
                {
                    if (!mountControlThread.IsAlive)
                    {
                        /// setup joystick packet sender
                        mountControlThread = new Thread(new ThreadStart(mountControlSend))
                        {
                            IsBackground = true,
                            Priority = ThreadPriority.AboveNormal,
                            Name = "Main camera mount sender"
                        };

                        mountControlThread.Start();
                    }
                    
                    if (!serialReaderThread.IsAlive)
                    {
                        serialReaderThread = new Thread(new ThreadStart(serialReader))
                        {
                            IsBackground = true,
                            Priority = ThreadPriority.AboveNormal,
                            Name = "Serial reader thread"
                        };

                        serialReaderThread.Start();
                    }
                }
            }
            else
            {
                MessageBox.Show("No COM port selected", "OCULUS FPV", MessageBoxButtons.OKCancel, MessageBoxIcon.Asterisk);
            }
        }

        private void disconnectButton_Click(object sender, EventArgs e)
        {
            disconnectButton.Enabled = false;
            connectButton.Enabled = true;
            mountControlThreadRun = false;
            serialReaderThreadRun = false;
            mountControlThread.Abort();
            serialReaderThread.Abort();
            comPort.Close();
            comPort.Dispose();
            comPort = null;
        }

        private void mountControlSend()
        {
            mountControlThreadRun = true;

            while (mountControlThreadRun)
            {
                MAVLink.mavlink_mount_control_t mc = new MAVLink.mavlink_mount_control_t();

                mc.target_component = comPort.MAV.compid;
                mc.target_system = comPort.MAV.sysid;
                lock (lockObject)
                {
                    mc.input_a = cameraPitch * 100;
                    mc.input_b = cameraRoll * 100;
                    mc.input_c = cameraYaw * 100;    
                }
                

                if (comPort.BaseStream.IsOpen)
                    comPort.sendPacket(mc);

                Thread.Sleep(20);
            }
        }

        private void serialReader()
        {
            heartBeatSend = DateTime.Now;
            serialReaderThreadRun = true;
            int minBytesToRead = 0;

            while (serialReaderThreadRun)
            {
                try
                {
                    Thread.Sleep(1);

                    // Send heart beat every second
                    if (heartBeatSend.Second != DateTime.Now.Second)
                    {
                        MAVLink.mavlink_heartbeat_t hb = new MAVLink.mavlink_heartbeat_t()
                        {
                            type = (byte)MAVLink.MAV_TYPE.GCS,
                            autopilot = (byte)MAVLink.MAV_AUTOPILOT.INVALID,
                            mavlink_version = 3,
                        };
                        comPort.sendPacket(hb);

                        heartBeatSend = DateTime.Now;
                    }

                    while (comPort.BaseStream.IsOpen && comPort.BaseStream.BytesToRead > minBytesToRead)
                    {
                        try
                        {
                            comPort.ReadPacket();
                        }
                        catch
                        {

                            throw;
                        }
                    }

                    // update current state
                    try
                    {
                        comPort.cs.UpdateCurrentSettings(null, false, comPort);
                    }
                    catch
                    {

                        throw;
                    }
                }
                catch (Exception e)
                {
                    // log serial reader fail
                    try
                    {
                        comPort.Close();
                    }
                    catch
                    {
                    }
                }
            }
        }

        private void pitchTrackBar_Scroll(object sender, EventArgs e)
        {
            lock (lockObject)
            {
                cameraPitch = pitchTrackBar.Value;
                pitchLabel.Text = cameraPitch.ToString();
                 
            }
        }

        private void rollTrackBar_Scroll(object sender, EventArgs e)
        {
            lock (lockObject)
            {
                cameraRoll = rollTrackBar.Value;
                rollLabel.Text = cameraRoll.ToString();
            }
        }

        private void yawTrackBar_Scroll(object sender, EventArgs e)
        {
            lock (lockObject)
            {
                cameraYaw = yawTrackBar.Value;
                yawLabel.Text = cameraYaw.ToString();
            }
        }

        private void comPortComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comPortComboBox.Text == "UDP" || comPortComboBox.Text == "TCP")
            {
                baudRateComboBox.Enabled = false;
            }
            else
            {
                baudRateComboBox.Enabled = true;
            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            MAVLink.mavlink_mount_control_t mc = new MAVLink.mavlink_mount_control_t();

            mc.target_component = comPort.MAV.compid;
            mc.target_system = comPort.MAV.sysid;
            lock (lockObject)
            {
                mc.input_a = 30 * 100;
                mc.input_b = 30 * 100;
                mc.input_c = 30 * 100;
            }
            comPort.sendPacket(mc);
        }
    }
}
