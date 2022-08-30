using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace App
{
    public partial class MesWrongStation : Form
    {
        public MesWrongStation()
        {
            InitializeComponent();
            StaticGlobal.MesWrongStationForm = this;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var adminLoginForm = new AdminLoginForm();
            if(adminLoginForm.ShowDialog() == DialogResult.Yes)
            {
                if (richTextBox1.InvokeRequired)
                {
                    richTextBox1.Invoke((Action)(() => { richTextBox1.Clear(); }));
                }
                else { richTextBox1.Clear(); }
                this.Hide();
            }
        }

        public void writeWrongStationSN(string text)
        {
            this.Show();
            if(richTextBox1.InvokeRequired)
            {
                richTextBox1.Invoke((Action)(() => { logwrite(text); }));
            }
            else { logwrite(text); }
        }

        void logwrite(string text)
        {
            richTextBox1.AppendText(text);
            richTextBox1.AppendText(Environment.NewLine);
        }

        private void MesWrongStation_Shown(object sender, EventArgs e)
        {
            this.Hide();
        }
    }
}
