using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Globalization;

namespace App.SearchTool
{

    public class Adv_Searcher : SearchHelper
    {
        //folder like that: .../20220727/...csv
        public override List<string> GetAllFiles(string directory)
        {
            List<string> files = new List<string>();
            var subDirs = Directory.GetDirectories(directory);
            foreach(var subDir in subDirs)
            {
                //get the date of subdirs
                try
                {
                    string dateTimeRegex = @"20\d\d\d\d\d\d";//2022mmdd
                    string dt = Regex.Match(subDir, dateTimeRegex).Value;
                    DateTime testDate = DateTime.ParseExact(dt, "yyyyMMdd", CultureInfo.GetCultureInfo("en-US"));
                    if (testDate < Settings.GetValue().DateTimeStart)
                    {
                        continue;
                    }
                    var listFile = Directory.GetFiles(subDir);
                    files.AddRange(listFile);
                }
                catch
                {

                }
                
            }
            GetlistLogChecked();
            return CompareList(files);
        }
    }

    public class setless_Searcher : SearchHelper
    {
        //folder like that: .../Logs/...20220712
        public override List<string> GetAllFiles(string directory)
        {           

            List<string> files = new List<string>();
            string dateTimeRegex = @"20\d\d\d\d\d\d";//2022mmdd
            var listsLog = Directory.GetFiles(directory);
            foreach (var file in listsLog)
            {
                try
                {
                    string dt = Regex.Match(file, dateTimeRegex).Value;
                    DateTime testDate = DateTime.ParseExact(dt, "yyyyMMdd", CultureInfo.GetCultureInfo("en-US"));
                    if (testDate < Settings.GetValue().DateTimeStart)
                    {
                        continue;
                    }
                    files.Add(file);
                }
                catch
                {
                    continue;
                }
            }
            GetlistLogChecked();
            return CompareList(files);       
        }
    }


    public abstract class SearchHelper
    {
        public virtual void WriteLog(string text)
        {
            StaticGlobal.Logger(text + Environment.NewLine);
        }

        public abstract List<string> GetAllFiles(string directory);
        //{
        //    List<string> files = new List<string>();
        //    {
        //        var logsNotCheck = CompareList(Directory.GetFiles(directory).ToList());
        //        files.AddRange(logsNotCheck);
        //        List<string> subDirs = Directory.GetDirectories(directory).ToList();
        //        foreach (string Dir in subDirs)
        //        {
        //            var subList = GetAllFiles(Dir);
        //            files.AddRange(subList);
        //        }
        //    }
        //    return files;
        //}


        public static List<PathLastWriteTime> fileBeingWrites = new List<PathLastWriteTime>(); 
        public virtual List<DutLog> Log2DutLogObj(string path, ilogConveter logconverter)
        {
            try
            {
                List<DutLog> logs = logconverter.Convert(path);
                return logs;
            }
            catch
            {
                fileBeingWrites.Add(new PathLastWriteTime(path));
                return null;
            }
        }

        public virtual List<PathLastWriteTime> listPathToPathMd5(List<string> paths)
        {
            List<PathLastWriteTime> pathwithmd5s = new List<PathLastWriteTime>();
            foreach (string path in paths)
            {
                pathwithmd5s.Add(new PathLastWriteTime(path));
            }
            return pathwithmd5s;
        }

        public static List<PathLastWriteTime> scanned=new List<PathLastWriteTime>();
        public virtual void AddListScanedToTextFile()
        {
            foreach(var s in fileBeingWrites)
            {
                scanned.Remove(s);
            }
            fileBeingWrites.Clear();
            string path = Directory.GetCurrentDirectory() + "\\Data\\listScanned.txt";
            foreach (var name in scanned)
            {
                string fommat = string.Format("<{0}>", name.ToString()) + Environment.NewLine;
                File.AppendAllText(path,fommat);
                System.Threading.Thread.Sleep(1);
            }
        }


        private static List<PathLastWriteTime> listLogChecked = new List<PathLastWriteTime>();
        public virtual void GetlistLogChecked()
        {
            var list=new List<PathLastWriteTime>();
            string path = Directory.GetCurrentDirectory() + "\\Data\\listScanned.txt";
            if(!File.Exists(path))
            {
                File.WriteAllText(path, "");
                System.Threading.Thread.Sleep(100);
            }
            string data = File.ReadAllText(path);
            string pattern = "<(?<fileName>.*?),(?<lastwritetime>.*?)>";
            var items = Regex.Matches(data, pattern);
            foreach (Match item in items)
            {
                string FilePath = item.Groups["fileName"].ToString();
                string md5 = item.Groups["lastwritetime"].ToString();
                list.Add(new PathLastWriteTime(FilePath,md5));
            }
            listLogChecked = list;
        }

        public virtual List<string> CompareList(List<string> files)
        {
            List<string> list = new List<string>();
            foreach(string file in files)
            {
                if(!CompareLogIsChecked(file))
                {
                    list.Add(file); 
                }
            }
            return list;
        }

        public virtual bool CompareLogIsChecked(string filename)
        {
            foreach(var item in listLogChecked)
            {
                if(item.Compare(filename)) return true;
            }
            return false;
        }

    }

    public interface ilogConveter
    {
        List<DutLog> Convert(string logPath);
    }

    public class PathLastWriteTime
    {
        string path;
        string lastwritetime;
        public PathLastWriteTime(string path)
        {
            this.path = path;
            try
            {
                lastwritetime = File.GetLastWriteTime(path).ToString("yyyyMMddHHmmss");
            }
            catch(Exception ex)
            {
                System.Windows.Forms.MessageBox.Show("ErrorCode:100000\n" + ex.ToString());
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="md5"></param>
        public PathLastWriteTime(string path, string lastwritetime)
        {
            this.path = path;
            this.lastwritetime = lastwritetime;
        }

        public bool Compare(string path)
        {
            var rs = false;
            if(path==this.path)
            {
                var _lastwritetime = ""; 
                try
                {
                    _lastwritetime = File.GetLastWriteTime(path).ToString("yyyyMMddHHmmss");
                }
                catch(Exception ex)
                {
                    System.Windows.Forms.MessageBox.Show("ErrorCode:100001\n" + ex.ToString());
                }
                if (lastwritetime == _lastwritetime) return true;
            }
            return rs;
        }
        public override string ToString()
        {
            string msg = string.Format("{0},{1}", path, lastwritetime);
            return msg;
        }
    }

    public class Pre_advertising : ilogConveter
    {
        List<DutLog> dutLogs = new List<DutLog>();

        public List<DutLog> Convert(string logPath)
        {
            //from logpath trim datetime
            string dateTimeRegex = @"2022\d\d\d\d";//2022mmdd
            string dt = Regex.Match(logPath, dateTimeRegex).Value;
            DateTime testDate = DateTime.ParseExact(dt, "yyyyMMdd", CultureInfo.GetCultureInfo("en-US"));
            if (testDate < Settings.GetValue().DateTimeStart)
            {
                return null;
            }
            string data = File.ReadAllText(logPath);            
            string pattern = @"\w{2}:\w{2}:\w{2}:\w{2}:\w{2}:\w{2}";
            var matchs=Regex.Matches(data,pattern);
            foreach(Match match in matchs)
            {
                dutLogs.Add(new DutLog() { MAC=match.Value,LogPath=logPath});
            }         
            return dutLogs;
        }
    }

    public class SetLess: ilogConveter
    {
        List<DutLog> dutLogs = new List<DutLog>();

        public List<DutLog> Convert(string logPath)
        {
            string dateTimeRegex = @"2022\d\d\d\d";//2022mmdd
            string dt=Regex.Match(logPath, dateTimeRegex).Value;
            DateTime testDate = DateTime.ParseExact(dt, "yyyyMMdd", CultureInfo.GetCultureInfo("en-US"));
            if(testDate< Settings.GetValue().DateTimeStart)
            {
                return null;
            }
            //add date:22-8-4 12 file*5s=60s
            DateTime lastwritetime = File.GetLastWriteTime(logPath);
            DateTime currentTime = DateTime.Now;
            //if lastwritetime > 2 minute

            var l = logPath.Split('\\');
            string filename = l[l.Length - 1];
            string destPath = Directory.GetCurrentDirectory() + "\\LogTemp";
            //clear folder first
         
            File.Copy(logPath,Path.Combine(destPath, filename));
            string data = File.ReadAllText(Path.Combine(destPath,filename));
            //
            string pattern = @"\w{2}:\w{2}:\w{2}:\w{2}:\w{2}:\w{2}";
            var matchs = Regex.Matches(data, pattern);
            foreach (Match match in matchs)
            {
                dutLogs.Add(new DutLog() { MAC = match.Value, LogPath = logPath });
            }

            foreach (string file in Directory.GetFiles(destPath))
            {
                File.Delete(file);
            }
            return dutLogs;
        }
    }
}
