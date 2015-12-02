using System;
using System.Collections.Generic;
using System.Reflection;

namespace OneHydra.Common.Utilities.Extensions
{
    public static class ObjectExtensions
    {

        #region Fields

        private static Dictionary<string, MethodInfo> _forSqlMethods;
        private static Dictionary<string, Dictionary<string, PropertyInfo>> _propertiesByType;

        #endregion Fields

        #region Method

        public static T CastTo<T>(this object obj)
        {
            try
            {
                return (T)obj;
            }
            catch (Exception)
            {
                throw new InvalidCastException("Unable to cast " + obj.GetType().Name + " to " + typeof(T).Name);
            }
        }

        public static List<T> GetListOfAnonymousType<T>(this T exampleOfType)
        {
            return new List<T>();
        }

        public static string ForSql(this object obj)
        {
            var forSqlMethods = GetForSqlMethods();
            var thisType = obj.GetType();
            string returnValue;
            if (thisType.FullName != null)
            {
                if (forSqlMethods.ContainsKey(thisType.FullName))
                {
                    var forSqlMethod = forSqlMethods[thisType.FullName];
                    returnValue = forSqlMethod.Invoke(obj, new[] { obj }).CastTo<string>();
                }
                else
                {
                    throw (new Exception("ForSql method is not defined for " + thisType.FullName + ".  Please add an extension method for this type."));
                }
            }
            else
            {
                throw (new Exception("ForSql method is not defined for " + thisType.FullName + ".  Please add an extension method for this type."));
            }
            return returnValue;
        }

        private static Dictionary<string, MethodInfo> GetForSqlMethods()
        {
            if (_forSqlMethods == null)
            {
                _forSqlMethods = new Dictionary<string, MethodInfo>();
                var assembly = Assembly.GetAssembly(typeof(ObjectExtensions));
                var types = assembly.GetTypes();
                foreach (var type in types)
                {
                    if (type.Name.EndsWith("Extensions"))
                    {
                        var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Static);
                        foreach (var method in methods)
                        {
                            if (method.Name == "ForSql")
                            {
                                var param = method.GetParameters()[0];
                                if (param.ParameterType.FullName != null)
                                {
                                    if (!_forSqlMethods.ContainsKey(param.ParameterType.FullName))
                                    {
                                        _forSqlMethods.Add(param.ParameterType.FullName, method);
                                    }
                                }
                            }
                        }
                    }
                }

            }
            return _forSqlMethods;
        }

        public static void SetPropertyValue<T>(this T theObject, Type typeOfT, string propertyName, object value)
        {
            PropertyInfo property;
            if (_propertiesByType == null)
            {
                _propertiesByType = new Dictionary<string, Dictionary<string, PropertyInfo>>();
            }
            if (_propertiesByType.ContainsKey(typeOfT.FullName))
            {
                var properties = _propertiesByType[typeOfT.FullName];
                if (properties.ContainsKey(propertyName))
                {
                    property = properties[propertyName];
                }
                else
                {
                    property = typeOfT.GetProperty(propertyName);
                    properties.Add(propertyName, property);
                }
            }
            else
            {
                property = typeOfT.GetProperty(propertyName);
                var propertiesDict = new Dictionary<string, PropertyInfo>{{property.Name, property}};
                _propertiesByType.Add(typeOfT.FullName, propertiesDict);
            }
            property.SetValue(theObject, value);
        }

        #endregion Methods
    }
}