using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App
{
    static public class StaticGlobal
    {
        public static Action<string> WriteLogAcction { get; set; }
        public static void Logger(string text)
        {
            if (WriteLogAcction == null) throw new Exception("write log not set");
            else
            {
                WriteLogAcction?.Invoke(text);
            }
        }

    }
}
