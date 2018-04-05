using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Xlent.Lever.Libraries2.Core.Assert;
using Xlent.Lever.Libraries2.Core.Error.Logic;
using Xlent.Lever.Libraries2.Core.Storage.Logic;
using Xlent.Lever.Libraries2.Core.Storage.Model;
using Xlent.Lever.Libraries2.SqlServer.Logic;
using Xlent.Lever.Libraries2.SqlServer.Model;

namespace Xlent.Lever.Libraries2.SqlServer
{
    /// <summary>
    /// Helper class for advanced SELECT statmements
    /// </summary>
    /// <typeparam name="TDatabaseItem"></typeparam>
    public partial class SimpleTableHandler<TDatabaseItem>
    {
        public Database Database { get; }
        public ISqlTableMetadata TableMetadata { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="tableMetadata"></param>
        public SimpleTableHandler(string connectionString, ISqlTableMetadata tableMetadata)
        {
            InternalContract.RequireValidated(tableMetadata, nameof(tableMetadata));
            Database = new Database(connectionString);
            TableMetadata = tableMetadata;
        }

        /// <summary>
        /// The name of the table that this class handles.
        /// </summary>
        public string TableName => TableMetadata.TableName;
    }

    public partial class SimpleTableHandler<TDatabaseItem> : CrudBase<TDatabaseItem, Guid>
    where TDatabaseItem : IUniquelyIdentifiable<Guid>
    {
        /// <inheritdoc />
        public override async Task CreateWithSpecifiedIdAsync(Guid id, TDatabaseItem item, CancellationToken token = default(CancellationToken))
        {
            InternalContract.RequireNotDefaultValue(id, nameof(id));
            InternalContract.RequireNotNull(item, nameof(item));
            item.Id = id;
            MaybeValidate(item);
            MaybeCreateNewEtag(item);
            MaybeUpdateTimeStamps(item, true);
            using (var db = Database.NewSqlConnection())
            {
                var sql = SqlHelper.Create(TableMetadata);
                await db.ExecuteAsync(sql, item);
            }
        }

        /// <inheritdoc />
        public override async Task DeleteAsync(Guid id, CancellationToken token = default(CancellationToken))
        {
            InternalContract.RequireNotDefaultValue(id, nameof(id));
            await DeleteWhereAsync("Id=@Id", new { Id = id }, token);
        }

        /// <inheritdoc />
        public override async Task DeleteAllAsync(CancellationToken token = default(CancellationToken))
        {
            await DeleteWhereAsync("1=1", null, token);
        }

        protected async Task DeleteWhereAsync(string where = null, object param = null, CancellationToken token = default(CancellationToken))
        {
            where = string.IsNullOrWhiteSpace(where) ? "1=1" : where;
            using (var db = Database.NewSqlConnection())
            {
                await db.ExecuteAsync(SqlHelper.Delete(TableMetadata, where), param);
            }
        }

        protected internal async Task ExecuteAsync(string statement, object param = null, CancellationToken token = default(CancellationToken))
        {
            InternalContract.RequireNotNullOrWhitespace(statement, nameof(statement));
            using (var db = Database.NewSqlConnection())
            {
                await db.ExecuteAsync(statement, param);
            }
        }

        public override async Task<PageEnvelope<TDatabaseItem>> ReadAllWithPagingAsync(int offset, int? limit = null, CancellationToken token = default(CancellationToken))
        {
            return await SearchAllAsync(null, offset, limit, token);
        }

        /// <inheritdoc />
        public override async Task<TDatabaseItem> ReadAsync(Guid id, CancellationToken token = default(CancellationToken))
        {
            InternalContract.RequireNotDefaultValue(id, nameof(id));
            return await SearchWhereSingle("Id = @Id", new { Id = id }, token);
        }

        /// <inheritdoc />
        public override async Task UpdateAsync(Guid id, TDatabaseItem item, CancellationToken token = default(CancellationToken))
        {
            InternalContract.RequireNotNull(item, nameof(item));
            MaybeValidate(item);
            await InternalUpdateAsync(id, item, token);
        }

        private async Task InternalUpdateAsync(Guid id, TDatabaseItem item, CancellationToken token)
        {
            InternalContract.RequireNotNull(item, nameof(item));
            MaybeValidate(item);
            await MaybeVerifyEtagForUpdateAsync(id, item, token);
            using (var db = Database.NewSqlConnection())
            {
                if (item is IOptimisticConcurrencyControlByETag etaggable)
                {
                    var sql = SqlHelper.Update(TableMetadata, etaggable.Etag);
                    var count = await db.ExecuteAsync(sql, item);
                    if (count == 0)
                        throw new FulcrumConflictException(
                            "Could not update. Your data was stale. Please reload and try again.");
                }
                else
                {
                    var sql = SqlHelper.Update(TableMetadata);
                    await db.ExecuteAsync(sql, item);
                }
            }
        }
    }

    #region ISearch
    public partial class SimpleTableHandler<TDatabaseItem> : ISearch<TDatabaseItem>
    {

        /// <inheritdoc />
        public async Task<PageEnvelope<TDatabaseItem>> SearchAllAsync(string orderBy, int offset,
            int? limit = null, CancellationToken token = default(CancellationToken))
        {
            InternalContract.RequireGreaterThanOrEqualTo(0, offset, nameof(offset));
            if (limit != null) InternalContract.RequireGreaterThanOrEqualTo(0, limit.Value, nameof(limit));
            return await SearchWhereAsync(null, orderBy, null, offset, limit, token);
        }

        /// <inheritdoc />
        public async Task<PageEnvelope<TDatabaseItem>> SearchAdvancedAsync(string countFirst, string selectFirst, string selectRest, string orderBy = null, object param = null, int offset = 0, int? limit = null, CancellationToken token = default(CancellationToken))
        {
            limit = limit ?? PageInfo.DefaultLimit;
            InternalContract.RequireGreaterThanOrEqualTo(0, offset, nameof(offset));
            InternalContract.RequireGreaterThanOrEqualTo(0, limit.Value, nameof(limit));
            var total = await CountItemsAdvancedAsync(countFirst, selectRest, param, token);
            var selectStatement = selectRest == null ? null : $"{selectFirst} {selectRest}";
            var data = await SearchInternalAsync(param, selectStatement, orderBy, offset, limit.Value);
            var dataAsArray = data as TDatabaseItem[] ?? data.ToArray();
            return new PageEnvelope<TDatabaseItem>
            {
                Data = dataAsArray,
                PageInfo = new PageInfo
                {

                    Offset = offset,
                    Limit = limit.Value,
                    Returned = dataAsArray.Length,
                    Total = total
                }
            };
        }

        /// <inheritdoc />
        public async Task<PageEnvelope<TDatabaseItem>> SearchWhereAsync(string where = null, string orderBy = null, object param = null, int offset = 0, int? limit = null, CancellationToken token = default(CancellationToken))
        {
            limit = limit ?? PageInfo.DefaultLimit;
            InternalContract.RequireGreaterThanOrEqualTo(0, offset, nameof(offset));
            InternalContract.RequireGreaterThanOrEqualTo(0, limit.Value, nameof(limit));
            var total = await CountItemsWhereAsync(where, param, token);
            var data = await SearchInternalWhereAsync(param, where, orderBy, offset, limit.Value);
            var dataAsArray = data as TDatabaseItem[] ?? data.ToArray();
            return new PageEnvelope<TDatabaseItem>
            {
                Data = dataAsArray,
                PageInfo = new PageInfo
                {

                    Offset = offset,
                    Limit = limit.Value,
                    Returned = dataAsArray.Length,
                    Total = total
                }
            };
        }

        /// <inheritdoc />
        public async Task<TDatabaseItem> SearchWhereSingle(string where, object param = null, CancellationToken token = default(CancellationToken))
        {
            if (where == null) where = "1=1";
            return await SearchAdvancedSingleAsync($"SELECT * FROM [{TableMetadata.TableName}] WHERE ({where})", param, token);
        }

        /// <inheritdoc />
        public async Task<TDatabaseItem> SearchAdvancedSingleAsync(string selectStatement, object param = null, CancellationToken token = default(CancellationToken))
        {
            InternalContract.RequireNotNullOrWhitespace(selectStatement, nameof(selectStatement));
            return await SearchFirstAdvancedAsync(selectStatement, null, param, token);
        }

        /// <inheritdoc />
        public async Task<TDatabaseItem> SearchFirstWhereAsync(string where = null, string orderBy = null, object param = null, CancellationToken token = default(CancellationToken))
        {
            var result = await SearchInternalWhereAsync(param, where, orderBy, 0, 1);
            return result.SingleOrDefault();
        }

        /// <inheritdoc />
        public async Task<TDatabaseItem> SearchFirstAdvancedAsync(string selectStatement, string orderBy = null, object param = null, CancellationToken token = default(CancellationToken))
        {
            InternalContract.RequireNotNullOrWhitespace(selectStatement, nameof(selectStatement));
            var result = await SearchInternalAsync(param, selectStatement, orderBy, 0, 1);
            return result.SingleOrDefault();
        }

        /// <inheritdoc />
        public async Task<int> CountItemsWhereAsync(string where = null, object param = null, CancellationToken token = default(CancellationToken))
        {
            where = where ?? "1=1";
            return await CountItemsAdvancedAsync("SELECT COUNT(*)", $"FROM [{TableMetadata.TableName}] WHERE ({@where})", param, token);
        }

        /// <inheritdoc />
        public async Task<int> CountItemsAdvancedAsync(string selectFirst, string selectRest, object param = null, CancellationToken token = default(CancellationToken))
        {
            InternalContract.RequireNotNullOrWhitespace(selectFirst, nameof(selectFirst));
            InternalContract.RequireNotNullOrWhitespace(selectRest, nameof(selectRest));
            if (selectRest == null) return 0;
            var selectStatement = $"{selectFirst} {selectRest}";
            using (IDbConnection db = Database.NewSqlConnection())
            {
                return (await db.QueryAsync<int>(selectStatement, param))
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
        private async Task<IEnumerable<TDatabaseItem>> SearchInternalWhereAsync(object param, string where, string orderBy,
            int offset, int limit)
        {
            InternalContract.RequireGreaterThanOrEqualTo(0, offset, nameof(offset));
            InternalContract.RequireGreaterThanOrEqualTo(0, limit, nameof(limit));
            where = where ?? "1=1";
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
        private async Task<IEnumerable<TDatabaseItem>> SearchInternalAsync(object param, string selectStatement, string orderBy,
            int offset, int limit)
        {
            InternalContract.RequireGreaterThanOrEqualTo(0, offset, nameof(offset));
            InternalContract.RequireGreaterThanOrEqualTo(0, limit, nameof(limit));
            InternalContract.RequireNotNullOrWhitespace(selectStatement, nameof(selectStatement));
            orderBy = orderBy ?? TableMetadata.GetOrderBy() ?? "1";
            using (IDbConnection db = Database.NewSqlConnection())
            {
                var sqlQuery = $"{selectStatement} " +
                               $" ORDER BY {orderBy}" +
                               $" OFFSET {offset} ROWS FETCH NEXT {limit} ROWS ONLY";

                return await db.QueryAsync<TDatabaseItem>(sqlQuery, param);
            }
        }
    }
    #endregion
}
