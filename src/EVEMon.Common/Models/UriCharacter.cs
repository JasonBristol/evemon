﻿using System;
using EVEMon.Common.Extensions;
using EVEMon.Common.Serialization.Eve;
using EVEMon.Common.Serialization.Settings;

namespace EVEMon.Common.Models
{
    /// <summary>
    /// Represents a character bound to an uri (pointing at a character sheet) rather than CCP API.
    /// </summary>
    public sealed class UriCharacter : Character
    {
        public const int BlankCharacterID = 9999999;
        private Uri m_uri;

        /// <summary>
        /// Default constructor for new uri characters.
        /// </summary>
        /// <param name="identity">The identitiy for this character</param>
        /// <param name="uri">The uri the provided deserialization object was acquired from</param>
        /// <param name="source">A deserialization object for characters</param>
        internal UriCharacter(CharacterIdentity identity, Uri uri, CCPAPIResult<SerializableAPICharacterSheet> source)
            : base(identity, Guid.NewGuid())
        {
            m_uri = uri;
            Import(source);
        }

        /// <summary>
        /// Exported character constructor.
        /// </summary>
        /// <param name="identity">The identitiy for this character</param>
        /// <param name="uri">The uri the provided deserialization object was acquired from</param>
        /// <param name="serial">The serial.</param>
        internal UriCharacter(CharacterIdentity identity, Uri uri, SerializableSettingsCharacter serial)
            : base(identity, Guid.NewGuid())
        {
            m_uri = uri;
            Import(serial);
        }

        /// <summary>
        /// Deserialization constructor.
        /// </summary>
        /// <param name="identity"></param>
        /// <param name="serial"></param>
        internal UriCharacter(CharacterIdentity identity, SerializableUriCharacter serial)
            : base(identity, serial.Guid)
        {
            Import(serial);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        internal override void Dispose()
        {
        }

        /// <summary>
        /// Gets an adorned name, with (file), (url) or (cached) labels.
        /// </summary>
        public override string AdornedName => $"{Name} {(m_uri.IsFile ? "(file)" : "(url)")}";

        /// <summary>
        /// Gets or sets the source's name.
        /// By default, it's the character's name
        /// but it may be overriden to help distinct tabs on the main window.
        /// </summary>
        public Uri Uri
        {
            get { return m_uri; }
            set
            {
                if (m_uri == value)
                    return;

                m_uri = value;
                EveMonClient.OnCharacterUpdated(this);
            }
        }

        /// <summary>
        /// Create a serializable character sheet for this character.
        /// </summary>
        /// <returns></returns>
        public override SerializableSettingsCharacter Export()
        {
            var serial = new SerializableUriCharacter();
            Export(serial);

            serial.Address = m_uri.AbsoluteUri;
            return serial;
        }

        /// <summary>
        /// Imports data from a serialization object.
        /// </summary>
        /// <param name="serial">The serial.</param>
        /// <exception cref="System.ArgumentNullException">serial</exception>
        public void Import(SerializableUriCharacter serial)
        {
            serial.ThrowIfNull(nameof(serial));

            Import((SerializableSettingsCharacter)serial);

            m_uri = new Uri(serial.Address);

            EveMonClient.OnCharacterUpdated(this);
        }

        /// <summary>
        /// Updates this character with the given informations
        /// </summary>
        /// <param name="identity"></param>
        /// <param name="uri"></param>
        /// <param name="result"></param>
        internal void Update(CharacterIdentity identity, Uri uri, CCPAPIResult<SerializableAPICharacterSheet> result)
        {
            CharacterID = identity.CharacterID;
            Identity = identity;
            m_uri = uri;
            Import(result);
        }

        /// <summary>
        /// Updates this character with the given informations.
        /// </summary>
        /// <param name="identity"></param>
        /// <param name="uri"></param>
        /// <param name="result"></param>
        internal void Update(CharacterIdentity identity, Uri uri, SerializableCCPCharacter result)
        {
            CharacterID = identity.CharacterID;
            Identity = identity;
            m_uri = uri;
            Import(result);
        }
    }
}