using System;
using System.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Xlent.Lever.Libraries2.Core.Application;
using Xlent.Lever.Libraries2.Core.Crud.Interfaces;
using Xlent.Lever.Libraries2.Core.Test.NuGet.Crud;
using Xlent.Lever.Libraries2.Core.Test.NuGet.Model;
using Xlent.Lever.Libraries2.SqlServer;
using Xlent.Lever.Libraries2.SqlServer.Model;

namespace Libraries2.SqlServer.Test
{
    [TestClass]
    public class CrudTestEtag : TestICrudEtag<Guid>
    {
        private CrudSql<TestItemBare, TestItemEtag<Guid>> _storage;

        [TestInitialize]
        public void Inititalize()
        {
            FulcrumApplicationHelper.UnitTestSetup(nameof(CrudTestEtag));
            var connectionString = ConfigurationManager.AppSettings["ConnectionString"];
            var tableMetadata = new SqlTableMetadata
            {
                TableName = "TestItem",
                EtagColumnName = "Etag",
                CustomColumnNames = new[] { "Value" },
                OrderBy = new string[] { }
            };
            _storage = new CrudSql<TestItemBare, TestItemEtag<Guid>>(connectionString, tableMetadata);
        }

        protected override ICrud<TestItemBare, TestItemEtag<Guid>, Guid> CrudStorage => _storage;
    }
}