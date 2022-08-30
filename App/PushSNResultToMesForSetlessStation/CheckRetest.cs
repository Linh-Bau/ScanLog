using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace App.PushSNResultToMesForSetlessStation
{
    public class CheckRetest : IDetectNewSN
    {
        public Action<string, int> logCallBack { get; set; }

        /// <summary>
        /// i=0, normar
        /// i=1, warring
        /// i=2, error
        /// </summary>
        /// <param name="text"></param>
        /// <param name="i"></param>
        public void logWrite(string text, int i = 0)
        {
            logCallBack?.Invoke(text, i);
        }

        struct test_info
        {
            public string SN;
            public int zig;
            public string date;
            public string time;
            public string result;
            public override string ToString()
            {
                return SN + "\tzig:"+zig+"\t" + date + "\t" + time + "\t" + result;
            }
        }

        List<test_info> list_tested = new List<test_info>();
        DateTime start_date;

        string defaultDir = @"C:\DGS\LOGS";

        public CheckRetest(DateTime start_date)
        {
            //load all SN
            this.start_date = start_date;
            string temp_Folder = Directory.GetCurrentDirectory() + "\\TodayTestLog";
            foreach(var file in Directory.GetFiles(temp_Folder))
            {
                File.Delete(file);
            }
        }

        private void loadAllSnToToday(DateTime start_date)
        {
            var logs = Directory.GetFiles(defaultDir);
            List<string> list_log = new List<string>();
            foreach(string log in logs)
            {
                string log_test_date = Regex.Match(log, "\\d{8}").ToString();
                try
                {
                    DateTime test_date= DateTime.ParseExact(log_test_date,"yyyyMMdd", CultureInfo.GetCultureInfo("en-US"));
                    if(test_date>=start_date)
                    {
                        list_log.Add(log);
                    }
                }
                catch
                {

                }
            }
            foreach(string log in list_log)
            {
                loadData(log);
            }
        }

        private void loadData(string log)
        {
            int zig_num = int.Parse(Regex.Match(log, @"..\.").Value.Replace(".", ""));//get zig number
            string data = "";
            using(StreamReader sr=new StreamReader(log))
            {
                data=sr.ReadToEnd();
                data = Regex.Replace(data, "\\s", "");
            }
            string macPattern = @"(#INIT).*?(SPEN_BT_ID)";
            var snLog = Regex.Matches(data, macPattern);
            foreach (Match match in snLog)
            {
                var test_info = getInfo(match.Value);
                test_info.zig = zig_num;
                list_tested.Add(test_info);
            }
        }

        private test_info getInfo(string data)
        {
            string regex_SN = @"(?:.*P/N.*?)(\w\w:..:..:..:..:..)";
            string regex_date = @"(?:.*DATE.*?)(\d{4}/\d{2}/\d{2})";
            string regex_time = @"(?:.*TIME.*?)(\d{2}:\d{2}:\d{2})";
            string regex_result = @"(?:.*RESULT.*)(PASS|FAIL)(?:.*?TEST-TIME)";
            string sn = Regex.Match(data, regex_SN).Groups[1].Value;
            string date = Regex.Match(data, regex_date).Groups[1].Value;
            string time = Regex.Match(data, regex_time).Groups[1].Value;
            string result = Regex.Match(data, regex_result).Groups[1].Value;
            return new test_info() { SN = sn, date = date, time = time, result = result };
        }



        public void CloseThread()
        {
           
        }

        bool isLoadData = false;
        public void RunLoop()
        {
            logWrite(String.Format("Đợi tải dữ liệu từ ngày {0} đến hôm nay, vui lòng đợi tải xong trước khi test", start_date.ToString("ddMMyyyy")));
            loadAllSnToToday(start_date);
            logWrite("Đã tải xong dữ liệu!");
            while (true)
            {
                try
                {
                    CheckRepeatSN();
                }
                catch(Exception ex)
                {
                    System.Windows.Forms.MessageBox.Show(ex.Message);
                }
            }
        }
        
        void CheckRepeatSN()
        {
            //get todaylog in directory log
            var logs = Directory.GetFiles(defaultDir);
            List<string> todayLog=new List<string>();
            string today = DateTime.Now.ToString("yyyyMMdd");
            foreach(var logFile in logs)
            {
                if (logFile.Contains(today)) todayLog.Add(logFile);
            }
            //check
            var list_new_sn = new List<test_info>();
            foreach(var logFile in todayLog)
            {
                list_new_sn.AddRange(detectNewSN(logFile));
            }
            foreach(var sn in list_new_sn)
            {
                CheckSnIsRepeat(sn);
            }
        }

        void CheckSnIsRepeat(test_info sn)
        {
            if (string.IsNullOrEmpty(sn.SN)) return;
            foreach (var tested in list_tested)
            {
                if(tested.SN==sn.SN)
                {
                    DetectRepeatSN(sn,tested);
                    break;
                }
            }
            list_tested.Add(sn);
        }

        private void DetectRepeatSN(test_info current, test_info lasttest_time)
        {
            string window_name = "OTA\nSN IS REPEATED";
            string exepath = Directory.GetCurrentDirectory() + "\\MesFail\\WrongStation.exe";
            string wkd = Directory.GetCurrentDirectory() + "\\MesFail";
            var p = Process.GetProcessesByName("WrongStation");
            bool p_openning = false;
            if (p.Length > 0)
            {
                p_openning = true;
                foreach (Process proc in p)
                    proc.Kill();
            }

            string msg = string.Format("{0}\nBị trùng với lần test trước\n{1}\n{2}", current.ToString(), lasttest_time.ToString(), "---------------------------------");
            using (StreamWriter sw = new StreamWriter(wkd + "\\SnWrong.txt", p_openning))
            {
                if(!p_openning)
                {
                    sw.WriteLine(window_name);
                }
                sw.WriteLine(msg);
                sw.Flush();
            }
            ProcessStartInfo info = new ProcessStartInfo();
            info.FileName = exepath;
            info.WorkingDirectory = wkd;
            Process.Start(info);
        }

        List<test_info> detectNewSN(string logpath)
        {
            var newSn= new List<test_info>();
            string Filename=Path.GetFileName(logpath);
            string temp_Folder = Directory.GetCurrentDirectory() + "\\TodayTestLog";
            string temp_File = Path.Combine(temp_Folder, Filename);
            //if log no exist, copy and return
            if(!File.Exists(temp_File))
            {
                File.Copy(logpath, temp_File,true);
                return newSn;
            }

            var root_last_writetime=File.GetLastWriteTime(logpath);
            var temp_last_writetime=File.GetLastWriteTime(temp_File);
            if(root_last_writetime!=temp_last_writetime)
            {
                int getEndIndex = 0;
                using(StreamReader sr=new StreamReader(temp_File))
                {
                    getEndIndex = sr.ReadToEnd().Length;
                }
                File.Copy(logpath, temp_File,true);
                System.Threading.Thread.Sleep(100);
                string newdata = "";
                using (StreamReader sr = new StreamReader(temp_File))
                {
                    string data = sr.ReadToEnd();

                    if(data.Length<getEndIndex)
                    {
                        System.Windows.Forms.MessageBox.Show("Opp, somethings are wrong!");
                        return newSn;
                    }
                    else
                    {
                        newdata = data.Substring(getEndIndex);
                        newdata = Regex.Replace(newdata, "\\s", "");
                    }
                }
                int zig_num = int.Parse(Regex.Match(logpath, @"..\.").Value.Replace(".", ""));//get zig number
                string macPattern = @"(#INIT).*?(SPEN_BT_ID)";
                var snLog = Regex.Matches(newdata, macPattern);
                foreach (Match match in snLog)
                {
                    var test_info = getInfo(match.Value);
                    test_info.zig = zig_num;
                    newSn.Add(test_info);
                }
                return newSn;
            }
            //if file not change, return
            else
            {
                return newSn;
            }
        }
    }
}
