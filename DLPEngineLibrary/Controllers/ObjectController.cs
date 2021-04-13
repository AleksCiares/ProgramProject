using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLPEngineLibrary.Controllers
{
    internal static class ObjectController 
    {
        internal static T GetDataAvailabity<T, T1>(T @object, string property, T1 comparator)
        {
            if (object.Equals(@object, default(T)))
                return default(T);
            ///if (EqualityComparer<T>.Default.Equals(@object, default(T)))
            ///    return default(T);

            Type type = typeof(T);
            if (type.GetProperty(property).GetValue(@object, null) != null)
            {
                T1 value = (T1)type.GetProperty(property).GetValue(@object, null);
                if (EqualityComparer<T1>.Default.Equals(value, comparator))
                    return default(T);
                else
                    return @object;
            }

            return default(T);
        }

        internal static bool CompareObjects<T>(T object1, T object2) where T : class
        {
            if (object.Equals(object1, object2))
                return true;

            if (object.Equals(object1, default(T)) || object.Equals(object2, default(T)))
                return false;

            Type type = typeof(T);
            foreach (System.Reflection.PropertyInfo property in type.GetProperties())
            {
                string object1Value = string.Empty;
                string object2Value = string.Empty;

                if (type.GetProperty(property.Name).GetValue(object1, null) != null)
                    object1Value = type.GetProperty(property.Name).GetValue(object1, null).ToString();

                if (type.GetProperty(property.Name).GetValue(object2, null) != null)
                    object2Value = type.GetProperty(property.Name).GetValue(object2, null).ToString();

                if (object1Value.Trim() != object2Value.Trim())
                    return false;
            }

            return true;
        }
    }
}
