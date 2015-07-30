﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;
using System.Net.NetworkInformation;


namespace GSTcpInLib
{
    public class GSTcpIn
    {
        static Thread thread;
        IPAddress addr;
        
        //byte[] b = new byte[4];//IP address
        Int32 TimePort = 255;
        NetworkStream stream;
        TcpClient GSTimeClient = new TcpClient();
        

        public class Item
        {
            public int ID { get; set; }
            public Double Value { get; set; }
        }
        public List<Item> TimeDataRecord;

        private Boolean GSTimeNewConnect(string GS_IPAddress)
        {
            //Задаем параметры подключения
            addr = IPAddress.Parse(GS_IPAddress);
            //Подключаемся
            return GSTimeConnect();

        }

        private Boolean GSTimeConnect()
        {
            try
            {
                //Подключение
                GSTimeClient.SendTimeout = 3000;
                GSTimeClient.ReceiveTimeout = 3000;
                GSTimeClient.Connect(addr, TimePort);
                return true; // возвращаем True, если получилось
            }
            catch (SocketException e)
            {
                MessageBox.Show("Соединение не установлено. " + e.Message);
                return false; // Вывожу ошибку и False, если не получилось подключиться.
            }
        }

        public void  GSStart (string GS_IPAddress)
        {
            // Запускаю подключение
            if (GSTimeNewConnect(GS_IPAddress)) 
            {
                //Запустить считывание
                thread = new Thread(GSTimeRead);
                thread.IsBackground = false;
                thread.Start();                
            }

        }

        public void GSStop()
        {

        }

        public Boolean GSChecTimekConnect()
        {
            IPGlobalProperties ipProperties = IPGlobalProperties.GetIPGlobalProperties();
            TcpConnectionInformation[] tcpConnections = ipProperties.GetActiveTcpConnections().Where(x => x.LocalEndPoint.Equals(GSTimeClient.Client.LocalEndPoint) && x.RemoteEndPoint.Equals(GSTimeClient.Client.RemoteEndPoint)).ToArray();

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
                    return false;
                }

            }
            else
            {
                return false;
            }
        }

        private void GSTimeRead()
        {
            //Пока подключенно
            if (GSChecTimekConnect())
            {
                
                
                stream = GSTimeClient.GetStream();

                GSCallTimeGID();
               // GSCallTimeGID();
                while (GSChecTimekConnect())
                {
                    GSCallTimeDate();
                }
                
                
            }

        }

        public List<Item> GetAllGSParam()
        {
            return TimeDataRecord;
        }

        public Double GetParam(int ParamGid)
        {

            return TimeDataRecord.Find(x => x.ID == ParamGid).Value; 
        }

        private void GSCallTimeGID()
        {
            
            byte[] paramsID = new byte[2];
            byte[] GSTimeBytes = new byte[3];
            NetworkStream GSTimeStream = GSSendReq(83);
            
            while (!GSTimeStream.DataAvailable)
            {
               Thread.Sleep(10);
            }
            TimeDataRecord = new List<Item>();
            if (GSTimeStream.DataAvailable)
            {
                GSTimeStream.Read(GSTimeBytes, 0, 3);
                int dataHeader = GSTimeBytes[2];
                if (dataHeader == 83) //S
                {
                    System.Int32 packLen = GSTimeBytes[0] + GSTimeBytes[1] * 256;
                    int K = (packLen - 3) / 2;
                    for (int i = 3; i < (K + 2); i++)
                    {
                        //Заносим GID в лист
                        GSTimeStream.Read(paramsID, 0, 2);
                        Item item = new Item();
                        item.ID = paramsID[0] + paramsID[1] * 256;
                        TimeDataRecord.Add(item);
                    }
                    while (GSTimeStream.DataAvailable)
                    {
                        GSTimeStream.Read(paramsID, 0, 1);
                    }
                    
                }
            }
        }

        private NetworkStream GSSendReq(byte ID)
        {
            NetworkStream GSTimeStream = GSTimeClient.GetStream();
            if (ID == 68)
            {
                byte[] q = new byte[9];
                q[0] = ID;
                GSTimeStream.Write(q, 0, q.Length);
            }
            else
            {
                byte[] q = new byte[1];
                q[0] = ID;
                GSTimeStream.Write(q, 0, q.Length);
            }
            
            
            
            return GSTimeStream;
        }

        private void GSCallTimeDate()
        {
            //Читаем значения

            byte[] Value = new byte[8];
            byte[] GSTimeBytes = new byte[3];
            NetworkStream GSTimeStream = GSSendReq(68);           
            


            Thread.Sleep(1500);
            while (!GSTimeStream.DataAvailable)
            {
                Thread.Sleep(10);
            }
            if (GSTimeStream.DataAvailable)
            {
                GSTimeStream.Read(GSTimeBytes, 0, 3);
                int dataHeader = GSTimeBytes[2];
                if ((dataHeader == 68) | (dataHeader == 77)) //M or D - пришел пакет с данными
                {
                    System.Int32 packLen = GSTimeBytes[0] + GSTimeBytes[1] * 256;
                    int K = (packLen - 3) / 8;
                    for (int i = 0; i < (K - 1); i++)
                    {
                        //Заносим GID в лист
                        GSTimeStream.Read(Value, 0, 8);                       
                        TimeDataRecord[i].Value = System.BitConverter.ToDouble(Value, 0);
                    }
                }
                while (GSTimeStream.DataAvailable)
                {
                    GSTimeStream.Read(Value, 0, 1);
                }
            }

        }



    }
}
