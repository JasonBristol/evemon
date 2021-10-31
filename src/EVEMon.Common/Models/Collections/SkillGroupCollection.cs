﻿using System.Linq;
using EVEMon.Common.Attributes;
using EVEMon.Common.Collections;
using EVEMon.Common.Data;

namespace EVEMon.Common.Models.Collections
{
    /// <summary>
    /// Represents a collection of a character's skills groups
    /// </summary>
    [EnforceUIThreadAffinity]
    public sealed class SkillGroupCollection : ReadonlyKeyedCollection<int, SkillGroup>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="character"></param>
        internal SkillGroupCollection(Character character)
        {
            foreach (var group in StaticSkills.AllGroups
                .Select(srcGroup => new SkillGroup(character, srcGroup)))
            {
                Items[group.ID] = group;
            }
        }

        /// <summary>
        /// Gets the skill group with the provided name
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public SkillGroup this[int id] => GetByKey(id);
    }
}