using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App
{
    static public class StaticGlobal
    {
        public enum project
        {
            B0,
            S23
        }
        public static SearchTool.SearchHelper SearchHelper { get; set; }
        public static Action<string> WriteLogAcction { get; set; }

        public static project Project { get; set; }


        public static void Logger(string text)
        {
            if (WriteLogAcction == null) throw new Exception("write log not set");
            else
            {
                WriteLogAcction?.Invoke(text);
            }
        }

        public static string currentStaion=GetCurrentStation();
        private static string GetCurrentStation()
        {
            string path = Environment.CurrentDirectory + "\\st.bin";
            string data=System.IO.File.ReadAllText(path);
            if(string.IsNullOrEmpty(data))
            {
                SearchHelper=new SearchTool.Adv_Searcher();
                return "adv";
            }
            else
            {
                SearchHelper =new SearchTool.setless_Searcher();
                return "setless";
            }
        }

    }
}
