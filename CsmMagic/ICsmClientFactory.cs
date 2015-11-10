namespace CsmMagic
{
    /// <summary>
    /// The CsmClientFactory interface.
    /// </summary>
    public interface ICsmClientFactory
    {
        /// <summary>
        /// The get csm client.
        /// </summary>
        /// <returns>
        /// The <see cref="ICsmClient"/>.
        /// </returns>
        ICsmClient GetCsmClient();
    }
}