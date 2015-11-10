using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsmMagic.Config
{
    public class CherwellConnectionConfigElement : ConfigurationElement
    {
        /// <summary>
        /// Gets or sets the connection name.
        /// </summary>
        [ConfigurationProperty("connectionName", IsRequired = true)]
        [StringValidator]
        public string ConnectionName
        {
            get
            {
                return (string)this["connectionName"];
            }

            set
            {
                this["connectionName"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        [ConfigurationProperty("password", IsRequired = true)]
        [StringValidator]
        public string Password
        {
            get
            {
                return (string)this["password"];
            }

            set
            {
                this["password"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the user name.
        /// </summary>
        [ConfigurationProperty("userName", IsRequired = true)]
        [StringValidator]
        public string UserName
        {
            get
            {
                return (string)this["userName"];
            }

            set
            {
                this["userName"] = value;
            }
        }
    }
}
