using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;

namespace CodePlex.SharePointInstaller
{
    public class ActivationReporter
    {
        private InstallOperation myOperation;
        private ListView myList = null;

        public static void ReportActivationsToList(InstallOperation operation, ListView list)
        {
            ActivationReporter reporter = new ActivationReporter(operation, list);
            reporter.ReportActivations();
            return;
        }

        private ActivationReporter(InstallOperation operation, ListView list)
        {
            myOperation = operation;
            myList = list;
        }
/*
        private void ReportSiteFeatureActivations()
        {
            IList<SiteLoc> featuredSites = InstallProcessControl.GetFeaturedSiteCollections();

            myList.View = View.Details;
            myList.Columns.Clear();
            myList.Columns.Add("WebApp", 150);
            myList.Columns.Add("Site Collection", 150);
            if (InstallConfiguration.FeatureId.Count > 1)
            {
                myList.Columns.Add("#Features", 50);
            }

            foreach (SiteLoc siteloc in featuredSites)
            {
                try
                {
                    using (SPSite site = new SPSite(siteloc.SiteId))
                    {
                        string webappName = GetWebAppName(site.WebApplication);
                        ListViewItem item = new ListViewItem(webappName);
                        item.SubItems.Add(site.RootWeb.Title);
                        if (InstallConfiguration.FeatureId.Count > 1)
                        {
                            item.SubItems.Add(string.Format("{0}/{1}", siteloc.featureList.Count, InstallConfiguration.FeatureId.Count));
                        }
                        myList.Items.Add(item);
                    }
                }
                catch (Exception exc)
                {
                    myList.Items.Add("Exception(" + siteloc.SiteId.ToString() + "): " + exc.Message);
                }
            }
        }
*/
        private void AddLocationItemToDisplay(SPFeatureScope scope, string webappTitle, string siteTitle, string webTitle, int featureCount)
        {
            string scopeString = (scope == SPFeatureScope.WebApplication ? "WebApp" : scope.ToString());
            ListViewItem item = new ListViewItem(scopeString);
            item.SubItems.Add(webappTitle);
            item.SubItems.Add(siteTitle);
            item.SubItems.Add(webTitle);
            if (InstallConfiguration.FeatureId.Count > 1)
            {
                item.SubItems.Add(featureCount.ToString());
            }
            myList.Items.Add(item);
        }
        private void ReportActivations()
        {
            FeatureLocations flocs = InstallProcessControl.GetFeaturedLocations(myOperation);
            myList.View = View.Details;
            myList.Columns.Clear();
            myList.Columns.Add("Scope", 50);
            myList.Columns.Add("WebApp", 100);
            myList.Columns.Add("Site", 100);
            myList.Columns.Add("Web", 100);
            if (InstallConfiguration.FeatureId.Count > 1)
            {
                myList.Columns.Add("#Features", 30);
            }
            // Farm
            foreach (FeatureLoc floc in flocs.FarmLocations)
            {
                AddLocationItemToDisplay(SPFeatureScope.Farm, "", "", "", floc.featureList.Count);
            }
            // Web Application
            foreach (FeatureLoc floc in flocs.WebAppLocations)
            {
                SPWebApplication webapp = GetWebAppById(floc.WebAppId);
                if (webapp == null) continue;
                AddLocationItemToDisplay(SPFeatureScope.WebApplication
                    , GetWebAppName(webapp), "", "", floc.featureList.Count);              
            }
            // Site Collection
            foreach (FeatureLoc floc in flocs.SiteLocations)
            {
                SPSite site = new SPSite(floc.SiteId);
                if (site == null) continue;
                try
                {
                    AddLocationItemToDisplay(SPFeatureScope.Site
                        , GetWebAppName(site.WebApplication), site.RootWeb.Title
                        , "", floc.featureList.Count);              
                }
                finally
                {
                    site.Dispose();
                }
            }
            // Web
            foreach (FeatureLoc floc in flocs.WebLocations)
            {
                SPSite site = new SPSite(floc.SiteId);
                if (site == null) continue;
                try
                {
                    SPWeb web = site.OpenWeb(floc.WebId);
                    if (web == null) continue;
                    try
                    {
                        AddLocationItemToDisplay(SPFeatureScope.Web
                            , GetWebAppName(web.Site.WebApplication), web.Site.RootWeb.Title
                            , web.Title, floc.featureList.Count);
                    }
                    finally
                    {
                        web.Dispose();
                    }
                }
                finally
                {
                    site.Dispose();
                }
            }
        }
        /// <summary>
        /// Get Web Application by ID
        /// </summary>
        public static SPWebApplication GetWebAppById(Guid webappId)
        {
            SPWebServiceCollection webServices = new SPWebServiceCollection(SPFarm.Local);
            foreach (SPWebService webService in webServices)
            {
                foreach (SPWebApplication app in webService.WebApplications)
                {
                    if (app.Id == webappId)
                    {
                        return app;
                    }
                }
            }
            return null;
        }
/*
        private void ReportWebFeatureActivations()
        {
            IList<WebLoc> featuredWebs = InstallProcessControl.GetFeaturedWebs();

            myList.View = View.Details;
            myList.Columns.Clear();
            myList.Columns.Add("WebApp", 100);
            myList.Columns.Add("Site Collection", 100);
            myList.Columns.Add("Site", 150);
            if (InstallConfiguration.FeatureId.Count > 1)
            {
                myList.Columns.Add("#Features", 50);
            }

            foreach (WebLoc webloc in featuredWebs)
            {
                try
                {
                    using (SPSite site = new SPSite(webloc.siteInfo.SiteId))
                    {
                        string webappName = GetWebAppName(site.WebApplication);
                        ListViewItem item = new ListViewItem(webappName);
                        item.SubItems.Add(site.RootWeb.Title);
                        using (SPWeb web = site.OpenWeb(webloc.WebId))
                        {
                            item.SubItems.Add(web.Title);
                        }
                        if (InstallConfiguration.FeatureId.Count > 1)
                        {
                            item.SubItems.Add(string.Format("{0}/{1}", webloc.featureList.Count, InstallConfiguration.FeatureId.Count));
                        }
                        myList.Items.Add(item);
                    }
                }
                catch (Exception exc)
                {
                    myList.Items.Add("Exception(" + webloc.siteInfo.SiteId.ToString() + "," + webloc.WebId.ToString() + "): " + exc.Message);
                }
            }
        }
 * */
        public static string GetWebAppName(SPWebApplication webapp)
        {
            string name = webapp.Name;
            if (!String.IsNullOrEmpty(name)) return name;
            if (webapp.AlternateUrls.Count > 0)
            {
                name = webapp.AlternateUrls[0].IncomingUrl;
                if (!String.IsNullOrEmpty(name)) return "(" + name + ")";
            }
            name = webapp.DefaultServerComment;
            if (!String.IsNullOrEmpty(name)) return "(" + name + ")";
            return webapp.Id.ToString();
        }
    }
}
