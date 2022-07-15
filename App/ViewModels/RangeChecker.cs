using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace App.ViewModels
{
    public class RangeChecker
    {
        string path = Directory.GetCurrentDirectory() + "\\Data\\Range.txt";
        List<limit> limits=new List<limit>();
        
        public RangeChecker()
        {
            string data = File.ReadAllText(path);
            string rexPattern = "<(?<LowLimit>.*?);(?<HightLimit>.*?)>";
            var items = Regex.Matches(data, rexPattern);
            foreach (Match item in items)
            {
                bool lowCheck = string.IsNullOrEmpty(item.Groups["LowLimit"].Value);
                bool hightCheck = string.IsNullOrEmpty(item.Groups["HightLimit"].Value);
                if(!lowCheck && !hightCheck)
                {
                    AddLimit(item.Groups["LowLimit"].Value, item.Groups["HightLimit"].Value);
                }
                else
                {
                    throw new ArgumentNullException();
                }
            }
        }

        public bool CheckOldMac(DutLog dutLog)
        {
            foreach(limit limit in limits)
            {
                if (limit.CheckInLimit(dutLog.MAC))
                {
                    return true;
                }
            }
            return false;
        }

        void AddLimit(string low,string hight)
        {
            string pattern = @"\w{2}:\w{2}:\w{2}:\w{2}:\w{2}:\w{2}";
            if(Regex.IsMatch(low, pattern)&&Regex.IsMatch(hight,pattern))
            {
                limit _limit = new limit() { LowLimit = low, HighLimit = hight };
                this.limits.Add(_limit);
            }
            else
            {
                throw new ArgumentException("limit "+low+","+hight+" is not MAC fomat");
            }

        }
        class limit
        {
            public string LowLimit { get; set; }
            public string HighLimit { get; set; }

            public bool CheckInLimit(string MAC)
            {
                ulong low = ulong.Parse(LowLimit.Replace(":", ""), System.Globalization.NumberStyles.HexNumber);
                ulong hight = ulong.Parse(HighLimit.Replace(":", ""), System.Globalization.NumberStyles.HexNumber);
                ulong compareValue= ulong.Parse(MAC.Replace(":", ""), System.Globalization.NumberStyles.HexNumber);
                if (compareValue >= low && compareValue <= hight)
                {
                    return true;
                }
                else return false;
            }
        }
    }
}
