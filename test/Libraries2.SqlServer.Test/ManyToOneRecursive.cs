using System;
using System.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Xlent.Lever.Libraries2.Core.Storage.Logic;
using Xlent.Lever.Libraries2.Core.Storage.Model;
using Xlent.Lever.Libraries2.Core.Test.NuGet;
using Xlent.Lever.Libraries2.Core.Test.NuGet.Model;
using Xlent.Lever.Libraries2.SqlServer;
using Xlent.Lever.Libraries2.SqlServer.Model;

namespace Libraries2.SqlServer.Test
{
    [TestClass]
    public class ManyToOneRecursive : TestIManyToOneRecursive<Guid, Guid?>
    {
        private IManyToOneRecursiveRelationComplete<TestItemManyToOne<Guid, Guid?>, Guid> _storage;

        [TestInitialize]
        public void Inititalize()
        {
            var connectionString = ConfigurationManager.AppSettings["ConnectionString"];
            var tableMetadata = new SqlTableMetadata();
            _storage = new ManyToOneRecursiveTableHandler<TestItemManyToOne<Guid, Guid?>>(connectionString, tableMetadata, "ParentId");
        }

        /// <inheritdoc />
        protected override IManyToOneRecursiveRelationComplete<TestItemManyToOne<Guid, Guid?>, Guid>
            ManyStorageRecursive => _storage;

        /// <inheritdoc />
        protected override IManyToOneRelationComplete<TestItemManyToOne<Guid, Guid?>, TestItemId<Guid>, Guid>
            ManyStorageNonRecursive => null;

        /// <inheritdoc />
        protected override ICrd<TestItemId<Guid>, Guid> OneStorage => null;
    }
}
