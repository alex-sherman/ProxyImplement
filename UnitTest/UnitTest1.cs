using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using InterfaceProxy;

namespace UnitTest
{
    public interface ITest
    {
        int Add(int a, int b);
        string Format(string derp, int second);
        void JustAVoid();
    }
    public class MyImplementation : IImplementor
    {
        public bool calledSomething = false;
        public object Intercept(string methodName, object[] args)
        {
            calledSomething = true;
            switch (methodName)
            {
                case "Add":
                    return (int)args[0] + (int)args[1];
                case "Format":
                    return string.Format((string)args[0], args[1]);
                default:
                    return null;
            }
        }
    }
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestAdd()
        {
            var derp = ProxyImplement.HookUp<ITest, MyImplementation>(new MyImplementation());
            var result = derp.Add(1, 2);
            Assert.AreEqual(result, 3);
            result = derp.Add(4, 2);
            Assert.AreEqual(result, 6);
        }
        [TestMethod]
        public void TestString()
        {
            var derp = ProxyImplement.HookUp<ITest, MyImplementation>(new MyImplementation());
            Assert.AreEqual(derp.Format("pass{0}interface", 4), "pass4interface");
        }
        [TestMethod]
        public void TestVoid()
        {
            var impl = new MyImplementation();
            var derp = ProxyImplement.HookUp<ITest, MyImplementation>(impl);
            derp.JustAVoid();
            Assert.IsTrue(impl.calledSomething);
        }
    }
}
