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
using FormulaLib;

namespace GGCalc
{
    
    public partial class GSCalcMnFrm : Form
    {
        GSTcpIn GSTcpConn = new GSTcpIn();
        GSTcpSend GSSender = new GSTcpSend(); 
        Boolean SendTag = false;
        public List<GSTcpIn.Item> TimeDataRecord;
        public GSCalcMnFrm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {

            GSTcpConn.GSStart(textBox2.Text);
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
            Double Val = GSTcpConn.GetParam(Convert.ToInt32(textBox1.Text));
            label1.Text = Val.ToString();
            
        }

        private void button4_Click(object sender, EventArgs e)
        {
            GSTcpConn.GSStop();
        }

        private void GSCalcMnFrm_FormClosing(object sender, FormClosingEventArgs e)
        {
            GSTcpConn.GSStop();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            SendTag = true;

        }

        private void SendDo(string IPAddr)
        {

            if (!GSSender.CheckConnect())
            {
                GSSender.Connect(IPAddr);
            }
            
            while (SendTag) 
            {
                if (GSSender.CheckConnect())
                {
                    Dictionary<String, Double> variables = new Dictionary<string, double>();
                    Double Val = GSTcpConn.GetParam(Convert.ToInt32(textBox1.Text));
                    variables.Add("x", Val);
                    variables.Add("X", Val);
                    String Formula = textBox3.Text;

                    Double ValForSend = Convert.ToDouble(new PostfixNotationExpression(Formula, variables).Calc().ToString());


                }
            }
        }
    }
}
