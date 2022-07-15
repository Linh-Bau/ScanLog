using NUnit.Framework;

namespace TestProject1
{
    using App.Models;
    using App;
    public class Test
    {

        Dut_old_But_OK dut_Old_But_OK;
        DutLog dut_Log_ok;
        DutLog dut_log_not_ok;

        [SetUp]
        public void Setup()
        {
            dut_Old_But_OK = new Dut_old_But_OK();
            dut_Log_ok = new DutLog() { MAC = "ok", LogPath = "na" };
            dut_log_not_ok = new DutLog() { MAC = "not_ok", LogPath = "na" };
        }

        [Test]
        public void Add_dut_log_Expected_is_false()
        {
            bool rs= dut_Old_But_OK.Add(dut_Log_ok);
            Assert.IsFalse(rs);
        }

        [Test]
        public void Check_Expected_is_true()
        {
            bool rs=dut_Old_But_OK.CheckMacExist(dut_Log_ok);
            Assert.IsTrue(rs);
        }

        [Test]
        public void Check_Expected_is_false()
        {
            bool rs = dut_Old_But_OK.CheckMacExist(dut_log_not_ok);
            Assert.IsFalse(rs);
        }
    }
}