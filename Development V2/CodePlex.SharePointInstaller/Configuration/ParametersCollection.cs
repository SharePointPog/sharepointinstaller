using System.Configuration;

namespace CodePlex.SharePointInstaller.Configuration
{
    public class ParametersCollection : ConfigurationElementCollection
    {
        public override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return ConfigurationElementCollectionType.AddRemoveClearMap;
            }
        }
       
        public Parameter this[object key]
        {
            get
            {
                return BaseGet(key) as Parameter;
            }
        }
        
        public Parameter this[int index]
        {
            get
            {
                return BaseGet(index) as Parameter;
            }
        }
             
        protected override ConfigurationElement CreateNewElement()
        {
            return new Parameter();
        }

        /// <summary>
        /// Gets element key.
        /// </summary>        
        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((Parameter)element).Name;
        }
    }
}
