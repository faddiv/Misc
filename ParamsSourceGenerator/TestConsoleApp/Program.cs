using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Foxy.Params.SourceGenerator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace TestConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {

            string code = @"using Foxy.Params;

namespace Something;

public partial class Foo
{
    [Params(MaxOverrides = 1)]
    private static void Format(string format, ReadOnlySpan<object> args)
    {
    }
}
"; ;

            var (diagnostics, outputs) = GetGeneratedOutput(code);

            if (diagnostics.Length > 0)
            {
                Console.WriteLine("Diagnostics:");
                foreach (var diag in diagnostics)
                {
                    Console.WriteLine("   " + diag.ToString());
                }
                Console.WriteLine();
                Console.WriteLine("Output:");
            }

            foreach (var outp in outputs)
            {
                Console.WriteLine("FileName: {0}", outp.Item1);
                Console.WriteLine(outp.Item2);
            }
        }

        private static (ImmutableArray<Diagnostic>, ImmutableList<(string, string)>) GetGeneratedOutput(string source)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(source);

            var references = new List<MetadataReference>();
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                if (!assembly.IsDynamic)
                {
                    references.Add(MetadataReference.CreateFromFile(assembly.Location));
                }
            }

            var compilation = CSharpCompilation.Create("foo", new SyntaxTree[] { syntaxTree }, references, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            // TODO: Uncomment these lines if you want to return immediately if the injected program isn't valid _before_ running generators
            //
            // ImmutableArray<Diagnostic> compilationDiagnostics = compilation.GetDiagnostics();
            //
            // if (diagnostics.Any())
            // {
            //     return (diagnostics, "");
            // }

            var generator = new ParamsIncrementalGenerator();

            var driver = CSharpGeneratorDriver.Create(generator);
            driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var generateDiagnostics);

            var outputs = outputCompilation.SyntaxTrees
                .Select(e => (e.FilePath, e.ToString()))
                .ToImmutableList();
            return (generateDiagnostics, outputs);
        }
    }
}
