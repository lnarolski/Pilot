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
using Pilot.Resx;
using System.Globalization;

namespace Pilot
{
    public partial class MainPage : ContentPage
    {
        //Editor KeyboardRead; //pole wykorzystywane do wprowadzania tekstu
        TouchTracking.TouchTrackingPoint StartPoint, EndPoint; //punkty rozpoczęcia i zakończenia ruchu palcem
        bool moveStart; //rozpoczęcie ruchu palcem
        bool touchPressed, touchEntered, touchMoved, touchReleased, touchCancelled, touchExited; //statusy dotknięcia ekranu
        Stopwatch rightMouseTimer; //timer wykorzystywany do określenia lewego lub prawego przycisku myszy
        bool softKeyboardFirstShow = true; //pierwsze wyświetlenie klawiatury
        public MainPage()
        {
            InitializeComponent();

            ConnectionClass.connectedIndicatorImage = this.connectedIndicatorImage;

            rightMouseTimer = new Stopwatch();

            ConnectionClass.connected = false;
            ConnectionClass.ipAddress = DatabaseClass.GetLastIPAddress();
            ConnectionClass.port = short.Parse(DatabaseClass.GetLastPort());
            ConnectionClass.password = DatabaseClass.GetLastPassword();
            moveStart = true;

            if (!ConnectionClass.connected)
            {
                if (ConnectionClass.ipAddress != "")
                {
                    if (ConnectionClass.Connect(ConnectionClass.ipAddress, ConnectionClass.port.ToString(), ConnectionClass.password) == ConnectionState.CONNECTION_NOT_ESTABLISHED)
                        DisplayAlert(AppResources.Error, AppResources.NoConnectionError + "\n" + ConnectionClass.exceptionText, AppResources.OK);
                }
            }
            else
            {
                if (ConnectionClass.Disconnect() == ConnectionState.DISCONECT_NOT_SUCCESS)
                    DisplayAlert(AppResources.Error, AppResources.NoConnectionError + "\n" + ConnectionClass.exceptionText, AppResources.OK);
            }
        }

        private void Button_Shortcuts(object sender, EventArgs e) //otworzenie strony ze skrótami
        {
            ShortcutsPage shortcutsPage = new ShortcutsPage();
            Navigation.PushModalAsync(shortcutsPage);
        }

        private void Show_keyboard(object sender, EventArgs e) //wyświetlenie klawiatury ekranowej
        {
            KeyboardRead.Unfocus();
            Thread.Sleep(100);
            KeyboardRead.Focus();

            if (softKeyboardFirstShow)
            {
                KeyboardRead.Text = " ";
                KeyboardRead.TextChanged += KeyboardRead_TextChanged;

                softKeyboardFirstShow = false;
            }
        }

        private void Button_Config(object sender, EventArgs e) //okno konfiguracji aplikacji
        {
            ConfigPage configPage = new ConfigPage();
            Navigation.PushModalAsync(configPage);
        }

        private void KeyboardRead_TextChanged(object sender, TextChangedEventArgs e) //zdarzenie generowane podczas wprowadzania tekstu
        {
            if (KeyboardRead.Text.Length == 1) //unikanie dublowania wywoływania zdarzenia
                return;
            
            Byte[] text; //odczytany tekst
            ConnectionState sendStatus;
            if (KeyboardRead.Text.Length == 0) //w przypadku usunięcia znaku wysłanie klawisza BACKSPACE
            {
                sendStatus = ConnectionClass.Send(Commands.SEND_BACKSPACE);
            }
            else //wysłanie tekstu
            {
                text = System.Text.Encoding.UTF8.GetBytes(KeyboardRead.Text.Substring(1));
                sendStatus = ConnectionClass.Send(Commands.SEND_TEXT, text);
            }

            switch (sendStatus)
            {
                case ConnectionState.CONNECTION_NOT_ESTABLISHED:
                    DisplayAlert(AppResources.Error, AppResources.NoConnectionError, AppResources.OK);
                    break;
                case ConnectionState.SEND_NOT_SUCCESS:
                    DisplayAlert(AppResources.Error, AppResources.NoConnectionError + "\n" + ConnectionClass.exceptionText, AppResources.OK);
                    break;
                default:
                    break;
            }
            KeyboardRead.Text = " "; //przywrócenie tekstu początkowego
        }

        private void TouchEffect_TouchAction(object sender, TouchTracking.TouchActionEventArgs args) //zdarzenie związane z dotykaniem "szarego" obszaru
        {
            switch (args.Type) //działania związane z rodzajem zdarzenia
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
                        if (rightMouseTimer.ElapsedMilliseconds > 800)
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
