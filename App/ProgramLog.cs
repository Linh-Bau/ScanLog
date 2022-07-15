using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace App
{
    static class ProgramLog
    {
        private static string Log = Directory.GetCurrentDirectory() + "\\Data\\log.txt";
        public static void Write(string s)
        {
            if(!File.Exists(Log))
            {
                string msg = "Create log date: " + DateTime.Now.ToString();
                File.WriteAllText(Log, msg + Environment.NewLine);
            }
            File.AppendAllText(Log, s+Environment.NewLine);
        }

        public static void Write(string s,List<DutLog> dutLogs,string date)
        {
            string k = "#########################################################";
            Write(s);
            List<string> list = new List<string>();
            foreach(DutLog dutLog in dutLogs)
            {
                list.Add(string.Format("{0}, {1}, {2}", dutLog.MAC, dutLog.LogPath, date));
            }
            
        }
    }
}
