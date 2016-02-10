using System;
using System.Collections.Specialized;
using System.Text;
using CodePlex.SharePointInstaller.CommandsApi.OperationHelpers;
using CodePlex.SharePointInstaller.CommandsApi.SPValidators;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;

namespace Stsadm.Commands
{
    public class ActivateFeatureCommand : SPOperation
    {
        #region / Param Constants /

        public const String UrlParam = "url";
        public const String FeatureIdParam = "featureid";
        public const String FeatureScopeParam = "scope";

        #endregion

        public ActivateFeatureCommand()
        {
            var parameters = new SPParamCollection
                                 {
                                     new SPParam(UrlParam, UrlParam, true, null, new SPUrlValidator(), "Please specify url to the site."),
                                     new SPParam(FeatureIdParam, FeatureIdParam, true, null, new SPGuidValidator(), "Please specify feature id."),
                                     new SPParam(FeatureScopeParam, FeatureScopeParam, true, null, new SPEnumValidator(typeof(SPFeatureScope)), "Please specify scope of the feature.")
                                 };

            var sb = new StringBuilder();
            sb.Append("\r\n\r\nActivate feature.\r\n\r\nParameters:");
            sb.AppendFormat("\r\n\t-{0}", UrlParam);
            sb.AppendFormat("\r\n\t-{0}", FeatureIdParam);
            sb.AppendFormat("\r\n\t-{0}", FeatureScopeParam);
            Init(parameters, sb.ToString());
        }

        public override int Execute(string command, StringDictionary keyValues, out string output)
        {
            output = string.Empty;
            try
            {
                var featureId = new Guid(Params[FeatureIdParam].Value);
                var scope = (SPFeatureScope)Enum.Parse(typeof (SPFeatureScope), Params[FeatureScopeParam].Value, true);
                switch (scope)
                {
                    case SPFeatureScope.Farm:
                        ActivateFeature(SPWebService.AdministrationService.Features, featureId, scope, out output);
                        break;

                    case SPFeatureScope.WebApplication:
                        ActivateFeature(RetrieveWebApplicationFeatures(), featureId, scope, out output);
                        break;

                    case SPFeatureScope.Web:
                        ActivateFeature(RetrieveWebFeatures(), featureId, scope, out output);
                        break;

                    case SPFeatureScope.Site:
                        ActivateFeature(RetrieveSiteFeatures(), featureId, scope, out output);
                        break;
                }

                return OUTPUT_SUCCESS;
            }
            catch (Exception exc)
            {
                output = String.Format("Failed to execute command. {0}", exc);
            }

            return OUTPUT_FAILED;
        }

        private SPFeatureCollection RetrieveSiteFeatures()
        {
            using (var spSite = new SPSite(Params[UrlParam].Value))
            {
                return spSite.Features;
            }
        }

        private SPFeatureCollection RetrieveWebFeatures()
        {
            using (var spSite = new SPSite(Params[UrlParam].Value))
            using (var spWeb = spSite.OpenWeb())
            {
                return spWeb.Features;
            }
        }

        private SPFeatureCollection RetrieveWebApplicationFeatures()
        {
            using (var spSite = new SPSite(Params[UrlParam].Value))
            {
                return spSite.WebApplication.Features;
            }
        }

        private void ActivateFeature(SPFeatureCollection featureCollection, Guid featureId, SPFeatureScope scope, out string output)
        {
            var featureDef = GetFeatureDefinition(featureCollection, featureId);
            if (featureDef != null)
            {
                // feature was already activated
                output = string.Format("{2} feature ({0}) '{1}' is already activated.", featureDef.DisplayName, featureDef.Id, scope);
            }
            else
            {
                // activate feature
                var feature = featureCollection.Add(featureId, false);
                output = string.Format("{2} Farm feature ({0}) '{1}' was successfully activated.", feature.Definition.DisplayName, feature.Definition.Id, scope);
            }
        }

        private SPFeatureDefinition GetFeatureDefinition(SPFeatureCollection collection, Guid featureId)
        {
            foreach (SPFeature feature in collection)
            {
                if (feature.Definition != null && feature.Definition.Id == featureId)
                    return feature.Definition;
            }

            return null;
        }

        public override string GetHelpMessage(string command)
        {
            return HelpMessage;
        }
    }
}