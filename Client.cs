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
        public struct MessageInfo
        {
            public int Number { get; set; }
            public string Username { get; set; }
            public string Message { get; set; }
        }
    class Client
    {

        public static MessageInfo GetMessageInfo(byte[] receivedMessage)
        {
            string receivedJson = Encoding.UTF8.GetString(receivedMessage);
            MessageInfo receivedData = JsonConvert.DeserializeObject<MessageInfo>(receivedJson);
            return receivedData;
        }

        public static List<MessageInfo> GetMessagesInfo(byte[] receivedMessage)
        {
            string receivedJson = Encoding.UTF8.GetString(receivedMessage);
            var receivedData = JsonConvert.DeserializeObject<List<MessageInfo>>(receivedJson);
            return receivedData;
        }

        public static byte[] GetMessageBytes(MessageInfo message)
        {
            string json = JsonConvert.SerializeObject(message);
            byte[] buffer = Encoding.UTF8.GetBytes(json);
            return buffer;
        }
        public static byte[] GetMessagesBytes(List<MessageInfo> messages)
        {
            string json = JsonConvert.SerializeObject(messages);
            byte[] buffer = Encoding.UTF8.GetBytes(json);
            return buffer;
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
    }
}
