using PanelController.PanelObjects;
using PanelController.PanelObjects.Properties;
using System.IO.Ports;
using System.Runtime.ConstrainedExecution;
using System.Text;

namespace PanelControllerBasics
{
    [AutoLaunch]
    public class SerialChannelOptions : IPanelObject
    {
        public static bool s_DTRDefault = true;

        public static bool s_RTSDefault = true;

        public static int s_DetectorDefaultBaudRate = 115200;

        [UserProperty]
        public bool DTRDefault { get => s_DTRDefault; set => s_DTRDefault = value; }

        [UserProperty]
        public bool RTSDefault { get => s_RTSDefault; set => s_RTSDefault = value; }

        [UserProperty]
        public int DetectorDefaultBaudRate { get => s_DetectorDefaultBaudRate; set => s_DetectorDefaultBaudRate = value; }
    }

    public class SerialChannel : IChannel
    {
        private SerialPort _port = new();

        private readonly int _openDelay;
        
        public static string[] s_oldPortNames = [];

        [UserProperty]
        public string ChannelName { get => $"SerialChannel:{(IsOpen ? _port.PortName : "")}"; }

        [UserProperty]
        public bool DTRDefault
        {
            get => SerialChannelOptions.s_RTSDefault;
            set => SerialChannelOptions.s_DTRDefault = value;
        }

        [UserProperty]
        public bool RTSDefault
        {
            get => SerialChannelOptions.s_RTSDefault;
            set => SerialChannelOptions.s_RTSDefault = value;
        }

        [UserProperty]
        public int DetectorDefaultBaudRate
        {
            get => SerialChannelOptions.s_DetectorDefaultBaudRate;
            set => SerialChannelOptions.s_DetectorDefaultBaudRate = value;
        }

        [UserProperty]
        public bool DTR { get => _port.DtrEnable; set => _port.DtrEnable = value; }

        [UserProperty]
        public bool RTS { get => _port.RtsEnable; set => _port.RtsEnable = value; }

        [UserProperty]
        public int OpenDelay { get => _openDelay; }

        public bool IsOpen => _port.IsOpen;

        public event EventHandler<byte[]>? BytesReceived;

        [UserConstructor("Open a channel using the serial port.")]
        public SerialChannel(string portName, int baudRate, int openDelay = 200)
        {
            _port.PortName = portName;
            _port.BaudRate = baudRate;
            DTR = DTRDefault;
            RTS = RTSDefault;
            _openDelay = openDelay;
            _port.DataReceived += DataReceived;
        }

        public void Close()
        {
            if (_port.IsOpen)
                _port.Close();
        }

        public object? Open()
        {
            try
            {
                _port.Open();
            }
            catch (Exception exc)
            {
                return exc;
            }

            Task.Delay(OpenDelay).Wait();
            return null;
        }

        public object? Send(byte[] data)
        {
            try
            {
                _port.Write(data, 0, data.Length);
            }
            catch (Exception exc)
            {
                return exc;
            }

            return null;
        }

        private void DataReceived(object? sender, SerialDataReceivedEventArgs e)
        {
            if (sender is not SerialPort port)
                return;
            BytesReceived?.Invoke(this, Encoding.UTF8.GetBytes(port.ReadExisting()));
        }

        [IChannel.Detector]
        public static IChannel[] Detect()
        {
            List<IChannel> channels = new();
            string[] currentPortNames = SerialPort.GetPortNames();

            foreach (string portName in currentPortNames)
            {
                if (s_oldPortNames.Contains(portName))
                    continue;
                channels.Add(new SerialChannel(portName, SerialChannelOptions.s_DetectorDefaultBaudRate));
            }

            s_oldPortNames = currentPortNames;
            return channels.ToArray();
        }
    }
}
