namespace NanaManagerAPI.Cryptography
{
    /// <summary>
    /// An interface to unify cryptography providers
    /// </summary>
    public interface ICryptographyProvider
    {
        /// <summary>
        /// For class initialisation. Leave empty if no preparation is required
        /// </summary>
        public void Initialise(string Key);
        /// <summary>
        /// For class termination. Put any disposal or cool-down coding here.
        /// </summary>
        public void Terminate();
        /// <summary>
        /// For encrypting the provided plain data
        /// </summary>
        /// <param name="PlainData">The data to encrypt</param>
        /// <param name="Key">The key to encrypt with</param>
        /// <returns>Encrypted data</returns>
        public byte[] Encrypt( byte[] PlainData );
        /// <summary>
        /// For encrypting the provided plain data
        /// </summary>
        /// <param name="PlainText">The data to encrypt</param>
        /// <param name="Key">The key to encrypt with</param>
        /// <returns>Encrypted data</returns>
        public string Encrypt( string PlainText );
        /// <summary>
        /// For decrypting the provided cipher data
        /// </summary>
        /// <param name="CipherData">The data to decrypt</param>
        /// <param name="Key">The key to decrypt with</param>
        /// <returns>Decrypted data</returns>
        public byte[] Decrypt( byte[] CipherData );
        /// <summary>
        /// For decrypting the provided cipher data
        /// </summary>
        /// <param name="CipherText">The data to decrypt</param>
        /// <param name="Key">The key to decrypt with</param>
        /// <returns>Decrypted data</returns>
        public string Decrypt( string CipherText );
    }
}
