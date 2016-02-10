using System;
using System.Xml;
using Microsoft.SharePoint;

namespace Stsadm.Commands.Import
{
    class UserField : FieldBase
    {
        public UserField()
        {
        }

        public UserField(XmlNode xml) : base(xml)
        {
        }

        public override object GetFieldValue(SPList list, object value)
        {
            var users = ((string)value).Split(';');
            var userCollection = new SPFieldUserValueCollection();
            foreach (var user in users)
            {
                SPFieldUserValue currentUser = Utility.ResolvePrincipalLookupValue(list.ParentWeb, user);
                if (currentUser != null)
                    userCollection.Add(currentUser);
            }
            return userCollection;
        }
    }
}
