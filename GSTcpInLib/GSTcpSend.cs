using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;
using System.Net.NetworkInformation;

namespace GSTcpInLib
{
    public class GSTcpSend
    {
        TcpClient GSSendClient;
        Int32 SendPort = 2211;
        NetworkStream GSSendStream;

        public void Connect(string GS_IPAddress)
        {
            try
            {
                GSSendClient = new TcpClient();
                IPAddress addr = IPAddress.Parse(GS_IPAddress);
                GSSendClient = new TcpClient();
                GSSendClient.SendTimeout = 3000;
                GSSendClient.ReceiveTimeout = 3000;
                GSSendClient.Connect(addr, SendPort);
                GSSendStream = GSSendClient.GetStream();                
            }
            catch (SocketException e)
            {
                MessageBox.Show("Ошибка соединения отправки: " + e.Message); // Вывод окна с ошибкой   
            }
        }

        public Boolean CheckConnect()
        {
            if ((GSSendClient != null) && (GSSendClient.Connected))
            {
                IPGlobalProperties ipProperties = IPGlobalProperties.GetIPGlobalProperties();
                TcpConnectionInformation[] tcpConnections = ipProperties.GetActiveTcpConnections().Where(x => x.LocalEndPoint.Equals(GSSendClient.Client.LocalEndPoint) && x.RemoteEndPoint.Equals(GSSendClient.Client.RemoteEndPoint)).ToArray();
                if (tcpConnections != null && tcpConnections.Length > 0)
                {
                    TcpState stateOfConnection = tcpConnections.First().State;
                    if (stateOfConnection == TcpState.Established)
                    {
                        // Connection is OK
                        return true;
                    }
                    else
                    {
                        // No active tcp Connection to hostName:port
                        GSSendClient.Close();
                        return false;
                    }

                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public Boolean SendValue (string ID, double Val)
        {
            if (CheckConnect())
            {
                string MsgForSend = "S" + ID + "=" + Val.ToString("0.0000") + "/r/n";
                Byte[] DataForSend = System.Text.Encoding.ASCII.GetBytes(MsgForSend);
                GSSendStream.Write(DataForSend, 0, DataForSend.Length);
                return true;
            }
            else
            {
                return false;
            }
        }

        public void Disconnect()
        {
            GSSendClient.Close();
        }
    }
}
