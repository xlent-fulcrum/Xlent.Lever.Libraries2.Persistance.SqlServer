using System;
using System.Threading.Tasks;
using Dapper;
using Xlent.Lever.Libraries2.Core.Assert;
using Xlent.Lever.Libraries2.Core.Storage.Model;
using Xlent.Lever.Libraries2.Persistance.SqlServer.Logic;
using Xlent.Lever.Libraries2.Persistance.SqlServer.Model;

namespace Xlent.Lever.Libraries2.Persistance.SqlServer
{
    public class GroupedTableHandler<TDatabaseItem, TGroupColumn> : SimpleTableHandler<TDatabaseItem>, IGrouped<TDatabaseItem, Guid, TGroupColumn>
        where TDatabaseItem : ITableItem, IValidatable
    {
        public string GroupColumnName { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="tableMetadata"></param>
        /// <param name="groupColumnName"></param>
        public GroupedTableHandler(string connectionString, ISqlTableMetadata tableMetadata, string groupColumnName)
            : base(connectionString, tableMetadata)
        {
            GroupColumnName = groupColumnName;
        }

        public async Task<TDatabaseItem> CreateAsync(TGroupColumn groupValue, TDatabaseItem item)
        {
            return await CreateAsync(item);
        }

        public async Task<PageEnvelope<TDatabaseItem, Guid>> ReadAllAsync(TGroupColumn groupValue, int offset = 0, int? limit = null)
        {
            return await SearchWhereAsync($"{GroupColumnName} = @GroupValue", TableMetadata.OrderBy(), new {GroupValue = groupValue}, offset, limit);
        }

        public async Task DeleteAllAsync(TGroupColumn groupValue)
        {
            using (var db = NewSqlConnection())
            {
                await db.ExecuteAsync(SqlHelper.DeleteBasedOnColumnValue(TableMetadata, GroupColumnName, "GroupValue"), new { GroupValue = groupValue });
            }
        }
    }
}
