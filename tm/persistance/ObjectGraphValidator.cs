using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace tm.persistance
{
    public class ObjectGraphValidator
    {

        private List<Type> ignore = new List<Type> { typeof(City), typeof(String), typeof(DateTime) };
        private static int total = 0;

        List<object> _knownObjects = new List<object>();
        Dictionary<Type, int> _encounteredCount = new Dictionary<Type, int>();
        List<Type> _nonReferenceTypes = new List<Type>();

        public void ValidateObjectGraph(object obj)
        {
            total++;
            //if (total > 100000) return;
            Console.WriteLine("[ValidateObjectGraph]" + obj + " (" + total + ")");
            Type type = obj.GetType();
            if (_encounteredCount.ContainsKey(type))
                _encounteredCount[type]++;
            else
            {
                _encounteredCount.Add(type, 1);
                if (type.IsValueType)
                    _nonReferenceTypes.Add(type);
                DataContractAttribute att = Attribute.GetCustomAttribute(type, typeof(DataContractAttribute)) as DataContractAttribute;
                if (att == null || !att.IsReference)
                    _nonReferenceTypes.Add(type);
            }

            if (obj.GetType().IsValueType)
                return;
            if (_knownObjects.Contains(obj))
                return;
            _knownObjects.Add(obj);
            if (obj is IEnumerable && type != typeof(string))
            {
                foreach (object obj2 in (obj as IEnumerable))
                {
                    if (!obj2.GetType().IsPrimitive && !ignore.Contains(obj2.GetType()))
                    {
                        ValidateObjectGraph(obj2);
                    }
                }
            }
            foreach (PropertyInfo property in type.GetProperties(BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic))
                if (property.GetIndexParameters().Count() == 0)
                {
                    object value = property.GetValue(obj, null);
                    if (value == null)
                        continue;
                    if (!value.GetType().IsPrimitive && !ignore.Contains(value.GetType()))
                    {
                        ValidateObjectGraph(value);
                    }
                }
        }
    }

}
