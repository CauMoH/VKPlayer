using System.IO;
using System.Xml.Linq;
using VKPlayer.Helpers;

namespace VKPlayer.Configuration.XmlCommon
{
    public sealed class XmlConfig
    {
        private readonly XmlSetting _rootSetting;

        public XmlConfig(XmlSetting rootSetting)
        {
            _rootSetting = rootSetting;
        }

        public void Load(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return;
            }

            var root = XDocument.Load(filePath).Element(_rootSetting.ElemName);

            foreach (var setting in _rootSetting.Children)
            {
                setting.Load(root);
            }
        }

        public void Save(string filePath)
        {
            DirectoryHelper.EnsurePathDirectoryExists(filePath);

            var root = new XElement(_rootSetting.ElemName);

            foreach (var setting in _rootSetting.Children)
            {
                setting.Save(root);
            }

            root.Save(filePath);
        }
    }
}
