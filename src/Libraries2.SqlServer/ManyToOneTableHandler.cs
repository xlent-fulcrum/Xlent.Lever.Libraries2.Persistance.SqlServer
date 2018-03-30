using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Xlent.Lever.Libraries2.Core.Assert;
using Xlent.Lever.Libraries2.Core.Storage.Logic;
using Xlent.Lever.Libraries2.Core.Storage.Model;
using Xlent.Lever.Libraries2.SqlServer.Model;

namespace Xlent.Lever.Libraries2.SqlServer
{
    public class ManyToOneTableHandler<TManyModel, TOneModel> : SimpleTableHandler<TManyModel>, IManyToOneRelationComplete<TManyModel, Guid>
        where TManyModel : class, IUniquelyIdentifiable<Guid> where TOneModel : IUniquelyIdentifiable<Guid>
    {
        public string ParentColumnName { get; }
        protected SimpleTableHandler<TOneModel> OneTableHandler { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="tableMetadata"></param>
        /// <param name="parentColumnName"></param>
        /// <param name="oneTableHandler"></param>
        public ManyToOneTableHandler(string connectionString, ISqlTableMetadata tableMetadata, string parentColumnName, SimpleTableHandler<TOneModel> oneTableHandler)
            : base(connectionString, tableMetadata)
        {
            ParentColumnName = parentColumnName;
            OneTableHandler = oneTableHandler;
        }

        /// <summary>
        /// Read all referenced items that a specific column references.
        /// </summary>
        /// <param name="groupColumnName"></param>
        /// <param name="groupColumnValue"></param>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        /// <remarks>This method is here to support the <see cref="ManyToManyTableHandler{TDatabaseItem,TOneModel1,TOneModel2}."/></remarks>
        internal async Task<PageEnvelope<TOneModel>> ReadAllParentsInGroupAsync(string groupColumnName, Guid groupColumnValue, int offset, int? limit = null)
        {
            var selectRest = $"FROM [{TableMetadata.TableName}] AS many" +
                             $" JOIN [{OneTableHandler.TableName}] AS one ON (one.Id = many.[{ParentColumnName}])" +
                             $" WHERE [{groupColumnName}] = @ColumnValue";
            return await OneTableHandler.SearchAdvancedAsync("SELECT COUNT(one.[Id])", "SELECT one.*", selectRest, TableMetadata.GetOrderBy("many."), new { ColumnValue = groupColumnValue }, offset, limit);
        }

        /// <summary>
        /// Delete all referenced items that a specific column references.
        /// </summary>
        /// <param name="groupColumnName"></param>
        /// <param name="groupColumnValue"></param>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        /// <remarks>This method is here to support the <see cref="ManyToManyTableHandler{TDatabaseItem,TOneModel1,TOneModel2}."/></remarks>
        internal async Task<PageEnvelope<TOneModel>> DeleteAllParentsInGroupAsync(string groupColumnName, Guid groupColumnValue)
        {
            var selectRest = $"FROM [{TableMetadata.TableName}] AS many" +
                             $" JOIN [{OneTableHandler.TableName}] AS one ON (one.Id = many.[{ParentColumnName}])" +
                             $" WHERE [{groupColumnName}] = @ColumnValue";
            return await OneTableHandler.SearchAdvancedAsync("SELECT COUNT(one.[Id])", "DELETE one", selectRest, "1", new { ColumnValue = groupColumnValue });
        }

        /// <inheritdoc />
        public async Task<PageEnvelope<TManyModel>> ReadChildrenWithPagingAsync(Guid parentId, int offset, int? limit = null)
        {
            return await SearchWhereAsync($"[{ParentColumnName}] = @ParentId", null, new { ParentId = parentId }, offset, limit);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<TManyModel>> ReadChildrenAsync(Guid parentId, int limit = int.MaxValue)
        {
            return await StorageHelper.ReadPages(offset => ReadChildrenWithPagingAsync(parentId, offset));
        }

        /// <inheritdoc />
        public async Task DeleteChildrenAsync(Guid parentId)
        {
            using (IDbConnection db = Database.NewSqlConnection())
            {
                var sqlQuery = "DELETE" +
                               $" FROM [{TableMetadata.TableName}]" +
                               $" WHERE [{ParentColumnName}] = @ParentId";

                await db.ExecuteAsync(sqlQuery, new { ParentId = parentId});
            }
        }
    }
}
