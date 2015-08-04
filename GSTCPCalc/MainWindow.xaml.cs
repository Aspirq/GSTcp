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
        

        public MainWindow()
        {
            InitializeComponent();
            List<SettingDataTable> SettingTbl = new List<SettingDataTable>();            
            TestTable.ItemsSource = SettingTbl;
            TekZnTbl.ItemsSource = GSTcpConn.DataDict;
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
            GSTcpConn.GSStart(IpAdrText.Text);
            TekZnTbl.ItemsSource = GSTcpConn.DataDict;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
           GSTcpConn.GSStop();
            
        }




    }
}
