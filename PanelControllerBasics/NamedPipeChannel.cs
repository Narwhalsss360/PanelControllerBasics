using PanelController.PanelObjects;
using PanelController.PanelObjects.Properties;
using System.IO.Pipes;

namespace PanelControllerBasics
{
    public class NamedPipeChannel : IChannel
    {
        [ItemName]
        public string Name { get => $"Pipe:{PipeName}"; }

        public string PipeName { get; init; }

        public bool IsOpen => _pipe.IsConnected;

        public event EventHandler<byte[]>? BytesReceived;

        private NamedPipeServerStream _pipe;

        private Thread? _readerThread;

        public NamedPipeChannel(string pipeName)
        {
            PipeName = pipeName;
            _pipe = new NamedPipeServerStream(pipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
        }

        public void Close()
        {
            if (_pipe.IsConnected)
                _pipe.Close();
            _readerThread?.Join();
        }

        public object? Open()
        {
            if (_pipe.IsConnected)
                return null;

            bool connected = _pipe.WaitForConnectionAsync().Wait(5000);
            if (!connected)
                return "No pipe client connected.";

            _readerThread = new Thread(Reader);
            _readerThread.Start();
            return null;
        }

        public object? Send(byte[] data)
        {
            _pipe.Write(data, 0, data.Length);
            return null;
        }

        private void Reader()
        {
            while (_pipe.IsConnected)
            {
                int read = _pipe.ReadByte();
                if (read == -1)
                    break;
                BytesReceived?.Invoke(this, [(byte)read]);
            }
        }
    }
}
