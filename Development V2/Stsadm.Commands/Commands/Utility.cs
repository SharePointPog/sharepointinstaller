using System;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;

namespace Stsadm.Commands
{
    public class Utility
    {
        public static SPPrincipal ResolvePrincipal(SPWeb web, String name)
        {
            var lookupValue = ResolvePrincipalLookupValue(web, name);
            if (lookupValue != null)
            {
                if (lookupValue.User != null)
                    return lookupValue.User;

                return ResolveGroup(web, lookupValue.LookupValue);
            }
            return null;
        }

        public static SPGroup ResolveGroup(SPWeb web, string name)
        {
            try
            {
                return web.SiteGroups[name];
            }
            catch (SPException)
            {
                return null;
            }
        }

        public static SPFieldUserValue ResolvePrincipalLookupValue(SPWeb web, String name)
        {
            var principal = SPUtility.ResolvePrincipal(
                web, name,
                SPPrincipalType.User | SPPrincipalType.SecurityGroup | SPPrincipalType.SharePointGroup,
                SPPrincipalSource.All, null, false);

            return principal != null ? new SPFieldUserValue(web, principal.PrincipalId, principal.DisplayName) : null;
        }
    }
}
