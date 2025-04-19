namespace TestRunner;

internal class Service3Implementation(IService4 service4) : IService3
{
    public void CallHelloWorld()
    {
        service4.CallHelloWorld();
    }
}
