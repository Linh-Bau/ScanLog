using NUnit.Framework;
using System.Collections.Generic;
namespace TestProject1.SearchTool
{
    using App;
    using App.SearchTool;
    public class SearchHelper_Tester
    {
        string test_get_all_file;
        string test_Log2DutLogObj;
        [SetUp]
        public void SetUp()
        {
            test_get_all_file = @"D:\Log file";
            test_Log2DutLogObj = @"D:\Log file\820_BLE2713.csv";
        }
        [Test]
        public void Get_All_File_Test()
        {
            List<string> list = SearchHelper.GetAllFiles(test_get_all_file);
            bool rs = list.Count > 1000;
            Assert.IsTrue(rs);
        }
        [Test]
        public void Log2DutLogObj_Test()
        {
            var list = SearchHelper.Log2DutLogObj(test_Log2DutLogObj, new Pre_advertising());
            bool rs = list.Count == 5;
            Assert.IsTrue(rs);
        }

        [Test]
        public void AllFileToObj()
        {
            List<string> path_List = SearchHelper.GetAllFiles(test_get_all_file);
            List<DutLog> obj_List = new List<DutLog>();
            foreach (string file in path_List)
            {
                var l = SearchHelper.Log2DutLogObj(file, new Pre_advertising());
                obj_List.AddRange(l);
            }
            bool rs = obj_List.Count > 10000;
            Assert.IsTrue(rs);
        }
    }
}
