using System.CommandLine;
using PackageCleaner;

return CommandLineApp.Build(Console.Out, Console.In, !Console.IsInputRedirected).Parse(args).Invoke();
