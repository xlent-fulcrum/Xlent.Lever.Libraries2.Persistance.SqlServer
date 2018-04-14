using System;
using System.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Xlent.Lever.Libraries2.Core.Crud.Interfaces;
using Xlent.Lever.Libraries2.Core.Test.NuGet;
using Xlent.Lever.Libraries2.Core.Test.NuGet.Model;
using Xlent.Lever.Libraries2.SqlServer;
using Xlent.Lever.Libraries2.SqlServer.Model;

namespace Libraries2.SqlServer.Test
{
    [TestClass]
    public class ManyToOneTest : TestIManyToOne<Guid, Guid?>
    {
        private CrudSql<TestItemId<Guid>> _oneStorage;
        private IManyToOneRelationComplete<TestItemManyToOne<Guid, Guid?>, Guid> _manyStorage;

        [TestInitialize]
        public void Inititalize()
        {
            var connectionString = ConfigurationManager.AppSettings["ConnectionString"];
            var manyTableMetadata = new SqlTableMetadata
            {
                TableName = "TestItem",
                CustomColumnNames = new[] { "Value", "ParentId" },
                OrderBy = new string[] { }
            };
            var oneTableMetadata = new SqlTableMetadata
            {
                TableName = "TestItem",
                CustomColumnNames = new[] { "Value" },
                OrderBy = new string[] { }
            };
            _oneStorage = new CrudSql<TestItemId<Guid>>(connectionString, oneTableMetadata);
            _manyStorage = new ManyToOneSql<TestItemManyToOne<Guid, Guid?>, TestItemId<Guid>>(connectionString, manyTableMetadata, "ParentId", _oneStorage);
        }

        /// <inheritdoc />
        protected override IManyToOneRelationComplete<TestItemManyToOne<Guid, Guid?>, Guid>
            ManyStorageRecursive => null;

        /// <inheritdoc />
        protected override IManyToOneRelationComplete<TestItemManyToOne<Guid, Guid?>, Guid>
            ManyStorageNonRecursive => _manyStorage;

        /// <inheritdoc />
        protected override ICrd<TestItemId<Guid>, Guid> OneStorage => _oneStorage;
    }
}
