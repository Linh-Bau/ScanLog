using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace App
{
    public static class Settings
    {
        private static Setting _Setting = new Setting();

        public static bool TryLoadSettings => _Setting.Load();
        public static SettingValues GetValue()
        {
            if (_Setting.Load())
                return _Setting.Value;
            else return null;
        }

        public static void SaveSettings(SettingValues settingValue) => _Setting.Value = settingValue;
    }
    public class Setting
    {
        public string path = Directory.GetCurrentDirectory() + "\\Settings.ini";
        private SettingValues values = new SettingValues();
        public SettingValues Value
        {
            get { return values; }
            set 
            { 
                values = value; 
                Save();
            }
        }
        public Setting()
        {
            Load();
        }

        public bool Load()
        {
            try
            {
                if(File.Exists(path))
                {
                    string data=File.ReadAllText(path);
                    string rexPattern_Station = string.Format("<(?<Keyword>Station);(?<Value>.*?)>");
                    string rexPattern_logpath = string.Format("<(?<Keyword>LogPath);(?<Value>.*?)>");
                    string rexPattern_datetime = string.Format("<(?<Keyword>DateTimeStart);(?<Value>.*?)>");
                    string rexPattern_coundown = string.Format("<(?<Keyword>Coundown);(?<Value>.*?)>");
                    string station = Regex.Match(data, rexPattern_Station).Groups[2].Value;
                    string logpath = Regex.Match(data, rexPattern_logpath).Groups[2].Value;
                    string DateTimeStart=Regex.Match(data, rexPattern_datetime).Groups[2].Value;
                    string Coundown=Regex.Match(data, rexPattern_coundown).Groups[2].Value;
                    var _stvalue = new SettingValues();
                    _stvalue.Station= station;
                    _stvalue.LogPath= logpath;
                    _stvalue.DateTimeStart = DateTime.Parse(DateTimeStart);
                    _stvalue.Coundown = TimeSpan.Parse(Coundown);
                    this.values = _stvalue;
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }
        public void Save()
        {
            CreateNewSettings();
            string station = string.Format("<Station;{0}>", values.Station) + Environment.NewLine;
            string logPath = string.Format("<LogPath;{0}>", values.LogPath) + Environment.NewLine;
            string startTime = string.Format("<DateTimeStart;{0}>", values.DateTimeStart.ToString()) + Environment.NewLine;
            string Coundown = string.Format("<Coundown;{0}>", values.Coundown.ToString()) + Environment.NewLine;
            File.AppendAllText(path, station);
            File.AppendAllText(path, logPath);
            File.AppendAllText(path, startTime);
            File.AppendAllText(path, Coundown);
        }
        public void CreateNewSettings()
        {
            if(File.Exists(path))
            {
                File.Delete(path);
            }
            string header = "################ Settings File, dont's change ##############"+Environment.NewLine;
            File.WriteAllText(path, header);
        }

    }

    public class SettingValues
    {
        public string Station { get; set; }
        public string LogPath { get; set; }
        public DateTime DateTimeStart { get; set; }
        public TimeSpan Coundown { get; set; }
    }
}
