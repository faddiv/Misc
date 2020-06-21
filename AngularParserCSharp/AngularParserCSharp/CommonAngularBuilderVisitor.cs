using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;

namespace AngularParserCSharp
{
    public class CommonAngularBuilderVisitor : AngularParserBaseVisitor<Expression>
    {
        #region Fields

        private readonly Regex _escapeRecognizer = new Regex("\\\\(['\"nfrtv]|u[\\da-fA-F]{4})");
        private readonly ParameterExpression _helperParameter;
        private readonly ParameterExpression _scopeParameter;
        private readonly ParameterExpression _localsParameter;
        private readonly List<TryGetInfo> _tryGetInfos = new List<TryGetInfo>();
        private bool _expectStringIdentifier;
        private readonly Stack<ScopeInfo> _actualScope;

        #endregion

        #region  Constructors

        public CommonAngularBuilderVisitor(ParameterExpression scopeParameter, ParameterExpression localsParameter, ParameterExpression helperParameter)
        {
            _scopeParameter = scopeParameter;
            _localsParameter = localsParameter;
            _helperParameter = helperParameter;
            _actualScope = new Stack<ScopeInfo>();
            _actualScope.Push(new ScopeInfo(_scopeParameter));
        }

        #endregion

        private ScopeInfo CurrentScope => _actualScope.Peek();

        #region  Public Methods

        public override Expression VisitProgram(AngularParser.ProgramContext context)
        {
            _tryGetInfos.Clear();
            var visitProgram = Visit(context.GetChild(0));
            if (_tryGetInfos.Count == 0)
            {
                return visitProgram;
            }
            var variables = _tryGetInfos.Select(e => e.Variable).ToList();
            var operations = _tryGetInfos.Select(e => e.VariableInit).ToList();
            operations.Add(visitProgram);
            var block = Expression.Block(variables, operations);
            return block;
        }

        public override Expression VisitAssignment([NotNull] AngularParser.AssignmentContext context)
        {
            var primaries = context.primary();
            if (primaries.Length == 1)
                return Visit(primaries[0]);
            _actualScope.Push(new ScopeInfo(_actualScope.Peek(), true));
            var leftSide = Visit(primaries[0]);
            _actualScope.Pop();
            var rigthSide = Visit(primaries[1]);
            switch (leftSide.NodeType)
            {
                case ExpressionType.MemberAccess:
                case ExpressionType.Parameter:
                case ExpressionType.Assign:
                case ExpressionType.Index:
                case ExpressionType.RuntimeVariables:
                    rigthSide = ExpressionHelpers.CastIfDifferentType(rigthSide, leftSide.Type);
                    var assignment = Expression.Assign(leftSide, rigthSide);
                    return assignment;
                default:
                    throw new AngularException($"in an assignment the left side is not assignable to the right side. Location: {context.Start.Column}:{context.Start.Line} {context.GetText()}");
            }
        }

        public override Expression VisitPrimary(AngularParser.PrimaryContext context)
        {
            var primary = context.GetChild(0);
            var accessContext = context.afterPrimary();
            if (accessContext == null)
            {
                return Visit(primary);
            }
            _actualScope.Push(new ScopeInfo(CurrentScope, false));
            Expression resultExpression = Visit(primary);
            _actualScope.Pop();
            _actualScope.Push(new ScopeInfo(resultExpression, CurrentScope.AssignScope));
            resultExpression = Visit(accessContext);
            _actualScope.Pop();
            return resultExpression;
        }

        public override Expression VisitAfterPrimaryIdentifier(AngularParser.AfterPrimaryIdentifierContext context)
        {
            //TODO: What to do?
            var identifier = context.primary();
            if (identifier != null)
            {
                return Visit(identifier);
            }
            throw new Exception($"Unknown tree branch at {context.Start.Column}:{context.Start.Line} {context.GetText()}");
        }

        public override Expression VisitAfterPrimaryComputed(AngularParser.AfterPrimaryComputedContext context)
        {
            _actualScope.Push(new ScopeInfo(_scopeParameter));
            var computedExpression = Visit(context.primary());
            _actualScope.Pop();
            if (computedExpression == null)
                throw new Exception(
                    $"Unknown tree branch at {context.Start.Column}:{context.Start.Line} {context.GetText()}");
                return GetIdentifier(null, computedExpression);
        }

        public override Expression VisitArray(AngularParser.ArrayContext context)
        {
            var subExpressions = new List<Expression>();
            foreach (var primaryContext in context.assignment())
            {
                var expression = Visit(primaryContext);
                expression = ExpressionHelpers.CastIfDifferentType(expression, typeof(object));
                subExpressions.Add(expression);
            }
            var arrayExpression = Expression.NewArrayInit(typeof(object), subExpressions);
            return arrayExpression;
        }

        public override Expression VisitIdentifierToken(AngularParser.IdentifierTokenContext context)
        {
            var strValue = context.IDENTIFIER().GetText();
            if (_expectStringIdentifier)
                return Expression.Constant(strValue);
            object value;
            switch (strValue)
            {
                case "true":
                    value = true;
                    break;
                case "false":
                    value = false;
                    break;
                case "null":
                    value = null;
                    break;
                case "this":
                    return _scopeParameter;
                default:
                    return GetIdentifierCommon(strValue);
            }
            return Expression.Constant(value);
        }

        public ElementInit VisitKeyValuePair(AngularParser.NameValuePairContext context)
        {
            var keyExpression = VisitKey(context.GetChild<ParserRuleContext>(0));
            var valueExpression = Visit(context.assignment());
            valueExpression = ExpressionHelpers.CastIfDifferentType(valueExpression, typeof(object));
            var keyValueExpression = Expression.ElementInit(DictionaryAdd, keyExpression, valueExpression);
            return keyValueExpression;
        }

        public override Expression VisitNumberLiteral(AngularParser.NumberLiteralContext context)
        {
            var numberNode = context.NUMBER();
            var text = numberNode.GetText();
            try
            {
                if (_expectStringIdentifier)
                {
                    return Expression.Constant(text);
                }
                object value;
                double generalValue = double.Parse(text);
                if (Math.Abs(Math.Floor(generalValue) - generalValue) > double.Epsilon)
                {
                    value = generalValue;
                }
                else
                {
                    if (IsInRange(generalValue, byte.MinValue, byte.MaxValue))
                    {
                        value = (byte) generalValue;
                    } else if (IsInRange(generalValue, short.MinValue, short.MaxValue))
                    {
                        value = (short)generalValue;
                    }
                    else if (IsInRange(generalValue, int.MinValue, int.MaxValue))
                    {
                        value = (int) generalValue;
                    }
                    else
                    {
                        value = (long)generalValue;
                    }
                }
                return Expression.Constant(value);
            }
            catch (FormatException e)
            {
                throw new AngularException($"{CompileExcetion(context)} Unknown integer: {text}", e);
            }
        }

        private static bool IsInRange(double value, long minValue, long maxValue)
        {
            return minValue <= value && value <= maxValue;
        }

        public override Expression VisitObject(AngularParser.ObjectContext context)
        {
            var elements = context.nameValuePair().Select(VisitKeyValuePair).ToList();
            var newExpression = Expression.New(DictionaryConstruct);
            if (elements.Count == 0)
            {
                return newExpression;
            }
            var objectExpression = Expression.ListInit(newExpression, elements);
            return objectExpression;
        }


        public override Expression VisitStringLiteral(AngularParser.StringLiteralContext context)
        {
            var stringNode = context.STRING();
            var value = stringNode.GetText();
            var valueNoQuote = value.Substring(1, value.Length - 2);
            var escapedValue = Unescape(valueNoQuote);
            return Expression.Constant(escapedValue);
        }

        #endregion

        #region  Nonpublic Methods

        private string CompileExcetion(ParserRuleContext context)
        {
            return $"Compile exception at {context.Start.Line}:{context.Start.Column}";
        }

        private Expression GetIdentifierCommon(string identifierStr)
        {
            return GetIdentifier(identifierStr);
        }
        
        private Expression GetIdentifier(string identifier, Expression computed = null)
        {
            Type returnType;
            MemberInfo uniqueMember;
            if (identifier != null)
            {
                if (CurrentScope.IsJavascriptObject)
                {
                    uniqueMember = Indexer;
                    returnType = typeof(object);
                }
                else
                {
                    uniqueMember = GetPropertyOrFieldMember(identifier);
                    returnType = GetReturnType(uniqueMember);
                }
            }
            else
            {
                if(computed == null) throw new ArgumentNullException(nameof(computed));
                if (!CurrentScope.IsJavascriptObject)
                {
                    throw new AngularException($"Can't use computed property access for concrete type: {computed.Type.Name}");
                }
                var propertyInfo = CurrentScope.Type.GetProperty("Item", new []{ typeof(string) });
                uniqueMember = propertyInfo;
                returnType = propertyInfo.PropertyType;
            }

            if (CurrentScope.AssignScope)
            {
                var makeMemberAccess = Expression.MakeMemberAccess(CurrentScope.ScopeExpression, uniqueMember);
                return makeMemberAccess;
            }
            var selectValueMethodInfo =
                TypedJavascriptObjectHelper.SelectValueInfo.MakeGenericMethod(CurrentScope.ScopeExpression.Type,
                    returnType);
            var selectValueExpression = Expression.Call(_helperParameter, selectValueMethodInfo,
                CurrentScope.ScopeExpression,
                _localsParameter, Expression.Constant(identifier));
            var variable = Expression.Variable(returnType);
            var varibaleInit = Expression.Assign(variable, selectValueExpression);
            _tryGetInfos.Add(new TryGetInfo
            {
                Variable = variable,
                VariableInit = varibaleInit
            });
            return variable;
        }

        private MemberInfo GetPropertyOrFieldMember(string identifier)
        {
            var member = CurrentScope.ScopeExpression.Type.GetMember(identifier);
            if (member.Length == 0)
            {
                throw new AngularException($"No member {identifier} on {CurrentScope.Type.Name}");
            }
            if (member.Length > 1)
            {
                throw new AngularException($"There is no unique member with name {identifier} on {CurrentScope.ScopeExpression.Type.Name}");
            }

            MemberInfo uniqueMember = member[0];
            if (!(uniqueMember is PropertyInfo) && !(uniqueMember is FieldInfo))
            {
                throw new AngularException($"The member {identifier} of {CurrentScope.Type.Name} is not property or field so cant used as left side of assign");
            }

            return uniqueMember;
        }

        private static Type GetReturnType(MemberInfo memberInfo)
        {
            if (memberInfo is PropertyInfo propertyInfo)
            {
                return propertyInfo.PropertyType;
            }
            if (memberInfo is FieldInfo fieldInfo)
            {
                return fieldInfo.FieldType;
            }
            throw new AngularException($"Unknown type of member: {memberInfo.GetType().Name} Name: {memberInfo.Name}");
        }

        public override Expression VisitFunctionCall(AngularParser.FunctionCallContext context)
        {
            var callingContext = _actualScope.Peek();
            var functionName = context.identifier().GetText();

            MethodCallExpression callExpression;
            if (callingContext.ScopeExpression.Type == typeof(JavascriptObject) || callingContext.ScopeExpression.Type == typeof(object))
            {
                var delegateExpression = Expression.Call(_helperParameter, JavascriptObjectHelper.SelectFunctionInfo, callingContext.ScopeExpression,
                    Expression.Constant(null, typeof(object)), Expression.Constant(functionName));
                List<Expression> parameterExpressions = VisitParameters(context);
                CastAllParameter(parameterExpressions, typeof(object));
                var variables = Expression.NewArrayInit(typeof(object), parameterExpressions);
                callExpression = Expression.Call(delegateExpression, DynamicInvoke, variables);
            }
            else
            {
                var functionDelegate = callingContext.ScopeExpression.Type.GetMethod(functionName);
                if (functionDelegate == null)
                    throw new AngularException($"Unknown method: {callingContext.ScopeExpression.Type.Name}.{functionName}");
                List<Expression> parameterExpressions = VisitParameters(context);
                var requiredParameters = functionDelegate.GetParameters();
                if (!TryMatchParameters(parameterExpressions, requiredParameters))
                    throw new AngularException($"Parameterrs not matching the provided parameters for {callingContext.ScopeExpression.Type.Name}.{functionName} Parameters: {string.Join(", ", parameterExpressions.Select(e => e.Type.Name))}");
                callExpression = Expression.Call(callingContext.ScopeExpression, functionDelegate, parameterExpressions);
            }
            var variable = Expression.Variable(callExpression.Type);
            var varibaleInit = Expression.Assign(variable, callExpression);
            _tryGetInfos.Add(new TryGetInfo
            {
                Variable = variable,
                VariableInit = varibaleInit
            });
            return variable;
        }

        private void CastAllParameter(IList<Expression> parameterExpressions, Type desiredType)
        {
            for (int i = 0; i < parameterExpressions.Count; i++)
            {
                parameterExpressions[i] = ExpressionHelpers.CastIfDifferentType(parameterExpressions[i], desiredType);
            }
            
        }

        private bool TryMatchParameters(IList<Expression> parameterExpressions, IReadOnlyList<ParameterInfo> requiredParameters)
        {
            if(parameterExpressions.Count != requiredParameters.Count)
            return false;
            for (int i = 0; i < parameterExpressions.Count; i++)
            {
                var requiredParameter = requiredParameters[i];
                if (requiredParameter.ParameterType == parameterExpressions[i].Type)
                    continue;
                parameterExpressions[i] = Expression.Convert(parameterExpressions[i], requiredParameter.ParameterType);
            }
            return true;
        }

        private List<Expression> VisitParameters(AngularParser.FunctionCallContext context)
        {
            var parameterExpressions = new List<Expression>();
            var parameterElements = context.assignment();
            _actualScope.Push(new ScopeInfo(_scopeParameter));
            foreach (var parameterElement in parameterElements)
            {
                var parameterExpression = Visit(parameterElement);
                parameterExpressions.Add(parameterExpression);
            }
            _actualScope.Pop();
            return parameterExpressions;
        }

        private string Unescape(string value)
        {
            var newValue = _escapeRecognizer.Replace(value, match =>
            {
                var str = match.Groups[1].Value;
                switch (str[0])
                {
                    case 'n':
                        return "\n";
                    case 'f':
                        return "\f";
                    case 'r':
                        return "\r";
                    case 't':
                        return "\t";
                    case 'v':
                        return "\v";
                    case 'u':
                        var hexLow = str.Substring(1, 2);
                        var hexHigh = str.Substring(3, 2);
                        var bytes = new[] { Convert.ToByte(hexHigh, 16), Convert.ToByte(hexLow, 16) };
                        return Encoding.Unicode.GetString(bytes);
                }
                return str;
            });
            return newValue;
        }

        private ConstantExpression VisitKey(ParserRuleContext constant)
        {
            _expectStringIdentifier = true;
            try
            {
                var name = Visit(constant);
                return (ConstantExpression)name;
            }
            finally
            {
                _expectStringIdentifier = false;
            }
        }

        #endregion

        private static readonly MethodInfo DictionaryAdd =
            typeof(JavascriptObject).GetMethod(nameof(JavascriptObject.Add), new[] { typeof(string), typeof(object) });

        private static readonly PropertyInfo Indexer =
            typeof(JavascriptObject).GetProperty("Item", new[] { typeof(string) });

        private static readonly ConstructorInfo DictionaryConstruct =
            typeof(JavascriptObject).GetConstructor(Type.EmptyTypes);
        
        private static readonly MethodInfo DynamicInvoke =
            typeof(Delegate).GetMethod(nameof(Delegate.DynamicInvoke));
    }
}