using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint;
using System.IO;
using CodePlex.SharePointInstaller.Resources;

namespace CodePlex.SharePointInstaller
{
    public partial class InstallProcessControl : InstallerControl
    {
        private abstract class FeatureCommand : Command
        {
            protected FeatureCommand(InstallProcessControl parent) : base(parent) { }

            // Modif JPI - Début
            protected static void DeactivateFeature(ReadOnlyCollection<Guid?> featureIds)
            {
                try
                {
                    if (featureIds != null && featureIds.Count > 0)
                    {
                        foreach (Guid? featureId in featureIds)
                        {
                            if (featureId != null)
                            {
                                SPFeature feature = SPWebService.AdministrationService.Features[featureId.Value];
                                if (feature != null)
                                {
                                    SPWebService.AdministrationService.Features.Remove(featureId.Value);
                                }
                            }
                        }
                    }
                }
                catch (ArgumentException ex)  // Missing assembly in GAC
                {
                    log.Warn(ex.Message, ex);
                }
                catch (InvalidOperationException ex)  // Missing receiver class
                {
                    log.Warn(ex.Message, ex);
                }
            }
            // Modif JPI - Fin
        }

        private class ActivateFarmFeatureCommand : FeatureCommand
        {
            internal ActivateFarmFeatureCommand(InstallProcessControl parent) : base(parent) { }

            internal override string Description
            {
                get
                {
                    return string.Format(CommonUIStrings.activateFarmFeatureMessage, InstallConfiguration.FeatureIdList.Count);
                }
            }

            protected internal override bool Execute()
            {
                try
                {
                    // Modif JPI - Début
                    ReadOnlyCollection<Guid?> featureIds = InstallConfiguration.FeatureIdList;
                    if (featureIds != null && featureIds.Count > 0)
                    {
                        foreach (Guid? featureId in featureIds)
                        {
                            if (featureId != null)
                            {
                                SPFeature feature = FeatureActivator.ActivateOneFeature(SPWebService.AdministrationService.Features, featureId.Value, log);
                            }
                        }
                    }
                    return true;
                    // Modif JPI - Fin
                }
                catch (Exception ex)
                {
                    throw new InstallException(ex.Message, ex);
                }
            }

            protected internal override bool Rollback()
            {
                DeactivateFeature(InstallConfiguration.FeatureIdList);
                return true;
            }
        }

        private class DeactivateFarmFeatureCommand : FeatureCommand
        {
            internal DeactivateFarmFeatureCommand(InstallProcessControl parent) : base(parent) { }

            internal override string Description
            {
                get
                {
                    return string.Format(CommonUIStrings.deactivateFarmFeatureMessage, InstallConfiguration.FeatureIdList.Count);
                }
            }

            protected internal override bool Execute()
            {
                try
                {
                    // Modif JPI - Début
                    ReadOnlyCollection<Guid?> featureIds = InstallConfiguration.FeatureIdList;
                    if (featureIds != null && featureIds.Count > 0)
                    {
                        foreach (Guid? featureId in featureIds)
                        {
                            if (featureId != null && SPWebService.AdministrationService.Features[featureId.Value] != null)
                            {
                                SPWebService.AdministrationService.Features.Remove(featureId.Value);
                            }
                        }
                    }

                    return true;
                    // Modif JPI - Fin
                }
                catch (Exception ex)
                {
                    log.Error(ex.Message, ex);
                }

                return true;
            }
        }


        private class ActivateSiteCollectionFeatureCommand : SiteCollectionFeatureCommand
        {
            internal ActivateSiteCollectionFeatureCommand(InstallProcessControl parent, IList<SiteLoc> siteCollectionLocs)
                : base(parent, siteCollectionLocs, SiteCollectionCommand.Activate)
            {
            }
            internal override string Description
            {
                get { return String.Format(CommonUIStrings.activateSiteCollectionFeatureMessage, base.SiteCollectionCount); }
            }
        }
        private class DeactivateSiteCollectionFeatureCommand : SiteCollectionFeatureCommand
        {
            internal DeactivateSiteCollectionFeatureCommand(InstallProcessControl parent, IList<SiteLoc> siteCollectionLocs)
                : base(parent, siteCollectionLocs, SiteCollectionCommand.Deactivate)
            {
            }
            internal override string Description
            {
                get { return String.Format(CommonUIStrings.deactivateSiteCollectionFeatureMessage, "?", base.SiteCollectionCount); }
            }
        }

        /// <summary>
        /// Command which actually does activations and deactivations for site collection features
        /// and also handles rollbacks
        /// </summary>
        private abstract class SiteCollectionFeatureCommand : Command
        {
            public enum SiteCollectionCommand { Activate, Deactivate };

            private readonly IList<SiteLoc> siteCollectionLocs;
            private IList<SiteLoc> completedLocs = new List<SiteLoc>();
            private SiteCollectionCommand command;
            private bool rollback = false;

            internal SiteCollectionFeatureCommand(InstallProcessControl parent, IList<SiteLoc> siteCollectionLocs, SiteCollectionCommand command)
                : base(parent)
            {
                this.siteCollectionLocs = siteCollectionLocs;
                this.command = command;
            }

            protected int SiteCollectionCount { get { return this.siteCollectionLocs.Count; } }

            /// <summary>
            /// Actually activate/deactivate users specified features on requested site collection list
            /// </summary>
            protected internal override bool Execute()
            {
                if (command == SiteCollectionCommand.Activate)
                {
                    log.Info(CommonUIStrings.logFeatureActivate);
                }
                else
                {
                    log.Info(CommonUIStrings.logFeatureDeactivate);
                }
                ReadOnlyCollection<Guid?> featureIds = InstallConfiguration.FeatureIdList;
                if (featureIds == null || featureIds.Count == 0)
                {
                    log.Warn(CommonUIStrings.logNoFeaturesSpecified);
                    return true;
                }
                if (siteCollectionLocs == null || siteCollectionLocs.Count == 0)
                {
                    // caller responsible for giving message, depending on operation (install, upgrade,...)
                    return true;
                }
                try
                {
                    foreach (SiteLoc siteLoc in siteCollectionLocs)
                    {
                        SPSite siteCollection = null;
                        try
                        {
                            siteCollection = new SPSite(siteLoc.SiteId);
                            foreach (Guid? featureId in siteLoc.featureList)
                            {
                                if (featureId == null) continue;

                                if (command == SiteCollectionCommand.Activate)
                                {
                                    FeatureActivator.ActivateOneFeature(siteCollection.Features, featureId.Value, log);
                                    if (!FeatureActivator.IsFeatureActivatedOnSite(siteCollection, featureId.Value))
                                    {
                                        // do not add to completedLocs, b/c it didn't complete
                                        log.Warn("Activation failed on " + siteCollection.Url + " : " + GetFeatureName(featureId));
                                    }
                                    else
                                    {
                                        completedLocs.Add(siteLoc);
                                        log.Info(siteCollection.Url + " : " + GetFeatureName(featureId));
                                    }
                                }
                                else
                                {
                                    siteCollection.Features.Remove(featureId.Value, true);
                                    completedLocs.Add(siteLoc);
                                    log.Info(siteCollection.Url + " : " + GetFeatureName(featureId));
                                }
                            }
                        }
                        catch (Exception exc)
                        {
                            if (rollback)
                            {
                                // during rollback simply log errors and continue
                                string message;
                                if (command == SiteCollectionCommand.Activate)
                                    message = "Activating feature(s)";
                                else
                                    message = "Deactivating feature(s)";
                                log.Error(message, exc);
                            }
                            else
                            {
                                log.Error(siteCollection.Url);
                                throw exc;
                            }
                        }
                        finally
                        {
                            // guarantee SPSite is released ASAP even in face of exception
                            if (siteCollection != null) siteCollection.Dispose();
                        }
                    }

                    return true;
                }
                catch (Exception exc)
                {
                    if (rollback)
                    {
                        log.Error("Error during rollback", exc);
                        return false;
                    }
                    rollback = true;
                    // rollback work accomplished so far
                    if (command == SiteCollectionCommand.Activate)
                    {
                        DeactivateSiteCollectionFeatureCommand reverseCommand = new DeactivateSiteCollectionFeatureCommand(this.Parent, completedLocs);
                        reverseCommand.Execute();
                    }
                    else
                    {
                        ActivateSiteCollectionFeatureCommand reverseCommand = new ActivateSiteCollectionFeatureCommand(this.Parent, completedLocs);
                        reverseCommand.Execute();
                    }
                    throw exc;
                }
            }

            protected internal override bool Rollback()
            {
                // simply redo entire job in reverse
                this.completedLocs.Clear();
                if (command == SiteCollectionCommand.Activate)
                    command = SiteCollectionCommand.Deactivate;
                else
                    command = SiteCollectionCommand.Activate;
                rollback = true;
                return Execute();
            }
        }


        private class ActivateWebFeatureCommand : WebFeatureCommand
        {
            internal ActivateWebFeatureCommand(InstallProcessControl parent, IList<WebLoc> webLocs)
                : base(parent, webLocs, WebCommand.Activate)
            {
            }
            internal override string Description
            {
                get { return String.Format(CommonUIStrings.activateWebFeatureMessage, base.WebCount); }
            }
        }
        private class DeactivateWebFeatureCommand : WebFeatureCommand
        {
            internal DeactivateWebFeatureCommand(InstallProcessControl parent, IList<WebLoc> webLocs)
                : base(parent, webLocs, WebCommand.Deactivate)
            {
            }
            internal override string Description
            {
                get { return String.Format(CommonUIStrings.deactivateWebFeatureMessage, "?", base.WebCount); }
            }
        }


        /// <summary>
        /// Command which actually does activations and deactivations for web features
        /// and also handles rollbacks
        /// </summary>
        private abstract class WebFeatureCommand : Command
        {
            public enum WebCommand { Activate, Deactivate };

            private readonly IList<WebLoc> webLocs;
            private IList<WebLoc> completedLocs = new List<WebLoc>();
            private readonly WebCommand command;
            private bool rollback = false;

            internal WebFeatureCommand(InstallProcessControl parent, IList<WebLoc> webLocs, WebCommand command)
                : base(parent)
            {
                this.webLocs = webLocs;
                this.command = command;
            }

            protected int WebCount { get { return this.webLocs.Count; } }


            /// <summary>
            /// Actually activate/deactivate users specified features on requested web list
            /// </summary>
            protected internal override bool Execute()
            {
                if (command == WebCommand.Activate)
                {
                    log.Info(CommonUIStrings.logFeatureActivate);
                }
                else
                {
                    log.Info(CommonUIStrings.logFeatureDeactivate);
                }
                ReadOnlyCollection<Guid?> featureIds = InstallConfiguration.FeatureIdList;
                if (featureIds == null || featureIds.Count == 0)
                {
                    log.Warn(CommonUIStrings.logNoFeaturesSpecified);
                    return true;
                }
                if (webLocs == null || webLocs.Count == 0)
                {
                    // caller responsible for giving message, depending on operation (install, upgrade,...)
                    return true;
                }
                try
                {
                    foreach (WebLoc webLoc in webLocs)
                    {
                        SPSite siteCollection = null;
                        SPWeb web = null;
                        try
                        {
                            siteCollection = new SPSite(webLoc.siteInfo.SiteId);
                            web = siteCollection.OpenWeb(webLoc.WebId);

                            foreach (Guid? featureId in webLoc.featureList)
                            {
                                if (featureId == null) continue;

                                if (command == WebCommand.Activate)
                                {
                                    FeatureActivator.ActivateOneFeature(web.Features, featureId.Value, log);
                                    if (!FeatureActivator.IsFeatureActivatedOnWeb(web, featureId.Value))
                                    {
                                        // do not add to completedLocs, b/c it didn't complete
                                        log.Warn("Activation failed on " + web.Url + " : " + GetFeatureName(featureId));
                                    }
                                    else
                                    {
                                        completedLocs.Add(webLoc);
                                        log.Info(web.Url + " : " + GetFeatureName(featureId));
                                    }
                                }
                                else
                                {
                                    web.Features.Remove(featureId.Value, true);
                                    completedLocs.Add(webLoc);
                                    log.Info(web.Url + " : " + GetFeatureName(featureId));
                                }
                            }
                        }
                        catch (Exception exc)
                        {
                            if (rollback)
                            {
                                // during rollback simply log errors and continue
                                string message;
                                if (command == WebCommand.Activate)
                                    message = "Activating feature(s)";
                                else
                                    message = "Deactivating feature(s)";
                                log.Error(message, exc);
                            }
                            else
                            {
                                throw exc;
                            }
                        }
                        finally
                        {
                            // guarantee SPWeb is released ASAP even in face of exception
                            if (web != null) web.Dispose();
                            // guarantee SPSite is released ASAP even in face of exception
                            if (siteCollection != null) siteCollection.Dispose();
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.Error(ex.Message, ex);
                }
                return true;
            }
        }

        /// <summary>
        /// Command which actually does activations and deactivations for features
        /// and also handles rollbacks
        /// </summary>
        private class FeaturesCommand : Command
        {
            private readonly FeatureActivator.ActivationDirection direction;
            private readonly InstallOperation operation;
            private FeatureLocations requestedLocs = null;

            public static Command CreateDeactivatorCommand(InstallProcessControl parent, InstallOperation operation)
            {
                FeaturesCommand cmd = new FeaturesCommand(parent, FeatureActivator.ActivationDirection.Deactivate, operation);
                return cmd;
            }
            public static Command CreateActivatorCommand(InstallProcessControl parent, InstallOperation operation)
            {
                FeaturesCommand cmd = new FeaturesCommand(parent, FeatureActivator.ActivationDirection.Activate, operation);
                return cmd;
            }
            private FeaturesCommand(InstallProcessControl parent, FeatureActivator.ActivationDirection direction, InstallOperation operation)
                : base(parent)
            {
                this.direction = direction;
                this.operation = operation;
                requestedLocs = InstallProcessControl.GetFeaturedLocations(operation);
            }

            internal override string Description { get { return FeatureActivator.Describe(requestedLocs, direction); } }

            /// <summary>
            /// Actually activate/deactivate users specified features on all listed locations
            /// </summary>
            protected internal override bool Execute()
            {
                log.Info(this.Description);

                ReadOnlyCollection<Guid?> featureIds = InstallConfiguration.FeatureIdList;
                if (featureIds == null || featureIds.Count == 0)
                {
                    log.Warn(CommonUIStrings.logNoFeaturesSpecified);
                    return true;
                }
                if (requestedLocs == null || requestedLocs.LocationsCount == 0)
                {
                    // caller responsible for giving message, depending on operation (install, upgrade,...) ?
                    log.Warn(CommonUIStrings.logNoLocationsForActivationDeactivation);
                    return true;
                }

                FeatureActivator.PerformActivations(direction, operation, log);

                return true;
            }
        }
        public static string GetFeatureName(Guid? featureId) { return FeatureActivator.GetFeatureName(featureId); }

    }
    class FeatureActivator
    {
        public static SPFeature ActivateOneFeature(SPFeatureCollection features, Guid featureId, ILog log)
        {
            bool force = true;
#if SP2013_UNUSED
            // Unneeded - 14/15 hive deployment is governed at solution deployment time
            // Full-blown solution, not sandboxed
            SPFeatureDefinitionScope featureScope = SPFeatureDefinitionScope.Farm;
            // Note: 2013-03-20
            // This does not appear to work, at least with an SP2010 wsp
            // it appears to always get activated at level 14
            // This article describes an undocumented workaround
            //  http://www.boostsolutions.com/blog/how-to-install-a-farm-solution-in-2010-and-2013-mode-for-sharepoint-2013/
            if (InstallConfiguration.CompatibilityLevel == "14" 
                || InstallConfiguration.CompatibilityLevel.ToUpper() == "OLD"
                || InstallConfiguration.CompatibilityLevel.ToUpper() == "OLDVERSION"
                || InstallConfiguration.CompatibilityLevel.ToUpper() == "OLDVERSIONS"
                )
            {
                log.Info("Activating feature at level 14: " + featureId.ToString());
                return features.Add(featureId, 14, force, featureScope);
            }
            else if (InstallConfiguration.CompatibilityLevel == "15"
                || InstallConfiguration.CompatibilityLevel.ToUpper() == "NEW"
                || InstallConfiguration.CompatibilityLevel.ToUpper() == "NEWVERSION"
                || InstallConfiguration.CompatibilityLevel.ToUpper() == "NEWVERSIONS"
                )
            {
                log.Info("Activating feature at level 15: " + featureId.ToString());
                return features.Add(featureId, 15, force, featureScope);
            }
            else if (InstallConfiguration.CompatibilityLevel == "14,15"
                || InstallConfiguration.CompatibilityLevel.ToUpper() == "ALL"
                || InstallConfiguration.CompatibilityLevel.ToUpper() == "ALLVERSIONS"
                )
            {
                log.Info("Activating feature at level 14 then 15: " + featureId.ToString());
                features.Add(featureId, 14, force, featureScope);
                return features.Add(featureId, 15, force, featureScope);
            }
            else
            {
                return features.Add(featureId, force);
            }
#else
            return features.Add(featureId, force);
#endif

        }
        public static string Describe(FeatureLocations locs, FeatureActivator.ActivationDirection direction)
        {
            string fmt = (direction == FeatureActivator.ActivationDirection.Activate ? CommonUIStrings.activatingFeaturesMessage
                : CommonUIStrings.deactivatingFeaturesMessage);
            return String.Format(fmt
                , locs.ActivationsCount
                , InstallConfiguration.FeatureIdList.Count
                , locs.LocationsCount
            );
        }

        public static void PerformActivations(ActivationDirection direction, InstallOperation operation, ILog log)
        {
            FeatureActivator factor = new FeatureActivator(direction, operation, log);
            try
            {
                factor.PerformAllActivations();
                return;
            }
            catch (Exception exc)
            {
                log.Error("Error: " + direction.ToString(), exc);
            }
            if (factor.rollback) return;
            if (factor.completedLocs.LocationsCount == 0)
            {
                log.Info("No feature de/activations completed, so none to rollback");
                return;
            }

            // Switch actions & execute rollback
            factor.rollback = true;
            factor.direction = (factor.direction == ActivationDirection.Activate ? ActivationDirection.Deactivate : ActivationDirection.Activate);
            factor.requestedLocs = factor.completedLocs;
            try
            {
                log.Info("Rollback: " + FeatureActivator.Describe(factor.requestedLocs, factor.direction));
                factor.PerformAllActivations();
            }
            catch (Exception exc)
            {
                log.Error("Error during rollback: " + direction.ToString(), exc);
            }

        }

        public enum ActivationDirection { Activate, Deactivate };

        private ActivationDirection direction;
        private InstallOperation operation;
        private readonly ILog log = null;
        private FeatureLocations requestedLocs = null;
        private FeatureLocations completedLocs = new FeatureLocations();
        private bool rollback = false;

        private FeatureActivator(ActivationDirection direction, InstallOperation operation, ILog log)
        {
            this.direction = direction;
            this.operation = operation;
            this.log = log;
            requestedLocs = InstallProcessControl.GetFeaturedLocations(operation);
        }
        private void PerformAllActivations()
        {
            // this.operation tells us which operation they're performing (Install, Repair, Upgrade, Uninstall)
            //   but we activate whatever features they specified
            // If they specify farm features, presumably they want to control the exact activation/deactivation order

            // From outer (farm) to inner
            
            if (requestedLocs.FarmLocations.Count > 0 && direction == ActivationDirection.Activate)
                DoFarmActivations();

            if (requestedLocs.WebAppLocations.Count > 0 && direction == ActivationDirection.Activate)
                DoWebAppActivations();

            if (requestedLocs.SiteLocations.Count > 0 && direction == ActivationDirection.Activate)
                DoSiteActivations();

            // inmost (web)
            
            if (requestedLocs.WebAppLocations.Count > 0)
                DoWebActivations();

            // from inner to outer (farm)

            if (requestedLocs.SiteLocations.Count > 0 && direction == ActivationDirection.Deactivate)
                DoSiteActivations();

            if (requestedLocs.WebAppLocations.Count > 0 && direction == ActivationDirection.Deactivate)
                DoWebAppActivations();
 

            if (requestedLocs.FarmLocations.Count > 0 && direction == ActivationDirection.Deactivate)
                DoFarmActivations();
        }
        private void DoFarmActivations()
        {
            string method = string.Format(CommonUIStrings.deactivateFarmFeatureMessage, requestedLocs.FarmFeatures.Count);
            if (direction == ActivationDirection.Activate)
                method = string.Format(CommonUIStrings.activateFarmFeatureMessage, requestedLocs.FarmFeatures.Count);

            string context = method;
            log.Info(context);
            try
            {
                if (requestedLocs.FarmLocations.Count > 1)
                {
                    throw new Exception("Error: requestedLocs.FarmLocations.Count > 1");
                }
                FeatureLoc floc = requestedLocs.FarmLocations[0];
                FeatureLoc completedLoc = FeatureLoc.CopyExceptFeatureList(floc);
                SPFarm farm = SPFarm.Local;
                foreach (Guid featureId in floc.featureList)
                {
                    context = method + " feature: " + featureId.ToString();

                    SPWebService adminService = SPWebService.AdministrationService;
                    if (direction == ActivationDirection.Activate)
                    {
                        if (IsFeatureActivatedOnFarm(farm, featureId))
                        {
                            string msg = string.Format(CommonUIStrings.ActivatingSkipped_FeatureAlreadyActiveOnFarm
                                , GetFeatureName(featureId));
                            log.Warn(msg);
                        }
                        else
                        {
                            ActivateOneFeature(adminService.Features, featureId, log);

                            if (!IsFeatureActivatedOnFarm(farm, featureId))
                            {
                                // do not add to completedLocs, b/c it didn't complete
                                log.Warn("Activation failed on farm : " + GetFeatureName(featureId));
                            }
                            else
                            {
                                AddFeatureIdToFeatureLoc(completedLoc, featureId);
                                log.Info(context + " : " + GetFeatureName(featureId));
                            }
                        }
                    }
                    else
                    {
                        if (IsFeatureActivatedOnFarm(farm, featureId))
                        {
                            adminService.Features.Remove(featureId, true);
                            AddFeatureIdToFeatureLoc(completedLoc, featureId);
                            log.Info(context + " : " + GetFeatureName(featureId));
                        }
                        else
                        {
                            string msg = string.Format(CommonUIStrings.DeactivatingSkipped_FeatureAlreadyActiveOnFarm
                                , GetFeatureName(featureId));
                            log.Warn(msg);
                        }
                    }
                }
                if (completedLoc.featureList != null)
                {
                    completedLocs.AddFeatureLocation(completedLoc);
                }
            }
            catch (Exception exc)
            {
                log.Error(context, exc);
                throw exc;
            }
        }
        private void DoWebAppActivations()
        {
            string method = string.Format(CommonUIStrings.deactivateWebAppFeatureMessage
                , requestedLocs.WebAppFeatures.Count, requestedLocs.WebAppLocations.Count);
            if (direction == ActivationDirection.Activate)
                method = string.Format(CommonUIStrings.activateWebAppFeatureMessage
                    , requestedLocs.WebAppFeatures.Count, requestedLocs.WebAppLocations.Count);

            string context = method;
            log.Info(context);
            try
            {
                foreach (FeatureLoc floc in requestedLocs.WebAppLocations)
                {
                    FeatureLoc completedLoc = FeatureLoc.CopyExceptFeatureList(floc);
                    SPWebApplication webapp = ActivationReporter.GetWebAppById(floc.WebAppId);
                    string webappName = ActivationReporter.GetWebAppName(webapp);

                    foreach (Guid featureId in floc.featureList)
                    {
                        context = method + " feature: " + featureId.ToString();

                        if (direction == ActivationDirection.Activate)
                        {
                            if (IsFeatureActivatedOnWebApp(webapp, featureId))
                            {
                                string msg = string.Format(CommonUIStrings.ActivatingSkipped_FeatureAlreadyActiveOnWebApp
                                    , GetFeatureName(featureId), webappName);
                                log.Warn(msg);
                            }
                            else
                            {
                                ActivateOneFeature(webapp.Features, featureId, log);

                                if (!IsFeatureActivatedOnWebApp(webapp, featureId))
                                {
                                    // do not add to completedLocs, b/c it didn't complete
                                    log.Warn("Activation failed on webapp : " + GetFeatureName(featureId));
                                }
                                else
                                {
                                    AddFeatureIdToFeatureLoc(completedLoc, featureId);
                                    log.Info(context + " : " + GetFeatureName(featureId));
                                }
                            }
                        }
                        else
                        {
                            if (IsFeatureActivatedOnWebApp(webapp, featureId))
                            {
                                webapp.Features.Remove(featureId, true);
                                AddFeatureIdToFeatureLoc(completedLoc, featureId);
                                log.Info(context + " : " + GetFeatureName(featureId));
                            }
                            else
                            {
                                string msg = string.Format(CommonUIStrings.DeactivatingSkipped_FeatureAlreadyActiveOnWebApp
                                    , GetFeatureName(featureId), webappName);
                                log.Warn(msg);
                            }
                        }
                    }
                    if (completedLoc.featureList != null)
                    {
                        completedLocs.AddFeatureLocation(completedLoc);
                    }
                }
            }
            catch (Exception exc)
            {
                log.Error(context, exc);
                throw exc;
            }
        }
        private void DoSiteActivations()
        {
            string method = string.Format(CommonUIStrings.deactivateSiteCollectionFeatureMessage
                , requestedLocs.SiteFeatures.Count, requestedLocs.SiteLocations.Count);
            if (direction == ActivationDirection.Activate)
                method = string.Format(CommonUIStrings.activateSiteCollectionFeatureMessage
                    , requestedLocs.SiteFeatures.Count, requestedLocs.SiteLocations.Count);

            string context = method;
            log.Info(context);
            try
            {
                foreach (FeatureLoc floc in requestedLocs.SiteLocations)
                {
                    FeatureLoc completedLoc = FeatureLoc.CopyExceptFeatureList(floc);
                    SPSite site = new SPSite(floc.SiteId);
                    string siteName = site.RootWeb.Url;

                    foreach (Guid featureId in floc.featureList)
                    {
                        context = method + " feature: " + featureId.ToString();

                        if (direction == ActivationDirection.Activate)
                        {
                            if (IsFeatureActivatedOnSite(site, featureId))
                            {
                                string msg = string.Format(CommonUIStrings.ActivatingSkipped_FeatureAlreadyActiveOnSite
                                    , GetFeatureName(featureId), siteName);
                                log.Warn(msg);
                            }
                            else
                            {
                                ActivateOneFeature(site.Features, featureId, log);

                                if (!IsFeatureActivatedOnSite(site, featureId))
                                {
                                    // do not add to completedLocs, b/c it didn't complete
                                    log.Warn("Activation failed on site : " + GetFeatureName(featureId));
                                }
                                else
                                {
                                    AddFeatureIdToFeatureLoc(completedLoc, featureId);
                                    log.Info(context + " : " + GetFeatureName(featureId));
                                }
                            }
                        }
                        else
                        {
                            if (IsFeatureActivatedOnSite(site, featureId))
                            {
                                site.Features.Remove(featureId, true);
                                AddFeatureIdToFeatureLoc(completedLoc, featureId);
                                log.Info(context + " : " + GetFeatureName(featureId));
                            }
                            else
                            {
                                string msg = string.Format(CommonUIStrings.DeactivatingSkipped_FeatureAlreadyActiveOnSite
                                    , GetFeatureName(featureId), siteName);
                                log.Warn(msg);
                            }
                        }
                    }
                    if (completedLoc.featureList != null)
                    {
                        completedLocs.AddFeatureLocation(completedLoc);
                    }
                }
            }
            catch (Exception exc)
            {
                log.Error(context, exc);
                throw exc;
            }
        }
        private void DoWebActivations()
        {
            string method = string.Format(CommonUIStrings.deactivateWebFeatureMessage
                , requestedLocs.WebFeatures.Count, requestedLocs.WebLocations.Count);
            if (direction == ActivationDirection.Activate)
                method = string.Format(CommonUIStrings.activateWebFeatureMessage
                    , requestedLocs.WebFeatures.Count, requestedLocs.WebLocations.Count);

            string context = method;
            log.Info(context);
            try
            {
                foreach (FeatureLoc floc in requestedLocs.WebLocations)
                {
                    FeatureLoc completedLoc = FeatureLoc.CopyExceptFeatureList(floc);

                    SPSite site = null;
                    SPWeb web = null;
                    try
                    {
                        site = new SPSite(floc.SiteId);
                        web = site.OpenWeb(floc.WebId);
                        string webName = web.Url;

                        foreach (Guid featureId in floc.featureList)
                        {
                            context = method + " feature: " + featureId.ToString();

                            if (direction == ActivationDirection.Activate)
                            {
                                if (IsFeatureActivatedOnWeb(web, featureId))
                                {
                                    string msg = string.Format(CommonUIStrings.ActivatingSkipped_FeatureAlreadyActiveOnWeb
                                        , GetFeatureName(featureId), webName);
                                    log.Warn(msg);
                                }
                                else
                                {
                                    ActivateOneFeature(web.Features, featureId, log);

                                    if (!IsFeatureActivatedOnWeb(web, featureId))
                                    {
                                        // do not add to completedLocs, b/c it didn't complete
                                        log.Warn("Activation failed on web : " + GetFeatureName(featureId));
                                    }
                                    else
                                    {
                                        AddFeatureIdToFeatureLoc(completedLoc, featureId);
                                        log.Info(context + " : " + GetFeatureName(featureId));
                                    }
                                }
                            }
                            else
                            {
                                if (IsFeatureActivatedOnWeb(web, featureId))
                                {
                                    web.Features.Remove(featureId, true);
                                    AddFeatureIdToFeatureLoc(completedLoc, featureId);
                                    log.Info(context + " : " + GetFeatureName(featureId));
                                }
                                else
                                {
                                    string msg = string.Format(CommonUIStrings.DeactivatingSkipped_FeatureAlreadyActiveOnWeb
                                        , GetFeatureName(featureId), webName);
                                    log.Warn(msg);
                                }
                            }
                        }
                        if (completedLoc.featureList != null)
                        {
                            completedLocs.AddFeatureLocation(completedLoc);
                        }
                    }
                    finally
                    {
                        if (web != null)
                        {
                            web.Dispose();
                            web = null;
                        }
                        if (site != null)
                        {
                            site.Dispose();
                            site = null;
                        }
                    }
                }
            }
            catch (Exception exc)
            {
                log.Error(context, exc);
                throw exc;
            }
        }
        public static bool IsFeatureActivatedOnFarm(SPFarm farm, Guid featureId)
        {
            return isFeatureActivated(SPWebService.AdministrationService.Features, featureId);
        }
        public static bool IsFeatureActivatedOnWebApp(SPWebApplication webapp, Guid featureId)
        {
            return isFeatureActivated(webapp.Features, featureId);
        }
        public static bool IsFeatureActivatedOnSite(SPSite sitex, Guid featureId)
        {
            // Create new site object to get updated feature data to check
            SPSite site = null;
            try
            {
                site = new SPSite(sitex.ID);
                return isFeatureActivated(site.Features, featureId);
            }
            finally
            {
                if (site != null)
                    site.Dispose();
            }
        }
        public static bool IsFeatureActivatedOnWeb(SPWeb webx, Guid featureId)
        {
            // Create new site object to get updated feature data to check
            SPSite site = null;
            SPWeb web = null;
            try
            {
                site = new SPSite(webx.Site.ID);
                web = site.OpenWeb(webx.ID);
                return isFeatureActivated(web.Features, featureId);
            }
            finally
            {
                if (web != null)
                    web.Dispose();
                if (site != null)
                    site.Dispose();
            }
        }
        private static bool isFeatureActivated(SPFeatureCollection features, Guid featureId)
        {
            foreach (SPFeature feature in features)
            {
                if (feature.Definition.Id == featureId)
                {
                    return true;
                }
            }
            return false;
        }
        private void AddFeatureIdToFeatureLoc(FeatureLoc floc, Guid featureId)
        {
            if (floc.featureList == null)
            {
                floc.featureList = new List<Guid>();
            }
            floc.featureList.Add(featureId);
        }
        public static string GetFeatureName(Guid? featureId)
        {
            if (featureId == null) return "NULL feature ID";
            SPFeatureDefinition fdef = SPFarm.Local.FeatureDefinitions[featureId.Value];
            if (fdef == null)
                return featureId.ToString();
            return fdef.DisplayName + " (" + featureId.ToString() + ")";
        }

    }
}
