using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

namespace App.SearchTool
{
    public static class SearchHelper
    {

        public static List<string> GetAllFiles(string directory)
        {
            GetlistLogChecked();
            List<string> files = new List<string>();
            {
                var logsNotCheck = CompareList(Directory.GetFiles(directory).ToList());
                files.AddRange(logsNotCheck);
                List<string> subDirs = Directory.GetDirectories(directory).ToList();
                foreach (string Dir in subDirs)
                {
                    var subList = GetAllFiles(Dir);
                    files.AddRange(subList);
                }
            }
            return files;
        }

        public static List<string> fileBeingWrites = new List<string>(); 
        public static List<DutLog> Log2DutLogObj(string path, ilogConveter logconverter)
        {
            try
            {
                string data = File.ReadAllText(path);
                List<DutLog> logs = logconverter.Convert(path, data);
                return logs;
            }
            catch(Exception ex)
            {
                fileBeingWrites.Add(path);
                return null;
            }
        }

        public static List<string> scanned=new List<string>();
        public static void AddListScanedToTextFile()
        {
            foreach(string s in fileBeingWrites)
            {
                scanned.Remove(s);
            }
            fileBeingWrites.Clear();
            string path = Directory.GetCurrentDirectory() + "\\Data\\listScanned.txt";
            foreach (string name in scanned)
            {
                string fommat = string.Format("<{0}>", name) + Environment.NewLine;
                File.AppendAllText(path,fommat);
            }
        }

        private static List<string> listLogChecked = new List<string>();
        public static void GetlistLogChecked()
        {
            var list=new List<string>();
            string path = Directory.GetCurrentDirectory() + "\\Data\\listScanned.txt";
            if(!File.Exists(path))
            {
                File.WriteAllText(path, "");
                System.Threading.Thread.Sleep(100);
            }
            string data = File.ReadAllText(path);
            string pattern = "<(?<fileName>.*?)>";
            var items = Regex.Matches(data, pattern);
            foreach (Match item in items)
            {
                string FilePath = item.Groups["fileName"].ToString();
                listLogChecked.Add(FilePath);
            }
        }

        public static List<string> CompareList(List<string> files)
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

        public static bool CompareLogIsChecked(string filename)
        {
            foreach(var item in listLogChecked)
            {
                if(filename==item)
                {
                    return true;
                }
            }
            return false;
        }

    }

    public interface ilogConveter
    {
        List<DutLog> Convert(string logPath,string data);
    }

    public class Pre_advertising : ilogConveter
    {
        List<DutLog> dutLogs = new List<DutLog>();

        public List<DutLog> Convert(string logPath,string data)
        {
            //check date
            var datetimeStart = Settings.GetValue().DateTimeStart;
            string date_pattern = @"\d{2}/\d{2}/\d{2}";
            var d = Regex.Match(data, date_pattern).Value;
            d = "20" + d;
            DateTime testDate = DateTime.Parse(d);
            //
            string pattern = @"\w{2}:\w{2}:\w{2}:\w{2}:\w{2}:\w{2}";
            var matchs=Regex.Matches(data,pattern);
            foreach(Match match in matchs)
            {
                dutLogs.Add(new DutLog() { MAC=match.Value,LogPath=logPath});
            }
            if (testDate < datetimeStart)
            {
                return null;
            }
            return dutLogs;
        }
    }
}
