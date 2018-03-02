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
                CustomColumnNames = ""
            }
            _table = new SimpleTableHandler<CacheItem>(connectionString, tableMetadata);
        }

        /// <inheritdoc />
        public async Task<TId> CreateAsync(TItem item)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public async Task<TItem> CreateAndReturnAsync(TItem item)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public async Task CreateWithSpecifiedIdAsync(TId id, TItem item)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public async Task<TItem> CreateWithSpecifiedIdAndReturnAsync(TId id, TItem item)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public async Task<TItem> ReadAsync(TId id)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public async Task DeleteAsync(TId id)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public async Task<PageEnvelope<TItem>> ReadAllAsync(int offset = 0, int? limit = null)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public async Task DeleteAllAsync()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public async Task UpdateAsync(TId id, TItem item)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public async Task<TItem> UpdateAndReturnAsync(TId id, TItem item)
        {
            throw new NotImplementedException();
        }
    }
}
