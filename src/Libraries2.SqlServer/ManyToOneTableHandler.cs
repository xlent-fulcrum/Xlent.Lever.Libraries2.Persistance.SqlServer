using System;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using Xlent.Lever.Libraries2.Core.Assert;
using Xlent.Lever.Libraries2.Core.Storage.Model;
using Xlent.Lever.Libraries2.SqlServer.Model;

namespace Xlent.Lever.Libraries2.SqlServer
{
    public class ManyToOneTableHandler<TManyModel, TOneModel> : ManyToOneRecursiveTableHandler<TManyModel>, IManyToOneRelationComplete<TManyModel, Guid>
        where TManyModel : class, IUniquelyIdentifiable<Guid> 
        where TOneModel : IUniquelyIdentifiable<Guid>
    {
        protected SimpleTableHandler<TOneModel> OneTableHandler { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="tableMetadata"></param>
        /// <param name="parentColumnName"></param>
        /// <param name="oneTableHandler"></param>
        public ManyToOneTableHandler(string connectionString, ISqlTableMetadata tableMetadata, string parentColumnName, SimpleTableHandler<TOneModel> oneTableHandler)
            : base(connectionString, tableMetadata, parentColumnName)
        {
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
        internal new async Task<PageEnvelope<TOneModel>> ReadAllParentsInGroupAsync(string groupColumnName, Guid groupColumnValue, int offset, int? limit = null)
        {
            var selectRest = $"FROM [{TableMetadata.TableName}] AS many" +
                             $" JOIN [{OneTableHandler.TableName}] AS one ON (one.Id = many.[{ParentColumnName}])" +
                             $" WHERE [{groupColumnName}] = @ColumnValue";
            return await OneTableHandler.SearchAdvancedAsync("SELECT COUNT(one.[Id])", "SELECT one.*", selectRest, TableMetadata.GetOrderBy("many."), new { ColumnValue = groupColumnValue }, offset, limit);
        }

        /// <inheritdoc />
        public new async Task<TOneModel> ReadParentAsync(Guid childId)
        {
            var selectStatement = "SELECT one.*" +
                             $" FROM [{TableMetadata.TableName}] AS many" +
                             $" JOIN [{OneTableHandler.TableName}] AS one ON (one.Id = many.[{ParentColumnName}])" +
                             $" WHERE many.[Id] = @ChildId";
            return await OneTableHandler.SearchAdvancedSingleAsync(selectStatement, new { ChildId = childId });
        }
    }
}
