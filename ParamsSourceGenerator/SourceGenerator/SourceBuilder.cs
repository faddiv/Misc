using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Formatting;
using System;
using System.Collections.Generic;
using System.Text;

namespace Foxy.Params.SourceGenerator
{
    internal class SourceBuilder
    {
        private readonly StringBuilder _builder = new StringBuilder();
        public string Intend { get; set; } = "    ";
        private int _intendLevel = 0;
        public Stack<string> _scope = new Stack<string>();

        public override string ToString()
        {
            return _builder.ToString();
        }
        public void Namespace(string name)
        {
            AddLineInternal($"namespace {name}");
            OpenBlock(name);
        }

        public void Class(string name)
        {
            AddLineInternal($"partial class {name}");
            OpenBlock(name);
        }

        public void GenericStruct(string name, string genericParam1)
        {
            AddLineInternal($"file struct {name}<{genericParam1}>");
            OpenBlock(name);
        }

        public void Constructor(IEnumerable<string> args)
        {
            AddIntend();
            string className = _scope.Peek();
            _builder.Append($"public {className}(");
            CommaSeparatedItemList(args);
            _builder.AppendLine(")");
            OpenBlock(className);
        }

        internal void Method(
            string name,
            IEnumerable<string> args,
            bool isStatic,
            string returnType,
            List<string> typeArguments,
            List<TypeConstrainInfo> typeConstraintsList)
        {
            AddIntend();
            _builder.Append("public");
            if (isStatic)
            {
                _builder.Append(" static");

            }
            _builder.Append(" ").Append(returnType);
            _builder.Append($" {name}");
            if (typeArguments.Count > 0)
            {
                _builder.Append($"<");
                CommaSeparatedItemList(typeArguments);
                _builder.Append($">");
            }
            _builder.Append($"(");
            CommaSeparatedItemList(args);
            _builder.AppendLine(")");
            AddTypeConstraints(typeConstraintsList);
            OpenBlock(name);
        }

        public void Attribute(string name)
        {
            AppendLine($"[global::{name}]");
        }

        public void Field(string type, string name)
        {
            AppendLine($"public {type} {name};");
        }

        public void AutoGenerated()
        {
            AddLineInternal("// <auto-generated />");
        }

        public void NullableEnable()
        {
            AddLineInternal("#nullable enable");
        }

        public void CloseBlock()
        {
            DecreaseIntend();
            AddLineInternal("}");
        }

        public void AppendLine()
        {
            _builder.AppendLine();
        }

        public void AppendLine(string text)
        {
            AddIntend();
            _builder.AppendLine(text);
        }

        public void OpenBlock(string scope)
        {
            AddLineInternal("{");
            IncreaseIntend(scope);
        }

        public void IncreaseIntend(string scope) {
            _scope.Push(scope);
            _intendLevel++;
        }
        public void DecreaseIntend() {
            _scope.Pop();
            _intendLevel--;
        }

        public SourceLine StartLine()
        {
            return new SourceLine(this);
        }

        public class SourceLine
        {
            private readonly SourceBuilder _builder;

            public SourceLine(SourceBuilder builder)
            {
                _builder = builder;
                _builder.AddIntend();
            }

            public void Returns()
            {
                _builder._builder.Append("return ");
            }

            public void AddSegment(string segment)
            {
                _builder._builder.Append(segment);
            }

            public void AddCommaSeparatedList(IEnumerable<string> elements)
            {
                _builder._builder.Append(string.Join(", ", elements));
            }

            public void EndLine() {
                _builder._builder.AppendLine(";");
            }
        }

        private void AddTypeConstraints(List<TypeConstrainInfo> typeConstraintsList)
        {
            if (typeConstraintsList.Count <= 0)
                return;

            IncreaseIntend(_scope.Peek());
            foreach (var typeConstraints in typeConstraintsList)
            {
                AddIntend();
                _builder.Append($"where {typeConstraints.Type} : ");
                CommaSeparatedItemList(typeConstraints.Constraints);
                _builder.AppendLine();
            }
            DecreaseIntend();
        }

        private void AddLineInternal(string text)
        {
            AddIntend();
            _builder.AppendLine(text);
        }

        private void AddIntend()
        {
            for (int i = 0; i < _intendLevel; i++)
            {
                _builder.Append(Intend);
            }
        }
        private void CommaSeparatedItemList(IEnumerable<string> args)
        {
            ItemList(", ", args);
        }
        private void ItemList(string separator, IEnumerable<string> args)
        {
            var more = false;
            foreach (var item in args)
            {
                if (more)
                {
                    _builder.Append(separator);
                }
                more = true;
                _builder.Append(item);
            }
        }

        internal void AddCompilationElement(ref SyntaxNode trivias)
        {
            var text = trivias.
                NormalizeWhitespace().ToFullString();
            foreach (var line in text.Split(new string[] { "\r\n" }, StringSplitOptions.None))
            {
                AddIntend();
                _builder.AppendLine(line);
            }
        }
        
    }
}

