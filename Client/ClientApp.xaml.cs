using System.Windows;
using System.Windows.Controls;

namespace Chatter
{
    public partial class ClientApp : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            MainWindow = new ClientWindow();
            MainWindow.Show();

            // Start the Client
            Client.Start(MainWindow.FindName("ChatLog") as TextBox);

            // Hook up message box's controls
            TextBox messageBox = MainWindow.FindName("MessageBox") as TextBox;
            messageBox.PreviewMouseLeftButtonDown += (sender, me) => messageBox.Text = "";
            messageBox.PreviewKeyDown += (sender, ke) =>
            {
                if (Client.Status != ClientStatus.CONNECTED)
                    return;

                if (ke.Key == System.Windows.Input.Key.Enter)
                {
                    string data = messageBox.Text;
                    if (data.Length > 0)
                    {
                        messageBox.Text = "";
                        Client.SendMessageToServer(Rules.MESSAGE, data);
                        Client.PrintToChatLog("You > " + data);
                    }
                }
            };
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            // Stop the client
            Client.Stop();
        }
    }
}
