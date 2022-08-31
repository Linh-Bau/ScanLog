using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace App
{
    public partial class SettingsForm : Form
    {
        public SettingsForm()
        {
            InitializeComponent();
            string bd = File.GetLastWriteTime(Assembly.GetExecutingAssembly().Location).ToString("dd-MM-yy hh:mm:ss");
            string ver = "1.0.8.31.0, BuildDate: "+bd;
            label2.Text = ver;
            if(StaticGlobal.currentStaion=="setless")
                comboBox1.DataSource = new List<string>() { "setless" };
            else comboBox1.DataSource = new List<string>() { "Pre-Advertising", "100% Advertising" };
        }

    

        SettingValues settingValues;

        string station;
        string logpath;
        DateTime dateTime;
        TimeSpan coundown;
        
        private void button3_Click(object sender, EventArgs e)
        {
            button2_Click(sender, e);
            try
            {
                if (lb_logPath.Text != "NA" && lb_datetime.Text != "NA"&&lb_station.Text!="NA")
                {
                    int minutes = int.Parse(txb_coundown.Text);
                    coundown = new TimeSpan(0, minutes, 0);
                    settingValues = new SettingValues();
                    this.settingValues.Station = station;
                    this.settingValues.LogPath = logpath;
                    this.settingValues.DateTimeStart = dateTime;
                    this.settingValues.Coundown = coundown;
                    Settings.SaveSettings(settingValues);
                    Exit();
                    return;
                }
            }
            catch
            {
            }
            MessageBox.Show("Input value not correct!");
        }

        private void Exit()
        {
            this.DialogResult = DialogResult.Yes;
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.ShowDialog();
            if(!string.IsNullOrEmpty(folderBrowserDialog1.SelectedPath.Trim()))
            {
                logpath=folderBrowserDialog1.SelectedPath;
                lb_logPath.Text=logpath;
            }
            else
            {
                logpath = "NA";
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            dateTimePicker1.MaxDate = DateTime.Today;
            lb_datetime.Text = dateTimePicker1.Value.ToString();
            this.dateTime=dateTimePicker1.Value;
        }

        private void SettingsForm_Load(object sender, EventArgs e)
        {
            dateTimePicker1.Value = DateTime.Parse("2022/01/01");
            try
            {
                this.settingValues = Settings.GetValue();
                station = settingValues.Station;
                logpath = settingValues.LogPath;
                dateTime = settingValues.DateTimeStart;
                coundown = settingValues.Coundown;

                lb_station.Text = station;
                lb_logPath.Text = logpath.ToString();
                lb_datetime.Text = dateTime.ToString();
                txb_coundown.Text = coundown.TotalMinutes.ToString();

                dateTimePicker1.Value = dateTime;
            }
            catch
            {

            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            station = comboBox1.SelectedItem.ToString();
            lb_station.Text = station;
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}
