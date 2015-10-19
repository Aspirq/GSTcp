using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using GSTcpInLib;
using FormulaLib;
using System.Threading;
using System.Xml;


namespace GSTCPCalc
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    

    public partial class MainWindow : Window
    {
        GSTcpIn GSTcpConn = new GSTcpIn();
        GSTcpSend GSTcpSender = new GSTcpSend();
        Boolean SendTag = false;
        static Thread SendThread;
        TextBox TextSenIP;
        static Thread CalcThread;
        List<SettingDataTable> SettingTblList;
        List<KeyValuePair<string, double>> RealDataList;
        XmlDocument SettingDoc = new XmlDocument();
        string SettingTblPath = Directory.GetCurrentDirectory() + "//SettingTbl.xml";
        string IPPath = Directory.GetCurrentDirectory() + "//IPAdr.xml";
        System.Xml.Serialization.XmlSerializer SetTblSer = new System.Xml.Serialization.XmlSerializer(typeof(List<SettingDataTable>));
        System.Xml.Serialization.XmlSerializer IpTblSer = new System.Xml.Serialization.XmlSerializer(typeof(string));
        FileStream fs = new FileStream(Directory.GetCurrentDirectory() + "//Error.txt", FileMode.Create);
        StreamWriter sw;


        void MyHandler(object sender, UnhandledExceptionEventArgs args)
        {
            Exception e = (Exception)args.ExceptionObject;
            Console.WriteLine("======================");
            Console.WriteLine("Time : " + DateTime.Now);
            Console.WriteLine("MyHandler MSG : " + e.Message);
            Console.WriteLine("MyHandler HELP : " + e.HelpLink);
            Console.WriteLine("MyHandler Source : " + e.Source);
            Console.WriteLine("Runtime terminating: {0}", args.IsTerminating);
            Console.WriteLine("======================");
            Console.WriteLine(e);
            Console.WriteLine("======================");
            Console.WriteLine("======================");           
        }

        public MainWindow()
        {

            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(MyHandler);
            
            sw = new StreamWriter(fs);
            sw.AutoFlush = true;
            Console.SetOut(sw);
            Console.WriteLine("Start et:" + DateTime.Now);
            //sw.Close();
            InitializeComponent(); 
            SettingTblList = new List<SettingDataTable>();
            if (System.IO.File.Exists(SettingTblPath))
            {
                System.IO.StreamReader file2 = new System.IO.StreamReader(SettingTblPath);
                SettingTblList = (List<SettingDataTable>)SetTblSer.Deserialize(file2);
            }

            if (System.IO.File.Exists(IPPath))
            {
                System.IO.StreamReader file2 = new System.IO.StreamReader(IPPath);
                IpAdrText.Text = (string)IpTblSer.Deserialize(file2);
            }
            SetTable.ItemsSource = SettingTblList;
            TekZnTbl.ItemsSource = RealDataList;
    

       }

        public class SettingDataTable
        {            
            public int GID { get; set; }
            public string Discr { get; set; }
            public string Formula { get; set; }
            public Boolean IsOn { get; set; }
            public Double CalcResult { get; set; }
        }

        public class RealData
        {
            public String Key { get; set; }
            public Double Value { get; set; }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
                        
            System.IO.FileStream file = System.IO.File.Create(SettingTblPath);
            SetTblSer.Serialize(file, SettingTblList); 
            file.Close();

            file = System.IO.File.Create(IPPath);
            IpTblSer.Serialize(file, IpAdrText.Text);
            file.Close();
            
            SendTag = true;
            SetTable.IsReadOnly = true;
            GSTcpConn.GSStart(IpAdrText.Text);

            if (!GSTcpSender.CheckConnect())
                GSTcpSender.Connect(IpAdrText.Text);

            if (GSTcpConn.GSChecTimekConnect()) 
            {            
                if (CalcThread != null) CalcThread.Abort();
                CalcThread = new Thread(CalcDo);
                CalcThread.IsBackground = true;
                CalcThread.Start();

                if (SendThread != null) SendThread.Abort();
                SendThread = new Thread(SendDo);
                SendThread.IsBackground = true;
                SendThread.Start();
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
           GSTcpConn.GSStop();
        }

        delegate void TekZnTblRenewDelegate();

        private void TekZnTblRenew()
        {
            //
            //if (TekZnTbl.ItemsSource == null) 
            TekZnTbl.ItemsSource = RealDataList;
            TekZnTbl.Items.Refresh();
            SetTable.Items.Refresh();
        }

        private void StopBtn_Click(object sender, RoutedEventArgs e)
        {
            SendTag = false;
            SetTable.IsReadOnly = false;
            GSTcpConn.GSStop();
            GSTcpSender.Disconnect();
        }

        private void SendDo()
        {
            Thread.Sleep(2000);
            while (SendTag)
            {
                Thread.Sleep(500);
                for (int i = 0; i < SettingTblList.Count; i++)
                {
                    GSTcpSender.SendValue(SettingTblList[i].GID.ToString(), SettingTblList[i].CalcResult);
                }
            }
        }

        private void CalcDo()
        {
            Thread.Sleep(2000);
            while (SendTag)
            {
               Thread.Sleep(500);
               for (int i=0; i < SettingTblList.Count; i++) 
               {
                   Double CalcResult = Convert.ToDouble(new PostfixNotationExpression(SettingTblList[i].Formula, GSTcpConn.DataDict).Calc().ToString());
                   SettingTblList[i].CalcResult = CalcResult;
                   GSTcpConn.DataDict["S" + SettingTblList[i].GID.ToString()] = CalcResult;
                   CalcResult = Double.NaN;
                   //Console.WriteLine(SettingTblList[i].CalcResult);
               }
               RealDataList = GSTcpConn.DataDict.ToList();
               
               this.Dispatcher.Invoke(new TekZnTblRenewDelegate(TekZnTblRenew));
               
            }
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            AboutBox a = new AboutBox();
            a.ShowDialog();
        }




    }
}
