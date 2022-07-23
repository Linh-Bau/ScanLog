using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace App
{
    public partial class ShowList : Form
    {
        List<DutLog> dutLogs;
        public ShowList(List<DutLog> dutLogs)
        {
            InitializeComponent();
            this.dutLogs = dutLogs;
            richTextBox1.Clear();
            richTextBox1.WordWrap = false;
            foreach(DutLog dutLog in dutLogs)
            {
                string s = string.Format("{0}, file://{1}", dutLog.MAC, dutLog.LogPath.Replace(" ", "%20"));
                richTextBox1.AppendText(s + Environment.NewLine);
            }
            nof = new System.Threading.Thread(PlaySound);
            nof.Start();
        }

        System.Threading.Thread nof;
        string nofpath = System.IO.Directory.GetCurrentDirectory() + "\\nof.wav";
        void PlaySound()
        {
            System.Media.SoundPlayer player = new SoundPlayer(nofpath);
            while(true)
            {
                player.Play();
                System.Threading.Thread.Sleep(1000);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ShowList_FormClosing(sender, null);
        }

        private bool quit()
        {
            AdminLoginForm adminLoginForm = new AdminLoginForm();
            bool rs = adminLoginForm.ShowDialog() == DialogResult.Yes;
            if (rs)
            {
                StopPlayingSound();
                this.DialogResult = DialogResult.Yes;
                return true;
            }
            return false;
        }

        private void ShowList_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(!quit()) e.Cancel = true;
        }

        private void richTextBox1_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            try
            {
                Process.Start(e.LinkText);
            }
            catch
            {

            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            StopPlayingSound();
        }

        public void StopPlayingSound()
        {
            try
            {
                nof.Abort();
            }
            catch
            {

            }
        }
    }
}
