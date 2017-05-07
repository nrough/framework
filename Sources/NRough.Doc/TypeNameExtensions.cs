using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace NRough.Doc
{
    public static class TypeExtensions
    {
        public static bool IsDelegate(this Type type)
        {
            return type.BaseType == typeof(MulticastDelegate);
        }

        public static bool IsCompilerGenerated(this Type type)
        {
            return Attribute.GetCustomAttribute(type, typeof(CompilerGeneratedAttribute)) != null;
        }

        public static bool IsEnabled(this Type type)
        {
            var attribute = (AssemblyTreeVisibleAttribute) Attribute.GetCustomAttribute(
                type, typeof(AssemblyTreeVisibleAttribute));

            if (attribute != null)
                return attribute.IsVisible;

            return type.IsVisible;
        }

        public static string GetFriendlyName(this Type type)
        {
            string friendlyName = type.Name;
            if (type.IsGenericType)
            {
                int iBacktick = friendlyName.IndexOf('`');
                if (iBacktick > 0)
                {
                    friendlyName = friendlyName.Remove(iBacktick);
                }
                friendlyName += "<";
                Type[] typeParameters = type.GetGenericArguments();
                for (int i = 0; i < typeParameters.Length; ++i)
                {
                    string typeParamName = typeParameters[i].Name;
                    friendlyName += (i == 0 ? typeParamName : "," + typeParamName);
                }
                friendlyName += ">";
            }

            return friendlyName;
        }

        public static IEnumerable<T> ToIEnumerable<T>(this IEnumerator<T> enumerator)
        {
            while (enumerator.MoveNext())
                yield return enumerator.Current;
        }
    }
}
