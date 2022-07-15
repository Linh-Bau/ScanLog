using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

namespace App.Models
{
    public class Dut_old_But_OK
    {
        string path = Directory.GetCurrentDirectory() + "\\Data\\DutCheckOk.txt";

        List<DutLog> LogOkList = new List<DutLog>();
        public Dut_old_But_OK()
        {
            if(!File.Exists(path))
            {
                File.WriteAllText(path, "###############Please don't delete this file################################\n");
            }
            importFromData();
        }

        private void importFromData()
        {
            string data=File.ReadAllText(path);
            string pattern = "<(?<MAC>.*?);(?<LogPath>.*?)>";
            var items = Regex.Matches(data, pattern);
            foreach (Match item in items)
            {
                string mac = item.Groups["MAC"].ToString();
                string logPath = item.Groups["LogPath"].ToString();
                if(!string.IsNullOrEmpty(mac)&&!string.IsNullOrEmpty(logPath))
                {
                    DutLog log = new DutLog() { MAC = mac, LogPath = logPath };
                    LogOkList.Add(log);
                }
            }
        }


        public bool AddRange(List<DutLog> dutLogs)
        {
            bool rs = true;
            foreach(DutLog dutLog in dutLogs)
            {
                if(!Add(dutLog))
                    rs = false;
            }
            return rs;
        }

        public bool Add(DutLog dutLog)
        {
            if(CheckMacExist(dutLog))
            {
                return false;
            }
            else
            {
                string content = string.Format("<{0};{1}>" + Environment.NewLine, dutLog.MAC, dutLog.LogPath);
                File.AppendAllText(path, content);
                return true;
            }
        }

        public bool CheckMacExist(DutLog dut)
        {
            foreach(DutLog log in this.LogOkList)
            {
                if (log.MAC == dut.MAC) return true;
            }
            return false;
        }

    }
}
