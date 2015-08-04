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
using System.Threading;

namespace GGCalc
{
    
    public partial class GSCalcMnFrm : Form
    {
        static Thread thread;
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
            if (thread != null) thread.Abort();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            SendTag = true;
            textBox3.ReadOnly = true;
            thread = new Thread(SendDo);
            thread.IsBackground = false;
            thread.Start(textBox2.Text); 

        }



        private void SendDo(object IPAddr)
        {
            if (!GSSender.CheckConnect())
            {
                GSSender.Connect(IPAddr.ToString());
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
                    GSSender.SendValue(textBox4.Text, ValForSend);
                    this.Invoke(new SetLableTextDelegate(SetLableText), new object[] { Val.ToString(), ValForSend.ToString("0.00000")});
                    Thread.Sleep(50);
                }
                else
                {
                    SendTag = false;
                    GSSender.Disconnect();
                }
            }
        }

        delegate void SetLableTextDelegate(string text1, string text2);

        void SetLableText(string text1, string text2)
        {
            label1.Text = text1;
            label2.Text = text2;
            this.Refresh();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            thread.Abort();
            SendTag = false;
            textBox3.ReadOnly = false;
            GSSender.Disconnect();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            String Formula = textBox5.Text;
            label3.Text = new PostfixNotationExpression(Formula, GSTcpConn.DataDict).Calc().ToString();
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
}
