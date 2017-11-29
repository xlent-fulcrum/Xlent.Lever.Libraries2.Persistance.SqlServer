using System;
using System.Threading.Tasks;
using Xlent.Lever.Libraries2.Core.Assert;
using Xlent.Lever.Libraries2.Core.Storage.Model;
using Xlent.Lever.Libraries2.SqlServer.Model;

namespace Xlent.Lever.Libraries2.SqlServer
{
    public class ForeignKeyTableHandler<TLocalItem, TForeignItem> : GroupedTableHandler<TLocalItem, Guid>
        where TLocalItem : ITableItem, IValidatable
        where TForeignItem : ITableItem, IValidatable
    {
        public SimpleTableHandler<TForeignItem> ForeignHandler { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="tableMetadata"></param>
        /// <param name="groupColumnName"></param>
        /// <param name="foreignHandler"></param>
        public ForeignKeyTableHandler(string connectionString, ISqlTableMetadata tableMetadata, string groupColumnName, SimpleTableHandler<TForeignItem> foreignHandler)
            : base(connectionString, tableMetadata, groupColumnName)
        {
            ForeignHandler = foreignHandler;
        }

        public async Task<PageEnvelope<TForeignItem>> ReadForeignAsync(Guid groupValue, int offset = 0, int? limit = null)
        {
            var selectRest = $"FROM [{TableMetadata.TableName}] AS local" +
                             $" JOIN [{ForeignHandler.TableName}] AS foregin ON (foreign.Id = local.{GroupColumnName})" +
                             $" WHERE local.[{GroupColumnName}] = @GroupValue";
            return await ForeignHandler.SearchAdvancedAsync("SELECT COUNT(foreign.[Id])", "SELECT foreign.*", selectRest, TableMetadata.OrderBy("local."), new { GroupValue = groupValue }, offset, limit);
        }
    }
}
