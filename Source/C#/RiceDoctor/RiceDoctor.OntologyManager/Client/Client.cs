using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using JetBrains.Annotations;
using RiceDoctor.Shared;

namespace RiceDoctor.OntologyManager
{
    public class Client
    {
        [NotNull] private readonly EndPoint _address;

        public Client([NotNull] string ip, int port)
        {
            Check.NotEmpty(ip, nameof(ip));

            _address = new IPEndPoint(IPAddress.Parse(ip), port);
        }

        [NotNull]
        public string Send([NotNull] string message)
        {
            Check.NotEmpty(message, nameof(message));

            string result;
            using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                socket.Connect(_address);

                var sendingLength = Encoding.UTF8.GetByteCount(message);
                var sendingBytes = Encoding.UTF8.GetBytes(message);
                var sendingLengthInBytes = BitConverter.GetBytes(sendingLength);

                Logger.Log($"Client sends {sendingLength} bytes: \"{message}\"");

                socket.Send(sendingLengthInBytes);
                socket.Send(sendingBytes);

                var receivedLengthInBytes = new byte[4];
                socket.Receive(receivedLengthInBytes);
                var receivedLength = BitConverter.ToInt32(receivedLengthInBytes, 0);

                var receivingBytes = new List<byte>();
                var tmpReceivingBytes = new byte[receivedLength];
                var receivingLength = receivedLength;
                do
                {
                    var length = socket.Receive(tmpReceivingBytes);
                    receivingBytes.AddRange(tmpReceivingBytes.Take(length));
                    receivingLength -= length;
                } while (receivingLength != 0);

                var receivedBytes = receivingBytes.ToArray();
                result = Encoding.UTF8.GetString(receivedBytes);

                Logger.Log($"Client receives {receivedLength} bytes: \"{result}\"");
            }

            return result;
        }
    }
}