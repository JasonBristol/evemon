﻿using System.Linq;
using EVEMon.Common.Attributes;
using EVEMon.Common.Collections;
using EVEMon.Common.Models;

namespace EVEMon.Common.Data
{
    /// <summary>
    /// Represents a certificate category from a character's point of view.
    /// </summary>
    [EnforceUIThreadAffinity]
    public sealed class CertificateGroup : ReadonlyKeyedCollection<string, CertificateClass>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="character"></param>
        /// <param name="src"></param>
        internal CertificateGroup(Character character, StaticCertificateGroup src)
        {
            StaticData = src;

            foreach (var certClass in src
                .Select(srcClass => new CertificateClass(character, srcClass, this)))
            {
                Items[certClass.Name] = certClass;
            }
        }

        /// <summary>
        /// Constructor, used to build an non-character CertificateGroup, only used by CertificateClassCollection.
        /// </summary>
        /// <param name="src">The source.</param>
        internal CertificateGroup(StaticCertificateGroup src)
            : this(null, src)
        {
        }

        /// <summary>
        /// Gets the static data associated with this object.
        /// </summary>
        public StaticCertificateGroup StaticData { get; }

        /// <summary>
        /// Gets this skill's id
        /// </summary>
        public int ID => StaticData.ID;

        /// <summary>
        /// Gets this skill's name
        /// </summary>
        public string Name => StaticData.Name;

        /// <summary>
        /// Gets this skill's description
        /// </summary>
        public string Description => StaticData.Description;
    }
}