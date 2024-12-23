using PanelController.PanelObjects;
using PanelController.PanelObjects.Properties;
using System.IO.Pipes;
using System.Security.AccessControl;
using System.Security.Principal;

namespace PanelControllerBasics
{
    public class NamedPipeChannelSettings
    {
        private static NamedPipeChannelSettings? s_instance = null;

        public static uint s_ClientWaitTimeoutMilliseconds = 5000;

        public uint ClientWaitTimeoutMilliseconds
        {
            get => s_ClientWaitTimeoutMilliseconds;
            set => s_ClientWaitTimeoutMilliseconds = value;
        }

        public NamedPipeChannelSettings()
        {
            if (s_instance is not null)
                throw new InvalidOperationException("Can only create one instance of this type");
        }
    }

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
            _pipe = CreatePipe(pipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
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

            bool connected = _pipe.WaitForConnectionAsync().Wait((int)NamedPipeChannelSettings.s_ClientWaitTimeoutMilliseconds);
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

        private static NamedPipeServerStream CreatePipe(string pipeName, PipeDirection direction, int maxNumberOfServerInstances, PipeTransmissionMode transmissionMode, PipeOptions options)
        {
#if WINDOWS
            PipeSecurity pipeSecurity = new();

            pipeSecurity.AddAccessRule
            (
                new PipeAccessRule
                (
                    "Users",
                    PipeAccessRights.ReadWrite,
                    AccessControlType.Allow
                )
            );

            pipeSecurity.AddAccessRule
            (
                new PipeAccessRule
                (
                    WindowsIdentity.GetCurrent().Name,
                    PipeAccessRights.FullControl,
                    AccessControlType.Allow
                )
            );

            pipeSecurity.AddAccessRule
            (
                new PipeAccessRule
                (
                    "SYSTEM", PipeAccessRights.FullControl,
                    AccessControlType.Allow
                )
            );

            return NamedPipeServerStreamAcl.Create(pipeName, direction, maxNumberOfServerInstances, transmissionMode, options, default, default, pipeSecurity);
#else
            return new(pipeName, direction, maxNumberOfServerInstances, transmissionMode, options);
#endif
        }
    }
}
