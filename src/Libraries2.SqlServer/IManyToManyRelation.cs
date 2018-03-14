using System;
using System.Threading.Tasks;

namespace Xlent.Lever.Libraries2.Core.Storage.Model
{
    /// <summary>
    /// Functionality for persisting groups of objects.
    /// </summary>
    public interface IManyToManyRelation<TOneModel1, TOneModel2, in TId>
    {
        /// <summary>
        /// Find all referenced items with foreign key 1 set to <paramref name="foreignKey1Value"/>.
        /// </summary>
        /// <param name="foreignKey1Value">The specific foreign key value to read the referenced items for.</param>
        /// <param name="offset">The number of items that will be skipped in result.</param>
        /// <param name="limit">The maximum number of items to return.</param>
        Task<PageEnvelope<TOneModel2>> ReadReferencedItemsByForeignKey1(Guid foreignKey1Value, int offset = 0, int? limit = null);

        /// <summary>
        /// Find all referenced items with foreign key 2 set to <paramref name="foreignKey2Value"/>.
        /// </summary>
        /// <param name="foreignKey2Value">The specific foreign key value to read the referenced items for.</param>
        /// <param name="offset">The number of items that will be skipped in result.</param>
        /// <param name="limit">The maximum number of items to return.</param>
        Task<PageEnvelope<TOneModel1>> ReadReferencedItemsByForeignKey2(Guid foreignKey2Value, int offset = 0, int? limit = null);

        /// <summary>
        /// Delete all references where foreign key 1 is set to <paramref name="foreignKey1Value"/>.
        /// </summary>
        Task DeleteReferencesByForeignKey1(Guid foreignKey1Value);

        /// <summary>
        /// Delete all references where foreign key 2 is set to <paramref name="foreignKey2Value"/>.
        /// </summary>
        Task DeleteReferencesByForeignKey2(Guid foreignKey2Value);
    }
}
