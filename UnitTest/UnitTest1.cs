using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using InterfaceProxy;

namespace UnitTest
{
    public interface ITest
    {
        int Add(int a, int b);
    }
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var derp = ProxyImplement.HookUp<ITest>();
            var result = derp.Add(1, 2);
            Assert.AreEqual(result, 3);
            result = derp.Add(4, 2);
            Assert.AreEqual(result, 6);
        }
    }
}
