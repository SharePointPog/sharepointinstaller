/******************************************************************/
/*                                                                */
/*                SharePoint Solution Installer                   */
/*                                                                */
/*    Copyright 2007 Lars Fastrup Nielsen. All rights reserved.   */
/*    http://www.fastrup.dk                                       */
/*                                                                */
/*    This program contains the confidential trade secret         */
/*    information of Lars Fastrup Nielsen.  Use, disclosure, or   */
/*    copying without written consent is strictly prohibited.     */
/*                                                                */
/* KML: Added SiteCollectionFeatureId                             */
/* KML: Updated InstallOperation enum to be public                */
/* KML: Added BackWardCompatibilityConfigProps                    */
/* KML: Added ConfigProps                                         */
/* KML: Added RequireMoss, FeatureScope, SolutionId, FeatureId,   */
/*      SiteCollectionRelativeConfigLink, SSPRelativeConfigLink,  */
/*      DefaultDeployToSRP, and DocumentationUrl properties       */
/* KML: Added ConfigProps                                         */
/*                                                                */
/******************************************************************/
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Text;

using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using System.Data.SqlClient;
using CodePlex.SharePointInstaller.Resources;


namespace CodePlex.SharePointInstaller
{
  internal class InstallConfiguration
  {
    #region Constants

    public class BackwardCompatibilityConfigProps
    {
      // "Apllication" mispelled on purpose to match original mispelling released
      public const string RequireDeploymentToCentralAdminWebApllication = "RequireDeploymentToCentralAdminWebApllication";
      // Require="MOSS" = RequireMoss="true" 
      public const string Require = "Require";
      // FarmFeatureId = FeatureId with FeatureScope = Farm
      public const string FarmFeatureId = "FarmFeatureId";
    }

    public class ConfigProps
    {
      public const string BannerImage = "BannerImage";
      public const string LogoImage = "LogoImage";
      public const string EULA = "EULA";
      public const string RequireMoss = "RequireMoss";
      public const string MinSharePointVersion = "MinSharePointVersion";
      public const string MaxSharePointVersion = "MaxSharePointVersion";
      public const string UpgradeDescription = "UpgradeDescription";
      public const string RequireDeploymentToCentralAdminWebApplication = "RequireDeploymentToCentralAdminWebApplication";
      public const string RequireDeploymentToAllContentWebApplications = "RequireDeploymentToAllContentWebApplications";
      public const string DefaultDeployToSRP = "DefaultDeployToSRP";
      public const string DefaultDeployToAdminWebApplications = "DefaultDeployToAdminWebApplications";
      public const string DefaultDeployToContentWebApplications = "DefaultDeployToContentWebApplications";
      public const string PromptForWebApplications = "PromptForWebApplications";
      public const string DefaultUpgrade = "DefaultUpgrade";
      public const string SolutionId = "SolutionId";
      public const string SolutionFile = "SolutionFile";
      public const string SolutionTitle = "SolutionTitle";
      public const string SolutionVersion = "SolutionVersion";
      public const string FeatureScope = "FeatureScope";
      public const string FeatureId = "FeatureId";
      public const string SiteCollectionRelativeConfigLink = "SiteCollectionRelativeConfigLink";
      public const string SSPRelativeConfigLink = "SSPRelativeConfigLink";
      public const string DocumentationUrl = "DocumentationUrl";
      public const string CompatibilityLevel = "CompatibilityLevel";
    }

    #endregion

    #region Internal Static Properties

    internal static string BannerImage
    {
      get { return ConfigurationManager.AppSettings[ConfigProps.BannerImage]; }
    }

    internal static string LogoImage
    {
      get { return ConfigurationManager.AppSettings[ConfigProps.LogoImage]; }
    }

    internal static string EULA
    {
      get { return ConfigurationManager.AppSettings[ConfigProps.EULA]; }
    }

    internal static bool RequireMoss
    {
      get
      {
        bool rtnValue = false;
        string valueStr = ConfigurationManager.AppSettings[ConfigProps.RequireMoss];
        if (String.IsNullOrEmpty(valueStr))
        {
          valueStr = ConfigurationManager.AppSettings[BackwardCompatibilityConfigProps.Require];
          rtnValue = valueStr != null && valueStr.Equals("MOSS", StringComparison.OrdinalIgnoreCase);
        }
        else
        {
          rtnValue = Boolean.Parse(valueStr);
        }
        return rtnValue;
      }
    }

    public static string Stringify(string str) { return str == null ? "" : str; }
    private static void VerifyVersionString(string str)
    {
      try
      {
        Version vtest = new Version(str);
      }
      catch
      {
        throw new Exception("Invalid version string: " + str);
      }
    }

    internal static string MinSharePointVersion
    {
      get
      {
        string str = Stringify(ConfigurationManager.AppSettings[ConfigProps.MinSharePointVersion]);
        if (str != "") VerifyVersionString(str);
        return str;
      }
    }

    internal static string MaxSharePointVersion
    {
      get
      {
        string str = Stringify(ConfigurationManager.AppSettings[ConfigProps.MaxSharePointVersion]);
        if (str != "") VerifyVersionString(str);
        return str;
      }
    }

    internal static Guid SolutionId
    {
      get { return new Guid(ConfigurationManager.AppSettings[ConfigProps.SolutionId]); }
    }

    private static bool _solutionInstalled = false;
    internal static bool SolutionInstalled
    {
        get { return _solutionInstalled; }
        set { _solutionInstalled = value; }
    }

    internal static string SolutionFile
    {
      get { return ConfigurationManager.AppSettings[ConfigProps.SolutionFile]; }
    }

    internal static string SolutionTitle
    {
      get { return ConfigurationManager.AppSettings[ConfigProps.SolutionTitle]; }
    }

    internal static Version SolutionVersion
    {
      get { return new Version(ConfigurationManager.AppSettings[ConfigProps.SolutionVersion]); }
    }

    internal static string UpgradeDescription
    {
      get
      {
        string str = ConfigurationManager.AppSettings[ConfigProps.UpgradeDescription];
        if (str != null)
        {
          str = FormatString(str);
        }
        return str;
      }
    }

    internal static SPFeatureScope FeatureScope
    {
      get
      {
        // Default to farm features as this is what the installer only supported initially
        SPFeatureScope featureScope = SPFeatureScope.Farm;
        string valueStr = ConfigurationManager.AppSettings[ConfigProps.FeatureScope];
        if (!String.IsNullOrEmpty(valueStr))
        {
          featureScope = (SPFeatureScope)Enum.Parse(typeof(SPFeatureScope), valueStr, true);
        }
        return featureScope;
      }
    }

    /// <summary>
    /// Dictionary of all scopes represented by installed features specified in config file
    /// </summary>
    public class FeatureScopeInfoType
    {
        Dictionary<Microsoft.SharePoint.SPFeatureScope, int> ScopeTable = new Dictionary<SPFeatureScope, int>();
        public void AddFeatureScope(Microsoft.SharePoint.SPFeatureScope scope)
        {
            int count = 0;
            if (ScopeTable.ContainsKey(scope))
            {
                count = ScopeTable[scope];
            }
            ScopeTable[scope] = count + 1;
        }
        public bool HasNonFarmFeatures
        {
            get
            {
                if (ScopeTable.Count > 1) return true;
                if (ScopeTable.Count == 1 && !ScopeTable.ContainsKey(Microsoft.SharePoint.SPFeatureScope.Farm)) return true;
                return false;
            }
        }
        public bool HasScope(Microsoft.SharePoint.SPFeatureScope scope)
        {
            return ScopeTable.ContainsKey(scope);
        }
    }

  /// <summary>
  /// singleton feature scope info object (so we can cache the info after computing it once)
  /// </summary>
    static FeatureScopeInfoType theFeatureScopeInfo = null;

    /// <summary>
    /// Check all specified features & report info about them -- what scopes are represented
    /// </summary>
    /// <returns></returns>
    private static FeatureScopeInfoType GetFeatureScopeInfo()
    {
        FeatureScopeInfoType fsinfo = new FeatureScopeInfoType();
        if (InstallConfiguration.FeatureIdList != null)
        {
            foreach (Guid? guid in InstallConfiguration.FeatureIdList)
            {
                if (guid == null) continue; // Perry, 2010-10-06: I don't know why we allow null GUIDs in this list anyway
                SPFeatureDefinition fdef = SPFarm.Local.FeatureDefinitions[guid.Value];
                if (fdef != null)
                {
                    fsinfo.AddFeatureScope(fdef.Scope);
                }
            }
        }
        return fsinfo;
    }

    /// <summary>
    /// Property access to feature scope info, wrapping up the caching
    /// </summary>
    private static FeatureScopeInfoType FeatureScopeInfo
    {
        get
        {
            if (theFeatureScopeInfo == null)
                theFeatureScopeInfo = GetFeatureScopeInfo();
            return theFeatureScopeInfo;
        }
    }

    internal static bool HasFeatureScope(Microsoft.SharePoint.SPFeatureScope scope)
    {
        return FeatureScopeInfo.HasScope(scope);
    }

    // Modif JPI - Début
    private static List<Guid?> _theFeatureIdList = null;
    internal static ReadOnlyCollection<Guid?> FeatureIdList
    {
      get
      {
          if (_theFeatureIdList != null) { return _theFeatureIdList.AsReadOnly(); }

          _theFeatureIdList = new List<Guid?>();
          string valueStr = ConfigurationManager.AppSettings[ConfigProps.FeatureId];
        //
        // Backwards compatibility with old configuration files before site collection features allowed
        //
        if (String.IsNullOrEmpty(valueStr))
        {
          valueStr = ConfigurationManager.AppSettings[BackwardCompatibilityConfigProps.FarmFeatureId];
        }

        if (!String.IsNullOrEmpty(valueStr))
        {
            string[] _strGuidArray = valueStr.Split(";".ToCharArray());
            if (_strGuidArray.Length >= 0)
            {
                foreach (string _strGuid in _strGuidArray)
                {
                    _theFeatureIdList.Add(new Guid(_strGuid));
                }
            }
        }
        return _theFeatureIdList.AsReadOnly();
      }
    }
    // Modif JPI - Fin

    internal static bool RequireDeploymentToCentralAdminWebApplication
    {
      get
      {
        string valueStr = ConfigurationManager.AppSettings[ConfigProps.RequireDeploymentToCentralAdminWebApplication];

        //
        // Backwards compatability with old configuration files containing spelling error in the 
        // application setting key (Bug 990).
        //
        if (String.IsNullOrEmpty(valueStr))
        {
          valueStr = ConfigurationManager.AppSettings[BackwardCompatibilityConfigProps.RequireDeploymentToCentralAdminWebApllication];
        }

        if (!String.IsNullOrEmpty(valueStr))
        {
          return valueStr.Equals("true", StringComparison.OrdinalIgnoreCase);
        }

        return false;
      }
    }

    internal static bool RequireDeploymentToAllContentWebApplications
    {
      get
      {
        string valueStr = ConfigurationManager.AppSettings[ConfigProps.RequireDeploymentToAllContentWebApplications];
        if (!String.IsNullOrEmpty(valueStr))
        {
          return valueStr.Equals("true", StringComparison.OrdinalIgnoreCase);
        }
        return false;
      }
    }

    internal static bool DefaultDeployToSRP
    {
      get
      {
        string valueStr = ConfigurationManager.AppSettings[ConfigProps.DefaultDeployToSRP];
        if (!String.IsNullOrEmpty(valueStr))
        {
          return valueStr.Equals("true", StringComparison.OrdinalIgnoreCase);
        }
        return false;
      }
    }

    internal static bool DefaultDeployToAdminWebApplications
    {
      get
      {
        string valueStr = ConfigurationManager.AppSettings[ConfigProps.DefaultDeployToAdminWebApplications];
        if (!String.IsNullOrEmpty(valueStr))
        {
          return valueStr.Equals("true", StringComparison.OrdinalIgnoreCase);
        }
        return false;
      }
    }

    internal static bool DefaultDeployToContentWebApplications
    {
      get
      {
        string valueStr = ConfigurationManager.AppSettings[ConfigProps.DefaultDeployToContentWebApplications];
        if (!String.IsNullOrEmpty(valueStr))
        {
          return valueStr.Equals("true", StringComparison.OrdinalIgnoreCase);
        }
        return false;
      }
    }

    internal static bool PromptForWebApplications
    {
      get
      {
        string valueStr = ConfigurationManager.AppSettings[ConfigProps.PromptForWebApplications];
        if (!String.IsNullOrEmpty(valueStr))
        {
          return valueStr.Equals("true", StringComparison.OrdinalIgnoreCase);
        }
        return false;
      }
    }

    internal static string CompatibilityLevel
    {
        get
        {
            string str = Stringify(ConfigurationManager.AppSettings[ConfigProps.CompatibilityLevel]);
            return str;
        }
    }


    public static bool DefaultUpgrade
    {
      get
      {
        string valueStr = ConfigurationManager.AppSettings[ConfigProps.DefaultUpgrade];
        if (!String.IsNullOrEmpty(valueStr))
        {
          return valueStr.Equals("true", StringComparison.OrdinalIgnoreCase);
        }
        return false;
      }
    }

    internal static Version InstalledVersion
    {
      get
      {
        try
        {
          SPFarm farm = SPFarm.Local;
          string key = "Solution_" + SolutionId.ToString() + "_Version";
          return farm.Properties[key] as Version;
        }

        catch (NullReferenceException ex)
        {
          throw new InstallException(CommonUIStrings.installExceptionDatabase, ex);
        }

        catch (SqlException ex)
        {
          throw new InstallException(ex.Message, ex);
        }
      }

      set
      {
        try
        {
          SPFarm farm = SPFarm.Local;
          string key = "Solution_" + SolutionId.ToString() + "_Version";
          farm.Properties[key] = value;
          farm.Update();
        }

        catch (NullReferenceException ex)
        {
            throw new InstallException(CommonUIStrings.installExceptionDatabase, ex);
        }

        catch (SqlException ex)
        {
          throw new InstallException(ex.Message, ex);
        }
      }
    }

    public static bool ShowFinishedControl
    {
      get
      {
        return !String.IsNullOrEmpty(ConfigurationManager.AppSettings[ConfigProps.SiteCollectionRelativeConfigLink]) ||
          !String.IsNullOrEmpty(ConfigurationManager.AppSettings[ConfigProps.SSPRelativeConfigLink]);
      }
    }

    public static string SiteCollectionRelativeConfigLink
    {
      get { return ConfigurationManager.AppSettings[ConfigProps.SiteCollectionRelativeConfigLink]; }
    }

    public static string SSPRelativeConfigLink
    {
      get { return ConfigurationManager.AppSettings[ConfigProps.SSPRelativeConfigLink]; }
    }

    public static string DocumentationUrl
    {
      get { return ConfigurationManager.AppSettings[ConfigProps.DocumentationUrl]; }
    }

    #endregion

    #region Internal Static Methods

    internal static string FormatString(string str)
    {
      return FormatString(str, null);
    }

    internal static string FormatString(string str, params object[] args)
    {
      string formattedStr = str;
      string solutionTitle = SolutionTitle;
      if (!String.IsNullOrEmpty(solutionTitle))
      {
        formattedStr = formattedStr.Replace("{SolutionTitle}", solutionTitle);
      }
      if (args != null)
      {
        formattedStr = String.Format(formattedStr, args);
      }
      return formattedStr;
    }

    #endregion
  }

  public enum InstallOperation
  {
    Install,
    Upgrade,
    Repair,
    Uninstall
  }
}
