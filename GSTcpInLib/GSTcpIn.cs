using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace GSTcpInLib
{
    public class GSTcpIn
    {
        Thread thread;
        byte[] b = new byte[4];//IP address
        Int32 port;
        NetworkStream stream;
        TcpClient client;

        public void GSConnect( string GS_IPAddress)
        {
            IPAddress addr = IPAddress.Parse(GS_IPAddress);
            client = new TcpClient();

            try
            {
                client.SendTimeout = 3000;
                client.ReceiveTimeout = 3000;
                client.Connect(addr, port);
            }
            finally
            {

            }


        }


    }
}
