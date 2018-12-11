using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        Editor KeyboardRead;
        TouchTracking.TouchTrackingPoint StartPoint, EndPoint;
        bool moveStart;
        bool touchPressed, touchEntered, touchMoved, touchReleased, touchCancelled, touchExited;
        Stopwatch rightMouseTimer;
        public MainPage()
        {
            InitializeComponent();

            KeyboardRead = new Editor
            {
                Text = " ",
                //Keyboard = Keyboard.Plain
            };

            KeyboardRead.Keyboard = Keyboard.Create(KeyboardFlags.None);
            KeyboardRead.TextChanged += KeyboardRead_TextChanged;
            KeyboardRead.IsVisible = false;

            gridPage.Children.Add(KeyboardRead, 0, 0);
            Grid.SetColumnSpan(KeyboardRead, 3);

            KeyboardRead.IsVisible = false;

            rightMouseTimer = new Stopwatch();

            ConnectionClass.connected = false;
            ConnectionClass.ipAddress = "a1";
            moveStart = true;

            if (!ConnectionClass.connected)
            {
                if (ConnectionClass.Connect(ConnectionClass.ipAddress) == ConnectionState.CONNECTION_NOT_ESTABLISHED)
                    DisplayAlert("Błąd", "Brak połączenia z komputerem\n" + ConnectionClass.exceptionText, "OK");
                else
                {
                    ConnectionClass.Disconnect();
                }
            }
            else
            {
                if (ConnectionClass.Disconnect() == ConnectionState.DISCONECT_NOT_SUCCESS)
                    DisplayAlert("Błąd", "Brak połączenia z komputerem\n" + ConnectionClass.exceptionText, "OK");
                else
                {
                    ConnectionClass.connected = false;
                }
            }

            

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
            
            Byte[] text;
            ConnectionState sendStatus;
            if (KeyboardRead.Text.Length == 0)
            {
                sendStatus = ConnectionClass.Send(Commands.SEND_BACKSPACE);
            }
            else
            {
                text = System.Text.Encoding.UTF8.GetBytes(KeyboardRead.Text.Substring(1));
                sendStatus = ConnectionClass.Send(Commands.SEND_TEXT, text);
            }

            switch (sendStatus)
            {
                case ConnectionState.CONNECTION_NOT_ESTABLISHED:
                    DisplayAlert("Błąd", "Brak połączenia z komputerem", "OK");
                    break;
                case ConnectionState.SEND_NOT_SUCCESS:
                    DisplayAlert("Błąd", "Brak połączenia z komputerem\n" + ConnectionClass.exceptionText, "OK");
                    break;
                default:
                    break;
            }
            KeyboardRead.Text = " ";
        }

        private void TouchEffect_TouchAction(object sender, TouchTracking.TouchActionEventArgs args)
        {
            switch (args.Type)
            {
                case TouchTracking.TouchActionType.Entered:
                    break;
                case TouchTracking.TouchActionType.Pressed:
                    touchPressed = true;
                    touchMoved = false;
                    touchReleased = false;
                    rightMouseTimer.Start();
                    break;
                case TouchTracking.TouchActionType.Moved:
                    if (moveStart)
                    {
                        StartPoint = args.Location;
                        moveStart = false;
                    }
                    else
                    {
                        touchMoved = true;
                        moveStart = true;
                        EndPoint = args.Location;
                        double moveX = EndPoint.X - StartPoint.X;
                        double moveY = EndPoint.Y - StartPoint.Y;
                        if (moveX == 0.0 && moveY == 0.0)
                        {
                            touchMoved = false;
                        }
                        else
                        {
                            Byte[] moveX_byte = BitConverter.GetBytes(moveX);
                            Byte[] moveY_byte = BitConverter.GetBytes(moveY);
                            Byte[] data = new Byte[moveX_byte.Length + moveY_byte.Length];
                            Buffer.BlockCopy(moveX_byte, 0, data, 0, moveX_byte.Length);
                            Buffer.BlockCopy(moveY_byte, 0, data, moveX_byte.Length, moveY_byte.Length);
                            ConnectionClass.Send(Commands.SEND_MOVE_MOUSE, data);
                        }
                    }
                    break;
                case TouchTracking.TouchActionType.Released:
                    moveStart = true;
                    touchReleased = true;
                    rightMouseTimer.Stop();
                    if (touchPressed && !touchMoved)
                        if (rightMouseTimer.ElapsedMilliseconds > 500)
                            ConnectionClass.Send(Commands.SEND_RIGHT_MOUSE);
                        else
                            ConnectionClass.Send(Commands.SEND_LEFT_MOUSE);
                    rightMouseTimer.Reset();
                    break;
                case TouchTracking.TouchActionType.Cancelled:
                    moveStart = true;
                    break;
                case TouchTracking.TouchActionType.Exited:
                    moveStart = true;
                    break;
                default:
                    break;
            }
        }
    }
}
