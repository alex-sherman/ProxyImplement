# ProxyImplement

A simple way to dynamically implement interfaces at run time in C#.

## Example Usage
```c#
public interface ITest
{
    int Add(int a, int b);
}
public class MyImplementation : ProxyImplement
{
    public override object Intercept(string methodName, object[] args)
    {
        return (int)args[0] + (int)args[1];
    }
}

[TestClass]
public class UnitTest1
{
    [TestMethod]
    public void TestAdd()
    {
        ITest proxy = ProxyImplement.HookUp<ITest, MyImplementation>();
        int result = proxy.Add(1, 2);
        Assert.AreEqual(result, 3);
        result = proxy.Add(4, 2);
        Assert.AreEqual(result, 6);
    }
}
```
