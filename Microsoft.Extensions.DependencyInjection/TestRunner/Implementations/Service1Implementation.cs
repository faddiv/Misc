namespace TestRunner;

internal class Service1Implementation(IService2 service2, IService3 service3) : IService1
{
    public void CallHelloWorld()
    {
        service2.CallHelloWorld();
        service3.CallHelloWorld();
    }
}
