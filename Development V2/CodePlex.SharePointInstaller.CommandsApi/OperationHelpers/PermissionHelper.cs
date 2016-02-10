using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Principal;
using System.Text;
using System.Xml;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint.Utilities;
using Microsoft.SharePoint.WebPartPages;

namespace CodePlex.SharePointInstaller.CommandsApi.OperationHelpers
{
    public static class PermissionHelper
    {
        public static bool AddRoleDefinitions(SPWeb web, SPRoleAssignment roleAssignment, XmlElement roleAssignmentElement)
        {
            bool modified = false;
            foreach (XmlElement roleDefinitionElement in roleAssignmentElement.SelectNodes("RoleDefinitionBindings/RoleDefinition"))
            {
                string name = roleDefinitionElement.GetAttribute("Name");
                if (name == "Limited Access")
                    continue;
                SPRoleDefinition existingRoleDef = null;
                try
                {
                    existingRoleDef = web.RoleDefinitions[name];
                }
                catch (Exception) { }
                if (existingRoleDef == null)
                {
                    SPBasePermissions perms = SPBasePermissions.EmptyMask;
                    foreach (string perm in roleDefinitionElement.GetAttribute("BasePermissions").Split(','))
                    {
                        perms = perms | (SPBasePermissions)Enum.Parse(typeof(SPBasePermissions), perm, true);
                    }
                    existingRoleDef = new SPRoleDefinition();
                    existingRoleDef.Name = name;
                    existingRoleDef.BasePermissions = perms;
                    existingRoleDef.Description = roleDefinitionElement.GetAttribute("Description");
                    existingRoleDef.Order = int.Parse(roleDefinitionElement.GetAttribute("Order"));
                    SPWeb tempWeb = web;
                    while (!tempWeb.HasUniqueRoleDefinitions)
                        tempWeb = tempWeb.ParentWeb;
                    tempWeb.RoleDefinitions.Add(existingRoleDef);
                    tempWeb.Update();
                }
                if (!roleAssignment.RoleDefinitionBindings.Contains(existingRoleDef))
                {
                    roleAssignment.RoleDefinitionBindings.Add((!web.HasUniqueRoleAssignments ? web.ParentWeb : web).RoleDefinitions[name]);
                    modified = true;
                }
            }
            List<SPRoleDefinition> roleDefsToRemove = new List<SPRoleDefinition>();
            foreach (SPRoleDefinition roleDef in roleAssignment.RoleDefinitionBindings)
            {
                if (roleDef.Name == "Limited Access")
                    continue;
                bool found = false;
                foreach (XmlElement roleDefinitionElement in roleAssignmentElement.SelectNodes("RoleDefinitionBindings/RoleDefinition"))
                {
                    if (roleDef.Name == roleDefinitionElement.GetAttribute("Name"))
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    roleDefsToRemove.Add(roleDef);
                    modified = true;
                }
            }
            foreach (SPRoleDefinition roleDef in roleDefsToRemove)
            {
                roleAssignment.RoleDefinitionBindings.Remove(roleDef);
            }
            return modified;
        }
    }
}
