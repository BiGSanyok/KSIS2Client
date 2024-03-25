using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Net;
using Newtonsoft;
using Newtonsoft.Json;

namespace KSIS2Client
{
    class Network
    {
        public struct MessageInfo
        {
            public string Username { get; set; }
            public string Message { get; set; }
        }

        public static MessageInfo GetMessageInfo(byte[] receivedMessage)
        {
            string receivedJson = Encoding.UTF8.GetString(receivedMessage);
            MessageInfo receivedData = JsonConvert.DeserializeObject<MessageInfo>(receivedJson);
            return receivedData;
        }

        public static byte[] GetMessageBytes(MessageInfo message)
        {
            string json = JsonConvert.SerializeObject(message);
            byte[] buffer = Encoding.UTF8.GetBytes(json);
            return buffer;
        }
        

        public static MessageInfo CommunicateWithClient(Socket clientSocket)
        {
            StringBuilder inputMessage = new StringBuilder(); 
            int bytesRead = 0; 
            byte[] inputData = new byte[256]; 
            do
            {
                bytesRead = clientSocket.Receive(inputData);
            }
            while (bytesRead > 0);
            //Console.WriteLine(DateTime.Now.ToShortTimeString() + ": " + inputMessage.ToString());
            byte[] outputData = GetMessageBytes(new MessageInfo {Username = "server", Message = "сообщение получено"});
            clientSocket.Send(outputData);
            return GetMessageInfo(inputData);
        }

        public static bool IsPortAvailable(int myPort)
        {
            var properties = IPGlobalProperties.GetIPGlobalProperties();
            var activeTcpConnections = properties.GetActiveTcpConnections();
            var activeTcpListeners = properties.GetActiveTcpListeners();
            var activeUdpListeners = properties.GetActiveUdpListeners();

            var allPorts = new List<int>();
            allPorts.AddRange(activeTcpConnections.Select(c => c.LocalEndPoint.Port));
            allPorts.AddRange(activeTcpListeners.Select(l => l.Port));
            allPorts.AddRange(activeUdpListeners.Select(l => l.Port));

            return !allPorts.Contains(myPort);
        }

        public static MessageInfo CommunicateWithServer(Socket socket, string username, string message)
        {
            var messageBytes = GetMessageBytes(new MessageInfo { Username = username, Message = message });
            socket.Send(messageBytes);
            socket.Shutdown(SocketShutdown.Send);
            int bytesRead = 0; 
            List<byte> inputBuffer = new List<byte>();
            byte[] inputData = new byte[256]; 
            MessageInfo result = new MessageInfo { Username = "Server", Message = "Empty" };
            do
            {
                bytesRead = 0;
                bytesRead =  socket!.Receive(inputData);
                foreach (byte b in inputData)
                    if (b != '\0')
                        inputBuffer.Add(b);
                if (bytesRead != 0)
                    result = GetMessageInfo(inputData);
            }
            while (bytesRead > 0/* && result.Username != "server" && result.Message != "сообщение получено"*/);

            return result;
        }
    }
}
