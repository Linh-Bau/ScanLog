using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace App
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }



    //public class MultiFormContext : ApplicationContext
    //{
    //    private int openForms;
    //    private Form mesForm;
    //    public MultiFormContext(params Form[] forms)
    //    {
    //        closeAppBefore();
    //        openForms = forms.Length;
    //        mesForm = forms[0];
    //        foreach (var form in forms)
    //        {
    //            form.Show();
    //        }
    //    }

    //    private void closeAppBefore()
    //    {
    //        var process = Process.GetProcessesByName("App");
    //        foreach(Process p in process)
    //        {
    //            if(p.Id!=Process.GetCurrentProcess().Id)
    //            {
    //                p.Kill();
    //            }
    //        }
    //    }
    //}
}
