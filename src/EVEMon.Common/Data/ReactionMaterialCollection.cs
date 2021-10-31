using System.Collections.Generic;
using EVEMon.Common.Collections;
using EVEMon.Common.Serialization.Datafiles;

namespace EVEMon.Common.Data
{
    public sealed class ReactionMaterialCollection : ReadonlyCollection<SerializableReactionInfo>
    {       
        /// <summary>
        /// Initializes a new instance of the <see cref="ReactionMaterialCollection"/> class.
        /// </summary>
        /// <param name="reactionInfo">The reactionInfo.</param>
        internal ReactionMaterialCollection(ICollection<SerializableReactionInfo> reactionInfo)
            : base(reactionInfo?.Count ?? 0)
        {
            if (reactionInfo == null)
                return;

            foreach (var reaction in reactionInfo)
            {
                Items.Add(reaction);
            }
        }
    }
}