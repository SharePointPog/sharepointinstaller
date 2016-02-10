using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace CodePlex.SharePointInstaller.Configuration
{
    public class CommandsConfiguration : ConfigurationSection
    {
        private ConfigurationProperty CustomCommandsProperty = new ConfigurationProperty(null, typeof(CommandsCollection<CustomCommand>), null, ConfigurationPropertyOptions.IsDefaultCollection);

        [ConfigurationProperty("", Options=ConfigurationPropertyOptions.IsDefaultCollection)]
        [ConfigurationCollection(typeof(CommandsCollection<CustomCommand>), CollectionType = ConfigurationElementCollectionType.BasicMap, AddItemName = "command")]
        public CommandsCollection<CustomCommand> CustomCommands
        {
            get { return (CommandsCollection<CustomCommand>)base[CustomCommandsProperty]; }
        }
    }
}
