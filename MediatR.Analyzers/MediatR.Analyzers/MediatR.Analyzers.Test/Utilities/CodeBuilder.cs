using System.Collections.Generic;
using System.Text;

namespace Mediatr.Analyzers.Test.Utilities
{
    public class CodeBuilder
    {
        private readonly List<string> _codes = new List<string>();

        public bool MediatRIncluded { get; private set; }
        public bool DefaultUsingsIncluded { get; private set; }
        public IReadOnlyList<string> Codes { get => _codes; }
        public string DefaultNamespace { get; private set; }
        public static CodeBuilder WithDefaults()
        {
            return new CodeBuilder()
                .AddMediatR()
                .AddDefaultUsings()
                .SetDefaultNamespace("Test");
        }

        public CodeBuilder SetDefaultNamespace(string @namespace)
        {
            DefaultNamespace = @namespace;
            return this;
        }

        public CodeBuilder AddMediatR()
        {
            MediatRIncluded = true;
            return this;
        }

        public CodeBuilder AddDefaultUsings()
        {
            DefaultUsingsIncluded = true;
            return this;
        }

        public CodeBuilder AddCode(string source)
        {
            AddCode(DefaultNamespace, source);
            return this;
        }

        public CodeBuilder AddCode(string @namespace, string source)
        {
            var builder = new StringBuilder();
            
            if (!string.IsNullOrEmpty(@namespace))
            {
                builder.AppendLine($"namespace {@namespace} {{");
            }
            if (DefaultUsingsIncluded)
            {
                builder.AppendLine(MediatRCode.defaultNamespaces);
            }
            builder.AppendLine(source);
            if (!string.IsNullOrEmpty(@namespace))
            {
                builder.AppendLine($"}}");
            }
            _codes.Add(builder.ToString());
            return this;
        }

        public override string ToString()
        {
            StringBuilder str = new StringBuilder();
            foreach (var item in _codes)
            {
                str.AppendLine(item);
            }
            if (MediatRIncluded)
            {
                str.AppendLine(MediatRCode.Source);
            }
            return str.ToString();
        }
    }
}
