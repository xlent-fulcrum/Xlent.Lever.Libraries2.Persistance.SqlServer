using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xlent.Lever.Libraries2.Core.Storage.Model;
using Xlent.Lever.Libraries2.SqlServer.Model;

namespace Xlent.Lever.Libraries2.SqlServer.Cache
{
    public class TimeLimitedCache<TItem, TId> : ICrud<TItem, TId>
    {
        private ICrud<CacheItem, Guid> _table; 
        public TimeLimitedCache(string connectionString, TimeSpan? expiration = null)
        {
            var tableMetadata = new SqlTableMetadata
            {
                TableName = "Cache",
                CustomColumnNames = new[] {""}
            };
            _table = new SimpleTableHandler<CacheItem>(connectionString, tableMetadata);
        }

        /// <inheritdoc />
        public Task<TId> CreateAsync(TItem item)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task<TItem> CreateAndReturnAsync(TItem item)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task CreateWithSpecifiedIdAsync(TId id, TItem item)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task<TItem> CreateWithSpecifiedIdAndReturnAsync(TId id, TItem item)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task<TItem> ReadAsync(TId id)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task DeleteAsync(TId id)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task<PageEnvelope<TItem>> ReadAllAsync(int offset = 0, int? limit = null)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task DeleteAllAsync()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task UpdateAsync(TId id, TItem item)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task<TItem> UpdateAndReturnAsync(TId id, TItem item)
        {
            throw new NotImplementedException();
        }
    }
}
