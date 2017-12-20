using System;
using System.Threading.Tasks;
using Dapper;
using Xlent.Lever.Libraries2.Core.Assert;
using Xlent.Lever.Libraries2.Core.Storage.Logic;
using Xlent.Lever.Libraries2.Core.Storage.Model;
using Xlent.Lever.Libraries2.SqlServer.Logic;
using Xlent.Lever.Libraries2.SqlServer.Model;

namespace Xlent.Lever.Libraries2.SqlServer
{
    public class GroupedTableHandler<TDatabaseItem, TGroupColumn> : GroupedBase<TDatabaseItem, Guid, TGroupColumn>
        where TDatabaseItem : ITableItem, IValidatable
    {
        public string GroupColumnName { get; }

        protected SimpleTableHandler<TDatabaseItem> SimpleTableHandler { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="tableMetadata"></param>
        /// <param name="groupColumnName"></param>
        public GroupedTableHandler(string connectionString, ISqlTableMetadata tableMetadata, string groupColumnName)
        {
            SimpleTableHandler = new SimpleTableHandler<TDatabaseItem>(connectionString, tableMetadata);
            GroupColumnName = groupColumnName;
        }

        public override async Task CreateWithSpecifiedIdAsync(TGroupColumn groupValue, Guid id, TDatabaseItem item)
        {
            await SimpleTableHandler.CreateWithSpecifiedIdAsync(id, item);
        }

        public override async Task<PageEnvelope<TDatabaseItem>> ReadAllAsync(TGroupColumn groupValue, int offset = 0, int? limit = null)
        {
            return await SimpleTableHandler.SearchWhereAsync($"{GroupColumnName} = @GroupValue", SimpleTableHandler.TableMetadata.OrderBy(), new {GroupValue = groupValue}, offset, limit);
        }

        public override async Task<TDatabaseItem> ReadAsync(TGroupColumn groupValue, Guid id)
        {
            return await SimpleTableHandler.SearchFirstWhereAsync($"{GroupColumnName} = @GroupValue AND Id=@Id", null, new { GroupValue = groupValue, Id = id });
        }

        public override async Task DeleteAsync(TGroupColumn groupValue, Guid id)
        {
            using (var db = SimpleTableHandler.Database.NewSqlConnection())
            {
                await db.ExecuteAsync(SqlHelper.DeleteBasedOnColumnValue(SimpleTableHandler.TableMetadata, GroupColumnName, "GroupValue"), new { GroupValue = groupValue });
            }
        }

        public override async Task DeleteAllAsync(TGroupColumn groupValue)
        {
            using (var db = SimpleTableHandler.Database.NewSqlConnection())
            {
                await db.ExecuteAsync(SqlHelper.DeleteBasedOnColumnValue(SimpleTableHandler.TableMetadata, GroupColumnName, "GroupValue"), new { GroupValue = groupValue });
            }
        }
    }
}
