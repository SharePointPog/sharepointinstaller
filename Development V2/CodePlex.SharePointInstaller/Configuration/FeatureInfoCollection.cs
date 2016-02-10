using System.Collections.Generic;
using System.Configuration;
using Microsoft.SharePoint;

namespace CodePlex.SharePointInstaller.Configuration
{
    public class FeatureInfoCollection : ConfigurationElementCollection
    {
        public delegate bool Predicate(FeatureInfo featureInfo);

        public override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return ConfigurationElementCollectionType.AddRemoveClearMap;
            }
        }

        public FeatureInfo this[object key]
        {
            get
            {
                return BaseGet(key) as FeatureInfo;
            }
        }

        public FeatureInfo this[int index]
        {
            get
            {
                return BaseGet(index) as FeatureInfo;
            }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new FeatureInfo();
        }

        /// <summary>
        /// Gets element key.
        /// </summary>        
        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((FeatureInfo)element).Id;
        }

        private IList<FeatureInfo> Find(Predicate predicate)
        {
            IList<FeatureInfo> result = new List<FeatureInfo>();
            var enumerator = GetEnumerator();
            while(enumerator.MoveNext())
            {
                var featureInfo = (FeatureInfo) enumerator.Current;
                if(predicate(featureInfo))
                    result.Add(featureInfo);
            }
            return result;
        }

        public IList<FeatureInfo> FarmFeatures
        {
            get
            {
                return Find(featureInfo => featureInfo.Scope == SPFeatureScope.Farm);
            }
        }

        public IList<FeatureInfo> WebAppFeatures
        {
            get
            {
                return Find(featureInfo => featureInfo.Scope == SPFeatureScope.WebApplication);
            }
        }

        public IList<FeatureInfo> SiteCollectionFeatures
        {
            get
            {
                return Find(featureInfo => featureInfo.Scope == SPFeatureScope.Site);
            }
        }

        public IList<FeatureInfo> SiteFeatures
        {
            get
            {
                return Find(featureInfo => featureInfo.Scope == SPFeatureScope.Web);
            }
        }
    }
}
