using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace Pilot
{
    enum ConnectionState
    {
        CONNECTION_ESTABLISHED,
        CONNECTION_NOT_ESTABLISHED,
        DISCONECT_SUCCESS,
        DISCONECT_NOT_SUCCESS,
        SEND_SUCCESS,
        SEND_NOT_SUCCESS,
    }
    static class ConnectionClass
    {
        public static TcpClient tcpClient;
        public static bool connected;
        public static string exceptionText;
        public static string ipAddress;
        public static NetworkStream stream;
        public static ConnectionState Connect(string ipAddress) {
            try
            {
                tcpClient = new TcpClient(ipAddress, 1234);
                ConnectionClass.ipAddress = ipAddress;
                connected = true;
                stream = ConnectionClass.tcpClient.GetStream();
                return ConnectionState.CONNECTION_ESTABLISHED;
            }
            catch (Exception error)
            {
                exceptionText = error.ToString();
                connected = false;
                return ConnectionState.CONNECTION_NOT_ESTABLISHED;
            }
        }
        public static ConnectionState Disconnect()
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
        public static ConnectionState Send(Commands commands, Byte[] data = null)
        {
            if (ConnectionClass.connected)
            {
                try
                {
                    ConnectionClass.Connect(ConnectionClass.ipAddress);
                    Byte[] command;
                    Byte[] dataToSend;
                    command = BitConverter.GetBytes((int)commands);
                    if (commands == Commands.SEND_BACKSPACE)
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
