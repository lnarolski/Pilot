using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration;

namespace Pilot
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            KeyboardRead.IsVisible = false;

            ConnectionClass.connected = false;

        }

        private void Button_Shortcuts(object sender, EventArgs e)
        {
            ShortcutsPage shortcutsPage = new ShortcutsPage();
            Navigation.PushModalAsync(shortcutsPage);
        }

        private void Show_keyboard(object sender, EventArgs e)
        {
            KeyboardRead.Unfocus();
            Thread.Sleep(500);
            KeyboardRead.Focus();
        }

        private void Button_Config(object sender, EventArgs e)
        {
            ConfigPage configPage = new ConfigPage();
            Navigation.PushModalAsync(configPage);
        }

        private void KeyboardRead_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (KeyboardRead.Text.Length == 1)
                return;
            if (ConnectionClass.connected)
            {
                try
                {
                    ConnectionClass.Connect(ConnectionClass.ipAddress);
                    Byte[] data;
                    Byte[] command;
                    Byte[] text;
                    if (KeyboardRead.Text.Length == 0)
                    {
                        data = BitConverter.GetBytes((int)Commands.SEND_BACKSPACE);
                    }
                    else
                    {
                        command = BitConverter.GetBytes((int)Commands.SEND_TEXT);
                        text = System.Text.Encoding.UTF8.GetBytes(KeyboardRead.Text.Substring(1));
                        data = new Byte[command.Length + text.Length];
                        Buffer.BlockCopy(command, 0, data, 0, command.Length);
                        Buffer.BlockCopy(text, 0, data, command.Length, text.Length);
                    }
                    ConnectionClass.stream.Write(data, 0, data.Length);
                    ConnectionClass.Disconnect();
                }
                catch (Exception error)
                {
                    DisplayAlert("Błąd", error.ToString(), "OK");
                }
            }
            else
            {
                DisplayAlert("Błąd", "Brak połączenia z komputerem\n" + ConnectionClass.exceptionText, "OK");
            }
            KeyboardRead.Text = " ";
        }
    }
}
