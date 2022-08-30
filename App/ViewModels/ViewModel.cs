using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.ViewModels
{
    public class ViewModel
    {
        Models.Dut_old_But_OK dut_Old_But_OK = new Models.Dut_old_But_OK();
        public Models.Dut_old_But_OK GetOkList { get { return dut_Old_But_OK; } }
        public Action updateSettings { get; set; }
        public Action<string, object> ScanCallback { get; set; }
        public Action DetectedNewCallback { get; set; }
        public Action DataWriting { get; set; }
        public string LocationLog { get; set; }

        RangeChecker rangeChecker;

        SettingValues settingValues;

        public enum LoginStutus
        {
            SUCCESS,
            FAIL
        }
        public ViewModel()
        {
            rangeChecker=new RangeChecker();
            settingValues = Settings.GetValue();
        }

        public bool FormClose()
        {
            if (LoginEvent() == LoginStutus.SUCCESS)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public void Start_Appication()
        {
            //settings not found
            if(!Settings.TryLoadSettings)
            {
                if(LoginEvent()==LoginStutus.SUCCESS)
                {
                    SettingsForm settingsForm = new SettingsForm();
                    settingsForm.ShowDialog();
                }
                else
                {
                    Start_Appication();
                }
            }
            //settings ok;
            else
            {

            }
            updateSettings();
        }

        public void UpdateFormValue()
        {
            updateSettings?.Invoke();
        }

        
        private LoginStutus LoginEvent()
        {
            AdminLoginForm loginForm = new AdminLoginForm();
            loginForm.ShowDialog();
            if(loginForm.DialogResult==System.Windows.Forms.DialogResult.Yes)
            {
                return LoginStutus.SUCCESS;
            }    
            else return LoginStutus.FAIL;
        }

        public void Setting_Click()
        {
            if (LoginEvent() == LoginStutus.SUCCESS)
            {
                SettingsForm settingsForm = new SettingsForm();
                settingsForm.ShowDialog();
            }
            updateSettings?.Invoke();
        }

        System.Threading.Thread t;
        public void ScanClick()
        {
            settingValues = Settings.GetValue();
            t = new System.Threading.Thread(ScanClick_Action);
            t.Start();
        }
        
        void ScanClick_Action()
        {
            try
            {
                ScanCallback("Start", null);
                dut_Old_But_OK = new Models.Dut_old_But_OK();
                var allObject = GetAllObjFromLog(settingValues.LogPath);
                StaticGlobal.Logger("Start find olds mac!");
                var GetOlds = GetOldMacDutLogs(allObject);
                StaticGlobal.Logger(GetOlds.Count + " MACs");
                var NewFailDut = Compare(GetOlds);
                ScanCallback("Finish", new List<object>() { allObject.Count, GetOlds.Count, NewFailDut.Count, NewFailDut.Count > 0 });
                ScanCallback("NewList", NewFailDut);
            }
            catch(Exception ex)
            {
                System.Windows.Forms.MessageBox.Show("exception error: " + ex.ToString());
            }
        }
        public bool DetectedNew(List<DutLog> dutLogs)
        {
            ShowList showList = new ShowList(dutLogs);  
            if(showList.ShowDialog() == System.Windows.Forms.DialogResult.Yes)
            {
                DataWriting.Invoke();
                GetOkList.AddRange(dutLogs);
                return true;
            }
            return false;
        }
            
        public List<DutLog> Compare(List<DutLog> dutLogs)
        {
            List<DutLog> duts= new List<DutLog>();
            foreach(DutLog dutLog in dutLogs)
            {
                if(!GetOkList.CheckMacExist(dutLog))
                {
                    duts.Add(dutLog);
                }
            }
            return duts;
        }

        private List<DutLog> GetOldMacDutLogs(List<DutLog> allDutLogs)
        {
            List<DutLog> dutLogs = new List<DutLog>();
            foreach(DutLog item in allDutLogs)
            {
                if(rangeChecker.CheckOldMac(item))
                {
                    dutLogs.Add(item);  
                }
            }
            return dutLogs;
        }

        private List<DutLog> GetAllObjFromLog(string directory)
        {
            StaticGlobal.Logger("Start get all logs file: ");
            var pathLogs= StaticGlobal.SearchHelper.GetAllFiles(directory);
            SearchTool.SearchHelper.scanned = StaticGlobal.SearchHelper.listPathToPathMd5(pathLogs);
            StaticGlobal.Logger(pathLogs.Count+ " files!");
            StaticGlobal.Logger("Start get all mac in log file: ");
            List<DutLog> list = new List<DutLog>();
            foreach(var file in pathLogs)
            {
                SearchTool.ilogConveter ilogConveter = null;
                if (StaticGlobal.currentStaion == "setless") ilogConveter = new SearchTool.SetLess();
                else ilogConveter = new SearchTool.Pre_advertising();     
                var objs = StaticGlobal.SearchHelper.Log2DutLogObj(file, ilogConveter);
                if(objs != null)
                    list.AddRange(objs);
                System.Threading.Thread.Sleep(1);
            }
            StaticGlobal.Logger(list.Count+ " MACs");
            return list;
        }
    }
}
