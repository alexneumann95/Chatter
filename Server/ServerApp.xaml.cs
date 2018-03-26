using System.Windows;

namespace Chatter
{
    public partial class ServerApp : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            MainWindow = new ServerWindow();
            MainWindow.Show();

            // Start Server
            Server.Start(MainWindow.FindName("ServerLog") as System.Windows.Controls.TextBox);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            // Stop Server
            Server.Stop();
        }
    }
}
