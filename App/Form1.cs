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
    public partial class Form1 : Form
    {

        ViewModels.ViewModel _vm;
        List<DutLog> listFail = new List<DutLog>();
        TimeSpan coundown;

        bool w = false;
        int wait = 0;
        public Form1()
        {
            InitializeComponent();

            timer1.Enabled = true;

            _vm = new ViewModels.ViewModel();
            _vm.updateSettings = this.UpdateSettings;
            _vm.ScanCallback = this.ScanCallBack;

            this.Shown += new EventHandler((sender, e) => _vm.Start_Appication());
            this.btn_Settings.Click += new EventHandler((sender, e) => _vm.Setting_Click());
            this.btn_Scan.Click+=new EventHandler((sender, e) => _vm.ScanClick());
            this.FormClosing += Form1_FormClosing;
            
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(_vm.FormClose())
            {

            }
            else
            {
                e.Cancel = true;
            }
        }

        public void UpdateSettings()
        {
            Station.Text = Settings.GetValue().Station;
            lb_LocationPath.Text = Settings.GetValue().LogPath;
            lb_dateTimeStart.Text = Settings.GetValue().DateTimeStart.ToString();
            lb_coundown.Text = Settings.GetValue().Coundown.ToString();
            coundown = Settings.GetValue().Coundown;
        }

        public void ScanCallBack(string keywork,object data)
        {
            if(keywork=="Start")
            {
                start();
            }
            else if(keywork=="Finish")
            {
                var listObj = data as List<object>;
                int? total = listObj[0] as int?;
                int? olds = listObj[1] as int?;
                int? notchecks = listObj[2] as int?;
                bool? detected = listObj[3] as bool?;
                if (total.HasValue && olds.HasValue && notchecks.HasValue && detected.HasValue)
                    Finish(total,olds,notchecks,detected);
                else
                    MessageBox.Show("oh, bug!!!!");
            }
            else if(keywork== "NewList")
            {
                List<DutLog> DetectedList=data as List<DutLog>;
                if(DetectedList.Count==0)
                {
                    WriteList();
                }
                else
                {
                    WriteList(DetectedList);
                }
            }
        }
        
        private void Finish(int? totalScan,int? olds, int? notchecks, bool? detectedNew)
        {
            controlInvoke(lb_totalScan, (Action)(() => { lb_totalScan.Text = totalScan.ToString(); }));
            controlInvoke(lb_totalScan, (Action)(() => { lb_olds.Text = olds.ToString(); }));
            controlInvoke(lb_totalScan, (Action)(() => { lb_notChecks.Text = notchecks.ToString(); }));

            controlInvoke(lb_status, (Action)(() => { lb_status.Text = "Finish"; }));
            if(detectedNew!=null && detectedNew==true)
            {
                controlInvoke(lb_status, (Action)(() => { lb_status.BackColor = Color.Red; }));
            }
            else
            {
                controlInvoke(lb_status, (Action)(() => { lb_status.BackColor = Color.Green; }));
            }
            
            controlInvoke(lb_count, () => { lb_count.Text = "Clear after 10 secons"; });
            wait = 10;
            w = true;
        }
        private void start()
        {
            controlInvoke(lb_totalScan, (Action)(() => { lb_totalScan.Text = "--"; }));
            controlInvoke(lb_totalScan, (Action)(() => { lb_olds.Text = "--"; }));
            controlInvoke(lb_totalScan, (Action)(() => { lb_notChecks.Text = "--"; }));
            
            controlInvoke(lb_status, (Action)(() => { lb_status.Text = "Scanning"; }));
            controlInvoke(lb_status, (Action)(() => { lb_status.BackColor = Color.Yellow; }));

            controlInvoke(lb_count, () => { lb_count.Text = ""; });
        }
        private void controlInvoke (Control control,Action action)
        {
            if(control.InvokeRequired)
            {
                control.Invoke((action));
            }
            else
                action();
        }

        void WriteList()
        {
            if (richTextBox1.InvokeRequired)
            {
                richTextBox1.Invoke((Action)(() =>
                {
                    richTextBox1.Clear();
                    richTextBox1.AppendText("Everythings is ok!");
                }));
            }
            else
            {
                richTextBox1.Clear();
                richTextBox1.AppendText("Everythings is ok!");
            }    
        }
        void WriteList(List<DutLog> dutLogs)
        {
            if (richTextBox1.InvokeRequired)
            {
                richTextBox1.Invoke((Action)(() =>
                {
                    richTextBox1.Clear();
                    richTextBox1.DetectUrls = true;
                    {
                        foreach (DutLog dutLog in dutLogs)
                        {
                            var s = string.Format("{0}, file://{1}", dutLog.MAC, dutLog.LogPath);
                            richTextBox1.AppendText(s + Environment.NewLine);
                        }
                        while (true)
                        {
                            if (_vm.DetectedNew(dutLogs))
                            {
                                break;
                            }
                        }
                    }
                }));
            }
            else
            {
                richTextBox1.Clear();
                richTextBox1.DetectUrls = true;
                {
                    foreach (DutLog dutLog in dutLogs)
                    {
                        var s = string.Format("{0}, {1}", dutLog.MAC, dutLog.LogPath);
                        richTextBox1.AppendText(s + Environment.NewLine);
                    }
                    while (true)
                    {
                        if (_vm.DetectedNew(dutLogs))
                        {
                            break;
                        }
                    }
                }
            }
            
        }

        private void richTextBox1_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(e.LinkText);
            }
            catch
            {

            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            coundown = coundown - new TimeSpan(0, 0, 1);
            if(coundown.TotalSeconds==0)
            {
                coundown = Settings.GetValue().Coundown;
                _vm.ScanClick();
            }
            lb_coundown.Text = coundown.ToString();

            //
            if(w)
            {
                if(wait>-1)
                {
                    wait = wait - 1;
                    controlInvoke(lb_count, () => { lb_count.Text = String.Format("Clear after {0} secons",wait.ToString()); });
                }
                else
                {
                    w = false;
                    wait = 0;
                    controlInvoke(lb_status, () => { lb_status.Text = "Waiting..."; });
                    controlInvoke(lb_status, () => { lb_status.BackColor = Color.Gray; });
                    controlInvoke(lb_count, () => { lb_count.Text = ""; });
                }
            }
        }
        

        //private void panel4_Paint(object sender, PaintEventArgs e)
        //{

        //}
        //private void button1_Click(object sender, EventArgs e)
        //{
        //    _vm = new ViewModels.ViewModel();
        //    var list = _vm.Scan(@"D:\Log file");
        //    listFail.AddRange(list);
        //    foreach (var item in list)
        //    {
        //        richTextBox1.AppendText(item.MAC + "," + item.LogPath + Environment.NewLine);
        //    }
        //}

        //private void button2_Click(object sender, EventArgs e)
        //{
        //    _vm.GetOkList.AddRange(listFail);
        //}

        //private void label5_Click(object sender, EventArgs e)
        //{

        //}
    }
}
