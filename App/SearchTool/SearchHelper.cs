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
            List<string> files = new List<string>();

            {
                files.AddRange(Directory.GetFiles(directory));
                List<string> subDirs = Directory.GetDirectories(directory).ToList();
                foreach (string Dir in subDirs)
                {
                    var subList = GetAllFiles(Dir);
                    files.AddRange(subList);
                }
            }
            return files;
        }

        public static List<DutLog> Log2DutLogObj(string path, ilogConveter logconverter)
        {
            string data = File.ReadAllText(path);
            List<DutLog> logs = logconverter.Convert(path,data);
            return logs;
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
