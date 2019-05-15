using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace VKPlayer.Configuration.XmlCommon
{
    public sealed class XmlConfigBuilder : IXmlBlockInitializer
    {
        private readonly XmlSetting _rootSetting;

        private XmlConfigBuilder(XmlSetting rootSetting)
        {
            _rootSetting = rootSetting;
        }

        public static XmlConfigBuilder Root(string rootElemName)
        {
            return new XmlConfigBuilder(new XmlSetting(rootElemName));
        }

        public XmlConfigBuilder Setting(string elemName, Func<string> getValue, Action<string> setValue)
        {
            var setting = new XmlSetting(elemName);
            setting.Initialize(getValue, setValue);
            _rootSetting.Children.Add(setting);
            return this;
        }

        public XmlConfigBuilder Setting<TValue>(string elemName, Expression<Func<TValue>> propSelector)
        {
            var setting = new XmlSetting(elemName);
            setting.Initialize(propSelector);
            _rootSetting.Children.Add(setting);
            return this;
        }

        public XmlConfigBuilder Settings(string elemName, Func<IEnumerable<string>> getValues, Action<List<string>> setValues)
        {
            var setting = new XmlSetting(elemName);
            setting.Initialize(getValues, setValues);
            _rootSetting.Children.Add(setting);
            return this;
        }

        public XmlConfigBuilder Block(string elemName, Action<IXmlBlockInitializer> initializer)
        {
            var setting = new XmlSetting(elemName);
            _rootSetting.Children.Add(setting);

            var builder = new XmlConfigBuilder(setting);
            initializer(builder);
            return this;
        }

        public XmlConfig Build()
        {
            return new XmlConfig(_rootSetting);
        }

        IXmlBlockInitializer IXmlBlockInitializer.Setting(string elemName, Func<string> getValue, Action<string> setValue)
        {
            return Setting(elemName, getValue, setValue);
        }

        IXmlBlockInitializer IXmlBlockInitializer.Setting<TValue>(string elemName, Expression<Func<TValue>> propSelector)
        {
            return Setting(elemName, propSelector);
        }

        IXmlBlockInitializer IXmlBlockInitializer.Settings(string elemName, Func<IEnumerable<string>> getValues, Action<List<string>> setValues)
        {
            return Settings(elemName, getValues, setValues);
        }
    }

    public interface IXmlBlockInitializer
    {
        IXmlBlockInitializer Setting(string elemName, Func<string> getValue, Action<string> setValue);
        IXmlBlockInitializer Settings(string elemName, Func<IEnumerable<string>> getValues, Action<List<string>> setValues);
        IXmlBlockInitializer Setting<TValue>(string elemName, Expression<Func<TValue>> propSelector);
    }
}
