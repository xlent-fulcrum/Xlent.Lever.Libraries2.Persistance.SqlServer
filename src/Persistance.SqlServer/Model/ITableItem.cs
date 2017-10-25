using System;
using Xlent.Lever.Libraries2.Core.Storage.Model;

namespace Xlent.Lever.Libraries2.Persistance.SqlServer.Model
{
    /// <summary>
    /// Metadata for creating SQL statmements
    /// </summary>
    public interface ITableItem : IStorableItem<Guid>, IOptimisticConcurrencyControlByETag
    {
    }
}