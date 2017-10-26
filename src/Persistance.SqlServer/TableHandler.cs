using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Xlent.Lever.Libraries2.Core.Assert;
using Xlent.Lever.Libraries2.Core.Error.Logic;
using Xlent.Lever.Libraries2.Core.Storage.Model;
using Xlent.Lever.Libraries2.Persistance.SqlServer.Logic;
using Xlent.Lever.Libraries2.Persistance.SqlServer.Model;

namespace Xlent.Lever.Libraries2.Persistance.SqlServer
{
    /// <summary>
    /// Helper class for advanced SELECT statmements
    /// </summary>
    /// <typeparam name="TDatabaseItem"></typeparam>
    public partial class TableHandler<TDatabaseItem> : Database
        where TDatabaseItem : ITableItem, IValidatable, new()
    {
        private readonly string _foreignKeyColumnName;
        protected ISqlTableMetadata TableMetadata { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="tableMetadata"></param>
        public TableHandler(string connectionString, ISqlTableMetadata tableMetadata)
            : base(connectionString)
        {
            TableMetadata = tableMetadata;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="tableMetadata"></param>
        /// <param name="foreignKeyColumnName"></param>
        public TableHandler(string connectionString, ISqlTableMetadata tableMetadata, string foreignKeyColumnName)
            : this(connectionString, tableMetadata)
        {
            _foreignKeyColumnName = foreignKeyColumnName;
        }

        /// <summary>
        /// The name of the table that this class handles.
        /// </summary>
        public string TableName => TableMetadata.TableName;
    }

    public partial class TableHandler<TDatabaseItem> : ICrudAll<TDatabaseItem, Guid>
            where TDatabaseItem : ITableItem, IValidatable, new()
        {

            #region ICrud

            /// <inheritdoc />
            public virtual async Task<TDatabaseItem> CreateAsync(TDatabaseItem item)
        {
            var id = await InternalCreateAsync(item);
            return await ReadAsync(id);
        }

        /// <inheritdoc />
        private async Task<Guid> InternalCreateAsync(TDatabaseItem item)
        {
            InternalContract.RequireNotNull(item, nameof(item));
            if (item.Id == Guid.Empty) item.Id = Guid.NewGuid();
            item.ETag = Guid.NewGuid().ToString();
            InternalContract.RequireValidated(item, nameof(item));
            using (var db = NewSqlConnection())
            {
                await db.ExecuteAsync(SqlHelper.Create(TableMetadata), item);
            }
            return item.Id;
        }

        /// <inheritdoc />
        public async Task DeleteAsync(Guid id)
        {
            InternalContract.RequireNotDefaultValue(id, nameof(id));
            var item = new TDatabaseItem
            {
                Id = id
            };
            using (var db = NewSqlConnection())
            {
                await db.ExecuteAsync(SqlHelper.Delete(TableMetadata), new {Id = id});
            }
        }

        /// <inheritdoc />
        public async Task<TDatabaseItem> ReadAsync(Guid id)
        {
            InternalContract.RequireNotDefaultValue(id, nameof(id));
            var item = new TDatabaseItem
            {
                Id = id
            };
            return await SearchWhereSingle("Id = @Id", item);
        }

        /// <inheritdoc />
        public async Task<TDatabaseItem> UpdateAsync(TDatabaseItem item)
        {
            InternalContract.RequireNotNull(item, nameof(item));
            InternalContract.RequireValidated(item, nameof(item));
            await InternalUpdateAsync(item);
            return await ReadAsync(item.Id);
        }

        /// <inheritdoc />
        private async Task InternalUpdateAsync(TDatabaseItem item)
        {
            InternalContract.RequireNotNull(item, nameof(item));
            InternalContract.RequireValidated(item, nameof(item));
            var oldItem = await ReadAsync(item.Id);
            if (oldItem == null)
                throw new FulcrumNotFoundException($"Table {TableMetadata.TableName} did not contain an item with id {item.Id}");
            if (!string.Equals(oldItem.ETag, item.ETag))
                throw new FulcrumConflictException(
                    "Could not update. Your data was stale. Please reload and try again.");
            item.ETag = Guid.NewGuid().ToString();
            using (var db = NewSqlConnection())
            {
                var count = await db.ExecuteAsync(SqlHelper.Update(TableMetadata, oldItem.ETag), item);
                if (count == 0)
                    throw new FulcrumConflictException(
                        "Could not update. Your data was stale. Please reload and try again.");
            }
        }

        #endregion

        #region ICrudAll

        /// <inheritdoc />
        public Task<IPageEnvelope<TDatabaseItem, Guid>> ReadAllAsync(int offset = 0, int limit = 100)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task DeleteAllAsync()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
    public partial class TableHandler<TDatabaseItem> : ISearch<TDatabaseItem>
        where TDatabaseItem : ITableItem, IValidatable, new()
    {
        #region ISearch

        /// <inheritdoc />
        public async Task<PageEnvelope<TDatabaseItem, Guid>> SearchAllAsync(string orderBy, int offset = 0,
            int limit = PageInfo.DefaultLimit)
        {
            InternalContract.RequireGreaterThanOrEqualTo(0, offset, nameof(offset));
            InternalContract.RequireGreaterThanOrEqualTo(0, limit, nameof(limit));
            return await SearchWhereAsync(null, orderBy, null, offset, limit);
        }

        /// <inheritdoc />
        public async Task<PageEnvelope<TDatabaseItem, Guid>> SearchAdvancedAsync(string countFirst, string selectFirst, string selectRest, string orderBy = null, object param = null, int offset = 0, int limit = 100)
        {
            InternalContract.RequireGreaterThanOrEqualTo(0, offset, nameof(offset));
            InternalContract.RequireGreaterThanOrEqualTo(0, limit, nameof(limit));
            var total = CountItemsAdvanced(countFirst, selectRest, param);
            var selectStatement = selectRest == null ? null : $"{selectFirst} {selectRest}";
            var data = await SearchInternalAsync(param, selectStatement, orderBy, offset, limit);
            var dataAsArray = data as TDatabaseItem[] ?? data.ToArray();
            return new PageEnvelope<TDatabaseItem, Guid>
            {
                Data = dataAsArray,
                PageInfo = new PageInfo
                {

                    Offset = offset,
                    Limit = limit,
                    Returned = dataAsArray.Length,
                    Total = total
                }
            };
        }

        /// <inheritdoc />
        public async Task<PageEnvelope<TDatabaseItem, Guid>> SearchWhereAsync(string where = null, string orderBy = null, object param = null, int offset = 0, int limit = 100)
        {
            InternalContract.RequireGreaterThanOrEqualTo(0, offset, nameof(offset));
            InternalContract.RequireGreaterThanOrEqualTo(0, limit, nameof(limit));
            var total = CountItemsWhere(where, param);
            var data = await SearchInternalWhereAsync(param, where, orderBy, offset, limit);
            var dataAsArray = data as TDatabaseItem[] ?? data.ToArray();
            return new PageEnvelope<TDatabaseItem, Guid>
            {
                Data = dataAsArray,
                PageInfo = new PageInfo
                {

                    Offset = offset,
                    Limit = limit,
                    Returned = dataAsArray.Length,
                    Total = total
                }
            };
        }

        /// <inheritdoc />
        public async Task<TDatabaseItem> SearchWhereSingle(string where, object param = null)
        {
            if (where == null) where = "1=1";
            var item = new TDatabaseItem();
            return await SearchAdvancedSingleAsync($"SELECT * FROM [{TableMetadata.TableName}] WHERE ({where})", param);
        }

        /// <inheritdoc />
        public async Task<TDatabaseItem> SearchAdvancedSingleAsync(string selectStatement, object param = null)
        {
            InternalContract.RequireNotNullOrWhitespace(selectStatement, nameof(selectStatement));
            return await SearchFirstAdvancedAsync(selectStatement, null, param);
        }

        /// <inheritdoc />
        public async Task<TDatabaseItem> SearchFirstWhereAsync(string where = null, string orderBy = null, object param = null)
        {
            var result = await SearchInternalWhereAsync(param, where, orderBy, 0, 1);
            return result.SingleOrDefault();
        }

        /// <inheritdoc />
        public async Task<TDatabaseItem> SearchFirstAdvancedAsync(string selectStatement, string orderBy = null, object param = null)
        {
            InternalContract.RequireNotNullOrWhitespace(selectStatement, nameof(selectStatement));
            var result = await SearchInternalAsync(param, selectStatement, orderBy, 0, 1);
            return result.SingleOrDefault();
        }

        /// <inheritdoc />
        public int CountItemsWhere(string where = null, object param = null)
        {
            if (where == null) where = "1=1";
            var item = new TDatabaseItem();
            return CountItemsAdvanced("SELECT COUNT(*)", $"FROM [{TableMetadata.TableName}] WHERE ({where})", param);
        }

        /// <inheritdoc />
        public int CountItemsAdvanced(string selectFirst, string selectRest, object param)
        {
            InternalContract.RequireNotNullOrWhitespace(selectFirst, nameof(selectFirst));
            InternalContract.RequireNotNullOrWhitespace(selectRest, nameof(selectRest));
            if (selectRest == null) return 0;
            var selectStatement = $"{selectFirst} {selectRest}";
            using (IDbConnection db = NewSqlConnection())
            {
                return db.Query<int>(selectStatement, param)
                    .SingleOrDefault();
            }
        }

        /// <summary>
        /// Find the items specified by the <paramref name="where"/> clause.
        /// </summary>
        /// <param name="param">The fields for the <paramref name="where"/> condition.</param>
        /// <param name="where">The search condition for the SELECT statement.</param>
        /// <param name="orderBy">An expression for how to order the result.</param>
        /// <param name="offset">The number of items that will be skipped in result.</param>
        /// <param name="limit">The maximum number of items to return.</param>
        /// <returns>The found items.</returns>
        private async Task<IEnumerable<TDatabaseItem>> SearchInternalWhereAsync(object param, string where = null, string orderBy = null,
            int offset = 0, int limit = PageInfo.DefaultLimit)
        {
            InternalContract.RequireGreaterThanOrEqualTo(0, offset, nameof(offset));
            InternalContract.RequireGreaterThanOrEqualTo(0, limit, nameof(limit));
            if (where == null) where = "1=1";
            var item = new TDatabaseItem();
            return await SearchInternalAsync(param, $"SELECT * FROM [{TableMetadata.TableName}] WHERE ({where})", orderBy, offset, limit);
        }

        /// <summary>
        /// Find the items specified by the <paramref name="selectStatement"/>.
        /// </summary>
        /// <param name="param">The fields for the <paramref name="selectStatement"/> condition.</param>
        /// <param name="selectStatement">The SELECT statement, including WHERE, but not ORDER BY.</param>
        /// <param name="orderBy">An expression for how to order the result.</param>
        /// <param name="offset">The number of items that will be skipped in result.</param>
        /// <param name="limit">The maximum number of items to return.</param>
        /// <returns>The found items.</returns>
        /// 
        private async Task<IEnumerable<TDatabaseItem>> SearchInternalAsync(object param, string selectStatement, string orderBy = null,
            int offset = 0, int limit = PageInfo.DefaultLimit)
        {
            InternalContract.RequireGreaterThanOrEqualTo(0, offset, nameof(offset));
            InternalContract.RequireGreaterThanOrEqualTo(0, limit, nameof(limit));
            InternalContract.RequireNotNullOrWhitespace(selectStatement, nameof(selectStatement));
            if (orderBy == null) orderBy = "1";
            using (IDbConnection db = NewSqlConnection())
            {
                var sqlQuery = $"{selectStatement} " +
                               $" ORDER BY {orderBy}" +
                               $" OFFSET {offset} ROWS FETCH NEXT {limit} ROWS ONLY";

               return await db.QueryAsync<TDatabaseItem>(sqlQuery, param);
            }
        }
        #endregion
    }
}
