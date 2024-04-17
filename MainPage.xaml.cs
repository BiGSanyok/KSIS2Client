using System.Net.Sockets;
using System.Net;
using System.Xml.Linq;
using KSiS2;
using System.Text;

namespace KSIS2Client
{
    public partial class MainPage : ContentPage
    {
        private string Username { get; set; }

        private static List<Message> Messages = new List<Message>();


        public MainPage()
        {
            InitializeComponent();
        }

        public async void GetUsername()
        {
            bool isCorrect = true;
            do
            { 
                isCorrect = true;
                Username = nickEntry.Text;
                if (Username.Length == 0)
                {
                    await DisplayAlert("Ошибка", "Пустой никнейм!", "ОK");
                    isCorrect = false;
                }
            } while (!isCorrect);
        }

        private void OnButtonReleased(object sender, EventArgs e)
        {
            (sender as Button)!.BackgroundColor = Color.Parse("Blue");
        }

        private void OnButtonFocused(object sender, EventArgs e)
        {
            (sender as Button)!.BackgroundColor = Color.Parse("Orange");
        }

        public void OnUsernameBtnClicked(object sender, EventArgs e)
        {
            GetUsername();
            SendButton.IsEnabled = true;
        }

        public void NickChanged(object sender, EventArgs e)
        {
            UsernameBtn.IsEnabled = (sender as Entry)!.Text.Length != 0;
            ipEntry.IsEnabled = UsernameBtn.IsEnabled;
            portEntry.IsEnabled = UsernameBtn.IsEnabled;    
        }

        private void AddToChat(string message)
        {
            Dispatcher.Dispatch(new Action(() =>
            {

                Label label = new Label
                {
                    Text = message,
                    FontSize = 12,

                };

                ChatCont.Children.Add(label);
            }));
        }

        private async void SendButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                IPAddress ip = IPAddress.Parse(ipEntry.Text);
                int port = int.Parse(portEntry.Text);
                if (port <= 65535)
                {
                    try
                    {
                        ConnectToServer(ipEntry.Text, port);
                        AddToChat("Cooщение отправлено! IP: " + ipEntry.Text + "Port: " + port.ToString());
                    }
                    catch
                    {
                        await DisplayAlert("Ошибка", "Ошибка при работе с сервером", "ОK");
                    }
                }
                else
                    await DisplayAlert("Ошибка", "Порт некорректен", "ОK");
            }
            catch
            {
                await DisplayAlert("Ошибка", "IP-адрес или порт введен некоректно", "ОK");
            }
        }
        private void ConnectToServer(string ipAddress, int port)
        {
            IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse(ipAddress), port);
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(ipPoint!);
            var message = new Message((socket.LocalEndPoint as IPEndPoint)!, Username);
            message.MessageType = MessageType.Init;
            socket.Send(message.GetSerializedBytes());

            /* Messages.Add(Client.CommunicateWithServer(socket, username, MessageEntry.Text));
             AddToChat(Messages.Last().Username + ": " + messages.Last().Message);
             socket.Shutdown(SocketShutdown.Both);
             socket.Close();*/
            Thread thread = new Thread(() =>
            {
                Thread send = new Thread(() => { SendDataThread(socket); });
                Thread receive = new Thread(() => { ReceiveDataThread(socket);  });
            });
        }

        public void SendDataThread(Socket socket)
        {

        }

        public void ReceiveDataThread(Socket socket)
        {

        }


        /*public static MessageInfo CommunicateWithClient(Socket clientSocket)
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
            byte[] outputData = GetMessageBytes(new MessageInfo { Username = "server", Message = "сообщение получено" });
            clientSocket.Send(outputData);
            return GetMessageInfo(inputData);
        }

        public static MessageInfo CommunicateWithServer(Socket socket, string username, string message)
        {
            var messageBytes = GetMessageBytes(new MessageInfo { Username = username, Message = message });
            socket.Send(messageBytes);
            socket.Shutdown(SocketShutdown.Send);
            int bytesRead = 0;
            List<byte> inputBuffer = new List<byte>();
            byte[] inputData = new byte[1024];
            MessageInfo result = new MessageInfo { Username = "Server", Message = "Empty" };
            do
            {
                bytesRead = 0;
                bytesRead = socket!.Receive(inputData);
                foreach (byte b in inputData)
                    if (b != '\0')
                        inputBuffer.Add(b);
                if (bytesRead != 0)
                    result = GetMessageInfo(inputData);
            }
            while (bytesRead > 0*//* && result.Username != "server" && result.Message != "сообщение получено"*//*);

            return result;
        }*/
    }

}
