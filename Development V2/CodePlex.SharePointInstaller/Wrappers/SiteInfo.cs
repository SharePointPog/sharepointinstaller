using System;
using System.IO;
using CodePlex.SharePointInstaller.Logging;
using Microsoft.SharePoint;

namespace CodePlex.SharePointInstaller.Wrappers
{
    public class SiteInfo : EntityInfo
    {
        public SiteInfo(SPWeb site) : base(site.ID, site.Url)
        {
            SiteCollectionId = site.Site.ID;
            WebApplicationId = site.Site.WebApplication.Id;
            IsRootSite = site.IsRootWeb;
            SetDescription(site);
        }        

        protected void SetDescription(SPWeb site)
        {
            try
            {
                Description = site.Title;
            }
            catch (UnauthorizedAccessException)
            {
                Description = "<cannot access web - access denied>";
                Corrupted = true;
            }
            catch (Exception ex)
            {
                Description = "<" + ex.Message + ">";
                Corrupted = true;
            }
            Description = String.Format(site.IsRootWeb ? "{0}    ({1} - Root Web)" : "{0}    ({1})", site.Url, Description);
        }

        public Guid SiteCollectionId
        {
            get; 
            set;
        }

        public Guid WebApplicationId
        {
            get;
            private set;
        }

        public bool IsRootSite
        {
            get; 
            set;
        }

        public bool Equals(SiteInfo obj)
        {
            return base.Equals(obj);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return Equals(obj as SiteInfo);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static bool operator ==(SiteInfo left, SiteInfo right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(SiteInfo left, SiteInfo right)
        {
            return !Equals(left, right);
        }

        public static bool IsAccessible(SPWeb web)
        {
            try
            {
                return web.DoesUserHavePermissions(SPBasePermissions.ViewPages | SPBasePermissions.ManageWeb);
            }
            catch (UnauthorizedAccessException e)
            {
                Log.Info(String.Format("Skipping restricted site/collection {0}.", web.Url), e);
            }
            catch (SPException e)
            {
                Log.Info(String.Format("Skipping inaccessible site/collection {0}.", web.Url), e);
            }
            return false;
        }
    }
}
