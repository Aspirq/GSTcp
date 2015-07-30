using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GSTcpInLib;

namespace GGCalc
{
    public partial class GSCalcMnFrm : Form
    {
        GSTcpIn GSTcpConn = new GSTcpIn();
        public List<GSTcpIn.Item> TimeDataRecord;
        public GSCalcMnFrm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            
            GSTcpConn.GSStart("172.16.10.249");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (GSTcpConn.GSChecTimekConnect())
            {
                MessageBox.Show("Есть коннект");
            }
            else
            {
                MessageBox.Show("Нет коннекта");
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Double Val = GSTcpConn.GetParam(101);
            label1.Text = Val.ToString();
            
        }
    }
}
