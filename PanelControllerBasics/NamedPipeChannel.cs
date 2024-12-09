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

        public bool IsOpen => throw new NotImplementedException();

        public event EventHandler<byte[]>? BytesReceived;

        private NamedPipeServerStream _pipe;

        private Thread? _readerThread;

        public NamedPipeChannel(string pipeName)
        {
            PipeName = pipeName;
            _pipe = new NamedPipeServerStream(pipeName, PipeDirection.InOut, 1);
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


            _pipe.WaitForConnection();
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
                if (ReadNextMessage() is byte[] data)
                    BytesReceived?.Invoke(this, data);
                else
                    Thread.Sleep(2);
            }
        }

        private byte[]? ReadNextMessage(int timeoutMilliseconds = 1)
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            byte[] firstByteBuffer = new byte[1];
            Task readTask = _pipe.ReadAsync(firstByteBuffer, 0, 1, cts.Token);
            bool timeout = Task.WaitAny(readTask, Task.Delay(timeoutMilliseconds)) == 1;

            if (readTask.IsFaulted && readTask.Exception is not null)
                throw readTask.Exception;

            if (timeout)
                return null;

            List<byte> message = new List<byte>() { firstByteBuffer[0] };
            while (!_pipe.IsMessageComplete)
                message.Add((byte)_pipe.ReadByte());

            return message.ToArray();
        }
    }
}
