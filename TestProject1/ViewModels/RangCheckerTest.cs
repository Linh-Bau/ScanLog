using NUnit.Framework;

namespace TestProject1.ViewModels
{
    using App;
    using App.ViewModels; 
    public class RangCheckerTest
    {
        RangeChecker rangeChecker;
        DutLog dutLog;

        [SetUp]
        public void SetUp()
        {
            rangeChecker = new RangeChecker();
        }

        [Test]
        public void CheckOldMac_1_Expected_False()
        {
            dutLog = new DutLog() { MAC = "BC:06:2C:90:00:00", LogPath = "NA" };
            var rs = rangeChecker.CheckOldMac(dutLog);
            Assert.IsFalse(rs);
        }
        [Test]
        public void CheckOldMac_2_Expected_True()
        {
            dutLog = new DutLog() { MAC = "BC:06:2D:90:10:00", LogPath = "NA" };
            var rs = rangeChecker.CheckOldMac(dutLog);
            Assert.IsTrue(rs);
        }
        [Test]
        public void CheckOldMac_3_Expected_False()
        {
            dutLog = new DutLog() { MAC = "BC:06:2D:F0:4F:E5", LogPath = "NA" };
            var rs = rangeChecker.CheckOldMac(dutLog);
            Assert.IsFalse(rs);
        }
        [Test]
        public void CheckOldMac_4_Expected_True()
        {
            dutLog = new DutLog() { MAC = "BC:06:2D:90:10:00", LogPath = "NA" };
            var rs = rangeChecker.CheckOldMac(dutLog);
            Assert.IsTrue(rs);
        }
    }
}
