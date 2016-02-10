using System;
using Microsoft.SharePoint;

namespace CodePlex.SharePointInstaller.Wrappers
{
    public class SiteCollectionInfo : EntityInfo
    {
        public SiteCollectionInfo(SPSite siteCollection) : base(siteCollection.ID, siteCollection.Url)
        {
            Corrupted = false;
            SetDescription(siteCollection);
        }

        protected void SetDescription(SPSite siteCollection)
        {
            string title;
            try
            {
                title = siteCollection.RootWeb.Title;
            }
            catch (UnauthorizedAccessException)
            {
                title = "<cannot access root web - access denied>";
                Corrupted = true;
            }
            catch (Exception ex)
            {
                title = "<" + ex.Message + ">";
                Corrupted = true;
            }
            Description = String.Format("{0}    ({1})", siteCollection.Url, title);
        }

        public bool Equals(SiteCollectionInfo obj)
        {
            return base.Equals(obj);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return Equals(obj as SiteCollectionInfo);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static bool operator ==(SiteCollectionInfo left, SiteCollectionInfo right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(SiteCollectionInfo left, SiteCollectionInfo right)
        {
            return !Equals(left, right);
        }
    }
}
