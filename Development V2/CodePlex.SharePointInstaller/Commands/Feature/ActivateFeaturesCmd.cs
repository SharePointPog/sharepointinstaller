using System;
using System.Collections.Generic;
using System.IO;
using CodePlex.SharePointInstaller.Configuration;
using CodePlex.SharePointInstaller.Logging;
using CodePlex.SharePointInstaller.Wrappers;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;

namespace CodePlex.SharePointInstaller.Commands.Feature
{
    public delegate void SiteOperation(SPSite siteCollection, SPWeb site);

    public delegate void SiteCollectionOperation(SPSite siteCollection);

    public class ActivateFeaturesCmd : DispatcherCmd
    {
        protected InstallationContext context;

        private Stack<FeatureInfo> activatedFarmFeatures = new Stack<FeatureInfo>();

        Dictionary<SPWebApplication, Stack<FeatureInfo>> activatedWebAppFeatures = new Dictionary<SPWebApplication, Stack<FeatureInfo>>();

        Dictionary<SiteCollectionInfo, Stack<FeatureInfo>>  activatedSiteCollectionFeatures = new Dictionary<SiteCollectionInfo, Stack<FeatureInfo>>();

        Dictionary<SiteInfo, Stack<FeatureInfo>>  activatedSiteFeatures = new Dictionary<SiteInfo, Stack<FeatureInfo>>();

        public ActivateFeaturesCmd(InstallationContext context)
        {
            this.context = context;
        }

        public override bool Execute()
        {
            DispatchMessage(Environment.NewLine + "Start activating features.");
            Log.Info("Activating feature(s)...");

            ActivateFarmFeatures();

            ActivateWebAppFeatures();

            ActivateSiteCollectionFeatures();

            ActivateSiteFeatures();

            return true;
        }

        public override bool Rollback()
        {
            Log.Info("Deactivating feature(s)...");

            DeactivateSiteFeatures();

            DeactivateSiteCollectionFeatures();

            DeactivateWebAppFeatures();

            DeactivateFarmFeatures();

            return true;
        }

        private void ActivateSiteFeatures()
        {
            DispatchMessage("Start activating site features.");

            foreach(var siteInfo in context.Sites)
            {

                DoSiteOperation(siteInfo.SiteCollectionId, siteInfo.Id, delegate(SPSite siteCollection, SPWeb site)
                {
                    foreach (var featureInfo in context.SolutionInfo.Features.SiteFeatures)
                    {
                        if (!String.IsNullOrEmpty(featureInfo.Url) || featureInfo.DeactivateOnly)
                            continue;
                        ActivateSiteFeature(siteInfo, featureInfo, site);
                    } 
                });               
            }

            var dictionary = SortFeatures(context.SolutionInfo.Features.SiteFeatures);

            foreach(var url in dictionary.Keys)
            {
                DoSiteOperation(url, delegate(SPSite siteCollection, SPWeb site)
                                         {
                                             foreach (var featureInfo in dictionary[url])
                                             {
                                                 if (featureInfo.DeactivateOnly)
                                                     continue;
                                                 ActivateSiteFeature(new SiteInfo(site), featureInfo, site);
                                             }
                                         });
            }

            DispatchMessage("Finish activating site features.");
        }

        private void ActivateSiteFeature(SiteInfo siteInfo, FeatureInfo featureInfo, SPWeb site)
        {
            try
            {
                site.Features.Add(featureInfo.Id, false);
                
                if (!activatedSiteFeatures.ContainsKey(siteInfo))
                    activatedSiteFeatures[siteInfo] = new Stack<FeatureInfo>();
                activatedSiteFeatures[siteInfo].Push(featureInfo);

                DispatchMessage("{3} feature {0} '{1}' activated in {2}.", featureInfo.Name, featureInfo.Id, site.Url, featureInfo.Scope);
                Log.Info(String.Format("{3} feature {0} '{1}' activated in {2}.", featureInfo.Name, featureInfo.Id, site.Url, featureInfo.Scope));

            }
            catch(Exception e)
            {
                DispatchErrorMessage("Failed to activate site feature {0} '{1}'.", featureInfo.Name, featureInfo.Id);
                Log.Error(String.Format("Failed to activate site feature {0} '{1}'.", featureInfo.Name, featureInfo.Id), e);
            }
        }

        private void DeactivateSiteFeatures()
        {
            DispatchMessage("Start deactivating site features.");

            foreach(var siteInfo in activatedSiteFeatures.Keys)
            {
                 DoSiteOperation(siteInfo.SiteCollectionId, siteInfo.Id, delegate(SPSite siteCollection, SPWeb site)
                 {
                    while(activatedSiteFeatures[siteInfo].Count > 0)
                    {
                        DeactivateSiteFeature(site, activatedSiteFeatures[siteInfo].Pop(), new InstallationContext(true));
                    }
                 });
            }

            DispatchMessage("Finish deactivating site features.");
        }

        protected void DeactivateSiteFeature(SPWeb site, FeatureInfo featureInfo, InstallationContext context)
        {
            try
            {
                if (site != null)
                {
                    var feature = site.Features[featureInfo.Id];
                    if (feature == null)
                        return;

                    site.Features.Remove(featureInfo.Id, false);
                    context.AddSelectedSite(new SiteInfo(site));

                    DispatchMessage("{3} feature {0} '{1}' deactivated in {2}.", featureInfo.Name, featureInfo.Id, site.Url, featureInfo.Scope);
                    Log.Info(String.Format("{3} feature {0} '{1}' deactivated in {2}.", featureInfo.Name, featureInfo.Id, site.Url, featureInfo.Scope));
                }
            }
            catch (Exception e)
            {
                DispatchErrorMessage("Failed to deactivate site feature {0} '{1}'.", featureInfo.Name, featureInfo.Id);
                Log.Error(String.Format("Failed to deactivate site feature {0} '{1}'.", featureInfo.Name, featureInfo.Id), e);
            }
        }

        private void ActivateSiteCollectionFeatures()
        {
            DispatchMessage("Start activating site collection features.");

            foreach(var siteCollectionInfo in context.SiteCollections)
            {
                DoSiteCollectionOperation(siteCollectionInfo.Id, delegate(SPSite siteCollection)
                 {
                     foreach (var featureInfo in context.SolutionInfo.Features.SiteCollectionFeatures)
                     {
                         if (!String.IsNullOrEmpty(featureInfo.Url) || featureInfo.DeactivateOnly)
                             continue;
                         ActivateSiteCollectionFeature(siteCollectionInfo, featureInfo, siteCollection);
                     }
                 });                
            }

            var dictionary = SortFeatures(context.SolutionInfo.Features.SiteCollectionFeatures);

            foreach (var url in dictionary.Keys)
            {
                DoSiteCollectionOperation(url, delegate(SPSite siteCollection)
                                                   {
                                                       foreach (var featureInfo in dictionary[url])
                                                       {
                                                           if (featureInfo.DeactivateOnly)
                                                               continue;
                                                           ActivateSiteCollectionFeature(
                                                               new SiteCollectionInfo(siteCollection), featureInfo,
                                                               siteCollection);
                                                       }
                                                   });
            }

            DispatchMessage("Finish activating site collection features.");
        }

        private Dictionary<String, List<FeatureInfo>> SortFeatures(IEnumerable<FeatureInfo> features)
        {
            var dictionary = new Dictionary<String, List<FeatureInfo>>();
            foreach(var feature in features)
            {
                if (String.IsNullOrEmpty(feature.Url))
                    continue;
                if(!dictionary.ContainsKey(feature.Url))
                    dictionary[feature.Url] = new List<FeatureInfo>();
                dictionary[feature.Url].Add(feature);
            }
            return dictionary;
        }

        private void ActivateSiteCollectionFeature(SiteCollectionInfo siteCollectionInfo, FeatureInfo featureInfo, SPSite siteCollection)
        {
            try
            {
                siteCollection.Features.Add(featureInfo.Id, false);
                
                if (!activatedSiteCollectionFeatures.ContainsKey(siteCollectionInfo))
                    activatedSiteCollectionFeatures[siteCollectionInfo] = new Stack<FeatureInfo>();
                activatedSiteCollectionFeatures[siteCollectionInfo].Push(featureInfo);

                DispatchMessage("{3} feature {0} '{1}' activated in {2}.", featureInfo.Name, featureInfo.Id, siteCollection.Url, featureInfo.Scope);
                Log.Info(String.Format("{3} feature {0} '{1}' activated in {2}.", featureInfo.Name, featureInfo.Id, siteCollection.Url, featureInfo.Scope));
            }
            catch (Exception e)
            {
                DispatchErrorMessage("Failed to activate site collection feature {0} '{1}'.", featureInfo.Name, featureInfo.Id);
                Log.Error(String.Format("Failed to activate site collection feature {0} '{1}'.", featureInfo.Name, featureInfo.Id), e);
            }
        }

        private void DeactivateSiteCollectionFeatures()
        {
            DispatchMessage("Start deactivating site collection features.");

             foreach(var siteCollectionInfo in activatedSiteCollectionFeatures.Keys)
             {
                 DoSiteCollectionOperation(siteCollectionInfo.Id, delegate(SPSite siteCollection)
                  {
                      while (activatedSiteCollectionFeatures[siteCollectionInfo].Count > 0)
                      {
                          DeactivateSiteCollectionFeature(siteCollection, activatedSiteCollectionFeatures[siteCollectionInfo].Pop(), new InstallationContext(true));
                      }   
                  });                 
             }

             DispatchMessage("Finish deactivating site collection features.");
        }

        protected void DeactivateSiteCollectionFeature(SPSite siteCollection, FeatureInfo featureInfo, InstallationContext context)
        {
            try
            {
                if (siteCollection != null)
                {
                    var feature = siteCollection.Features[featureInfo.Id];
                    if (feature == null)
                        return;

                    siteCollection.Features.Remove(featureInfo.Id, false);
                    context.AddSelectedSiteCollection(new SiteCollectionInfo(siteCollection));

                    DispatchMessage("{3} feature {0} '{1}' deactivated in {2}.", featureInfo.Name, featureInfo.Id, siteCollection.Url, featureInfo.Scope);
                    Log.Info(String.Format("{3} feature {0} '{1}' deactivated in {2}.", featureInfo.Name, featureInfo.Id, siteCollection.Url, featureInfo.Scope));
                }
            }
            catch (Exception e)
            {
                DispatchErrorMessage("Failed to deactivate site collection feature {0} '{1}'.", featureInfo.Name, featureInfo.Id);
                Log.Error(String.Format("Failed to deactivate site collection feature {0} '{1}'.", featureInfo.Name, featureInfo.Id), e);
            }            
        }

        private void ActivateWebAppFeatures()
        {
            DispatchMessage("Start activating web application features.");

            foreach (var webApp in context.WebApps)
            {
                DispatchMessage("Web application '{0}'", webApp.DisplayName);

                foreach (var featureInfo in context.SolutionInfo.Features.WebAppFeatures)
                {
                    if (featureInfo.DeactivateOnly)
                        continue;

                    var feature = webApp.Features[featureInfo.Id];
                    var message = "{0} feature {1} '{2}' activated in {3}.";
                    if (feature == null)
                    {
                        try
                        {
                            webApp.Features.Add(featureInfo.Id, false);
                            Log.Info(String.Format(message, featureInfo.Scope, featureInfo.Name, featureInfo.Id,
                                                   webApp.DisplayName));
                            if (!activatedWebAppFeatures.ContainsKey(webApp))
                                activatedWebAppFeatures[webApp] = new Stack<FeatureInfo>();
                            activatedWebAppFeatures[webApp].Push(featureInfo);
                        }
                        catch (Exception e)
                        {
                            DispatchErrorMessage("Failed to activate web application feature {0} '{1}'.",
                                                 featureInfo.Name, featureInfo.Id);
                            Log.Error(
                                String.Format("Failed to activate web application feature '{0}'.", featureInfo.Id), e);
                        }
                    }
                    else
                    {
                        message = "Skipping feature {1} '{2}' already activated at {0} scope.";
                    }

                    DispatchMessage(message, featureInfo.Scope, featureInfo.Name, featureInfo.Id, webApp.DisplayName);
                    Log.Info(String.Format("{2} feature '{0}' activated in {1}.", featureInfo.Id, webApp.DisplayName,
                                           featureInfo.Scope));
                }
            }

            DispatchMessage("Finish activating web application features.");
        }

        private void DeactivateWebAppFeatures()
        {
            DispatchMessage("Start deactivating web application features.");

            foreach(var webApp in activatedWebAppFeatures.Keys)
            {
                DispatchMessage("Web application '{0}'", webApp.DisplayName);

                while(activatedWebAppFeatures[webApp].Count > 0)
                {
                    DeactivateWebAppFeature(webApp, activatedWebAppFeatures[webApp].Pop(), new InstallationContext(true));
                }
            }

            DispatchMessage("Finish deactivating web application features.");
        }

        protected void DeactivateWebAppFeature(SPWebApplication webApp, FeatureInfo featureInfo, InstallationContext context)
        {
            try
            {
                var feature = webApp.Features[featureInfo.Id];
                if (feature == null)
                    return;
                webApp.Features.Remove(featureInfo.Id, false);
                context.AddSelectedWebApp(webApp);

                DispatchMessage("{3} feature {0} '{1}' deactivated in {2}.", featureInfo.Name, featureInfo.Id, webApp.DisplayName, featureInfo.Scope);
                Log.Info(String.Format("{3} feature {0} '{1}' deactivated in {2}.", featureInfo.Name, featureInfo.Id, webApp.DisplayName, featureInfo.Scope));
            }
            catch (Exception e)
            {
                DispatchErrorMessage("Failed to deactivate web application feature {0} '{1}'.", featureInfo.Name, featureInfo.Id);
                Log.Error(String.Format("Failed to deactivate web application feature {0} '{1}'.", featureInfo.Name, featureInfo.Id), e);
            }

            Log.Info(String.Format("{2} feature '{0}' deactivated in {1}.", featureInfo.Id, webApp.DisplayName, featureInfo.Scope));

        }

        private void ActivateFarmFeatures()
        {
            DispatchMessage("Start activating farm features.");

            foreach(var featureInfo in context.SolutionInfo.Features.FarmFeatures)
            {
                if (featureInfo.DeactivateOnly)
                    continue;
                try
                {
                    //first see if feature is already activated. some farm features activate by default
                    var feature = SPWebService.AdministrationService.Features[featureInfo.Id];
                    var message = "{0} feature {1} '{2}' activated in farm.";
                    if (feature == null)
                    {
                        SPWebService.AdministrationService.Features.Add(featureInfo.Id, false);
                        activatedFarmFeatures.Push(featureInfo);
                    }
                    else
                    {
                        message = "Skipping feature {1} '{2}' already activated at {0} scope.";
                    }

                    DispatchMessage(message, featureInfo.Scope, featureInfo.Name, featureInfo.Id);
                    Log.Info(String.Format(message, featureInfo.Scope, featureInfo.Name, featureInfo.Id));
                }
                catch (Exception e)
                {
                    DispatchErrorMessage("Failed to activate farm feature {0} '{1}'.", featureInfo.Name, featureInfo.Id);
                    Log.Error(String.Format("Failed to activate farm feature {0} '{1}'.", featureInfo.Name, featureInfo.Id), e);
                }
            }

            DispatchMessage("Finish activating farm features.");
        }

        private void DeactivateFarmFeatures()
        {
            DispatchMessage("Start deactivating farm features.");

            while (activatedFarmFeatures.Count > 0)
            {
                DeactivateFarmFeature(activatedFarmFeatures.Pop()); 
            }

            DispatchMessage("Finish deactivating farm application features.");
        }

        protected void DeactivateFarmFeature(FeatureInfo featureInfo)
        {
            try
            {
                var feature = SPWebService.AdministrationService.Features[featureInfo.Id];
                if (feature != null)
                {
                    SPWebService.AdministrationService.Features.Remove(featureInfo.Id);

                    DispatchMessage("{2} feature {0} '{1}' deactivated in farm.", featureInfo.Name, featureInfo.Id, featureInfo.Scope);
                    Log.Info(String.Format("{2} feature {0} '{1}' deactivated in farm.", featureInfo.Name, featureInfo.Id, featureInfo.Scope));
                }
            }            
            catch (Exception e)
            {
                DispatchErrorMessage("Failed to deactivate farm feature {0} '{1}'.", featureInfo.Name, featureInfo.Id);
                Log.Error(String.Format("Failed to deactivate farm feature {0} '{1}'.", featureInfo.Name, featureInfo.Id), e);
            }
        }

        protected void DoSiteOperation(Guid siteCollectionId, Guid siteId, SiteOperation operation)
        {
            if (operation != null)
            {
                using (var siteCollection = new SPSite(siteCollectionId))
                {
                    using (var site = siteCollection.OpenWeb(siteId))
                    {
                        operation(siteCollection, site);
                    }
                }
            }
        }

        protected void DoSiteOperation(String url, SiteOperation operation)
        {
            try
            {
                if (operation != null)
                {
                    using (var siteCollection = new SPSite(url))
                    {
                        using (var site = siteCollection.OpenWeb(new Uri(url).AbsolutePath))
                        {
                            operation(siteCollection, site);
                        }
                    }
                }
            }
            catch(FileNotFoundException e)
            {
                Log.Error("Failed to get site instance to activate the feature. Please check feature url in the config.", e);
            }
        }

        protected void DoSiteCollectionOperation(Guid siteCollectionId, SiteCollectionOperation operation)
        {
            if (operation != null)
            {
                using (var siteCollection = new SPSite(siteCollectionId))
                {
                    operation(siteCollection);
                }
            }
        }

        protected void DoSiteCollectionOperation(String url, SiteCollectionOperation operation)
        {
            try
            {
                if (operation != null)
                {
                    using (var siteCollection = new SPSite(url))
                    {
                        operation(siteCollection);
                    }
                }
            }
            catch(FileNotFoundException e)
            {
                Log.Error("Failed to get site collection instance to activate the feature. Please check feature url in the config.", e);
            }
        }

        public override String Description
        {
            get
            {
                return "Activating feature(s).";
            }
        }
    }
}
