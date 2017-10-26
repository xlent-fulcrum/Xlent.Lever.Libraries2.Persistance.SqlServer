using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Xlent.Lever.Libraries2.Core.Assert;
using Xlent.Lever.Libraries2.Core.Storage.Model;
using Xlent.Lever.Libraries2.Persistance.SqlServer.Logic;
using Xlent.Lever.Libraries2.Persistance.SqlServer.Model;

namespace Xlent.Lever.Libraries2.Persistance.SqlServer
{
    public class ManyToOneTableHandler<TDatabaseItem, TGroupColumn> : SimpleTableHandler<TDatabaseItem>, IGrouped<TDatabaseItem, Guid, TGroupColumn>
        where TDatabaseItem : ITableItem, IValidatable, IStorableItem<Guid>, new()
    {
        protected string GroupColumnName { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="tableMetadata"></param>
        /// <param name="groupColumnName"></param>
        public ManyToOneTableHandler(string connectionString, ISqlTableMetadata tableMetadata, string groupColumnName)
            : base(connectionString, tableMetadata)
        {
            GroupColumnName = groupColumnName;
        }

        public async Task<TDatabaseItem> CreateAsync(TGroupColumn groupId, TDatabaseItem item)
        {
            return await CreateAsync(item);
        }

        public async Task<IPageEnvelope<TDatabaseItem, Guid>> ReadAllAsync(TGroupColumn groupId, int offset = 0, int? limit = null)
        {
            return await SearchWhereAsync($"{GroupColumnName} = @GroupId", TableMetadata.OrderBy, new {GroupId = groupId}, offset, limit);
        }

        public async Task DeleteAllAsync(TGroupColumn groupId)
        {
            using (var db = NewSqlConnection())
            {
                await db.ExecuteAsync(SqlHelper.DeleteBasedOnColumnValue(TableMetadata, GroupColumnName, "GroupId"), new { GroupId = groupId });
            }
        }
    }
}
