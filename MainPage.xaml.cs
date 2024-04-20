using System.Net.Sockets;
using System.Net;
using System.Xml.Linq;
using KSiS2;
using System.Text;
using System.Collections.Concurrent;
using Obvs;
using System.Drawing.Interop;

namespace KSIS2Client
{
    public partial class MainPage : ContentPage
    {
        private string Username { get; set; }

        private static List<Message> Messages = new List<Message>();
        //private ConcurrentDictionary<IPEndPoint, byte[]> DataToSend = new();
        private ConcurrentQueue<byte[]> DataToSend = new();
        private IPEndPoint? LocalIPEndPoint;


        public MainPage()
        {
            InitializeComponent();
            nickEntry.Text = "alex";
            ipEntry.Text = "127.0.0.1";
            portEntry.Text = "1";
            MessageEntry.Text = "hello world!";
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
            UsernameBtn.IsEnabled = false;
            SendButton.IsEnabled = true;
        }

        public void NickChanged(object sender, EventArgs e)
        {
            UsernameBtn.IsEnabled = (sender as Entry)!.Text.Length != 0;
            ipEntry.IsEnabled = UsernameBtn.IsEnabled;
            portEntry.IsEnabled = UsernameBtn.IsEnabled;    
        }

        private void SendButton_Clicked(object sender, EventArgs e)
        {
            Message message = new Message(LocalIPEndPoint, MessageEntry.Text);
            message.MessageType = MessageType.Text;
            DataToSend.Enqueue(message.GetMessageBytes());
        }

        private async Task AddToChat(string message)
        {
            await this.Dispatcher.DispatchAsync(() =>
            {
                Label label = new Label
                {
                    Text = message,
                    FontSize = 12,

                };

                ChatCont.Children.Add(label);
            });
        }

        private async void ConnectButton_Clicked(object sender, EventArgs e)
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
                        await AddToChat("Cooщение отправлено! IP: " + ipEntry.Text + "Port: " + port.ToString());
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
        private async void ConnectToServer(string ipAddress, int port)
        {
            IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse(ipAddress), port);
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(ipPoint!);
            LocalIPEndPoint = socket.LocalEndPoint as IPEndPoint;
            var message = new Message((socket.LocalEndPoint as IPEndPoint)!, Username);
            message.MessageType = MessageType.Init;
            //AddToChat($"Text {message.GetText()} Type {message.MessageType} IP {message.GetIPEndPoint()} length {message.GetSerializedBytes().Length }");
            byte[] arr = [];
            try
            {
                arr = message.GetSerializedBytes();
            }
            catch
            {
                await AddToChat("hah");
            }
                socket.Send(arr);


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

        private async Task Process(Socket clientSocket)
        {
            var receiveTask = ReceiveDataThread(clientSocket);
            var sendTask = SendDataThread(clientSocket);
            await Task.WhenAny(receiveTask, sendTask); // Ждем завершения любой из задач
        }

        public async Task SendDataThread(Socket socket)
        {
            while (true && socket.Connected)
            {
                byte[] sendBuffer = [];
                if (DataToSend.TryDequeue(out sendBuffer))
                {
                    await socket.SendAsync(new ArraySegment<byte>(sendBuffer), SocketFlags.None);
                }
            }
        }

        public async Task ReceiveDataThread(Socket socket)
        {
            byte[] buffer = new byte[2048];
            while (true && socket.Connected)
            {
                var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), SocketFlags.None);
                await AddToChat($"{result} received data");
                if (result > 0)
                {
                    Message message = new();
                    var answer = message;
                    try
                    {
                        byte[] data = buffer.Take(result).ToArray();
                        message = new Message(data);
                        await AddToChat($"Text {message.GetText()} Type {message.MessageType} IP {message.GetIPEndPoint()}  ");
                        if (message != null)
                            answer = await ProcessMessage(message);
                    }
                    catch (Exception ex)
                    {
                        await AddToChat($"{ex.Message}");
                    }
                    if (answer.MessageType != MessageType.Error)
                    {
                        await socket.SendAsync(new ArraySegment<byte>(answer.GetSerializedBytes()), SocketFlags.None);
                        await AddToChat("Success " + ((int)answer.MessageType).ToString());
                    }
                    else
                    {
                        await AddToChat("Error " + ((int)answer.MessageType).ToString());
                    }
                }
            }
        }

        private async Task<Message> ProcessMessage(Message message)
        {
            return null;
        }

        private void ipOrPortEntry_TextChanged(object sender, TextChangedEventArgs e)
        {
            ConnectButton.IsEnabled = (IPAddress.TryParse(ipEntry.Text, out IPAddress? ipAddress) && Client.IsPortAvailable(int.Parse(portEntry.Text))); 
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
