using System;
using System.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Xlent.Lever.Libraries2.Crud.Interfaces;
using Xlent.Lever.Libraries2.Crud.Test.NuGet.ManyToOne;
using Xlent.Lever.Libraries2.Crud.Test.NuGet.Model;
using Xlent.Lever.Libraries2.SqlServer;
using Xlent.Lever.Libraries2.SqlServer.Model;

namespace Libraries2.SqlServer.Test
{
    [TestClass]
    public class ManyToOneTest : TestIManyToOne<Guid, Guid?>
    {
        private CrudSql<TestItemId<Guid>> _oneStorage;
        private ICrudManyToOne<TestItemManyToOneCreate<Guid?>, TestItemManyToOne<Guid, Guid?>, Guid> _manyStorage;

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
            _manyStorage = new ManyToOneSql<TestItemManyToOneCreate<Guid?>, TestItemManyToOne<Guid, Guid?>, TestItemId<Guid>>(connectionString, manyTableMetadata, "ParentId", _oneStorage);
        }

        /// <inheritdoc />
        protected override ICrudManyToOne<TestItemManyToOneCreate<Guid?>, TestItemManyToOne<Guid, Guid?>, Guid>
            CrudManyStorageRecursive => null;

        /// <inheritdoc />
        protected override ICrudManyToOne<TestItemManyToOneCreate<Guid?>, TestItemManyToOne<Guid, Guid?>, Guid>
            CrudManyStorageNonRecursive => _manyStorage;

        /// <inheritdoc />
        protected override ICrd<TestItemId<Guid>, Guid> OneStorage => _oneStorage;
    }
}
