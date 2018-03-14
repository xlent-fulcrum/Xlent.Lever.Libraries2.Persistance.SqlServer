using System;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using Xlent.Lever.Libraries2.Core.Assert;
using Xlent.Lever.Libraries2.Core.Storage.Model;
using Xlent.Lever.Libraries2.SqlServer.Model;

namespace Xlent.Lever.Libraries2.SqlServer
{
    public class ManyToOneTableHandler<TManyModel, TOneModel> : SimpleTableHandler<TManyModel>, IManyToOneRelation<TManyModel, TOneModel, Guid>
        where TManyModel : ITableItem, IValidatable
        where TOneModel : ITableItem, IValidatable
    {
        public string ParentColumnName { get; }
        public SimpleTableHandler<TOneModel> OneTableHandler { get; }

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
        internal async Task<PageEnvelope<TOneModel>> ReadAllParentsInGroupAsync(string groupColumnName, Guid groupColumnValue, int offset = 0, int? limit = null)
        {
            var selectRest = $"FROM [{TableMetadata.TableName}] AS many" +
                             $" JOIN [{OneTableHandler.TableName}] AS one ON (one.Id = many.[{ParentColumnName}])" +
                             $" WHERE [{groupColumnName}] = @ColumnValue";
            return await OneTableHandler.SearchAdvancedAsync("SELECT COUNT(one.[Id])", "SELECT one.*", selectRest, TableMetadata.OrderBy("many."), new { ColumnValue = groupColumnValue }, offset, limit);
        }

        /// <inheritdoc />
        public async Task<PageEnvelope<TManyModel>> ReadChildrenAsync(Guid parentId, int offset = 0, int? limit = null)
        {
            return await SearchWhereAsync($"[{ParentColumnName}] = @ParentId", null, new { ParentId = parentId }, offset, limit);
        }

        /// <inheritdoc />
        public async Task<TOneModel> ReadParentAsync(Guid childId)
        {
            var selectStatement = "SELECT one.*" +
                             $" FROM [{TableMetadata.TableName}] AS many" +
                             $" JOIN [{OneTableHandler.TableName}] AS one ON (one.Id = many.[{ParentColumnName}])" +
                             $" WHERE many.[Id] = @ChildId";
            return await OneTableHandler.SearchAdvancedSingleAsync(selectStatement, new { ChildId = childId });
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
