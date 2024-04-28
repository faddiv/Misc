using System;

namespace ConsoleApp
{
    partial class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Types in this assembly:");
            foreach (Type t in typeof(Program).Assembly.GetTypes())
            {
                Console.WriteLine(t.FullName);
            }
        }

        [Foxy.Params.Params(MaxOverrides = 1)]
        public static void Format(Exception ex, string s, ReadOnlySpan<object> span)
        {

        }
    }
}
