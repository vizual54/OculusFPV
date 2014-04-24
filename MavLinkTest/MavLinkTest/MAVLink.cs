using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive.Subjects;
using System.Reactive.Linq;
using System.IO;
using System.IO.Ports;
using System.Threading;
using System.Windows.Forms;
using Comms;

namespace MavLinkTest
{
    public class MAVLinkInterface : MAVLink, IDisposable
    {
        public ICommsSerial BaseStream { get; set; }

        //public SerialPort BaseStream {get; set;};
        public CurrentState cs = new CurrentState();

        private Form1 form;

        public event EventHandler ParamListChanged;

        internal string plaintxtline = "";
        string buildplaintxtline = "";

        public MAVState MAV = new MAVState();
        
        public double CONNECT_TIMEOUT_SECONDS = 30;
        /// <summary>
        /// used for outbound packet sending
        /// </summary>
        internal int packetcount = 0;
        /// <summary>
        /// used to calc packets per second on any single message type - used for stream rate comparaison
        /// </summary>
        public double[] packetspersecond { get; set; }
        /// <summary>
        /// time last seen a packet of a type
        /// </summary>
        DateTime[] packetspersecondbuild = new DateTime[256];

        private readonly Subject<int> _bytesReceivedSubj = new Subject<int>();
        private readonly Subject<int> _bytesSentSubj = new Subject<int>();
        /// <summary>
        /// Observable of the count of bytes received, notified when the bytes themselves are received
        /// </summary>
        public IObservable<int> BytesReceived { get { return _bytesReceivedSubj; } }
        /// <summary>
        /// Observable of the count of bytes sent, notified when the bytes themselves are received
        /// </summary>
        public IObservable<int> BytesSent { get { return _bytesSentSubj; } }

        /// <summary>
        /// Observable of the count of packets skipped (on reception), 
        /// calculated from periods where received packet sequence is not
        /// contiguous
        /// </summary>
        public Subject<int> WhenPacketLost { get; set; }

        public Subject<int> WhenPacketReceived { get; set; }

        /// <summary>
        /// used as a serial port write lock
        /// </summary>
        volatile object writelock = new object();
        /// <summary>
        /// used for a readlock on readpacket
        /// </summary>
        volatile object readlock = new object();
        /// <summary>
        /// time seen of last mavlink packet
        /// </summary>
        public DateTime lastvalidpacket { get; set; }
        /// <summary>
        /// mavlink version
        /// </summary>
        byte mavlinkversion = 0;
        /// <summary>
        /// used for last bad serial characters
        /// </summary>
        byte[] lastbad = new byte[2];
        
        int bps1 = 0;
        int bps2 = 0;

        public int bps { get; set; }
        public DateTime bpstime { get; set; }

        float synclost;
        internal float packetslost = 0;
        internal float packetsnotlost = 0;
        DateTime packetlosttimer = DateTime.MinValue;

        public MAVLinkInterface(Form1 _control)
        {
            // init fields
            this.form = _control;
            
            //this.BaseStream = new SerialPort();
            this.packetcount = 0;
            
            this.packetspersecond = new double[0x100];
            this.packetspersecondbuild = new DateTime[0x100];
            this._bytesReceivedSubj = new Subject<int>();
            this._bytesSentSubj = new Subject<int>();
            this.WhenPacketLost = new Subject<int>();
            this.WhenPacketReceived = new Subject<int>();
            this.readlock = new object();
            this.lastvalidpacket = DateTime.MinValue;
            this.mavlinkversion = 0;
            this.bps1 = 0;
            this.bps = 0;
            this.bpstime = DateTime.MinValue;

            this.packetslost = 0f;
            this.packetsnotlost = 0f;
            this.packetlosttimer = DateTime.MinValue;
            //this.lastbad = new byte[2];

        }
        
        public void Close()
        {
            try
            {
                if (BaseStream.IsOpen)
                    BaseStream.Close();
            }
            catch { }
        }

        public void Open(string _comport, string _baudrate)
        {
            BaseStream.BaudRate = int.Parse(_baudrate);
            //BaseStream.DataBits = int.Parse(cmbDataBits.Text);
            //BaseStream.StopBits = (StopBits)Enum.Parse(typeof(StopBits), cmbStopBits.Text);
            //BaseStream.Parity = (Parity)Enum.Parse(typeof(Parity), "none");
            BaseStream.PortName = _comport;
            BaseStream.ReadBufferSize = 16 * 1024;
            form.UpdateTextBox("Trying to open port " + _comport + ".\n");
            Open();
        }

        public void Open()
        {
            // reset
            MAV.sysid = 0;
            MAV.compid = 0;
            MAV.packets.Initialize();
            MAV.VersionString = "";

            try
            {
                lock (writelock)
                {
                    BaseStream.Open();
                    BaseStream.DiscardInBuffer();
                    Thread.Sleep(1000);
                }

                byte[] buffer = new byte[0];
                byte[] buffer1 = new byte[0];

                DateTime start = DateTime.Now;
                DateTime deadline = start.AddSeconds(CONNECT_TIMEOUT_SECONDS);

                var countDown = new System.Timers.Timer { Interval = 1000, AutoReset = false };
                countDown.Elapsed += (sender, e) =>
                {
                    int secondsRemaining = (deadline - e.SignalTime).Seconds;
                    form.ClearTextBox();
                    form.UpdateTextBox(string.Format("Trying to connect.\nTimeout in {0}", secondsRemaining));
                    if (secondsRemaining > 0) countDown.Start();
                };
                countDown.Start();

                int count = 0;
                bool hbseen = false;

                // Connect loop
                while (true)
                { 
                    if (DateTime.Now > deadline)
                    {
                        if (hbseen)
                        {
                            form.UpdateTextBox("Only one Mavlink heatbeat packet received.");
                            throw new Exception("Only one Mavlink heart beat packet read from this port.");
                        }
                        else
                        {
                            form.UpdateTextBox("No Mavlink heatbeat packets received.");
                            throw new Exception("No Mavlink heart beat packets read from this port.");
                        }
                    }

                    System.Threading.Thread.Sleep(1);

                    if (buffer.Length == 0)
                        buffer = GetHeartBeat();

                    System.Threading.Thread.Sleep(1);

                    if (buffer1.Length == 0)
                        buffer1 = GetHeartBeat();


                    if (buffer.Length > 0 || buffer1.Length > 0)
                        hbseen = true;

                    count++;

                    if (buffer.Length > 5 && buffer1.Length > 5 && buffer[3] == buffer1[3] && buffer[4] == buffer1[4])
                    {
                        mavlink_heartbeat_t hb = buffer.ByteArrayToStructure<mavlink_heartbeat_t>(6);

                        if (hb.type != (byte)MAVLink.MAV_TYPE.GCS)
                        {

                            mavlinkversion = hb.mavlink_version;
                            MAV.aptype = (MAV_TYPE)hb.type;
                            MAV.apname = (MAV_AUTOPILOT)hb.autopilot;

                            //setAPType();

                            MAV.sysid = buffer[3];
                            MAV.compid = buffer[4];
                            MAV.recvpacketcount = buffer[2];
                            //log.InfoFormat("ID sys {0} comp {1} ver{2}", MAV.sysid, MAV.compid, mavlinkversion);
                            break;
                        }
                    }
                }

                countDown.Stop();

            }
            catch
            {
            
            }
            form.UpdateTextBox("Done open " + MAV.sysid + " " + MAV.compid);
            packetslost = 0;
            synclost = 0;
        }

        public byte[] GetHeartBeat()
        {
            DateTime start = DateTime.Now;
            int readcount = 0;
            while (true)
            {
                byte[] buffer = ReadPacket();
                readcount++;
                if (buffer.Length > 5)
                {
                    if (buffer[5] == (byte)MAVLINK_MSG_ID.HEARTBEAT)
                    {
                        mavlink_heartbeat_t hb = buffer.ByteArrayToStructure<mavlink_heartbeat_t>(6);
                        if (hb.type != (byte)MAVLink.MAV_TYPE.GCS)
                        {
                            return buffer;
                        }
                    }
                }
                if (DateTime.Now > start.AddMilliseconds(2200) || readcount > 200)
                {
                    return new byte[0];
                }
            }
        }

        public byte[] ReadPacket()
        {
            byte[] buffer = new byte[260];
            int count = 0;
            int length = 0;
            int readcount = 0;
            lastbad = new byte[2];

            BaseStream.ReadTimeout = 1200; // 1200 ms between chars - the gps detection requires this.

            DateTime start = DateTime.Now;

            lock (readlock)
            {
                while (BaseStream.IsOpen)
                {
                    try
                    { 
                        if (readcount > 300)
                        {
                            //No valid mavlink package
                            break;
                        }
                        readcount++;
                        //MAV.cs.datetime = DateTime.Now;

                        DateTime to = DateTime.Now.AddMilliseconds(BaseStream.ReadTimeout);

                        // Console.WriteLine(DateTime.Now.Millisecond + " SR1a " + BaseStream.BytesToRead);

                        while (BaseStream.IsOpen && BaseStream.BytesToRead <= 0)
                        {
                            if (DateTime.Now > to)
                            {
                                //log.InfoFormat("MAVLINK: 1 wait time out btr {0} len {1}", BaseStream.BytesToRead, length);
                                throw new Exception("Timeout");
                            }
                            System.Threading.Thread.Sleep(1);
                            //Console.WriteLine(DateTime.Now.Millisecond + " SR0b " + BaseStream.BytesToRead);
                        }

                        if (BaseStream.IsOpen)
                        {
                            BaseStream.Read(buffer, count, 1);
                        }

                    }
                    catch (Exception e)
                    {
                        break;
                    }

                    // check if looks like a mavlink packet and check for exclusions and write to console
                    if (buffer[0] != 254)
                    {
                        if (buffer[0] >= 0x20 && buffer[0] <= 127 || buffer[0] == '\n' || buffer[0] == '\r')
                        {
                            // check for line termination
                            if (buffer[0] == '\r' || buffer[0] == '\n')
                            {
                                // check new line is valid
                                if (buildplaintxtline.Length > 3)
                                    plaintxtline = buildplaintxtline;

                                // reset for next line
                                buildplaintxtline = "";
                            }
                            form.UpdateTextBox(((char)buffer[0]).ToString());
                            //TCPConsole.Write(buffer[0]);
                            //Console.Write((char)buffer[0]);
                            buildplaintxtline += (char)buffer[0];
                        }
                        _bytesReceivedSubj.OnNext(1);
                        count = 0;
                        lastbad[0] = lastbad[1];
                        lastbad[1] = buffer[0];
                        buffer[1] = 0;
                        continue;
                    }
                    // reset count on valid packet
                    readcount = 0;

                    // check for a header
                    if (buffer[0] == 254)
                    {
                        if (count == 0)
                        {
                            DateTime to = DateTime.Now.AddMilliseconds(BaseStream.ReadTimeout);
                            while (BaseStream.IsOpen && BaseStream.BytesToRead < 5)
                            {
                                if (DateTime.Now > to)
                                {
                                    throw new Exception("Timeout");
                                }
                                System.Threading.Thread.Sleep(1);
                            }
                            int read = BaseStream.Read(buffer, 1, 5);
                            count = read;
                        }
                        // packet length
                        length = buffer[1] + 6 + 2 - 2; // data + header + checksum - U - length
                        if (count >= 5)
                        {
                            if (MAV.sysid != 0)
                            {
                                if (MAV.sysid != buffer[3] || MAV.compid != buffer[4])
                                {
                                    if (buffer[3] == '3' && buffer[4] == 'D')
                                    {
                                        // this is a 3dr radio rssi packet
                                    }
                                    else
                                    {
                                        //log.InfoFormat("Mavlink Bad Packet (not addressed to this MAV) got {0} {1} vs {2} {3}", buffer[3], buffer[4], MAV.sysid, MAV.compid);
                                        return new byte[0];
                                    }
                                }
                            }

                            try
                            {
                                DateTime to = DateTime.Now.AddMilliseconds(BaseStream.ReadTimeout);

                                while (BaseStream.IsOpen && BaseStream.BytesToRead < (length - 4))
                                {
                                    if (DateTime.Now > to)
                                    {
                                        //log.InfoFormat("MAVLINK: 3 wait time out btr {0} len {1}", BaseStream.BytesToRead, length);
                                        break;
                                    }
                                    System.Threading.Thread.Sleep(1);
                                }
                                if (BaseStream.IsOpen)
                                {
                                    int read = BaseStream.Read(buffer, 6, length - 4);
                                }
                                count = length + 2;
                            }
                            catch
                            {
                                break;
                            }

                            break;
                        }
                    }

                    count++;
                    if (count == 299)
                        break;
                }
            } // end readlock

            Array.Resize<byte>(ref buffer, count);

            _bytesReceivedSubj.OnNext(buffer.Length);

            if (packetlosttimer.AddSeconds(5) < DateTime.Now)
            {
                packetlosttimer = DateTime.Now;
                packetslost = (packetslost * 0.8f);
                packetsnotlost = (packetsnotlost * 0.8f);
            }

            if (bpstime.Second != DateTime.Now.Second && BaseStream.IsOpen)
            {
                Console.Write("bps {0} loss {1} left {2} mem {3}      \n", bps1, synclost, BaseStream.BytesToRead, System.GC.GetTotalMemory(false) / 1024 / 1024.0);
                bps2 = bps1; // prev sec
                bps1 = 0; // current sec
                bpstime = DateTime.Now;
            }

            bps1 += buffer.Length;
            bps = (bps1 + bps2) / 2;

            ushort crc = MavlinkCRC.crc_calculate(buffer, buffer.Length - 2);

            if (buffer.Length > 5 && buffer[0] == 254)
            {
                crc = MavlinkCRC.crc_accumulate(MAVLINK_MESSAGE_CRCS[buffer[5]], crc);
            }

            if (buffer.Length > 5 && buffer[1] != MAVLINK_MESSAGE_LENGTHS[buffer[5]])
            {
                if (MAVLINK_MESSAGE_LENGTHS[buffer[5]] == 0) // pass for unknown packets
                {

                }
                else
                {
                    //log.InfoFormat("Mavlink Bad Packet (Len Fail) len {0} pkno {1}", buffer.Length, buffer[5]);
                    if (buffer.Length == 11 && buffer[0] == 'U' && buffer[5] == 0)
                    {
                        string message = "Mavlink 0.9 Heartbeat, Please upgrade your AP, This planner is for Mavlink 1.0\n\n";
                        MessageBox.Show(message);
                        throw new Exception(message);
                    }
                    return new byte[0];
                }
            }

            if (buffer.Length < 5 || buffer[buffer.Length - 1] != (crc >> 8) || buffer[buffer.Length - 2] != (crc & 0xff))
            {
                int packetno = -1;
                if (buffer.Length > 5)
                {
                    packetno = buffer[5];
                }
                if (packetno != -1 && buffer.Length > 5 && MAVLINK_MESSAGE_INFO[packetno] != null)
                {
                    //log.InfoFormat("Mavlink Bad Packet (crc fail) len {0} crc {1} vs {4} pkno {2} {3}", buffer.Length, crc, packetno, MAVLINK_MESSAGE_INFO[packetno].ToString(), BitConverter.ToUInt16(buffer, buffer.Length - 2));
                    form.UpdateTextBox("Mavlink Bad Packet.");
                }
                return new byte[0];
            }

            try
            {

                if ((buffer[0] == 'U' || buffer[0] == 254) && buffer.Length >= buffer[1])
                {
                    if (buffer[3] == '3' && buffer[4] == 'D')
                    {
                        // Do nothing
                    }
                    else
                    {
                        byte packetSeqNo = buffer[2];
                        int expectedPacketSeqNo = ((MAV.recvpacketcount + 1) % 0x100);
                        
                        if (packetSeqNo != expectedPacketSeqNo)
                        {
                            synclost++; // actualy sync loss's
                            int numLost = 0;

                            if (packetSeqNo < ((MAV.recvpacketcount + 1))) // recvpacketcount = 255 then   10 < 256 = true if was % 0x100 this would fail
                            {
                                numLost = 0x100 - expectedPacketSeqNo + packetSeqNo;
                            }
                            else
                            {
                                numLost = packetSeqNo - MAV.recvpacketcount;
                            }
                            packetslost += numLost;
                            WhenPacketLost.OnNext(numLost);

                            //log.InfoFormat("lost pkts new seqno {0} pkts lost {1}", packetSeqNo, numLost);
                        }

                        packetsnotlost++;
                        MAV.recvpacketcount = packetSeqNo;
                        WhenPacketReceived.OnNext(1);

                    }

                    if (double.IsInfinity(packetspersecond[buffer[5]]))
                        packetspersecond[buffer[5]] = 0;

                    packetspersecond[buffer[5]] = (((1000 / ((DateTime.Now - packetspersecondbuild[buffer[5]]).TotalMilliseconds) + packetspersecond[buffer[5]]) / 2));

                    packetspersecondbuild[buffer[5]] = DateTime.Now;

                    lock (writelock)
                    {
                        MAV.packets[buffer[5]] = buffer;
                        MAV.packetseencount[buffer[5]]++;
                    }

                    if (buffer[5] == (byte)MAVLink.MAVLINK_MSG_ID.STATUSTEXT) // status text
                    {
                        var msg = MAV.packets[(byte)MAVLink.MAVLINK_MSG_ID.STATUSTEXT].ByteArrayToStructure<MAVLink.mavlink_statustext_t>(6);

                        byte sev = msg.severity;

                        string logdata = Encoding.ASCII.GetString(msg.text);
                        int ind = logdata.IndexOf('\0');
                        if (ind != -1)
                            logdata = logdata.Substring(0, ind);
                        //log.Info(DateTime.Now + " " + logdata);

                        form.UpdateTextBox(logdata);
                        //MAV.cs.messages.Add(logdata);

                        if (sev >= 3)
                        {

                        }
                    }

                    // set ap type
                    if (buffer[5] == (byte)MAVLink.MAVLINK_MSG_ID.HEARTBEAT)
                    {
                        mavlink_heartbeat_t hb = buffer.ByteArrayToStructure<mavlink_heartbeat_t>(6);

                        if (hb.type != (byte)MAVLink.MAV_TYPE.GCS)
                        {
                            mavlinkversion = hb.mavlink_version;
                            MAV.aptype = (MAV_TYPE)hb.type;
                            MAV.apname = (MAV_AUTOPILOT)hb.autopilot;
                            //setAPType();
                        }
                    }

                }
            }
            catch { }

            if (buffer[3] == '3' && buffer[4] == 'D')
            {
                // dont update last packet time for 3dr radio packets
            }
            else
            {
                lastvalidpacket = DateTime.Now;
            }

            return buffer;
        }

        public void sendPacket(object indata)
        {
            bool validPacket = false;
            byte a = 0;
            foreach (Type ty in MAVLink.MAVLINK_MESSAGE_INFO)
            {
                if (ty == indata.GetType())
                {
                    validPacket = true;
                    generatePacket(a, indata);
                    return;
                }
                a++;
            }
            if (!validPacket)
            {
                //log.Info("Mavlink : NOT VALID PACKET sendPacket() " + indata.GetType().ToString());
            }
        }

        void generatePacket(byte messageType, object indata)
        {
            if (!BaseStream.IsOpen)
            {
                return;
            }

            lock (writelock)
            {
                byte[] data;

                data = MavlinkUtil.StructureToByteArray(indata);
                
                byte[] packet = new byte[data.Length + 6 + 2];

                packet[0] = 254;
                packet[1] = (byte)data.Length;
                packet[2] = (byte)packetcount;

                packetcount++;

                packet[3] = 255; // this is always 255 - MYGCS
                packet[4] = (byte)MAV_COMPONENT.MAV_COMP_ID_MISSIONPLANNER;
                packet[5] = messageType;

                int i = 6;
                foreach (byte b in data)
                {
                    packet[i] = b;
                    i++;
                }

                ushort checksum = MavlinkCRC.crc_calculate(packet, packet[1] + 6);

                checksum = MavlinkCRC.crc_accumulate(MAVLINK_MESSAGE_CRCS[messageType], checksum);

                byte ck_a = (byte)(checksum & 0xFF); ///< High byte
                byte ck_b = (byte)(checksum >> 8); ///< Low byte

                packet[i] = ck_a;
                i += 1;
                packet[i] = ck_b;
                i += 1;

                if (BaseStream.IsOpen)
                {
                    BaseStream.Write(packet, 0, i);
                    _bytesSentSubj.OnNext(i);
                }
            }
        }

        public void Dispose()
        {
            if (BaseStream.IsOpen)
                BaseStream.Close();
            BaseStream = null;
        }
    }

    public class MAVState
    {
        internal int recvpacketcount = 0;
        /// <summary>
        /// mavlink version
        /// </summary>
        public string VersionString { get; set; }
        /// <summary>
        /// mavlink remote sysid
        /// </summary>
        public byte sysid { get; set; }
        /// <summary>
        /// mavlink remove compid
        /// </summary>
        public byte compid { get; set; }
        /// <summary>
        /// mavlink ap type
        /// </summary>
        public MAVLink.MAV_TYPE aptype { get; set; }
        public MAVLink.MAV_AUTOPILOT apname { get; set; }
        /// <summary>
        /// storage of a previous packet recevied of a specific type
        /// </summary>
        public byte[][] packets { get; set; }
        public int[] packetseencount { get; set; }

        public MAVState()
        {
            this.sysid = 0;
            this.compid = 0;
            this.packets = new byte[0x100][];
            this.packetseencount = new int[0x100];
            this.aptype = 0;
            this.apname = 0;
            this.recvpacketcount = 0;
            this.VersionString = "";
        }
    }
}
