using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace CodePlex.SharePointInstaller.Configuration
{
    /// <summary>
    /// Collection of configuration elements each containing a simple mapping rule
    /// </summary>
    /// <typeparam name="TRuleElement">The type of the containing element.</typeparam>
    public class CommandsCollection<TCommand> : ConfigurationElementCollection
        where TCommand : CommandConfigurationElement, new()
    {
        /// <summary>
        /// Gets the type of the <see cref="T:System.Configuration.ConfigurationElementCollection"></see>.
        /// </summary>
        /// <value></value>
        /// <returns>The <see cref="T:System.Configuration.ConfigurationElementCollectionType"></see> of this collection.</returns>
        public override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return ConfigurationElementCollectionType.BasicMap;
            }
        }
        /// <summary>
        /// When overridden in a derived class, creates a new <see cref="T:System.Configuration.ConfigurationElement"></see>.
        /// </summary>
        /// <returns>
        /// A new <see cref="T:System.Configuration.ConfigurationElement"></see>.
        /// </returns>
        protected override ConfigurationElement CreateNewElement()
        {
            return new TCommand();
        }
        /// <summary>
        /// Gets the element key for a specified configuration element when overridden in a derived class.
        /// </summary>
        /// <param name="element">The <see cref="T:System.Configuration.ConfigurationElement"></see> to return the key for.</param>
        /// <returns>
        /// An <see cref="T:System.Object"></see> that acts as the key for the specified <see cref="T:System.Configuration.ConfigurationElement"></see>.
        /// </returns>
        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((TCommand)element).Key;
        }
        /// <summary>
        /// Gets the name used to identify this collection of elements in the configuration file when overridden in a derived class.
        /// </summary>
        /// <value></value>
        /// <returns>The name of the collection.</returns>
        protected override string ElementName
        {
            get
            {
                return this.AddElementName;
            }
        }
        /// <summary>
        /// Gets the <see cref="RegexRuleConfigurationElement"/> at the specified index.
        /// </summary>
        /// <value>configuration element for a simple mapping rule</value>
        public TCommand this[int index]
        {
            get
            {
                return (TCommand)BaseGet(index);
            }
        }
    }
}
