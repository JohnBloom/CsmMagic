using System;
using System.Configuration;
using CsmMagic.Config;

namespace CsmMagic
{
    public class CsmClientConfiguration
    {
        public CsmClientConfiguration(string username, string password, string connectionName)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentNullException("username");
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentNullException("password");
            }

            if (string.IsNullOrWhiteSpace(connectionName))
            {
                throw new ArgumentNullException("connectionName");
            }

            Username = username;
            Password = password;
            ConnectionName = connectionName;
        }

        public CsmClientConfiguration()
        {
            var config = ConfigurationManager.GetSection("CsmMagic") as CsmMagicConfiguration;
            Username = config.CherwellConnection.UserName;
            Password = config.CherwellConnection.Password;
            ConnectionName = config.CherwellConnection.ConnectionName;
        }

        public string ConnectionName { get; set; }

        public string Password { get; set; }

        public string Username { get; set; }
    }
}