using BenchmarkDotNet.Running;

// Examples:
//   dotnet run -c Release --project benchmarks/SqlParsers.Benchmarks -- --filter *
//   dotnet run -c Release --project benchmarks/SqlParsers.Benchmarks -- --filter * --join
BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
