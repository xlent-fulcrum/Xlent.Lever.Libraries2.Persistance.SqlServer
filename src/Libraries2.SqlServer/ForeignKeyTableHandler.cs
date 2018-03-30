using System;
using System.Threading.Tasks;
using Xlent.Lever.Libraries2.Core.Assert;
using Xlent.Lever.Libraries2.Core.Storage.Model;
using Xlent.Lever.Libraries2.SqlServer.Model;

namespace Xlent.Lever.Libraries2.SqlServer
{
    [Obsolete("Use ManyToOneTableHandler.")]
    public class ForeignKeyTableHandler<TLocalItem, TForeignItem> : SimpleTableHandler<TLocalItem>
        where TLocalItem : ITableItem, IValidatable
        where TForeignItem : ITableItem, IValidatable
    {
        public string GroupColumnName { get; }
        public SimpleTableHandler<TForeignItem> ForeignHandler { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="tableMetadata"></param>
        /// <param name="groupColumnName"></param>
        /// <param name="foreignHandler"></param>
        public ForeignKeyTableHandler(string connectionString, ISqlTableMetadata tableMetadata, string groupColumnName, SimpleTableHandler<TForeignItem> foreignHandler)
            : base(connectionString, tableMetadata)
        {
            GroupColumnName = groupColumnName;
            ForeignHandler = foreignHandler;
        }

        public async Task<PageEnvelope<TForeignItem>> ReadForeignAsync(Guid groupValue, int offset, int? limit = null)
        {
            var selectRest = $"FROM [{TableMetadata.TableName}] AS local" +
                             $" JOIN [{ForeignHandler.TableName}] AS foregin ON (foreign.Id = local.{GroupColumnName})" +
                             $" WHERE local.[{GroupColumnName}] = @GroupValue";
            return await ForeignHandler.SearchAdvancedAsync("SELECT COUNT(foreign.[Id])", "SELECT foreign.*", selectRest, TableMetadata.GetOrderBy("local."), new { GroupValue = groupValue }, offset, limit);
        }
    }
}
