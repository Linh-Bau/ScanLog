using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
using System.Net.Http;
using System.Diagnostics;

namespace App.PushSNResultToMesForSetlessStation
{
    public class AutoPush:IDetectNewSN
    {
        public Action<string, int> logCallBack { get; set; }
        /// <summary>
        /// i=0, normar
        /// i=1, warring
        /// i=2, error
        /// </summary>
        /// <param name="text"></param>
        /// <param name="i"></param>
        public void logWrite(string text, int i=0)
        {
            logCallBack?.Invoke(text,i);
        }


        struct test_info
        {
            public string SN;
            public string date;
            public string time;
            public string result;
            public override string ToString()
            {
                return SN + "\t" + date + "\t" + time + "\t" + result;
            }
        }

        string defaultDir = @"C:\DGS\LOGS";
        test_info[][] listScanned = new test_info[13][];
        string today = "";

        public AutoPush()
        {
            //clear folder
            foreach(string filename in Directory.GetFiles(Directory.GetCurrentDirectory() + "\\TodayTestLog"))
            {
                File.Delete(filename);
                sleep(5);
            }
            //when open program, copy the old log to the 
            today = DateTime.Now.ToString("yyyyMMdd");
            string[] allLog = Directory.GetFiles(defaultDir);
            foreach(string log in allLog)
            {
                //override the today log in the path
                if (log.Contains(today))
                {
                    File.Copy(log,Path.Combine(Directory.GetCurrentDirectory()+ "\\TodayTestLog", Path.GetFileName(log)),true);
                    sleep(10);
                }
            }
            //from today log, get all mac, mac is SN
            foreach(string log in Directory.GetFiles(Directory.GetCurrentDirectory() + "\\TodayTestLog"))
            {
                var x = new List<test_info>();
                GetSNFromCsv(log, out x);
            }
        }

        bool close_thread = false;
        public void CloseThread()
        {
            close_thread = true;
        }

        public void RunLoop()
        {
            while(true)
            {
                try
                {
                    string now = DateTime.Now.AddMinutes(-3).ToString("yyyyMMdd");
                    AutoPushToMesh(now);
                    sleep(1000);
                    if (now != today)
                    {
                        listScanned = new test_info[13][];
                        today = now;
                        logWrite("clear log", -1);
                    }
                }
                catch (Exception ex)
                {
                    System.Windows.Forms.MessageBox.Show(ex.ToString());
                }
                if (close_thread)
                {
                    break;
                }
            }
        }

        public void AutoPushToMesh(string _today)
        { 
            try
            {
                //get list file changed
                List<string> listHasChange = new List<string>();
                string[] allLog = Directory.GetFiles(defaultDir);
                foreach (var log in allLog)
                {
                    //get the today log
                    if (log.Contains(_today))
                    {
                        //get the zig number
                        int zig_num = int.Parse(Regex.Match(log, @"..\.").Value.Replace(".", ""));//get zig number
                        if (listScanned[zig_num] == null)
                        {
                            listHasChange.Add(log);
                        }
                        else
                        {
                            string filename = listScanned[zig_num][0].SN.Split(',')[0];
                            string lastwritetime = listScanned[zig_num][0].SN.Split(',')[1];
                            string sourcePathLastWriteTime = getLastWiteTime(log);
                            //
                            if (lastwritetime != sourcePathLastWriteTime)
                            {
                                listHasChange.Add(log);
                            }
                        }
                    }
                }
                //override the changed files
                foreach (string log in listHasChange)
                {
                    string dirPath = Path.Combine(Directory.GetCurrentDirectory() + "\\TodayTestLog", Path.GetFileName(log));
                    File.Copy(log, dirPath, true);
                    var new_sn = new List<test_info>();
                    GetSNFromCsv(dirPath, out new_sn);
                    int zignum = int.Parse(Regex.Match(log, @"..\.").Value.Replace(".", ""));//get zig number
                    Push(new_sn,zignum);
                }   
            }
            catch(Exception ex)
            {
                logWrite(ex.ToString(), 2);
            }              
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="newSN"></param>
        private void GetSNFromCsv(string filePath,out List<test_info> newSN)
        {
            newSN=new List<test_info>();            
            string data = File.ReadAllText(filePath);
            //data need remove all break line and space
            data = Regex.Replace(data, "\\s", "");
            //get lastwritetime and store listscaned header
            List<test_info> scanedMac = new List<test_info>() { new test_info { SN = Path.GetFileName(filePath) + "," + getLastWiteTime(filePath) } };
            //Each SN has a snLog
            string macPattern = @"(#INIT).*?(SPEN_BT_ID)";
            var snLog = Regex.Matches(data, macPattern);
            foreach (Match log in snLog)
            {
                var test_info = getInfo(log.Value);
                scanedMac.Add(test_info);
            }
            //scanedMac= listScannedMac+newMac
            int zig_num = int.Parse(Regex.Match(filePath, @"..\.").Value.Replace(".", ""));//get zig number
            if (listScanned[zig_num]==null)//when open program listScanned is null or empty
            {
                int totalmac = scanedMac.Count - 1;
                if(totalmac>0)
                    newSN=scanedMac.GetRange(1,totalmac);
            }
            else
            {
                //scanedMac = old Mac+ new Mac
                for (int i = 1; i < scanedMac.Count; i++)
                {
                    //listScanned[zig_num] is old mac
                    if (!listScanned[zig_num].Contains(scanedMac[i]))
                        newSN.Add(scanedMac[i]);
                }
                //scanedMac = old Mac+ new Mac
                //int totalMac = all_test_info.Count;
                //int totalOld = listScanned[zig_num].Length - 1;
                //try
                //{
                //    newSN = all_test_info.GetRange(totalOld, totalMac - totalOld);
                //}
                //catch(Exception ex)
                //{
                //    logWrite(ex.ToString(),-1);
                //}                
            }
            //after all, update list scanned mac
            listScanned[zig_num] = scanedMac.ToArray();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
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

        private string getLastWiteTime(string source)
        {
            return File.GetLastWriteTime(source).ToString("yyyyMMddHHmmss");
        }

        void Push(List<test_info> test_Infos,int zignum)
        {
            foreach (test_info testInfo in test_Infos)
            {
                Push(testInfo,zignum);
                logWrite("---------------------------------------------", -1);
            }
        }
        bool Push(test_info test_Info,int zignum)
        {
            try
            {
                if (string.IsNullOrEmpty(test_Info.SN) || string.IsNullOrEmpty(test_Info.SN) || string.IsNullOrEmpty(test_Info.SN) || string.IsNullOrEmpty(test_Info.SN))
                {
                    logWrite("Info: "+test_Info.ToString(),2);
                    return false;
                }
                string SN = test_Info.SN.Replace(":", "");
                string Station = "Set-less_jig_TEST";
                if(StaticGlobal.Project==StaticGlobal.project.S23) Station = "S23%20Set-less_jig_TEST";

                string Date = test_Info.date.Replace("/", "");//date fomat 2022/08/15
                string Time = test_Info.time.Replace(":", "");
                string getUrl = $@"http://172.23.46.18:8088/api/2/serial/{SN}/station/{Station}/date/{Date}/time/{Time}/result/{test_Info.result}";
                logWrite(string.Format("request url: {0}", getUrl));
                string res = "";
                string response = "";
                response = HttpPost(getUrl, null, out res);
                logWrite(test_Info.ToString());
                logWrite(string.Format("res= {0}\nresponse= {1}", res, response));
                if(response.Contains("ROUTE END")||response.Contains("Set-less"))
                {
                    DetectWrongStationSN(SN, zignum, response);
                }
                return true;
            }
            catch(Exception ex)
            {
                logWrite(test_Info.ToString(), 2);
                logWrite(ex.ToString(),2);
                return false;
            }
            
        }

        public void DetectWrongStationSN(string SN,int zig, string route_response)
        {
            string window_name = "CHECK MES FAIL\nCHECK MES FAIL";
            string exepath = Directory.GetCurrentDirectory() + "\\MesFail\\WrongStation.exe";
            string wkd = Directory.GetCurrentDirectory() + "\\MesFail";
            var p=Process.GetProcessesByName("WrongStation");
            bool p_openning = false;
            if(p.Length>0)
            {
                p_openning = true;
                foreach (Process proc in p)
                    proc.Kill();
            }
      
            string msg = string.Format("{0} in Zig:{1} Fail, Mes_res:{2}",SN,zig,route_response);
            using(StreamWriter sw= new StreamWriter(wkd+ "\\SnWrong.txt", p_openning))
            {
                if (!p_openning)
                {
                    sw.WriteLine(window_name);
                }
                sw.WriteLine(msg);
                sw.Flush();
            }
            ProcessStartInfo info=new ProcessStartInfo();
            info.FileName = exepath;
            info.WorkingDirectory = wkd;
            Process.Start(info);
        }

        void sleep(int miliseconds)
        {
            System.Threading.Thread.Sleep(miliseconds);
        }

        public static string HttpGet(string url)
        {
            var client = new HttpClient();
            var bytes = client.GetByteArrayAsync(url).GetAwaiter().GetResult();
            var result = System.Text.Encoding.UTF8.GetString(bytes);
            return result;
        }
        public static string HttpPost(string url, HttpContent content, out string statusCode)
        {
            var client = new HttpClient();
            var result = client.PostAsync(url, content).GetAwaiter().GetResult();
            var bytes = result.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult();
            var responseBody = System.Text.Encoding.UTF8.GetString(bytes);
            if (!result.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to post requests.Response code: {result.StatusCode}");
            }
            statusCode = result.StatusCode.ToString();
            return responseBody;
        }
    }
}
