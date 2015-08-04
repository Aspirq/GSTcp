using System;
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


namespace GSTCPCalc
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    

    public partial class MainWindow : Window
    {
        GSTcpIn GSTcpConn = new GSTcpIn();
        Boolean SendTag = false;
        static Thread SendThread;
        static Thread CalcThread;

        

        public MainWindow()
        {
            InitializeComponent();
            List<SettingDataTable> SettingTbl = new List<SettingDataTable>();            
            SetTable.ItemsSource = SettingTbl;
            SettingTbl.Add(new SettingDataTable { GID = 711, Formula = "S731*5", IsOn = true, CalcResult = 0 });
            SettingTbl.Add(new SettingDataTable { GID = 712, Formula = "S732*5", IsOn = true, CalcResult = 0 });
            
        }

        public class SettingDataTable
        {            
            public int GID { get; set; }
            public string Formula { get; set; }
            public Boolean IsOn { get; set; }
            public Double CalcResult { get; set; }

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SendTag = true;
            SetTable.IsReadOnly = true;
            GSTcpConn.GSStart(IpAdrText.Text);
            TekZnTblRene();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
           GSTcpConn.GSStop();
        }

        private void TekZnTblRene()
        {
            TekZnTbl.ItemsSource = GSTcpConn.DataDict;
        }

        private void StopBtn_Click(object sender, RoutedEventArgs e)
        {
            GSTcpConn.GSStop();
        }

        private void SendDo()
        {

        }

        private void CalcDo()
        {

        }




    }
}
