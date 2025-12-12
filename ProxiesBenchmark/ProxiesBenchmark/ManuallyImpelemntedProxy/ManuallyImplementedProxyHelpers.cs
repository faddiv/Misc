namespace ProxiesBenchmark.ManuallyImpelemntedProxy
{
    public class ManuallyImplementedProxyHelpers
    {
        public static ICalculator DecorateSimple(ICalculator calculator)
        {
            return new CalculatorInterceptor(calculator);
        }
    }
}
