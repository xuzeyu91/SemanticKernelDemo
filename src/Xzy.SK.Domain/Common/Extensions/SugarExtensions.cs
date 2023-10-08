using NPOI.Util;
using SqlSugar;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Xzy.SK.Domain
{
    public static class SugarExtensions
    {

        /// <summary>
        /// 兼容旧的查询参数(尽量使用新的语法,参照下面的Url地址)
        /// 新语法地址:https://www.donet5.com/Home/Doc?typeId=2314
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="queryable"></param>
        /// <param name="conditions">查询条件</param>
        /// <returns></returns>
        public static ISugarQueryable<T> Where<T>(this ISugarQueryable<T> queryable, List<Condition> conditions)
        {
            if (conditions == null || conditions.Count == 0)
            {
                return queryable;
            }
            IEnumerable<Condition> source2 = conditions.OrderBy(g => g.Prefix);
            IEnumerable<IGrouping<int, Condition>> enumerable = source2.GroupBy(g => g.Prefix);
            foreach (var condition in enumerable)
            {
                queryable = queryable.WhereUsingConditions(condition);
            }
            return queryable;
        }


        #region 辅助方法
        private static ISugarQueryable<T> WhereUsingConditions<T>(this ISugarQueryable<T> queryable, IGrouping<int, Condition> conditions)
        {
            Expression expression = null;
            ParameterExpression parameterExpression = Expression.Parameter(typeof(T), "TEntity");
            foreach (Condition condition in conditions)
            {
                if (!string.IsNullOrEmpty(Convert.ToString(condition.Value)))
                {
                    if (expression != null)
                    {
                        expression = ((condition.Relation == Relation.And) ? Expression.AndAlso(expression, ConditonToExpression<T>(condition, parameterExpression)) : Expression.OrElse(expression, ConditonToExpression<T>(condition, parameterExpression)));
                    }
                    else
                    {
                        expression = ConditonToExpression<T>(condition, parameterExpression);
                    }
                }
            }
            if (expression == null)
            {
                return queryable;
            }


            return queryable.Where(Expression.Lambda<Func<T, bool>>(expression, new ParameterExpression[]
            {
                parameterExpression
            }));


        }
        private static Expression ConditonToExpression<T>(Condition condition, Expression parameter)
        {
            Expression result = null;
            Type typeFromHandle = typeof(T);
            PropertyInfo property = typeFromHandle.GetProperty(condition.Field);
            Expression expression = Expression.Property(parameter, property);
            ICollection collection = condition.Value as ICollection;
            Expression expression2;
            if (collection == null)
            {
                expression2 = Expression.ConvertChecked(Expression.Constant(ChangeType(condition.Value, property.PropertyType)), property.PropertyType);
                if (property.PropertyType == typeof(string))
                {
                    expression2 = Expression.Constant(condition.Value, property.PropertyType);
                }
            }
            else
            {
                IList list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(new Type[]
                {
                    property.PropertyType
                }));
                foreach (object value in collection)
                {
                    list.Add(Convert.ChangeType(value, property.PropertyType));
                }
                expression2 = Expression.Constant(list, list.GetType());
            }
            switch (condition.Operator)
            {
                case Operator.Equal:
                    result = Expression.Equal(expression, expression2);
                    break;
                case Operator.NotEqual:
                    result = Expression.NotEqual(expression, expression2);
                    break;
                case Operator.LessThan:
                    result = Expression.LessThan(expression, expression2);
                    break;
                case Operator.LessThanOrEqual:
                    result = Expression.LessThanOrEqual(expression, expression2);
                    break;
                case Operator.GreaterThan:
                    result = Expression.GreaterThan(expression, expression2);
                    break;
                case Operator.GreaterThanOrEqual:
                    result = Expression.GreaterThanOrEqual(expression, expression2);
                    break;
                case Operator.Like:
                    result = GenerateLike(expression, expression2);
                    break;
                case Operator.IN:
                    result = GenerateIn(expression2, expression);
                    break;
                case Operator.NotIN:
                    result = GenerateNotIn(expression2, expression);
                    break;
            }
            return result;
        }

        private static Expression GenerateIn(Expression left, Expression right)
        {
            MethodInfo method = left.Type.GetMethod("Contains");
            return Expression.Call(left, method, new Expression[]
            {
                right
            });
        }

        private static Expression GenerateNotIn(Expression left, Expression right)
        {
            MethodInfo method = left.Type.GetMethod("Contains");
            return Expression.Not(Expression.Call(left, method, new Expression[]
            {
                right
            }));
        }

        
        private static Expression GenerateLike(Expression left, Expression right)
        {
            MethodInfo method = left.Type.GetMethod("Contains");
            return Expression.Call(left, method, new Expression[]
            {
                right
            });
        }

      
        private static object ChangeType(object value, Type conversionType)
        {
            if (value.ToString().Trim() == string.Empty && (typeof(int) == conversionType || typeof(short) == conversionType || typeof(long) == conversionType))
            {
                return null;
            }
            if (conversionType.IsGenericType && conversionType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {
                if (value.IsNull())
                {
                    return null;
                }
                NullableConverter nullableConverter = new NullableConverter(conversionType);
                conversionType = nullableConverter.UnderlyingType;
            }
            else if (conversionType == typeof(Guid))
            {
                TypeConverter converter = TypeDescriptor.GetConverter(typeof(Guid));
                return (Guid)converter.ConvertFrom(value);
            }
            return Convert.ChangeType(value, conversionType);
        }



        #endregion


    }

    #region 查询条件

    public class SearchCondition
    {
        public SearchCondition()
        { }

        public List<Condition> Conditions { get; set; }
        public string SortExpression { get; set; }
    }

    public class Condition
    {
        public Condition()
        {

        }

        public string Field { get; set; }
        public Operator Operator { get; set; }
        public int Prefix { get; set; }
        public Relation Relation { get; set; }
        public object Value { get; set; }
    }

    public enum Relation
    {
        And = 0,
        Or = 1,
    }

    public enum Operator
    {
        Equal = 0,
        NotEqual = 1,
        LessThan = 2,
        LessThanOrEqual = 3,
        GreaterThan = 4,
        GreaterThanOrEqual = 5,
        Like = 6,
        IN = 7,
        NotIN = 8,
    }
    #endregion
}
