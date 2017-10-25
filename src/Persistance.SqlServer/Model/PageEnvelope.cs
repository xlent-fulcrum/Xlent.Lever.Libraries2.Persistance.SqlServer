using System;
using System.Collections.Generic;
using Xlent.Lever.Libraries2.Core.Storage.Model;

namespace Xlent.Lever.Libraries2.Persistance.SqlServer.Model
{
    /// <summary>
    /// A paging envelope for returning segments of data.
    /// </summary>
    public class PageEnvelope<TData> : IPageEnvelope<TData, Guid>
        where TData : IStorableItem<Guid>
    {
        /// <inheritdoc />
        public IEnumerable<TData> Data { get; set; }

        /// <inheritdoc />
        public PageInfo PageInfo { get; set; }
    }
}
