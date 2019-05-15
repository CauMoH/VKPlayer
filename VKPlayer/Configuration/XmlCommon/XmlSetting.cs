using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Xml.Linq;

namespace VKPlayer.Configuration.XmlCommon
{
    public sealed class XmlSetting
    {
        private readonly string _elemName;
        private Func<object> _getValue;
        private Action<string> _setValue;
        private Func<IEnumerable<string>> _getValues;
        private Action<List<string>> _setValues;

        private bool _isCollection;

        public List<XmlSetting> Children { get; } = new List<XmlSetting>();

        public XmlSetting(string elemName)
        {
            _elemName = elemName;
        }

        public string ElemName => _elemName;

        public void Initialize(Func<string> getValue, Action<string> setValue)
        {
            _getValue = getValue;
            _setValue = setValue;
        }

        public void Initialize(Func<IEnumerable<string>> getValues, Action<List<string>> setValues)
        {
            _getValues = getValues;
            _setValues = setValues;
            _isCollection = true;
        }

        public void Initialize<TValue>(Expression<Func<TValue>> propSelector)
        {
            var body = (MemberExpression)propSelector.Body;
            var propInfo = (PropertyInfo)body.Member;

            _getValue = () =>
            {
                var owner = GetPropertyOwner(body.Expression);
                return ValueToString(propInfo.GetValue(owner));
            };

            _setValue = val =>
            {
                var owner = GetPropertyOwner(body.Expression);
                propInfo.SetValue(owner, ParseValue(val, typeof(TValue)));
            };
        }

        private object GetPropertyOwner(Expression expression)
        {
            if (expression is ConstantExpression constExpr)
            {
                return constExpr.Value;
            }

            return Expression.Lambda<Func<object>>(expression).Compile()();
        }

        public void Load(XElement root)
        {
            if (_isCollection)
            {
                LoadCollection(root);
            }
            else
            {
                LoadItem(root);
            }
        }

        private void LoadCollection(XElement root)
        {
            var elems = root.Elements(_elemName);
            var xElements = elems as XElement[] ?? elems.ToArray();
            if (!xElements.Any())
            {
                return;
            }

            _setValues(xElements.Select(e => e.Value).ToList());
        }

        private void LoadItem(XElement root)
        {
            var elem = root.Element(_elemName);
            if (elem == null)
            {
                return;
            }

            if (_setValue != null)
            {
                _setValue(elem.Value);
            }
            else
            {
                foreach (var chuld in Children)
                {
                    chuld.Load(elem);
                }
            }
        }

        public void Save(XElement root)
        {
            if (_isCollection)
            {
                SaveCollection(root);
            }
            else
            {
                SaveItem(root);
            }
        }

        private void SaveCollection(XElement root)
        {
            foreach (var val in _getValues())
            {
                root.Add(new XElement(_elemName, val));
            }
        }

        private void SaveItem(XElement root)
        {
            if (_getValue != null)
            {
                root.Add(new XElement(_elemName, _getValue()));
            }
            else
            {
                var elem = new XElement(_elemName);
                foreach (var chuld in Children)
                {
                    chuld.Save(elem);
                }

                root.Add(elem);
            }
        }

        private string ValueToString(object value)
        {
            if (value == null)
                return string.Empty;

            if (value is double d)
            {
                return d.ToString(CultureInfo.InvariantCulture);
            }

            if (value is float f)
            {
                return f.ToString(CultureInfo.InvariantCulture);
            }

            if (value.GetType().IsSubclassOf(typeof(Enum)))
            {
                return ((int)value).ToString();
            }

            return value.ToString();
        }

        private object ParseValue(string valueStr, Type targetType)
        {
            if (targetType == typeof(bool))
            {
                return bool.Parse(valueStr);
            }

            if (targetType == typeof(int))
            {
                return int.Parse(valueStr);
            }

            if (targetType == typeof(int?))
            {
                return string.IsNullOrWhiteSpace(valueStr) ? (object)null : int.Parse(valueStr);
            }

            if (targetType == typeof(float))
            {
                return float.Parse(valueStr, CultureInfo.InvariantCulture);
            }

            if (targetType == typeof(double))
            {
                return double.Parse(valueStr, CultureInfo.InvariantCulture);
            }

            if (targetType == typeof(short))
            {
                return short.Parse(valueStr, CultureInfo.InvariantCulture);
            }

            if (targetType == typeof(uint))
            {
                return uint.Parse(valueStr, CultureInfo.InvariantCulture);
            }

            if (targetType.IsSubclassOf(typeof(Enum)))
            {
                int.TryParse(valueStr, out var val);
                return Enum.ToObject(targetType, val);
            }

            return valueStr;
        }
    }
}
