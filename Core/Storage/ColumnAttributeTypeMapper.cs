using Dapper;
using System;
using System.Collections.Generic;
using System.Data.Linq.Mapping;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Core.Storage
{
    internal class ColumnAttributeTypeMapper<T> : SqlMapper.ITypeMap
    {
        private readonly IEnumerable<SqlMapper.ITypeMap> _mappers;

        public ColumnAttributeTypeMapper()
        {
            _mappers = new SqlMapper.ITypeMap[]
            {
                new CustomPropertyTypeMap(
                   typeof(T),
                   (type, columnName) =>
                       type.GetProperties().FirstOrDefault(prop =>
                           prop.GetCustomAttributes(false)
                               .OfType<ColumnAttribute>()
                               .Any(attr => attr.Name == columnName)
                           )
                   ),
                new DefaultTypeMap(typeof(T))
            };
        }

        public SqlMapper.IMemberMap GetMember(string columnName)
        {
            foreach (var mapper in _mappers)
            {
                try
                {
                    var result = mapper.GetMember(columnName);
                    if (result != null)
                    {
                        return result;
                    }
                }
                catch (NotImplementedException nix)
                {
                    // the CustomPropertyTypeMap only supports a no-args
                    // constructor and throws a not implemented exception.
                    // to work around that, catch and ignore.
                }
            }
            return null;
        }
        // implement other interface methods similarly

        // required sometime after version 1.13 of dapper
        public ConstructorInfo FindExplicitConstructor()
        {
            return _mappers.Select(mapper => mapper.FindExplicitConstructor()).FirstOrDefault(result => result != null);
        }

        public ConstructorInfo FindConstructor(string[] names, Type[] types)
        {
            foreach (var mapper in _mappers)
            {
                try
                {
                    var result = mapper.FindConstructor(names, types);

                    if (result != null)
                    {
                        return result;
                    }
                }
                catch (NotImplementedException nix)
                {
                    // the CustomPropertyTypeMap only supports a no-args
                    // constructor and throws a not implemented exception.
                    // to work around that, catch and ignore.
                }
            }
            return null;
        }

        public SqlMapper.IMemberMap GetConstructorParameter(ConstructorInfo constructor, string columnName)
        {
            foreach (var mapper in _mappers)
            {
                try
                {
                    var result = mapper.GetConstructorParameter(constructor, columnName);

                    if (result != null)
                    {
                        return result;
                    }
                }
                catch (NotImplementedException nix)
                {
                    // the CustomPropertyTypeMap only supports a no-args
                    // constructor and throws a not implemented exception.
                    // to work around that, catch and ignore.
                }
            }
            return null;
        }
    }
}
