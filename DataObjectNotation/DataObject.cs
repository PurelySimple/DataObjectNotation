using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DataObjectNotation
{
    public class DataObject
    {
        public string Name { get; set; }
        public Dictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();
        public List<DataObject> Children { get; set; } = new List<DataObject>();

        public DataObject(string name)
        {
            Name = name;
        }

        public T Deserialize<T>()
        {
            return (T)Deserialize(typeof(T));
        }

        public object Deserialize(Type instanceType)
        {
            if (FastPropertyTypes.ContainsKey(instanceType) && Properties.Count > 0)
                return FastPropertyTypes[instanceType].Invoke(Properties);
            if (FastChildTypes.ContainsKey(instanceType))
                return FastChildTypes[instanceType].Invoke(Children);

            if (instanceType.IsArray)
            {
                var elementType = instanceType.GetElementType();
                var array = Array.CreateInstance(elementType, Children.Count);

                if (KnownConvertors.TryGetValue(elementType, out Func<string, object> convertor))
                {
                    for (int i = 0; i < Children.Count; i++)
                        array.SetValue(convertor(Children[i].Name), i);
                }
                else
                {
                    for (int i = 0; i < Children.Count; i++)
                        array.SetValue(Children[i].Deserialize(elementType), i);
                }

                return array;
            }
            else if (instanceType.IsGenericType && instanceType.GetGenericTypeDefinition() == GenericListType)
            {
                var elementType = instanceType.GetGenericArguments()[0];
                var enumerableType = EnumerableType.MakeGenericType(elementType);
                var constructor = GenericListType.MakeGenericType(elementType).GetConstructor(new Type[] { enumerableType });

                // NOTE: This way may be slower than creating instance and calling add method N times
                var array = Array.CreateInstance(elementType, Children.Count);

                if (KnownConvertors.TryGetValue(elementType, out Func<string, object> convertor))
                {
                    for (int i = 0; i < Children.Count; i++)
                        array.SetValue(convertor(Children[i].Name), i);
                }
                else
                {
                    for (int i = 0; i < Children.Count; i++)
                        array.SetValue(Children[i].Deserialize(elementType), i);
                }

                return constructor.Invoke(new object[] { array });
            }

            var result = Activator.CreateInstance(instanceType);

            const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;
            var fields = instanceType.GetFields(flags);
            var properties = instanceType.GetProperties(flags);
            
            foreach (var property in Properties)
            {
                var match = properties.FirstOrDefault(p => p.Name.ToLower() == property.Key.ToLower()); // Ignore case
                if (match != null)
                {
                    if (KnownConvertors.TryGetValue(match.PropertyType, out Func<string,object> convertor))
                    {
                        match.SetValue(result, convertor(property.Value));
                    }
                    else
                    {
                        throw new Exception($"Unknown type {match.PropertyType.Name}");
                    }
                }
                else
                {
                    var field = fields.FirstOrDefault(f => f.Name.ToLower() == property.Key.ToLower()); // Ignore case
                    if (field != null)
                    {
                        if (KnownConvertors.TryGetValue(field.FieldType, out Func<string,object> convertor))
                        {
                            field.SetValue(result, convertor(property.Value));
                        }
                        else
                        {
                            throw new Exception($"Unknown type {match.PropertyType.Name}");
                        }
                    }
                }
            }

            foreach (var child in Children)
            {
                var match = properties.FirstOrDefault(p => p.Name.ToLower() == child.Name.ToLower()); // Ignore case
                if (match != null)
                {
                    match.SetValue(result, child.Deserialize(match.PropertyType));
                }
                else
                {
                    var field = fields.FirstOrDefault(f => f.Name.ToLower() == child.Name.ToLower()); // Ignore case
                    if (field != null)
                    {
                        field.SetValue(result, child.Deserialize(field.FieldType));
                    }
                    else
                    {
                        throw new Exception($"Unknown type {match.PropertyType.Name}");
                    }
                }
            }

            return result;
        }

        public T GetPropertyAs<T>(string propertyName)
        {
            var property = Properties.Keys.FirstOrDefault(k => k.ToLower() == propertyName.ToLower());
            if (property == null)
                return default(T);

            if (!KnownConvertors.TryGetValue(typeof(T), out Func<string, object> convertor))
                return default(T);

            return (T)convertor(Properties[property]);
        }

        private static Dictionary<Type, Func<Dictionary<string, string>, object>> FastPropertyTypes = new Dictionary<Type, Func<Dictionary<string, string>, object>>()
        {
            {typeof(HashSet<string>), d => new HashSet<string>(d.Keys) },
            {typeof(Dictionary<string,string>), d => new Dictionary<string,string>(d) },
        };

        private static Dictionary<Type, Func<List<DataObject>, object>> FastChildTypes = new Dictionary<Type, Func<List<DataObject>, object>>()
        {
            {typeof(HashSet<string>), l => new HashSet<string>(l.Select(o => o.Name)) },
            {typeof(List<string>), l => new List<string>(l.Select(o => o.Name)) }
        };

        private static Dictionary<Type, Func<string, object>> KnownConvertors = new Dictionary<Type, Func<string, object>>()
        {
            {typeof(string), s => s },

            {typeof(char), s => char.Parse(s) },
            {typeof(sbyte), s => sbyte.Parse(s) },
            {typeof(byte), s => byte.Parse(s) },
            {typeof(short), s => short.Parse(s) },
            {typeof(ushort), s => ushort.Parse(s) },
            {typeof(int), s => int.Parse(s) },
            {typeof(uint), s => uint.Parse(s) },
            {typeof(long), s => long.Parse(s) },
            {typeof(ulong), s => ulong.Parse(s) },
            {typeof(decimal), s => decimal.Parse(s) },
            {typeof(float), s => float.Parse(s) },
            {typeof(double), s => double.Parse(s) },
        };

        private static Type GenericListType = typeof(List<>);
        private static Type EnumerableType = typeof(IEnumerable<>);
    }
}
