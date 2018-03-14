using System;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using Xlent.Lever.Libraries2.Core.Assert;
using Xlent.Lever.Libraries2.Core.Storage.Model;
using Xlent.Lever.Libraries2.SqlServer.Model;

namespace Xlent.Lever.Libraries2.SqlServer
{
    public class ManyToManyTableHandler<TDatabaseItem, TOneModel1, TOneModel2> : SimpleTableHandler<TDatabaseItem>, IManyToManyRelation<TOneModel1, TOneModel2, Guid>
        where TDatabaseItem : ITableItem, IValidatable
        where TOneModel1 : ITableItem, IValidatable
        where TOneModel2 : ITableItem, IValidatable
    {
        public ManyToOneTableHandler<TDatabaseItem, TOneModel1> OneTableHandler1 { get; }
        public ManyToOneTableHandler<TDatabaseItem, TOneModel2> OneTableHandler2 { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="tableMetadata"></param>
        /// <param name="groupColumnName1"></param>
        /// <param name="foreignHandler1"></param>
        /// <param name="groupColumnName2"></param>
        /// <param name="foreignHandler2"></param>
        public ManyToManyTableHandler(string connectionString, ISqlTableMetadata tableMetadata, string groupColumnName1, SimpleTableHandler<TOneModel1> foreignHandler1, string groupColumnName2, SimpleTableHandler<TOneModel2> foreignHandler2)
            : base(connectionString, tableMetadata)
        {
            OneTableHandler1 = new ManyToOneTableHandler<TDatabaseItem, TOneModel1>(connectionString, tableMetadata, groupColumnName1, foreignHandler1);
            OneTableHandler2 = new ManyToOneTableHandler<TDatabaseItem, TOneModel2>(connectionString, tableMetadata, groupColumnName2, foreignHandler2);
        }

        #region The reference table (the many-to-many table)
        /// <summary>
        /// Get the item that has the specified <paramref name="foreignKey1Value"/> and <paramref name="foreignKey2Value"/>.
        /// </summary>
        public async Task<TDatabaseItem> Read(Guid foreignKey1Value, Guid foreignKey2Value)
        {
            var param = new { ForeignKey1Value = foreignKey1Value, ForeignKey2Value = foreignKey2Value};
            return await SearchWhereSingle($"{OneTableHandler1.ParentColumnName} = @ForeignKey1Value AND {OneTableHandler2.ParentColumnName}= @ForeignKey2Value", param);
        }

        /// <summary>
        /// Find all items that has foreign key 1 set to <paramref name="foreignKey1Value"/>.
        /// </summary>
        public async Task<PageEnvelope<TDatabaseItem>> ReadByForeignKey1(Guid foreignKey1Value, int offset = 0, int? limit = null)
        {
            return await OneTableHandler1.ReadChildrenAsync(foreignKey1Value, offset, limit);
        }

        /// <summary>
        /// Find all items that has foreign key 2 set to <paramref name="foreignKey2Value"/>.
        /// </summary>
        public async Task<PageEnvelope<TDatabaseItem>> ReadByForeignKey2(Guid foreignKey2Value, int offset = 0, int? limit = null)
        {
            return await OneTableHandler2.ReadChildrenAsync(foreignKey2Value, offset, limit);
        }

        /// <inheritdoc />
        public async Task DeleteReferencesByForeignKey1(Guid foreignKey1Value)
        {
            await OneTableHandler1.DeleteChildrenAsync(foreignKey1Value);
        }

        /// <inheritdoc />
        public async Task DeleteReferencesByForeignKey2(Guid foreignKey2Value)
        {
            await OneTableHandler2.DeleteChildrenAsync(foreignKey2Value);
        }
        #endregion

        #region The referenced tables

        /// <inheritdoc />
        public async Task<PageEnvelope<TOneModel2>> ReadReferencedItemsByForeignKey1(Guid foreignKey1Value, int offset = 0, int? limit = null)
        {
            return await OneTableHandler2.ReadAllParentsInGroupAsync(
                OneTableHandler1.ParentColumnName,
                foreignKey1Value, offset, limit);
        }

        /// <inheritdoc />
        public async Task<PageEnvelope<TOneModel1>> ReadReferencedItemsByForeignKey2(Guid foreignKey2Value, int offset = 0, int? limit = null)
        {
            return await OneTableHandler1.ReadAllParentsInGroupAsync(
                OneTableHandler2.ParentColumnName,
                foreignKey2Value, offset, limit);
        }

        #endregion
    }
}
