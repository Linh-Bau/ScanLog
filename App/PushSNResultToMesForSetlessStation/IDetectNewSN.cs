using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.PushSNResultToMesForSetlessStation
{
    public interface IDetectNewSN
    {
        Action<string,int> logCallBack { get; set; }
        void RunLoop();
        void CloseThread();
    }
}
