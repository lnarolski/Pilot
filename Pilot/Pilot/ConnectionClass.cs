using Pilot.Resx;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Pilot
{
    public enum ConnectionState //statusy związane z komunikacją
    {
        CONNECTION_ESTABLISHED,
        CONNECTION_NOT_ESTABLISHED,
        DISCONECT_SUCCESS,
        DISCONECT_NOT_SUCCESS,
        SEND_SUCCESS,
        SEND_NOT_SUCCESS,
    }

    public static class ConnectionClass //statyczna klasa odpowiedzialna za komunikację poprzez sieć
    {
        public static TcpClient tcpClient;

        public static Image connectedIndicatorImage;
        public static Label connectedIndicatorLabel;
        private static bool connectedValue;
        public static bool connected
        {
            get { return connectedValue; }
            set
            {
                if (connectedIndicatorImage != null)
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        connectedIndicatorImage.IsVisible = value;
                    });
                if (connectedIndicatorLabel != null)
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        connectedIndicatorLabel.IsVisible = value;
                    });
                connectedValue = value;
            }
        }

        private static bool readData = false;
        private static bool readDataStopped = true;
        private static Task readDataTask;

        public static string exceptionText;
        public static string ipAddress;
        public static NetworkStream stream;
        public static short port;
        public static string password = "";
        private static byte[] readBuffer = new byte[1048576]; // 1024 KB

        public static bool afterAutoreconnect = false;
        public static bool startApplicationConnectAttempt = true;
        private static AesCryptoServiceProvider _aes;
        private static IWidgetService widgetService;
        private static PasswordDeriveBytes pass;

        private static void ReceiveData()
        {
            readDataStopped = false;
            try
            {
                while (readData)
                {
                    if (stream.DataAvailable)
                    {
                        Int32 bytes = stream.Read(readBuffer, 0, readBuffer.Length); //odczyt danych z bufora

                        Byte[] dataDecoded = null;

                        AesCryptoServiceProvider _aes;
                        _aes = new AesCryptoServiceProvider();
                        _aes.KeySize = 256;
                        _aes.BlockSize = 128;
                        _aes.Padding = PaddingMode.Zeros;

                        try
                        {
                            using (var pass = new PasswordDeriveBytes(password, GenerateSalt(_aes.BlockSize / 8, password)))
                            {
                                using (var MemoryStream = new MemoryStream())
                                {
                                    _aes.Key = pass.GetBytes(_aes.KeySize / 8);
                                    _aes.IV = pass.GetBytes(_aes.BlockSize / 8);

                                    var proc = _aes.CreateDecryptor();
                                    using (var crypto = new CryptoStream(MemoryStream, proc, CryptoStreamMode.Write))
                                    {
                                        crypto.Write(readBuffer, 0, readBuffer.Length);
                                        crypto.Clear();
                                        crypto.Close();
                                    }
                                    MemoryStream.Close();

                                    dataDecoded = MemoryStream.ToArray();
                                }
                            }
                        }
                        catch (Exception error)
                        {
                            Debug.WriteLine(error.ToString());
                            continue;
                        }

                        if (dataDecoded.Length == 0)
                            continue;

                        for (int dataDecodedPosition = 0; dataDecodedPosition < dataDecoded.Length - sizeof(int);)
                        {
                            CommandsFromServer commandFromServer = (CommandsFromServer)BitConverter.ToInt32(dataDecoded, dataDecodedPosition); //wyodrębnienie odebranej komendy
                            try
                            {
                                switch (commandFromServer)
                                {
                                    case CommandsFromServer.SEND_PING:
                                        dataDecodedPosition += sizeof(int);
                                        break;
                                    case CommandsFromServer.SEND_PLAYBACK_INFO:
                                        if (dataDecoded.Length < (3 * sizeof(int)))
                                        {
                                            dataDecodedPosition = dataDecoded.Length; //przewiń na koniec odebranego bufora, gdy nie odebrano całej ramki
                                            break;
                                        }
                                        dataDecodedPosition += sizeof(int);
                                        int playbackInfoStringLength = BitConverter.ToInt32(dataDecoded, dataDecodedPosition); //wyodrębnienie długości odebranego tekstu
                                        dataDecodedPosition += sizeof(int);
                                        int playbackInfoThumbnailLength = BitConverter.ToInt32(dataDecoded, dataDecodedPosition); //wyodrębnienie długości odebranego thumbnail'a
                                        if (dataDecoded.Length < (3 * sizeof(int) + playbackInfoStringLength + playbackInfoThumbnailLength))
                                        {
                                            dataDecodedPosition = dataDecoded.Length; //przewiń na koniec odebranego bufora, gdy nie odebrano całej ramki
                                            break;
                                        }
                                        dataDecodedPosition += sizeof(int);
                                        string responseData = System.Text.Encoding.UTF8.GetString(dataDecoded, dataDecodedPosition, playbackInfoStringLength);
                                        string[] playbackInfoStringArray = responseData.Split(new char[] { '\u0006' });
                                        dataDecodedPosition += playbackInfoStringLength;
                                        if (playbackInfoStringArray.Length == 3)
                                        {
                                            bool playing = bool.Parse(playbackInfoStringArray[0]);
                                            string artist = playbackInfoStringArray[1];
                                            string title = playbackInfoStringArray[2];

                                            byte[] thumbnail = null;
                                            if (playbackInfoThumbnailLength != 0)
                                            {
                                                thumbnail = new byte[playbackInfoThumbnailLength];
                                                Buffer.BlockCopy(dataDecoded, dataDecodedPosition, thumbnail, 0, playbackInfoThumbnailLength);
                                            }

                                            var widgetService = DependencyService.Get<IWidgetService>();
                                            widgetService.UpdateWidget(artist, title, thumbnail);
                                        }
                                        dataDecodedPosition += playbackInfoStringLength * System.Text.Encoding.UTF8.GetByteCount(responseData);
                                        break;
                                    default:
                                        dataDecodedPosition += sizeof(int);
                                        break;
                                }
                            }
                            catch (Exception error)
                            {
                                Debug.WriteLine(error.ToString());
                            }
                        }
                    }
                    Thread.Sleep(100);
                }
            }
            catch (ObjectDisposedException error) { }
            catch (Exception error) { Debug.WriteLine(error.ToString()); }
            readDataStopped = true;
        }
        public static ConnectionState Connect(string ipAddress, string port, string password) //łączenie z serwerem, którego adres IP/nazwa hosta podana jest jako argument
        {
            try
            {
                tcpClient = new TcpClient();
                tcpClient.ReceiveTimeout = 5000;
                tcpClient.SendTimeout = 5000;
                tcpClient.Connect(ipAddress, int.Parse(port));
                connected = true;
                stream = ConnectionClass.tcpClient.GetStream();
                stream.WriteTimeout = 500;
                stream.ReadTimeout = 500;
                ConnectionClass.ipAddress = ipAddress;
                ConnectionClass.port = short.Parse(port);
                ConnectionClass.password = password;
                DatabaseClass.UpdateConfig(ConnectionClass.ipAddress, ConnectionClass.port.ToString(), ConnectionClass.password);

                while (!readDataStopped) { }
                readData = true;
                readDataTask = new Task(new Action(ReceiveData));
                readDataTask.Start();

                startApplicationConnectAttempt = false;

                _aes = new AesCryptoServiceProvider();
                _aes.KeySize = 256;
                _aes.BlockSize = 128;
                _aes.Padding = PaddingMode.Zeros;
                pass = new PasswordDeriveBytes(password, GenerateSalt(_aes.BlockSize / 8, password));
                _aes.Key = pass.GetBytes(_aes.KeySize / 8);
                _aes.IV = pass.GetBytes(_aes.BlockSize / 8);

                if (widgetService == null)
                    widgetService = DependencyService.Get<IWidgetService>();
                widgetService.CreateWidget();

                return ConnectionState.CONNECTION_ESTABLISHED;
            }
            catch (Exception error)
            {
                exceptionText = error.ToString();
                connected = false;
                readData = false;
                return ConnectionState.CONNECTION_NOT_ESTABLISHED;
            }
        }
        public static ConnectionState Disconnect() //zakończenie połączenia
        {
            if (stream == null || tcpClient == null)
                return ConnectionState.DISCONECT_NOT_SUCCESS;
            try
            {
                if (widgetService != null)
                    widgetService.RemoveWidget();

                stream.Close();
                tcpClient.Close();

                stream.Dispose();
                tcpClient.Dispose();

                if (pass != null)
                    pass.Dispose();

                connected = false;
                readData = false;

                return ConnectionState.DISCONECT_SUCCESS;
            }
            catch (Exception error)
            {
                exceptionText = error.ToString();
                connected = false;
                return ConnectionState.DISCONECT_NOT_SUCCESS;
            }
        }
        private static byte[] GenerateSalt(int size, string password)
        {
            var buffer = new byte[size];
            var passBytes = ASCIIEncoding.ASCII.GetBytes(password);

            if (passBytes.Length > buffer.Length) Array.Copy(passBytes, buffer, buffer.Length);
            else Array.Copy(passBytes, buffer, passBytes.Length);

            return buffer;
        }

        public static ConnectionState Send(CommandsFromClient commands, Byte[] data = null) //wysyłanie polecenia
        {
            if (!startApplicationConnectAttempt && (!afterAutoreconnect && !tcpClient.Connected || !ConnectionClass.connected))
            {
                afterAutoreconnect = true;

                Disconnect();
                for (int i = 0; i < 5; i++) //próba ponownego połączenia w przypadku utraty łączności
                {
                    Connect(ipAddress, port.ToString(), password);
                    if (tcpClient.Connected && ConnectionClass.connected)
                    {
                        afterAutoreconnect = false;
                        break;
                    }
                }
            }
            if (ConnectionClass.connected)
            {
                try
                {
                    Byte[] command;
                    Byte[] dataToSend;
                    Byte[] dataToSendEncoded;
                    command = BitConverter.GetBytes((int)commands);

                    if (data == null)
                    {
                        dataToSend = new Byte[sizeof(int) + command.Length];
                        Buffer.BlockCopy(BitConverter.GetBytes((int)command.Length), 0, dataToSend, 0, sizeof(int));
                        Buffer.BlockCopy(command, 0, dataToSend, sizeof(int), command.Length);
                        Debug.WriteLine("Message size: " + command.Length);
                        Debug.WriteLine("Message: " + String.Join(" ", dataToSend));

                        try
                        {
                            using (var stream = new MemoryStream())
                            {
                                var proc = _aes.CreateEncryptor();
                                using (var crypto = new CryptoStream(stream, proc, CryptoStreamMode.Write))
                                {
                                    crypto.Write(dataToSend, 0, dataToSend.Length);
                                    crypto.Clear();
                                    crypto.Close();
                                }
                                stream.Close();

                                dataToSendEncoded = stream.ToArray();
                            }
                        }
                        catch (Exception error)
                        {
                            exceptionText = error.ToString();
                            return ConnectionState.SEND_NOT_SUCCESS;
                        }
                    }
                    else
                    {
                        dataToSend = new Byte[sizeof(int) + command.Length + data.Length];
                        int messageSize = (int)(command.Length + data.Length);
                        Debug.WriteLine("Message size: " + messageSize);
                        Buffer.BlockCopy(BitConverter.GetBytes(messageSize), 0, dataToSend, 0, sizeof(int));
                        Buffer.BlockCopy(command, 0, dataToSend, sizeof(int), command.Length);
                        Buffer.BlockCopy(data, 0, dataToSend, sizeof(int) + command.Length, data.Length);
                        Debug.WriteLine("Message: " + String.Join(" ", dataToSend));

                        try
                        {
                            using (var stream = new MemoryStream())
                            {
                                var proc = _aes.CreateEncryptor();
                                using (var crypto = new CryptoStream(stream, proc, CryptoStreamMode.Write))
                                {
                                    crypto.Write(dataToSend, 0, dataToSend.Length);
                                    crypto.Clear();
                                    crypto.Close();
                                }
                                stream.Close();

                                dataToSendEncoded = stream.ToArray();
                            }
                        }
                        catch (Exception error)
                        {
                            exceptionText = error.ToString();
                            return ConnectionState.SEND_NOT_SUCCESS;
                        }
                    }

                    Debug.WriteLine("Encoded message: " + String.Join(" ", dataToSendEncoded));
                    ConnectionClass.stream.Write(dataToSendEncoded, 0, dataToSendEncoded.Length);
                    return ConnectionState.SEND_SUCCESS;
                }
                catch (Exception error)
                {
                    exceptionText = error.ToString();
                    Disconnect();
                    return ConnectionState.SEND_NOT_SUCCESS;
                }
            }
            else
            {
                return ConnectionState.CONNECTION_NOT_ESTABLISHED;
            }
        }
    }
}
