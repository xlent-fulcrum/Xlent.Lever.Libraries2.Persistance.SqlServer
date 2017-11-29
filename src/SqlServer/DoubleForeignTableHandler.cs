using System;
using System.Threading.Tasks;
using Xlent.Lever.Libraries2.Core.Assert;
using Xlent.Lever.Libraries2.Core.Storage.Model;
using Xlent.Lever.Libraries2.SqlServer.Model;

namespace Xlent.Lever.Libraries2.SqlServer
{
    public class DoubleForeignTableHandler<TDatabaseItem, TForeignModel1, TForeignModel2> : SimpleTableHandler<TDatabaseItem>
        where TDatabaseItem : ITableItem, IValidatable
        where TForeignModel1 : ITableItem, IValidatable
        where TForeignModel2 : ITableItem, IValidatable
    {
        public ForeignKeyTableHandler<TDatabaseItem, TForeignModel1> ForeignKeyTableHandler1 { get; }
        public ForeignKeyTableHandler<TDatabaseItem, TForeignModel2> ForeignKeyTableHandler2 { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="tableMetadata"></param>
        /// <param name="groupColumnName1"></param>
        /// <param name="foreignHandler1"></param>
        /// <param name="groupColumnName2"></param>
        /// <param name="foreignHandler2"></param>
        public DoubleForeignTableHandler(string connectionString, ISqlTableMetadata tableMetadata, string groupColumnName1, SimpleTableHandler<TForeignModel1> foreignHandler1, string groupColumnName2, SimpleTableHandler<TForeignModel2> foreignHandler2)
            : base(connectionString, tableMetadata)
        {
            ForeignKeyTableHandler1 = new ForeignKeyTableHandler<TDatabaseItem, TForeignModel1>(connectionString, tableMetadata, groupColumnName1, foreignHandler1);
            ForeignKeyTableHandler2 = new ForeignKeyTableHandler<TDatabaseItem, TForeignModel2>(connectionString, tableMetadata, groupColumnName2, foreignHandler2);
        }

        #region READ
        /// <summary>
        /// Get the item that has the specified <paramref name="foreignKey1Value"/> and <paramref name="foreignKey2Value"/>.
        /// </summary>
        public async Task<TDatabaseItem> Read(Guid foreignKey1Value, Guid foreignKey2Value)
        {
            var param = new { ForeignKey1Value = foreignKey1Value, ForeignKey2Value = foreignKey2Value};
            return await SearchWhereSingle($"{ForeignKeyTableHandler1.GroupColumnName} = @ForeignKey1Value AND {ForeignKeyTableHandler2.GroupColumnName}= @ForeignKey2Value", param);
        }

        /// <summary>
        /// Find all items that has foreign key 1 set to <paramref name="foreignKey1Value"/>.
        /// </summary>
        public async Task<PageEnvelope<TDatabaseItem>> ReadByForeignKey1(Guid foreignKey1Value, int offset = 0, int? limit = null)
        {
            var param = new { ForeignKey1Value = foreignKey1Value };
            return await SearchWhereAsync($"{ForeignKeyTableHandler1.GroupColumnName} = @ForeignKey1Value", null, param, offset, limit);
        }

        /// <summary>
        /// Find all items that has foreign key 2 set to <paramref name="foreignKey2Value"/>.
        /// </summary>
        public async Task<PageEnvelope<TDatabaseItem>> ReadByForeignKey2(Guid foreignKey2Value, int offset = 0, int? limit = null)
        {
            var param = new { ForeignKey2Value = foreignKey2Value };
            return await SearchWhereAsync($"{ForeignKeyTableHandler2.GroupColumnName} = @ForeignKey2Value", null, param, offset, limit);
        }
        #endregion


        #region SEARCH
        /// <summary>
        /// Find all items with foreign key 1 set to <paramref name="foreignKey1Value"/>.
        /// </summary>
        public async Task<PageEnvelope<TForeignModel2>> SearchByForeignKey1(Guid foreignKey1Value, int offset = 0, int? limit = null)
        {
            return await ForeignKeyTableHandler2.ReadForeignAsync(foreignKey1Value, offset, limit);
        }

        /// <summary>
        /// Find all items with foreign key 2 set to <paramref name="foreignKey2Value"/>.
        /// </summary>
        public async Task<PageEnvelope<TForeignModel1>> SearchByForeignKey2(Guid foreignKey2Value, int offset = 0, int? limit = null)
        {
            return await ForeignKeyTableHandler1.ReadForeignAsync(foreignKey2Value, offset, limit);
        }
        #endregion
    }
}
