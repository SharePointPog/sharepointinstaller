using System;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using CodePlex.SharePointInstaller.Logging;
using Microsoft.SharePoint.StsAdmin;


namespace CodePlex.SharePointInstaller.Configuration
{
    public class InstallConfiguration : ConfigurationSection
    {
        public struct Placeholders
        {
            public static string WebUrl = "{WebURL}";
            public static string SiteUrl = "{SiteURL}";
            public static string AppUrl = "{AppURL}";
        }

        private CommandsConfiguration availableCommandsConfiguration;

        [ConfigurationProperty(ConfigProps.SolutionsTag)]
        [ConfigurationCollection(typeof(SolutionInfoCollection), AddItemName = ConfigProps.SolutionTag)]
        public SolutionInfoCollection Solutions
        {
            get
            {
                return (SolutionInfoCollection)base[ConfigProps.SolutionsTag];
            }
            set
            {
                base[ConfigProps.SolutionsTag] = value;
            }
        }

        public static InstallConfiguration GetConfiguration()
        {
            var config = (InstallConfiguration)ConfigurationManager.GetSection(ConfigProps.SectionName);
            config.availableCommandsConfiguration = (CommandsConfiguration)ConfigurationManager.GetSection(ConfigProps.CommandsTag);
            return config;
        }

        public ISPStsadmCommand GetCommandToRun(CommandToRun commandToRun)
        {
            if ((commandToRun.CommandInstance == null))
            {
                foreach (CustomCommand command in availableCommandsConfiguration.CustomCommands)
                {
                    if (String.Compare(command.Name, commandToRun.Name, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        try
                        {
                            commandToRun.CommandInstance =
                                Activator.CreateInstance(Type.GetType(command.CommandType)) as ISPStsadmCommand;
                        }
                            // can recover from the following exceptions only
                        catch (ArgumentNullException e)
                        {
                            Log.Error(e);
                            return null;
                        }
                        catch (ArgumentException e)
                        {
                            Log.Error(e);
                            return null;
                        }
                        catch (NotSupportedException e)
                        {
                            Log.Error(e);
                            return null;
                        }
                        catch (TargetInvocationException e)
                        {
                            Log.Error(e);
                            return null;
                        }
                        catch (MethodAccessException e)
                        {
                            Log.Error(e);
                            return null;
                        }
                        catch (MemberAccessException e)
                        {
                            Log.Error(e);
                            return null;
                        }
                        catch (InvalidComObjectException e)
                        {
                            Log.Error(e);
                            return null;
                        }
                        catch (COMException e)
                        {
                            Log.Error(e);
                            return null;
                        }
                        catch (TypeLoadException e)
                        {
                            Log.Error(e);
                            return null;
                        }
                        catch (FileNotFoundException e)
                        {
                            Log.Error(e);
                            return null;
                        }
                        catch (FileLoadException e)
                        {
                            Log.Error(e);
                            return null;
                        }
                        catch (BadImageFormatException e)
                        {
                            Log.Error(e);
                            return null;
                        }
                    }
                }
            }
            return commandToRun.CommandInstance;
        }

        public class BackwardCompatibilityConfigProps
        {
            // "Apllication" mispelled on purpose to match original mispelling released
            public const string RequireDeploymentToCentralAdminWebApllication = "RequireDeploymentToCentralAdminWebApllication";
            // Require="MOSS" = RequireMoss="true" 
            public const string Require = "Require";
        }

        public class ConfigProps
        {
            public const String BannerImage = "BannerImage";
            public const String LogoImage = "LogoImage";
            public const String Vendor = "Vendor";
            public const String EULA = "EULA";
            public const String RequireMoss = "RequireMoss";
            public const String RequireSPS = "RequireSPS";
            public const String UpgradeDescription = "UpgradeDescription";
            public const String RequireDeploymentToCentralAdminWebApplication = "RequireDeploymentToCentralAdminWebApplication";
            public const String RequireDeploymentToAllContentWebApplications = "RequireDeploymentToAllContentWebApplications";
            public const String DefaultDeployToSRP = "DefaultDeployToSRP";
            public const String SolutionId = "SolutionId";
            public const String SolutionFile = "SolutionFile";
            public const String SolutionTitle = "SolutionTitle";
            public const String SolutionVersion = "SolutionVersion";
            public const String FeatureScope = "FeatureScope";
            public const String FeatureId = "FeatureId";
            public const String SiteCollectionRelativeConfigLink = "SiteCollectionRelativeConfigLink";
            public const String SSPRelativeConfigLink = "SSPRelativeConfigLink";
            public const String DocumentationUrl = "DocumentationUrl";
            public const String AllowUpgrade = "AllowUpgrade";
            public const String InstallationName = "InstallationName";
            public const String SectionName = "install";
            public const String SolutionsTag = "solutions";
            public const String SolutionTag = "solution";
            public const String CommandsTag = "commands";
        }

        public String InstallationName
        {
            get { return ConfigurationManager.AppSettings[ConfigProps.InstallationName]; }
        }        

        public bool AllowUpgrade
        {
            get { return Boolean.Parse(ConfigurationManager.AppSettings[ConfigProps.AllowUpgrade]); }
        }

        public String BannerImage
        {
            get { return ConfigurationManager.AppSettings[ConfigProps.BannerImage]; }
        }

        public String LogoImage
        {
            get { return ConfigurationManager.AppSettings[ConfigProps.LogoImage]; }
        }

        public String Vendor
        {
            get { return ConfigurationManager.AppSettings[ConfigProps.Vendor]; }
        }

        public String EULA //TODO: move into per solution parameters
        {
            get { return ConfigurationManager.AppSettings[ConfigProps.EULA]; }
        }

        public bool RequireMoss
        {
            get
            {
                var valueStr = ConfigurationManager.AppSettings[ConfigProps.RequireMoss];
                if (String.IsNullOrEmpty(valueStr))
                {
                    valueStr = ConfigurationManager.AppSettings[BackwardCompatibilityConfigProps.Require];
                    return valueStr != null && valueStr.Equals("MOSS", StringComparison.OrdinalIgnoreCase);
                }
                return Boolean.Parse(valueStr);
            }
        }

        public bool RequireSPS
        {
            get
            {
                var valueStr = ConfigurationManager.AppSettings[ConfigProps.RequireSPS];
                if (String.IsNullOrEmpty(valueStr))
                {
                    valueStr = ConfigurationManager.AppSettings[BackwardCompatibilityConfigProps.Require];
                    return valueStr != null && valueStr.Equals("SPS", StringComparison.OrdinalIgnoreCase);
                }
                return Boolean.Parse(valueStr);
            }
        }

        public String UpgradeDescription
        {
            get
            {
                return ConfigurationManager.AppSettings[ConfigProps.UpgradeDescription];                
            }
        }

        public bool RequireDeploymentToCentralAdminWebApplication
        {
            get
            {
                var valueStr = ConfigurationManager.AppSettings[ConfigProps.RequireDeploymentToCentralAdminWebApplication];

                //
                // Backwards compatability with old configuration files containing spelling error in the 
                // application setting key (Bug 990).
                //
                if (String.IsNullOrEmpty(valueStr))
                    valueStr = ConfigurationManager.AppSettings[BackwardCompatibilityConfigProps.RequireDeploymentToCentralAdminWebApllication];

                if (!String.IsNullOrEmpty(valueStr))
                    return valueStr.Equals("true", StringComparison.OrdinalIgnoreCase);

                return false;
            }
        }
    }

    public enum InstallAction { Install, Upgrade, Repair, Remove }
}