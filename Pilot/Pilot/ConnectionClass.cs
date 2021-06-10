using Pilot.Resx;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Pilot
{
    enum ConnectionState //statusy związane z komunikacją
    {
        CONNECTION_ESTABLISHED,
        CONNECTION_NOT_ESTABLISHED,
        DISCONECT_SUCCESS,
        DISCONECT_NOT_SUCCESS,
        SEND_SUCCESS,
        SEND_NOT_SUCCESS,
    }
    static class ConnectionClass //statyczna klasa odpowiedzialna za komunikację poprzez sieć
    {
        public static TcpClient tcpClient;

        public static Image connectedIndicatorImage;
        public static Label connectedIndicatorLabel;
        private static bool connectedValue;
        public static bool connected {
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
        private static byte[] readBuffer = new byte[4096];

        private static async void ReceiveData()
        {
            readDataStopped = false;
            try
            {
                while (readData)
                {
                    await stream.ReadAsync(readBuffer, 0, readBuffer.Length);
                }
            }
            catch (Exception) { }
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
                stream.ReadTimeout = 500;
                stream.WriteTimeout = 500;
                ConnectionClass.ipAddress = ipAddress;
                ConnectionClass.port = short.Parse(port);
                ConnectionClass.password = password;
                DatabaseClass.UpdateConfig(ConnectionClass.ipAddress, ConnectionClass.port.ToString(), ConnectionClass.password);

                while (!readDataStopped) { }
                readData = true;
                readDataTask = new Task(new Action(ReceiveData));
                readDataTask.Start();

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
            try
            {
                stream.Close();
                tcpClient.Close();

                stream.Dispose();
                tcpClient.Dispose();

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

        public static ConnectionState Send(Commands commands, Byte[] data = null) //wysyłanie polecenia
        {
            if (ConnectionClass.connected)
            {
                try
                {
                    Byte[] command;
                    Byte[] dataToSend;
                    Byte[] dataToSendEncoded;
                    command = BitConverter.GetBytes((int)commands);

                    AesCryptoServiceProvider _aes;
                    _aes = new AesCryptoServiceProvider();
                    _aes.KeySize = 256;
                    _aes.BlockSize = 128;

                    if (data == null)
                    {
                        dataToSend = new Byte[command.Length];
                        Buffer.BlockCopy(command, 0, dataToSend, 0, command.Length);

                        try
                        {
                            using (var pass = new PasswordDeriveBytes(password, GenerateSalt(_aes.BlockSize / 8, password)))
                            {
                                using (var stream = new MemoryStream())
                                {
                                    _aes.Key = pass.GetBytes(_aes.KeySize / 8);
                                    _aes.IV = pass.GetBytes(_aes.BlockSize / 8);

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
                        }
                        catch (Exception error)
                        {
                            exceptionText = error.ToString();
                            return ConnectionState.SEND_NOT_SUCCESS;
                        }
                    }
                    else
                    {
                        dataToSend = new Byte[command.Length + data.Length];
                        Buffer.BlockCopy(command, 0, dataToSend, 0, command.Length);
                        Buffer.BlockCopy(data, 0, dataToSend, command.Length, data.Length);

                        try
                        {
                            using (var pass = new PasswordDeriveBytes(password, GenerateSalt(_aes.BlockSize / 8, password)))
                            {
                                using (var stream = new MemoryStream())
                                {
                                    _aes.Key = pass.GetBytes(_aes.KeySize / 8);
                                    _aes.IV = pass.GetBytes(_aes.BlockSize / 8);

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
                        }
                        catch (Exception error)
                        {
                            exceptionText = error.ToString();
                            return ConnectionState.SEND_NOT_SUCCESS;
                        }
                    }

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
    };
}
