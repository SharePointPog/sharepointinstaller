using System;
using System.Collections.Generic;

namespace Stsadm.Commands.Import
{
    public static class FieldResolver
    {
        private static Dictionary<String, Type> mapping = new Dictionary<String, Type>();

        static FieldResolver()
        {
            Register("Text", typeof(TextField));
            Register("User", typeof(UserField));
            Register("Lookup", typeof(LookupField));
            Register("Choice", typeof(ChoiceField));
        }

        public static void Register(String name, Type type)
        {            
            if(!mapping.ContainsKey(name))
                mapping.Add(name, type);
        }

        public static FieldBase Get(String name, params object[] parameters)
        {
            if (!String.IsNullOrEmpty(name))
            {
                if (mapping.ContainsKey(name))
                    return (FieldBase) Activator.CreateInstance(mapping[name], parameters);         
            }

            return null;
        }        
    }
}
