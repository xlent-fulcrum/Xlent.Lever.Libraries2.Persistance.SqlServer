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
    public class ManyToOneRecursiveTableHandler<TModel> : SimpleTableHandler<TModel>, IManyToOneRecursiveRelationComplete<TModel, Guid>
        where TModel : class, IUniquelyIdentifiable<Guid>
    {
        public string ParentColumnName { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="tableMetadata"></param>
        /// <param name="parentColumnName"></param>
        public ManyToOneRecursiveTableHandler(string connectionString, ISqlTableMetadata tableMetadata, string parentColumnName)
            : base(connectionString, tableMetadata)
        {
            ParentColumnName = parentColumnName;
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
        internal async Task<PageEnvelope<TModel>> ReadAllParentsInGroupAsync(string groupColumnName, Guid groupColumnValue, int offset = 0, int? limit = null)
        {
            var selectRest = $"FROM [{TableMetadata.TableName}] AS many" +
                             $" JOIN [{TableName}] AS one ON (one.Id = many.[{ParentColumnName}])" +
                             $" WHERE [{groupColumnName}] = @ColumnValue";
            return await SearchAdvancedAsync("SELECT COUNT(one.[Id])", "SELECT one.*", selectRest, TableMetadata.GetOrderBy("many."), new { ColumnValue = groupColumnValue }, offset, limit);
        }

        /// <inheritdoc />
        public async Task<PageEnvelope<TModel>> ReadChildrenWithPagingAsync(Guid parentId, int offset = 0, int? limit = null)
        {
            return await SearchWhereAsync($"[{ParentColumnName}] = @ParentId", null, new { ParentId = parentId }, offset, limit);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<TModel>> ReadChildrenAsync(Guid parentId, int limit = int.MaxValue)
        {
            return await StorageHelper.ReadPages(offset => ReadChildrenWithPagingAsync(parentId, offset));
        }

        /// <inheritdoc />
        public async Task<TModel> ReadParentAsync(Guid childId)
        {
            var selectStatement = "SELECT one.*" +
                             $" FROM [{TableMetadata.TableName}] AS many" +
                             $" JOIN [{TableName}] AS one ON (one.Id = many.[{ParentColumnName}])" +
                             $" WHERE many.[Id] = @ChildId";
            return await SearchAdvancedSingleAsync(selectStatement, new { ChildId = childId });
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
