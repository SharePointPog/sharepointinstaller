using System;
using System.Configuration;
using CodePlex.SharePointInstaller.Logging;
using CodePlex.SharePointInstaller.Utils;
using CodePlex.SharePointInstaller.Wrappers;
using Microsoft.SharePoint;

namespace CodePlex.SharePointInstaller.Configuration
{
    public class SolutionInfo : ConfigurationElement
    {
        [ConfigurationProperty("parameters")]
        [ConfigurationCollection(typeof(ParametersCollection), AddItemName = "parameter")]
        public ParametersCollection Parameters
        {
            get
            {
                return (ParametersCollection)base["parameters"];
            }
            set
            {
                base["parameters"] = value;
            }
        }

        [ConfigurationProperty("features")]
        [ConfigurationCollection(typeof(FeatureInfoCollection), AddItemName = "feature")]
        public FeatureInfoCollection Features
        {
            get
            {
                return (FeatureInfoCollection)base["features"];
            }
            set
            {
                base["features"] = value;
            }
        }

        [ConfigurationProperty("run", IsDefaultCollection = true)]
        [ConfigurationCollection(typeof(CommandsCollection<CommandToRun>), CollectionType = ConfigurationElementCollectionType.BasicMap, AddItemName = "command")]
        public CommandsCollection<CommandToRun> CommandsToRun
        {
            get
            {
                return (CommandsCollection<CommandToRun>)base["run"];
            }
            set
            {
                base["run"] = value;
            }
        }

        [ConfigurationProperty("id", IsRequired = true)]        
        public Guid Id
        {
            get
            {
                return (Guid)this["id"];
            }
        }

        private InstallAction? action;
        private SiteInfo defaultTarget;
        private bool targetSet;

        private String GetParameterValue(String name)
        {
            return Parameters[name] != null ? Parameters[name].Value : String.Empty;
        }

        public String File
        {
            get
            {
                return GetParameterValue("File");
            }
        }

        public String FileName
        {
            get
            {
                var fileName = FileUtil.GetFileName(File);
                return !string.IsNullOrEmpty(fileName) ? fileName : "no file";
            }
        }

        public String Title
        {
            get
            {
                return GetParameterValue("Title");
            }
        }

        public String Version
        {
            get
            {
                return GetParameterValue("Version");
            }
        }

        public String Url
        {
            get
            {
                return GetParameterValue("Url");
            }
        }

        public bool ReactivateFeatures
        {
            get
            {
                bool reactivateFeatures;
                return Boolean.TryParse(GetParameterValue("ReactivateFeatures"), out reactivateFeatures) ? reactivateFeatures : true;
            }
        }

        public SiteInfo DefaultTarget
        {
            get
            {
                if (!targetSet && !String.IsNullOrEmpty(Url))
                {
                    SPSite site = null;
                    try
                    {
                        site = new SPSite(Url);
                        using (SPWeb web = site.OpenWeb())
                        {
                            if (web != null)
                            {
                                Uri webUri = new Uri(web.Url);
                                Uri configUri = new Uri(Url); //need to compare URI otherwise it opens web for URL substring
                                if (webUri.AbsolutePath == configUri.AbsolutePath)
                                    defaultTarget = new SiteInfo(web);
                            }
                        }
                    }
                    catch(Exception e)
                    {
                        Log.Info(String.Format("Unable to locate a web site under URL {0} for solution {1}. No pre-selection is made.", Url, Id), e);
                    }
                    finally
                    {
                        if (site != null)
                            site.Dispose();
                        targetSet = true;
                    }
                }
                return defaultTarget;
            }
        }

        public InstallAction Action
        {
            get
            {
                if (action == null)
                {
                    try
                    {
                        var actionName = GetParameterValue("Action");
                        if (actionName != String.Empty)
                            action = (InstallAction)Enum.Parse(typeof(InstallAction), actionName);
                    }
                    catch (ArgumentException e)
                    {
                        Log.Error("Unable to parse the default action. Ignoring it.", e);
                        action = InstallAction.Install;
                    }
                }
                return action ?? InstallAction.Install;
            }
            set
            {
                action = value;
            }
        }

        public override String ToString()
        {
            return Title;
        }
    }
}
