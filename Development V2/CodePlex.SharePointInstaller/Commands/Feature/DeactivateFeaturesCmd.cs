using System;
using System.Collections.Generic;
using System.IO;
using CodePlex.SharePointInstaller.Logging;
using CodePlex.SharePointInstaller.Wrappers;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;

namespace CodePlex.SharePointInstaller.Commands.Feature
{
    public class DeactivateFeaturesCmd : ActivateFeaturesCmd
    {
        public DeactivateFeaturesCmd(InstallationContext context) : base(context)
        {
        }

        public override bool Execute()
        {
            DispatchMessage(Environment.NewLine + "Start deactivating features.");
            Log.Info("Deactivating feature(s)...");

            try
            {
                bool iterateThroughSites = context.SolutionInfo.Features.SiteFeatures.Count > 0;
                bool iterateThroughSiteCollections = iterateThroughSites ||
                                                     context.SolutionInfo.Features.SiteCollectionFeatures.Count > 0;
                bool iterateThroughWebApplications = iterateThroughSiteCollections ||
                                                     context.SolutionInfo.Features.WebAppFeatures.Count > 0;

                if (iterateThroughWebApplications)
                {
                    IList<SPWebApplication> apps = new List<SPWebApplication>();
                    if (context.Configuration.RequireDeploymentToCentralAdminWebApplication)
                        foreach (var app in SPWebService.AdministrationService.WebApplications)
                            apps.Add(app);
                    foreach (var app in SPWebService.ContentService.WebApplications)
                        apps.Add(app);

                    foreach (var app in apps)
                    {
                        DispatchMessage("Web application '{0}'.", app.DisplayName);

                        if (iterateThroughSiteCollections)
                            foreach (SPSite siteCollection in app.Sites)
                            {
                                bool? catchAccessDeniedException = null; //to store the property setting before processing
                                try
                                {
                                    catchAccessDeniedException = siteCollection.CatchAccessDeniedException;
                                    siteCollection.CatchAccessDeniedException = false;
                                    if (!SiteInfo.IsAccessible(siteCollection.RootWeb))
                                        continue;
                                    var siteId = siteCollection.ID; //DirectoryNotFoundException here if SiteCollection is corrupted

                                    if (iterateThroughSites)
                                        foreach (SPWeb site in siteCollection.AllWebs)
                                        {
                                            using (site)
                                            {
                                                if (!SiteInfo.IsAccessible(site))
                                                    continue;
                                                var webId = site.ID; //DirectoryNotFoundException here if SiteCollection is corrupted
                                                foreach (var featureInfo in context.SolutionInfo.Features.SiteFeatures)
                                                {
                                                    DeactivateSiteFeature(site, featureInfo, context);
                                                }
                                            }
                                        }

                                    foreach (var featureInfo in context.SolutionInfo.Features.SiteCollectionFeatures)
                                    {
                                        DeactivateSiteCollectionFeature(siteCollection, featureInfo, context);
                                    }
                                }
                                catch (IOException e)
                                {
                                    Log.Info(
                                        String.Format("Skipping corrupted site collection {0}.", siteCollection.Url), e);
                                }
                                catch (UnauthorizedAccessException e)
                                {
                                    Log.Info(
                                        String.Format("Skipping restricted site collection {0}.", siteCollection.Url),
                                        e);
                                }
                                finally
                                {
                                    if (catchAccessDeniedException.HasValue)
                                        siteCollection.CatchAccessDeniedException = catchAccessDeniedException.Value;
                                    siteCollection.Dispose();
                                }
                            }

                        foreach (var featureInfo in context.SolutionInfo.Features.WebAppFeatures)
                        {
                            DeactivateWebAppFeature(app, featureInfo, context);
                        }
                    }
                }

                foreach (var featureInfo in context.SolutionInfo.Features.FarmFeatures)
                {
                    DeactivateFarmFeature(featureInfo);
                }

                DispatchMessage("Finish deactivating features.");
            }
            catch (Exception e)
            {
                DispatchErrorMessage("Failed to deativate features.");
                Log.Error("Failed to deactivate features. Unable to proceeed.", e);
                throw;
            }

            return true;
        }

        public override bool Rollback()
        {
            return true;
        }

        public override String Description
        {
            get
            {
                return "Deactivating feature(s).";
            }
        }
    }
}
