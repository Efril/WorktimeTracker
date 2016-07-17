using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Core.Storage
{
    internal static class TypeMapper
    {
        public static void Initialize(string @namespace, Assembly Assembly)
        {
            IEnumerable<Type> types = from type in Assembly.GetTypes()
                                      where type.IsClass && type.Namespace == @namespace && type.IsDefined(typeof(DapperTypeAttribute), true)
                                      select type;

            foreach (Type type in types)
            {
                var mapper = (SqlMapper.ITypeMap)Activator.CreateInstance(typeof(ColumnAttributeTypeMapper<>).MakeGenericType(type));
                SqlMapper.SetTypeMap(type, mapper);
            }
        }
        public static void Initialize(string @namespace)
        {
            Initialize(@namespace, Assembly.GetExecutingAssembly());
        }
    }
}
