using System.Net.Sockets;
using System.Net;
using System.Xml.Linq;

namespace KSIS2Client
{
    public partial class MainPage : ContentPage
    {
        string username = "";

        List<Network.MessageInfo> messages = new List<Network.MessageInfo>();

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
                username = nickEntry.Text;
                if (username.Length == 0)
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
            messages.Add(Network.CommunicateWithServer(socket, username, MessageEntry.Text));
            AddToChat(messages.Last().Username + ": " + messages.Last().Message);
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
        }
        private void AddToChat(string message)
        {
            Label label = new Label
            {
                Text = message,
                FontSize = 12,

            };

            ChatCont.Children.Add(label);
        }
    }

}
