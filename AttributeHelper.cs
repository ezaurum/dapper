using System;
using System.Linq;
using System.Reflection;

namespace Ezaurum.Dapper
{
    public static class AttributeHelper
    {
        public static bool HasAttribute(this PropertyInfo propertyInfo, Type type)
        {
            return propertyInfo.GetCustomAttributes(true).Any(attr => attr.GetType() == type);
        }

        public static bool HasAttribute<T>(this PropertyInfo propertyInfo)
        {
            return HasAttribute(propertyInfo, typeof(T));
        }
    }
}