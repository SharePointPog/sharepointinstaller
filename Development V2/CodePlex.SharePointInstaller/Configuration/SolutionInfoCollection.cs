using System.Configuration;
using System.Xml;

namespace CodePlex.SharePointInstaller.Configuration
{
    public class SolutionInfoCollection : ConfigurationElementCollection
    {
        public override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return ConfigurationElementCollectionType.AddRemoveClearMap;
            }
        }

        public SolutionInfo this[object key]
        {
            get
            {
                return BaseGet(key) as SolutionInfo;
            }
        }

        public SolutionInfo this[int index]
        {
            get
            {
                return BaseGet(index) as SolutionInfo;
            }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new SolutionInfo();
        }

        private XmlQualifiedName NodeQName { get; set; }

        protected override bool OnDeserializeUnrecognizedAttribute(string name, string value)
        {
            if (name=="xmlns")
                NodeQName = new XmlQualifiedName(ElementName, value);
            return true;
        }

        /// <summary>
        /// Gets element key.
        /// </summary>        
        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((SolutionInfo)element).Id;
        }
    }
}
