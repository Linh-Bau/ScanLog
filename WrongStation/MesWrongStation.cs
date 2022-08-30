using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
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
            int times = 0;
            string fileLog = Directory.GetCurrentDirectory() + "\\SnWrong.txt";
            while (times<3)
            {
                try
                {
                    var line=File.ReadAllLines(fileLog);
                    string tittle=line[0];
                    this.Text = tittle;
                    this.label1.Text = line[1];
                    string content = "";
                    for(int i=2;i<line.Length;i++)
                    {
                        content += line[i] + "\n";
                    }
                    richTextBox1.Text = content;
                    break;
                }
                catch
                {
                    System.Threading.Thread.Sleep(2000);
                    times++;
                }
            }
        }


        private void button1_Click(object sender, EventArgs e)
        {
            AdminLoginForm loginForm = new AdminLoginForm();
            loginForm.ShowDialog();
            if(loginForm.DialogResult==DialogResult.Yes)
            {
                this.Close();
            }
        }
        
        private void MesWrongStation_Shown(object sender, EventArgs e)
        {
            if (richTextBox1.Text == "")
                this.Close();
        }
    }
}
