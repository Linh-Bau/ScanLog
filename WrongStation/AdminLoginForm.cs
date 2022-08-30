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
    public partial class AdminLoginForm : Form
    {
        /// <summary>
        /// login success=> dialogResult=Yes
        /// else =>dialogResult=No
        /// </summary>
        public AdminLoginForm()
        {
            InitializeComponent();
        }

        string user = "ADMIN";
        string passwork = "ADMIN";

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Enter)
            {
                textBox1.Text = textBox1.Text.ToUpper();
                textBox2.Focus();
            }
        }

        private void textBox2_Enter(object sender, EventArgs e)
        {

        }

        private void textBox2_KeyDown(object sender, KeyEventArgs e)
        {
            if( e.KeyCode == Keys.Enter)
            {
                textBox2.Text= textBox2.Text.ToUpper();              
                if(CheckLogin())
                {
                    LoginSuccess();
                }
                else
                {
                    LoginFail();
                }
            }
        }

        private void LoginSuccess()
        {
            MessageBox.Show("Success!");
            this.DialogResult = DialogResult.Yes;
        }

        private void LoginFail()
        {
            MessageBox.Show("Fail!");
            textBox1.Clear();
            textBox2.Clear();
            textBox1.Focus();
        }

        private bool CheckLogin()
        {
            if(textBox1.Text.Trim() == user&&textBox2.Text.Trim()==passwork)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void label4_Click(object sender, EventArgs e)
        {
            LoginSuccess();
        }
    }
}
