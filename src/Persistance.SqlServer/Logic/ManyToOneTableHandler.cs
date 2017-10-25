using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Xlent.Lever.Libraries2.Core.Assert;
using Xlent.Lever.Libraries2.Core.Error.Logic;
using Xlent.Lever.Libraries2.Core.Storage.Model;
using Xlent.Lever.Libraries2.Persistance.SqlServer.Model;

namespace Xlent.Lever.Libraries2.Persistance.SqlServer.Logic
{
    /// <summary>
    /// Helper class for advanced SELECT statmements
    /// </summary>
    /// <typeparam name="TDatabaseItem"></typeparam>
    public class ManyToOneTableHandler<TDatabaseItem> : Database, IGroupStorage<TDatabaseItem, Guid>
        where TDatabaseItem : ITableItem, IValidatable, new()
    {
        private readonly TDatabaseItem _databaseItem;
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="connectionString"></param>
        public ManyToOneTableHandler(string connectionString)
            : base(connectionString)
        {
            _databaseItem = new TDatabaseItem();
        }

        /// <summary>
        /// The name of the table that this class handles.
        /// </summary>
        public string TableName => _databaseItem.TableName;

        public async Task<TDatabaseItem> CreateAsync(Guid groupId, TDatabaseItem item)
        {
            InternalContract.RequireNotNull(item, nameof(item));
            if (item.Id == Guid.Empty) item.Id = Guid.NewGuid();
            item.ETag = Guid.NewGuid().ToString();
            InternalContract.RequireValidated(item, nameof(item));
            using (var db = NewSqlConnection())
            {
                await db.ExecuteAsync(SqlHelper.Create(item), item);
            }
            return item.Id;
        }

        public async Task<IEnumerable<TDatabaseItem>> ReadAllAsync(Guid groupId)
        {
            throw new NotImplementedException();
        }

        public async Task DeleteAllAsync(Guid groupId)
        {
            throw new NotImplementedException();
        }
    }
}
