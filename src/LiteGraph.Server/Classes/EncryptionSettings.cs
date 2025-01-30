namespace LiteGraph.Server.Classes
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Encryption settings.
    /// </summary>
    public class EncryptionSettings
    {
        #region Public-Members

        /// <summary>
        /// Encryption key as a hex string.
        /// Value must be 64 hexadecimal characters, representing 32 bytes.
        /// </summary>
        public string Key
        {
            get
            {
                return _Key;
            }
            set
            {
                if (String.IsNullOrEmpty(value)) throw new ArgumentNullException(nameof(Key));
                if (value.Length != 64) throw new ArgumentException("Supplied encryption key must be a hex string containing 64 characters representing 32 bytes.");
                _Key = value;
            }
        }

        /// <summary>
        /// Initialization vector as a hex string.
        /// Value must be 32 hexadecimal characters, representing 16 bytes.
        /// </summary>
        public string Iv
        {
            get
            {
                return _Iv;
            }
            set
            {
                if (String.IsNullOrEmpty(value)) throw new ArgumentNullException(nameof(Iv));
                if (value.Length != 32) throw new ArgumentException("Supplied initialization vector must be a hex string containing 32 characters representing 16 bytes.");
                _Iv = value;
            }
        }

        #endregion

        #region Private-Members

        private string _Key = "0000000000000000000000000000000000000000000000000000000000000000";
        private string _Iv = "00000000000000000000000000000000";

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate.
        /// </summary>
        public EncryptionSettings()
        {

        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
