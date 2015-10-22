// Author: Joël Luijmes
//
//
// Based on: http://rogeralsing.com/2008/02/28/linq-expressions-creating-objects/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace NetworkingLibrary
{
    internal static class Creator<T>
    {
        private static object _lock = new object();
        private static Dictionary<Type, ObjectCreator> _factories = new Dictionary<Type, ObjectCreator>();

        public delegate T ObjectCreator(params object[] args);

        public static ObjectCreator GetCreator()
        {
            if (_factories.ContainsKey(typeof(T)))
                return _factories[typeof(T)];

            lock (_lock)
            {
                if (_factories.ContainsKey(typeof(T)))
                    return _factories[typeof(T)];

                var ctor = typeof(T).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance).First();

                var type = ctor.DeclaringType;
                var parameters = ctor.GetParameters();
                var parameter = Expression.Parameter(typeof(object[]), "args");

                var argumentExpression = new Expression[parameters.Length];
                for (var i = 0; i < argumentExpression.Length; ++i)
                {
                    var index = Expression.Constant(i);
                    var paramType = parameters[i].ParameterType;

                    var paramAccessorExpression = Expression.ArrayIndex(parameter, index);
                    var paramCastExpression = Expression.Convert(paramAccessorExpression, paramType);

                    argumentExpression[i] = paramCastExpression;
                }

                var newExpression = Expression.New(ctor, argumentExpression);
                var lambda = Expression.Lambda(typeof(ObjectCreator), newExpression, parameter);
                var creator = (ObjectCreator)lambda.Compile();

                _factories[typeof(T)] = creator;

                return creator;
            }
        }
    }
}
