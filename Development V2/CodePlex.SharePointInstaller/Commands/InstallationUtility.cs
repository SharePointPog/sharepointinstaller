using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using CodePlex.SharePointInstaller.Configuration;
using CodePlex.SharePointInstaller.Resources;
using Microsoft.SharePoint.Administration;

namespace CodePlex.SharePointInstaller.Commands
{
    public class InstallationUtility
    {
        private const string SolutionTitlePlaceholder = "{SolutionTitle}";
        private const string WelcomeVersionPlaceholder = "{WelcomeVersion}";

        public static Version GetInstalledVersion(SolutionInfo solution)
        {
            try
            {
                return SPFarm.Local.Properties["Solution_" + solution.Id + "_Version"] as Version;
            }
            catch (NullReferenceException ex)
            {
                throw new InstallException(CommonUIStrings.InstallExceptionDatabase, ex);
            }
            catch (SqlException ex)
            {
                throw new InstallException(ex.Message, ex);
            }
        }

        public static void SetInstalledVersion(SolutionInfo solution, Version version)
        {
            try
            {
                var farm = SPFarm.Local;                
                farm.Properties["Solution_" + solution.Id + "_Version"] = version;
                farm.Update();
            }
            catch (NullReferenceException ex)
            {
                throw new InstallException(CommonUIStrings.InstallExceptionDatabase, ex);
            }
            catch (SqlException ex)
            {
                throw new InstallException(ex.Message, ex);
            }
        }

        public static String FormatString(SolutionInfo solutionInfo, String str)
        {
            return FormatString(solutionInfo, str, null);
        }

        public static String FormatString(SolutionInfo solutionInfo, String str, params object[] args)
        {
            if (!String.IsNullOrEmpty(solutionInfo.Title))
            {
                str = str.Replace(SolutionTitlePlaceholder, solutionInfo.Title);
            }
            if (args != null)
            {
                str = String.Format(str, args);
            }
            return str;
        }

        public static String[] GetTitles(SolutionInfoCollection solutions)
        {
            var titles = new List<String>();
            foreach(SolutionInfo solutionInfo in solutions)
            {
                titles.Add(solutionInfo.Title);
            }
            return titles.ToArray();
        }

        public static String ReplaceSolutionTitles(String str, SolutionInfoCollection solutions)
        {
            return str.Replace(SolutionTitlePlaceholder, String.Join(", ", GetTitles(solutions)));
        }

        public static String ReplaceSolutionTitle(String str, string title)
        {
            return str.Replace(SolutionTitlePlaceholder, title);
        }

        public static String ReplaceWelcomeVersion(String str)
        {
#if SP2010
            return str.Replace(WelcomeVersionPlaceholder, CommonUIStrings.WelcomeVersion2010);
#else
            return str.Replace(WelcomeVersionPlaceholder, CommonUIStrings.WelcomeVersion2007);
#endif
        }

        public static void WriteLog(string msg)
        {
#if DEBUG
            Logging.Log.WriteTrace(msg);
#endif
        }
    }
}
