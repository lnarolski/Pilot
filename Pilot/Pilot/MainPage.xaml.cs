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
using Plugin.InAppBilling;

namespace Pilot
{
    [ContentProperty("VerticalContent")]
    public class VerticalContentView : ContentView
    {
        public View VerticalContent
        {
            get => (View)GetValue(ContentProperty);
            set => SetValue(ContentProperty, Verticalize(value));
        }

        public double ContentRotation { get; set; } = -90;

        private View Verticalize(View toBeRotated)
        {
            if (toBeRotated == null)
                return null;

            toBeRotated.Rotation = ContentRotation;
            var result = new RelativeLayout();

            result.Children.Add(toBeRotated,
            xConstraint: Constraint.RelativeToParent((parent) =>
            {
                return parent.X - ((parent.Height - parent.Width) / 2);
            }),
            yConstraint: Constraint.RelativeToParent((parent) =>
            {
                return (parent.Height / 2) - (parent.Width / 2);
            }),
            widthConstraint: Constraint.RelativeToParent((parent) =>
            {
                return parent.Height;
            }),
            heightConstraint: Constraint.RelativeToParent((parent) =>
            {
                return parent.Width;
            }));

            return result;
        }
    }
    public partial class MainPage : ContentPage
    {
        //Editor keyboardRead; //pole wykorzystywane do wprowadzania tekstu
        TouchTracking.TouchTrackingPoint StartPoint, EndPoint; //punkty rozpoczęcia i zakończenia ruchu palcem
        bool moveStart; //rozpoczęcie ruchu palcem
        bool doubleTapStarted; //flaga informująca o wykryciu podwójnego dotknięcia ekranu
        bool touchPressed, touchEntered, touchMoved, touchReleased, touchCancelled, touchExited; //statusy dotknięcia ekranu
        Stopwatch rightMouseTimer; //timer wykorzystywany do określenia lewego lub prawego przycisku myszy
        Stopwatch doubleTapMouseTimer; //timer wykorzystywany do określenia długiego wciśnięcia lewego przycisku myszy
        bool softKeyboardFirstShow = true; //pierwsze wyświetlenie klawiatury

        public MainPage()
        {
            InitializeComponent();

            if (Device.RuntimePlatform == Device.UWP)
            {
                showKeyboardButton.IsEnabled = false; //funkcja otwierania klawiatury ekranowej nie jest jeszcze wspierana przez API Windowsa
            }

            ConnectionClass.connectedIndicatorImage = this.connectedIndicatorImage;
            ConnectionClass.connectedIndicatorLabel = this.connectedIndicatorLabel;

            rightMouseTimer = new Stopwatch();
            doubleTapMouseTimer = new Stopwatch();

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

            mouseWheelSlider.DragCompleted += MouseWheelSlider_DragCompleted;
            mouseWheelSlider.DragStarted += MouseWheelSlider_DragStarted;

            Appearing += MainPage_Appearing;
        }

        private void MainPage_Appearing(object sender, EventArgs e)
        {
            WasItemPurchased("");
        }

        public async Task<bool> WasItemPurchased(string productId)
        {
            var billing = CrossInAppBilling.Current;
            try
            {
                var connected = await billing.ConnectAsync();

                if (!connected)
                {
                    //Couldn't connect
                    return false;
                }

                var purchases = await billing.GetPurchasesAsync(ItemType.InAppPurchase);

                //check for null just in case
                if (purchases?.Any(p => p.ProductId == productId) ?? false)
                {
                    //Purchase restored
                    // if on Android may be good to check if these purchases need to be acknowledge
                    return true;
                }
                else
                {
                    //no purchases found
                    return false;
                }
            }
            catch (InAppBillingPurchaseException purchaseEx)
            {
                //Billing Exception handle this based on the type
                Debug.WriteLine("Error: " + purchaseEx);
            }
            catch (Exception ex)
            {
                //Something has gone wrong
                Debug.WriteLine("Error: " + ex);
            }
            finally
            {
                await billing.DisconnectAsync();
            }

            return false;
        }

        private bool dragFinished = true;

        private void MouseWheelSlider_DragCompleted(object sender, EventArgs e) //powrót rolki myszki do pierwotnej pozycji
        {
            dragFinished = true;
            mouseWheelSlider.Value = 0;
            Byte[] sliderValue = BitConverter.GetBytes((Int32)mouseWheelSlider.Value);
            ConnectionClass.Send(CommandsFromClient.SEND_WHEEL_MOUSE, sliderValue);
        }

        private async void MouseWheelSlider_DragStarted(object sender, EventArgs e)
        {
            dragFinished = false;
            await Task.Run(async () =>
            {
                do
                {
                    Byte[] sliderValue = BitConverter.GetBytes((Int32)mouseWheelSlider.Value);
                    ConnectionClass.Send(CommandsFromClient.SEND_WHEEL_MOUSE, sliderValue);

                    Thread.Sleep(100);
                } while (!dragFinished);
            });
        }

        private void Button_Shortcuts(object sender, EventArgs e) //otworzenie strony ze skrótami
        {
            ShortcutsPage shortcutsPage = new ShortcutsPage();
            Navigation.PushModalAsync(shortcutsPage);
        }

        private void Show_keyboard(object sender, EventArgs e) //wyświetlenie klawiatury ekranowej
        {
            keyboardRead.Unfocus();
            Thread.Sleep(200);
            keyboardRead.Focus();

            if (softKeyboardFirstShow)
            {
                keyboardRead.Text = " ";
                keyboardRead.TextChanged += keyboardRead_TextChanged;

                softKeyboardFirstShow = false;
            }
        }

        private void Button_Config(object sender, EventArgs e) //okno konfiguracji aplikacji
        {
            ConfigPage configPage = new ConfigPage();
            Navigation.PushModalAsync(configPage);
        }

        private void keyboardRead_TextChanged(object sender, TextChangedEventArgs e) //zdarzenie generowane podczas wprowadzania tekstu
        {
            if (keyboardRead.Text.Length == 1) //unikanie dublowania wywoływania zdarzenia
                return;
            
            Byte[] text; //odczytany tekst
            ConnectionState sendStatus;
            if (keyboardRead.Text.Length == 0) //w przypadku usunięcia znaku wysłanie klawisza BACKSPACE
            {
                sendStatus = ConnectionClass.Send(CommandsFromClient.SEND_BACKSPACE);
            }
            else //wysłanie tekstu
            {
                text = System.Text.Encoding.UTF8.GetBytes(keyboardRead.Text.Substring(1));
                sendStatus = ConnectionClass.Send(CommandsFromClient.SEND_TEXT, text);
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
            keyboardRead.Text = " "; //przywrócenie tekstu początkowego
        }

        private void TouchEffect_TouchAction(object sender, TouchTracking.TouchActionEventArgs args) //zdarzenie związane z dotykaniem "szarego" obszaru
        {
            switch (args.Type) //działania związane z rodzajem zdarzenia
            {
                case TouchTracking.TouchActionType.Entered:
                    break;
                case TouchTracking.TouchActionType.Pressed:
                    if (doubleTapMouseTimer.IsRunning && doubleTapMouseTimer.ElapsedMilliseconds < 300) // Gdy wykryto podwójne wciśnięcie w czasie mniejszym niż 300 ms
                    {
                        rightMouseTimer.Stop();
                        doubleTapMouseTimer.Stop();

                        doubleTapStarted = true;
                        ConnectionClass.Send(CommandsFromClient.SEND_LEFT_MOUSE_LONG_PRESS_START);
                    }
                    else
                    {
                        touchPressed = true;
                        touchMoved = false;
                        touchReleased = false;
                        rightMouseTimer.Start();
                        doubleTapMouseTimer.Reset();
                        doubleTapMouseTimer.Start();
                    }
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
                            ConnectionClass.Send(CommandsFromClient.SEND_MOVE_MOUSE, data);
                        }
                    }
                    break;
                case TouchTracking.TouchActionType.Released:
                    moveStart = true;
                    touchReleased = true;
                    rightMouseTimer.Stop();
                    if (doubleTapStarted)
                    {
                        ConnectionClass.Send(CommandsFromClient.SEND_LEFT_MOUSE_LONG_PRESS_STOP);
                        doubleTapStarted = false;
                    }
                    else if (touchPressed && !touchMoved)
                        if (rightMouseTimer.ElapsedMilliseconds > 800)
                            ConnectionClass.Send(CommandsFromClient.SEND_RIGHT_MOUSE);
                        else
                            ConnectionClass.Send(CommandsFromClient.SEND_LEFT_MOUSE);
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
