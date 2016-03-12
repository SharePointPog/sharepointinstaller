using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint;


namespace CodePlex.SharePointInstaller
{
    // Objects used for storing lists of site collections or webs
    public struct SiteLoc
    {
        public Guid SiteId; // can't make readonly because of weakness of C# (no member initializers, so WebLoc can't initialize it)
        public List<Guid?> featureList;
        public SiteLoc(SPSite site)
        {
            this.SiteId = site.ID;
            featureList = new List<Guid?>();
        }
    }
    public struct WebLoc
    {
        public readonly SiteLoc siteInfo;
        public readonly Guid WebId;
        public List<Guid?> featureList;
        public WebLoc(SPWeb web)
        {
            this.WebId = web.ID;
            this.siteInfo.SiteId = web.Site.ID;
            this.siteInfo.featureList = null;
            featureList = new List<Guid?>();
        }
    }
    /// <summary>
    /// Location (farm, webapp, site, or web)
    /// </summary>
    public class FeatureLoc
    {
        public readonly SPFeatureScope Scope;
        public readonly Guid WebAppId;
        public readonly Guid SiteId;
        public readonly Guid WebId;
        public List<Guid> featureList; // features previously deactivated here

        public static FeatureLoc CopyExceptFeatureList(FeatureLoc floc)
        {
            return new FeatureLoc(floc.Scope, floc.WebAppId, floc.SiteId, floc.WebId);
        }

        public FeatureLoc(SPWeb web)
        {
            this.Scope = SPFeatureScope.Web;
            this.WebId = web.ID;
            this.SiteId = web.Site.ID;
            this.WebAppId = web.Site.WebApplication.Id;
        }
        public FeatureLoc(SPSite site)
        {
            this.Scope = SPFeatureScope.Site;
            this.SiteId = site.ID;
            this.WebAppId = site.WebApplication.Id;
        }
        public FeatureLoc(SPWebApplication webapp)
        {
            this.Scope = SPFeatureScope.WebApplication;
            this.WebAppId = webapp.Id;
        }
        public FeatureLoc(SPFarm farm)
        {
            this.Scope = SPFeatureScope.Farm;
            // leave everything null for farm
        }
        private FeatureLoc(SPFeatureScope scope, Guid WebAppId, Guid SiteId, Guid WebId)
        {
            this.Scope = scope;
            this.WebAppId = WebAppId;
            this.SiteId = SiteId;
            this.WebId = WebId;
        }
    }

    public class FeatureLocations
    {
        // Scopes (map features to #locations)
        public Dictionary<Guid, int> FarmFeatures = new Dictionary<Guid, int>();
        public Dictionary<Guid, int> WebAppFeatures = new Dictionary<Guid, int>();
        public Dictionary<Guid, int> SiteFeatures = new Dictionary<Guid, int>();
        public Dictionary<Guid, int> WebFeatures = new Dictionary<Guid, int>();
        // Locations (list all locations, each containing a list of features
        public List<FeatureLoc> WebLocations = new List<FeatureLoc>();
        public List<FeatureLoc> SiteLocations = new List<FeatureLoc>();
        public List<FeatureLoc> WebAppLocations = new List<FeatureLoc>();
        public List<FeatureLoc> FarmLocations = new List<FeatureLoc>();
        public int GetTotalFeatureLocations()
        {
            return WebLocations.Count + SiteLocations.Count
                + WebAppLocations.Count + FarmLocations.Count;
        }
        // Other data
        private int _LocationsCount = 0;
        private int _ActivationsCount = 0;
        public int LocationsCount { get { return _LocationsCount; } }
        public int ActivationsCount { get { return _ActivationsCount; } }
        public void AddFeatureLocation(FeatureLoc floc)
        {
            switch (floc.Scope)
            {
                case SPFeatureScope.WebApplication:
                    RecordFeatures(WebAppFeatures, floc);
                    WebAppLocations.Add(floc);
                    break;
                case SPFeatureScope.Site:
                    RecordFeatures(SiteFeatures, floc);
                    SiteLocations.Add(floc);
                    break;
                case SPFeatureScope.Web:
                    RecordFeatures(WebFeatures, floc);
                    WebLocations.Add(floc);
                    break;
                case SPFeatureScope.Farm:
                    RecordFeatures(FarmFeatures, floc);
                    FarmLocations.Add(floc);
                    break;
            }
            ++_LocationsCount;
            _ActivationsCount += floc.featureList.Count;
        }
        private void RecordFeatures(Dictionary<Guid, int> features, FeatureLoc floc)
        {
            foreach (Guid featureId in floc.featureList)
            {
                int count = 0;
                if (features.ContainsKey(featureId))
                {
                    count = features[featureId];
                }
                ++count;
                features[featureId] = count;
            }
        }
    }
}
