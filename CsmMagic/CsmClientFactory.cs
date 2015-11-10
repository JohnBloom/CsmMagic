using System;

namespace CsmMagic
{
    /// <summary>
    /// The CSM client factory.
    /// </summary>
    public class CsmClientFactory : ICsmClientFactory
    {
        private readonly CsmClientConfiguration _clientConfig;

        /// <summary>
        /// Initializes a new instance of the <see cref="CsmClientFactory"/> class.
        /// </summary>
        /// <param name="clientConfiguration">
        /// The client configuration.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// </exception>
        public CsmClientFactory(CsmClientConfiguration clientConfiguration)
        {
            if (clientConfiguration == null)
            {
                throw new ArgumentNullException("clientConfiguration");
            }

            _clientConfig = clientConfiguration;
        }

        public CsmClientFactory()
        {
            _clientConfig = new CsmClientConfiguration();
        }

        /// <summary>
        /// The get CSM client.
        /// </summary>
        /// <returns>
        /// The <see cref="ICsmClient"/>.
        /// </returns>
        public ICsmClient GetCsmClient()
        {
            return new CsmClient(_clientConfig);
        }
    }
}