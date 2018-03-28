using System;
using System.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Xlent.Lever.Libraries2.Core.Application;
using Xlent.Lever.Libraries2.Core.Storage.Model;
using Xlent.Lever.Libraries2.Core.Test.NuGet;
using Xlent.Lever.Libraries2.Core.Test.NuGet.Model;
using Xlent.Lever.Libraries2.SqlServer;
using Xlent.Lever.Libraries2.SqlServer.Model;

namespace Libraries2.SqlServer.Test
{
    [TestClass]
    public class CrudTestValidated : TestICrudValidated<Guid>
    {
        private SimpleTableHandler<TestItemValidated<Guid>> _storage;

        [TestInitialize]
        public void Inititalize()
        {
            FulcrumApplicationHelper.UnitTestSetup(nameof(CrudTestValidated));
            var connectionString = ConfigurationManager.AppSettings["ConnectionString"];
            var tableMetadata = new SqlTableMetadata
            {
                TableName = "TestItem",
                CustomColumnNames = new[] { "Value" },
                OrderBy = new string[] { }
            };
            _storage = new SimpleTableHandler<TestItemValidated<Guid>>(connectionString, tableMetadata);
        }

        protected override ICrud<TestItemValidated<Guid>, Guid> CrudStorage => _storage;
    }
}