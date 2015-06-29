using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Text.RegularExpressions;

namespace ServeRick.Compiling
{
    public class TypeFactory
    {
        private static Regex NameParser = new Regex(@"
(?<name> [^`]+)
(                       #optional arguments part
    `(?<argcount> \d+)
    \[
        (
            (?<parameter> 
                (
                    [^\[\]] |   
                    (?<depth> \[) | 
                    (?<-depth> \])
                )+
            ) 
            ,?          #parameters delimiter
        )+
    \]
)?
", RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

        public static Type ResolveType(string typeName)
        {
            var typeMatch = NameParser.Match(typeName);

            var grps=typeMatch.Groups;

            var name=grps["name"].Value;
            var argCount = grps["argcount"].Value;
            var parameters=grps["parameter"].Captures;

            var paramTypes = new List<Type>();
            for (int i = 0; i < parameters.Count; ++i)
            {
                var parameter = parameters[i].Value;
                var resolvedParameter = ResolveType(parameter);
                if (resolvedParameter == null)
                    throw new KeyNotFoundException("Cannot resolve type: '" + parameter+"'. Make sure it is a Fullname.");

                paramTypes.Add(resolvedParameter);
            }

            if (paramTypes.Count > 0)
            {
                name = string.Format("{0}`{1}", name, paramTypes.Count);
            }

            var type=ResolveFlatType(name);

            if (type == null)
                return null;

            if (type.IsGenericTypeDefinition)
            {
                type = type.MakeGenericType(paramTypes.ToArray());
            }
            return type;
        }

        public static Type ResolveFlatType(string typeName)
        {
            Type resolvedType = null;
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                resolvedType = assembly.GetType(typeName);
                if (resolvedType != null)
                    break;
            }

            return resolvedType;
        }

    }
}
