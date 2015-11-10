using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using CsmMagic.Attributes;
using CsmMagic.Models;

namespace CsmMagic.Queries
{
    //Adapted from https://github.com/clamidity/ZuoraMagic/blob/master/ZuoraMagic/LinqProvider/ZOQLVisitor.cs
    public static class CsmVisitor
    {
        internal static IEnumerable<ParsedExpression> GetQueries(Expression predicate)
        {
            var results = new List<ParsedExpression>();
            switch (predicate.NodeType)
            {
                case ExpressionType.Lambda:
                    return GetQueries(((LambdaExpression)predicate).Body);
                case ExpressionType.AndAlso:
                    results.AddRange(GetMultipleExpressions(predicate));
                    break;
                case ExpressionType.OrElse:
                    results.AddRange(GetMultipleExpressions(predicate));
                    results.ForEach(pe => pe.IsOr = true);
                    break;
                default:
                    results.Add(ParseExpression(predicate));
                    break;
            }
            return results;
        }

        private static IEnumerable<ParsedExpression> GetMultipleExpressions(Expression expression)
        {
            var binaryExpression = (BinaryExpression)expression;
            var leftSide = binaryExpression.Left;
            var rightSide = binaryExpression.Right;
            yield return ParseExpression(leftSide);
            yield return ParseExpression(rightSide);
        } 

        private static ParsedExpression ParseExpression(Expression expression)
        {
            return VisitExpression(expression);
        }

        private static ParsedExpression VisitExpression(Expression expression, bool valueExpression = false)
        {
            switch (expression.NodeType)
            {
                case ExpressionType.Not:
                    return VisitExpression(Expression.NotEqual(((UnaryExpression)expression).Operand, Expression.Constant(true)));
                case ExpressionType.GreaterThanOrEqual:
                    return VisitBinary(expression as BinaryExpression, CsmQueryOperator.GreaterThanEqual);
                case ExpressionType.LessThanOrEqual:
                    return VisitBinary(expression as BinaryExpression, CsmQueryOperator.LessThanEqual);
                case ExpressionType.LessThan:
                    return VisitBinary(expression as BinaryExpression, CsmQueryOperator.LessThan);
                case ExpressionType.GreaterThan:
                    return VisitBinary(expression as BinaryExpression, CsmQueryOperator.GreaterThan);
                case ExpressionType.Equal:
                    return VisitBinary(expression as BinaryExpression, CsmQueryOperator.Equal);
                case ExpressionType.NotEqual:
                    return VisitBinary(expression as BinaryExpression, CsmQueryOperator.NotEqual);
                case ExpressionType.Lambda:
                    return VisitLambda(expression as LambdaExpression);
                case ExpressionType.Convert:
                    return VisitExpression((expression as UnaryExpression).Operand);
                case ExpressionType.Constant:
                    return VisitConstant(expression as ConstantExpression);
                case ExpressionType.MemberAccess:
                    return VisitMember(expression as MemberExpression, valueExpression);
                default:
                    throw new NotSupportedException(string.Format("The query expression type {0} is not supported", expression.NodeType));
            }
        }

        private static ParsedExpression VisitBinary(BinaryExpression node, CsmQueryOperator opr)
        {
            var pe = new ParsedExpression
            {
                Property = VisitExpression(node.Left).Property,
                Value = VisitExpression(node.Right, true).Value,
                Operator = opr
            };
            return pe;
        }

        internal static string GetName(this PropertyInfo info)
        {
            return info.GetCustomAttribute<FieldAttribute>() != null
                ? info.GetCustomAttribute<FieldAttribute>().Name ?? info.Name
                : info.Name;
        }

        private static ParsedExpression VisitMember(MemberExpression node, bool valueExpression = false)
        {
            var pe = new ParsedExpression();
            if (node.Member is PropertyInfo && !valueExpression)
            {
                if (node.Expression is MemberExpression)
                {
                    pe.Property = ((MemberExpression)node.Expression).Member.Name + "." + ((PropertyInfo)node.Member).GetName();
                }
                else
                {
                    pe.Property = ((PropertyInfo)node.Member).GetName();    
                }
                
                return pe;
            }

            if (node.Expression is ConstantExpression)
            {
                var memConst = (ConstantExpression)node.Expression;
                pe.Value = ((FieldInfo)node.Member).GetValue(memConst.Value).ToString();

                return pe;
            }
            
            var memberConst = (MemberExpression)node.Expression;
            var captureConst = (ConstantExpression)memberConst.Expression;
            var fieldValue = ((FieldInfo) memberConst.Member).GetValue(captureConst.Value);
            var propValue = ((PropertyInfo) node.Member).GetValue(fieldValue);

            if (propValue == null)
            {
                throw new NotSupportedException("Null is not supported at this time");
            }

            pe.Value = propValue.ToString();

            return pe;
        }

        private static ParsedExpression VisitConstant(ConstantExpression node)
        {
            return new ParsedExpression {Value = node.Value == null ? null : node.Value.ToString()};
        }
        
        private static ParsedExpression VisitLambda(LambdaExpression node)
        {
            return VisitExpression(node.Body);
        }
    }
}
