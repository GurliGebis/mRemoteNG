using System;
using System.Data.Common;
using mRemoteNG.Config.DatabaseConnectors;
using mRemoteNG.Config.Serializers.ConnectionSerializers.Sql;
using mRemoteNG.Tree.Root;
using NSubstitute;
using NUnit.Framework;

namespace mRemoteNGTests.Config.Serializers.ConnectionSerializers.Sql
{
    [TestFixture]
    public class SqlDatabaseMetaDataRetrieverTests
    {
        private SqlDatabaseMetaDataRetriever _retriever;
        private IDatabaseConnector _mockConnector;
        private DbConnection _mockConnection;
        private DbCommand _mockCommand;
        private DbTransaction _mockTransaction;
        private DbParameterCollection _mockParameterCollection;

        [SetUp]
        public void Setup()
        {
            _retriever = new SqlDatabaseMetaDataRetriever();
            _mockConnector = Substitute.For<IDatabaseConnector>();
            _mockConnection = Substitute.For<DbConnection>();
            _mockCommand = Substitute.For<DbCommand>();
            _mockTransaction = Substitute.For<DbTransaction>();
            _mockParameterCollection = Substitute.For<DbParameterCollection>();

            _mockConnector.DbConnection().Returns(_mockConnection);
            _mockConnector.DbCommand(Arg.Any<string>()).Returns(_mockCommand);
            _mockConnection.BeginTransaction().Returns(_mockTransaction);
            _mockCommand.CreateParameter().Returns(_ => Substitute.For<DbParameter>());
            _mockCommand.Parameters.Returns(_mockParameterCollection);
        }

        [Test]
        public void WriteDatabaseMetaData_WithExplicitTransaction_UsesProvidedTransaction()
        {
            var rootNode = new RootNodeInfo(RootNodeType.Connection);
            var explicitTransaction = Substitute.For<DbTransaction>();

            _retriever.WriteDatabaseMetaData(rootNode, _mockConnector, explicitTransaction);

            // Verify that the command was associated with the explicit transaction
            Assert.That(_mockCommand.Transaction, Is.EqualTo(explicitTransaction));
            
            // Verify that Commit was NOT called on the explicit transaction by the retriever
            // (it should be committed by the caller)
            explicitTransaction.DidNotReceive().Commit();
            
            // Verify that the connector's connection was NOT used to begin a new transaction
            _mockConnection.DidNotReceive().BeginTransaction();
        }

        [Test]
        public void WriteDatabaseMetaData_WithoutExplicitTransaction_CreatesAndCommitsTransaction()
        {
            var rootNode = new RootNodeInfo(RootNodeType.Connection);

            _retriever.WriteDatabaseMetaData(rootNode, _mockConnector, null);

            // Verify that a new transaction was started
            _mockConnection.Received(1).BeginTransaction();
            
            // Verify that the command was associated with the new transaction
            Assert.That(_mockCommand.Transaction, Is.EqualTo(_mockTransaction));
            
            // Verify that Commit was called
            _mockTransaction.Received(1).Commit();
        }

        [Test]
        public void WriteDatabaseMetaData_OnFailure_RollsBackCreatedTransaction()
        {
            var rootNode = new RootNodeInfo(RootNodeType.Connection);
            _mockCommand.When(x => x.ExecuteNonQuery()).Do(x => throw new InvalidOperationException("DB Error"));

            Assert.Throws<InvalidOperationException>(() => _retriever.WriteDatabaseMetaData(rootNode, _mockConnector, null));

            // Verify that a new transaction was started
            _mockConnection.Received(1).BeginTransaction();
            
            // Verify that Rollback was called instead of Commit
            _mockTransaction.Received(1).Rollback();
            _mockTransaction.DidNotReceive().Commit();
        }

        /// <summary>
        /// The table-existence probe used to match the database name against
        /// information_schema.tables.table_schema only. That is where MySQL keeps it; SQL Server
        /// and PostgreSQL keep it in table_catalog and use table_schema for "dbo"/"public", so on
        /// those the probe never matched and only the direct-select fallback kept the load from
        /// concluding the schema was missing (upstream #3498). Both columns are accepted now.
        /// </summary>
        [Test]
        public void GetDatabaseMetaData_TableExistenceProbe_AcceptsDatabaseNameInSchemaOrCatalog()
        {
            var commandTexts = new System.Collections.Generic.List<string>();
            var parameters = new System.Collections.Generic.List<DbParameter>();
            _mockConnector.DbCommand(Arg.Do<string>(commandTexts.Add)).Returns(_mockCommand);
            _mockParameterCollection.Add(Arg.Do<object>(p => parameters.Add((DbParameter)p)));

            try
            {
                _retriever.GetDatabaseMetaData(_mockConnector);
            }
            catch (Exception)
            {
                // Everything past the probe runs against substitutes and is not under test here.
            }

            string probe = commandTexts.Find(t => t.Contains("information_schema.tables", StringComparison.OrdinalIgnoreCase));
            Assert.That(probe, Is.Not.Null, "no table-existence probe was issued");
            Assert.Multiple(() =>
            {
                Assert.That(probe, Does.Contain("table_schema = @DatabaseName"));
                Assert.That(probe, Does.Contain("or table_catalog = @DatabaseCatalog"));
                Assert.That(parameters.Count, Is.GreaterThanOrEqualTo(3));
                Assert.That(parameters[0].ParameterName, Is.EqualTo("@TableName"));
                Assert.That(parameters[1].ParameterName, Is.EqualTo("@DatabaseName"));
                Assert.That(parameters[2].ParameterName, Is.EqualTo("@DatabaseCatalog"));
                Assert.That(parameters[2].Value, Is.EqualTo(parameters[1].Value));
            });
        }
    }
}
