using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace VKPlayer.Configuration
{
    public sealed class Snapshot<TSource>
    {
        private readonly TSource _source;
        private readonly PropertyInfo[] _properties;
        private readonly Dictionary<string, object> _prevValues = new Dictionary<string, object>();

        public Snapshot(TSource source)
        {
            _source = source;
            _properties = typeof(TSource).GetProperties();
        }

        public void Save()
        {
            foreach (var prop in _properties)
            {
                var val = prop.GetValue(_source);
                if (val is IEnumerable<object> collection)
                {
                    _prevValues[prop.Name] = collection.ToArray();
                }
                else
                {
                    _prevValues[prop.Name] = val;
                }
            }
        }

        public List<ChangeProp> GetChangedPropNames()
        {
            if (_prevValues.Count == 0)
            {
                throw new InvalidOperationException("Cannot to detect changes because snapshot was not saved.");
            }

            var names = new List<ChangeProp>();
            foreach (var prop in _properties)
            {
                var prevVal = _prevValues[prop.Name];
                var curVal = prop.GetValue(_source);

                if (!AreValuesEqual(prevVal, curVal))
                {
                    names.Add(new ChangeProp()
                    {
                        PropName = prop.Name,
                        NewValue = curVal,
                        OldValue = prevVal
                    });
                }
            }

            return names;
        }

        private bool AreValuesEqual(object val1, object val2)
        {
            if (val1 == null && val2 == null)
            {
                return true;
            }

            if (val1 == null || val2 == null)
            {
                return false;
            }

            if (val1 is IEnumerable<object> collection1 && val2 is IEnumerable<object> collection2)
            {
                return collection1.SequenceEqual(collection2);
            }

            return val1.Equals(val2);
        }
    }

    public struct ChangeProp
    {
        public object PropName;
        public object NewValue;
        public object OldValue;
    }
}
