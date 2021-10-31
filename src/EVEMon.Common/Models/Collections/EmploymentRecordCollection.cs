﻿using System.Collections.Generic;
using System.Linq;
using EVEMon.Common.Collections;
using EVEMon.Common.Serialization.Eve;

namespace EVEMon.Common.Models.Collections
{
    public sealed class EmploymentRecordCollection : ReadonlyCollection<EmploymentRecord>
    {
        private readonly Character m_character;


        #region Constructor

        /// <summary>
        /// Internal constructor.
        /// </summary>
        /// <param name="character">The character.</param>
        internal EmploymentRecordCollection(Character character)
        {
            m_character = character;
        }

        #endregion


        #region Import & Export Methods

        /// <summary>
        /// Imports an enumeration of API objects.
        /// </summary>
        /// <param name="src">The enumeration of serializable objects from the API.</param>
        internal void Import(IEnumerable<SerializableEmploymentHistoryListItem> src)
        {
            Items.Clear();

            // Import the employment history from the API
            foreach (var srcEmploymentRecord in src)
            {
                Items.Add(new EmploymentRecord(m_character, srcEmploymentRecord));
            }
        }

        /// <summary>
        /// Imports an enumeration of serialization objects.
        /// </summary>
        /// <param name="src">The enumeration of serializable objects from the settings file.</param>
        internal void Import(IEnumerable<SerializableEmploymentHistory> src)
        {
            Items.Clear();

            foreach (var srcEmploymentRecord in src)
            {
                Items.Add(new EmploymentRecord(m_character, srcEmploymentRecord));
            }
        }

        /// <summary>
        /// Exports the serialization object for the settings file.
        /// </summary>
        /// <returns>List of serializable employment records.</returns>
        internal IEnumerable<SerializableEmploymentHistory> Export()
            => Items.Select(employmentRecord => employmentRecord.Export());

        #endregion
    }
}