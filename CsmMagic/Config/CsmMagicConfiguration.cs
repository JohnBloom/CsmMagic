using System.Configuration;

namespace CsmMagic.Config
{
    public class CsmMagicConfiguration : ConfigurationSection
    {
        [ConfigurationProperty("CherwellConnection", IsRequired = false)]
        public CherwellConnectionConfigElement CherwellConnection
        {
            get
            {
                return (CherwellConnectionConfigElement)this["CherwellConnection"];
            }

            set
            {
                this["CherwellConnection"] = value;
            }
        }
    }
}
