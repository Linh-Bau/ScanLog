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
    using PushSNResultToMesForSetlessStation;
    using System.Globalization;
    using System.IO;

    public partial class Form1 : Form
    {
        public Action closedFormCallBack { get; set; }


        ViewModels.ViewModel _vm;
        List<DutLog> listFail = new List<DutLog>();
        TimeSpan coundown;
        LogRichText scanlog;
        LogRichText pushmesslog;
        bool w = false;
        int wait = 0;
        IDetectNewSN pushSnWorker;

        public Form1()
        {
            InitializeComponent();

            scanlog = new LogRichText(this.richTextBox1);
            StaticGlobal.WriteLogAcction = (text) => scanlog.LogWrite(text, -1);

            timer1.Enabled = true;

            _vm = new ViewModels.ViewModel();
            _vm.updateSettings = this.UpdateSettings;
            _vm.ScanCallback = this.ScanCallBack;
            _vm.DataWriting = this.WriteData;

            this.Shown += new EventHandler((sender, e) => _vm.Start_Appication());
            this.btn_Settings.Click += new EventHandler((sender, e) => _vm.Setting_Click());
            this.btn_Scan.Click += Btn_Scan_Click;
            this.FormClosing += Form1_FormClosing;
            //mesWindow = new MesWrongStation();
            //check project
            string path=System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
            string exename=System.IO.Path.GetFileName(path);
            if(exename.Contains("S23"))
            {
                StaticGlobal.Project = StaticGlobal.project.S23;
            }
            else
            {
                StaticGlobal.Project = StaticGlobal.project.B0;
            }

            if(StaticGlobal.currentStaion== "setless")
            {
                if(exename.Contains("Check_OLD_ID"))
                {
                    pushmesslog = new LogRichText(this.richTextBox2);
                    MesLog.Text = "Check SN";
                    pushSnWorker = new CheckNoSNAndNoMesFromCsvFile(DateTime.Now);
                    pushSnWorker.logCallBack = (text, i) => pushmesslog.LogWrite(text, i);
                    Task autoPushSnToMesTask = new Task(() => pushSnWorker.RunLoop());
                    autoPushSnToMesTask.Start();
                }

                else if (exename.Contains("OTA"))
                {
                    _vm.Setting_Click();
                    pushmesslog = new LogRichText(this.richTextBox2);
                    MesLog.Text = "OTA";
                    var start_date = Settings.GetValue().DateTimeStart;
                    pushSnWorker = new CheckRetest(start_date);
                    pushSnWorker.logCallBack = (text, i) => pushmesslog.LogWrite(text, i);
                    Task autoPushSnToMesTask = new Task(() => pushSnWorker.RunLoop());
                    autoPushSnToMesTask.Start();
                }   
                else
                {
                    pushmesslog = new LogRichText(this.richTextBox2);
                    //pushSnWorker = new CheckRetest(DateTime.ParseExact("20220701", "yyyyMMdd", CultureInfo.GetCultureInfo("en-US")));
                    pushSnWorker=new AutoPush();
                    pushSnWorker.logCallBack = (text, i) => pushmesslog.LogWrite(text, i);
                    pushmesslog.LogWrite("Start scan log and push to mes!", -1);
                    Task autoPushSnToMesTask = new Task(() => pushSnWorker.RunLoop());
                    autoPushSnToMesTask.Start();
                }                
            }      
            //
            
        }

        private void Btn_Scan_Click(object sender, EventArgs e)
        {
            try
            {
                _vm.ScanClick();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            btn_Scan.Enabled = false;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(_vm.FormClose())
            {
                pushSnWorker.CloseThread();
            }
            else
            {
                e.Cancel = true;
            }
        }

        public void UpdateSettings()
        {
            string project = "B0";
            if (StaticGlobal.Project == StaticGlobal.project.S23) project = "S23";
            Station.Text = Settings.GetValue().Station+"  "+project;
            label4.Text = Station.Text;
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
                scanlog.LogWrite("Start scan Log",true);
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
                if (DetectedList.Count > 0) _vm.DetectedNew(DetectedList);
                StaticGlobal.SearchHelper.AddListScanedToTextFile();
                FinishedAll();
                scanlog.LogWrite("Write data success!",-1);
            }
        }
        
        private void WriteData()
        {
            controlInvoke(lb_status, () => { lb_status.Text = "Data Writing..."; });
        }

        private void FinishedAll()
        {
            controlInvoke(lb_count, () => { lb_count.Text = "Clear after 10 secons"; });
            wait = 10;
            w = true;
            controlInvoke(btn_Scan, () => btn_Scan.Enabled = true);
        }

        private void Finish(int? totalScan,int? olds, int? notchecks, bool? detectedNew)
        {
            controlInvoke(lb_totalScan, (Action)(() => { lb_totalScan.Text = totalScan.ToString(); }));
            controlInvoke(lb_totalScan, (Action)(() => { lb_olds.Text = olds.ToString(); }));
            controlInvoke(lb_totalScan, (Action)(() => { lb_notChecks.Text = notchecks.ToString(); }));

            controlInvoke(lb_status, (Action)(() => { lb_status.Text = "Scan Finish"; }));
            if(detectedNew!=null && detectedNew==true)
            {
                controlInvoke(lb_status, (Action)(() => { lb_status.BackColor = Color.Red; }));
            }
            else
            {
                controlInvoke(lb_status, (Action)(() => { lb_status.BackColor = Color.Green; }));
            }
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

        private void ScanLog_SelectedIndexChanged(object sender, EventArgs e)
        {
            autoReturnMestab(10000);
        }

        private void autoReturnMestab(int delay)
        {
            var thread = new System.Threading.Thread(
                () =>
                {
                    System.Threading.Thread.Sleep(delay);
                    controlInvoke(ScanLog, () => { ScanLog.SelectTab(0); });
                }
                );
            thread.Start();
        }

        private void label4_Click(object sender, EventArgs e)
        {
            //pushmesslog.LogWrite("clear log", -1);
        }
    }

    public class LogRichText
    {
        private RichTextBox log;

        public LogRichText(RichTextBox richTextBox)
        {
            this.log=richTextBox;
            log.TextChanged += Log_TextChanged;
            log.DetectUrls = false;

        }

        

        private void appendLogToText(string text)
        {
            string path =Directory.GetCurrentDirectory()+"\\mesLog\\"+ DateTime.Today.AddMinutes(5).ToString("yyMMdd") + ".txt";
            if(!File.Exists(path))
            {
                string msg = string.Format("create log {0}\nStart scan log and push to mes!\n",DateTime.Now.ToString("yyMMdd HH:mm:ss"));
                File.AppendAllText(path, msg);
                System.Threading.Thread.Sleep(100);
                this.log.Clear();
                this.log.Text = msg;
            }
            using(StreamWriter sw = new StreamWriter(path,true))
            {
                sw.WriteLine(text);
            }
        }

        private void Log_TextChanged(object sender, EventArgs e)
        {
            // set the current caret position to the end
            log.SelectionStart = log.Text.Length;
            // scroll it automatically
            log.ScrollToCaret();
        }

        public void LogWrite(string text,int i=-1)
        {
            string msg;
            switch (i)
            {
                case 0:
                    msg=text+Environment.NewLine;
                    break;
                case 1:
                    msg = "Warring: " + text + Environment.NewLine;
                    break;

                case 2:
                    msg = "Error: " + text + Environment.NewLine;
                    break;
                default:
                    msg = text + Environment.NewLine;
                    break;
            }

            
            if (this.log.InvokeRequired)
            {
                this.log.Invoke((Action)(() =>
                {
                    if(msg.Contains("clear log"))
                    {
                        this.log.Clear();
                        this.log.Text = "Start scan log and push to mes!\n";
                    }
                    else
                    {
                        this.log.AppendText(msg);
                    }                    
                }));
            }
            else
            {
                if (msg.Contains("clear log"))
                {
                    this.log.Clear();
                    this.log.Text = "Start scan log and push to mes!\n";
                }
                else
                {
                    this.log.AppendText(msg);
                }
            }
            //appendLogToText(msg);
        }

        public void LogWrite(string text, bool clearLog = false)
        {
            if (this.log.InvokeRequired)
            {
                this.log.Invoke((Action)(() =>
                {
                    if (clearLog)
                    {
                        this.log.Clear();
                    }
                    this.log.AppendText(text + Environment.NewLine);
                }));
            }
            else
            {
                if (clearLog)
                {
                    this.log.Clear();
                }
                this.log.AppendText(text + Environment.NewLine);
            }
        }
    }
}
