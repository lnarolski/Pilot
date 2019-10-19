using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

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
        public static bool connected;
        public static string exceptionText;
        public static string ipAddress;
        public static NetworkStream stream;
        public static ConnectionState Connect(string ipAddress) { //łączenie z serwerem, którego adres IP/nazwa hosta podana jest jako argument
            try
            {
                tcpClient = new TcpClient(ipAddress, 1234);
                tcpClient.ReceiveTimeout = 500;
                tcpClient.SendTimeout = 500;
                connected = true;
                stream = ConnectionClass.tcpClient.GetStream();
                stream.ReadTimeout = 500;
                stream.WriteTimeout = 500;
                ConnectionClass.ipAddress = ipAddress;
                DatabaseClass.UpdateLastIPAddress(ConnectionClass.ipAddress);
                return ConnectionState.CONNECTION_ESTABLISHED;
            }
            catch (Exception error)
            {
                exceptionText = error.ToString();
                connected = false;
                return ConnectionState.CONNECTION_NOT_ESTABLISHED;
            }
        }
        public static ConnectionState Disconnect() //zakończenie połączenia
        {
            try
            {
                stream.Close();
                tcpClient.Close();
                return ConnectionState.DISCONECT_SUCCESS;
            }
            catch (Exception error)
            {
                exceptionText = error.ToString();
                connected = false;
                return ConnectionState.DISCONECT_NOT_SUCCESS;
            }
        }
        public static ConnectionState Send(Commands commands, Byte[] data = null) //wysyłanie polecenia
        {
            if (ConnectionClass.connected)
            {
                try
                {
                    ConnectionClass.Connect(ConnectionClass.ipAddress);
                    Byte[] command;
                    Byte[] dataToSend;
                    command = BitConverter.GetBytes((int)commands);
                    if (data == null)
                    {
                        dataToSend = new Byte[command.Length];
                        Buffer.BlockCopy(command, 0, dataToSend, 0, command.Length);
                    }
                    else
                    {
                        dataToSend = new Byte[command.Length + data.Length];
                        Buffer.BlockCopy(command, 0, dataToSend, 0, command.Length);
                        Buffer.BlockCopy(data, 0, dataToSend, command.Length, data.Length);
                    }
                    ConnectionClass.stream.Write(dataToSend, 0, dataToSend.Length);
                    ConnectionClass.Disconnect();
                    return ConnectionState.SEND_SUCCESS;
                }
                catch (Exception error)
                {
                    exceptionText = error.ToString();
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
