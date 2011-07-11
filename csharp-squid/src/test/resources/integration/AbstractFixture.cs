using System;
using System.Collections;
using System.Data;
using GisSharpBlog.NetTopologySuite.IO;
using log4net;
using log4net.Config;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Connection;
using NHibernate.Mapping;
using NHibernate.Spatial.Dialect;
using NHibernate.Spatial.Mapping;
using NHibernate.Spatial.Metadata;
using NHibernate.Tool.hbm2ddl;
using NHibernate.Type;
using NUnit.Framework;
using Settings = Tests.NHibernate.Spatial.Properties.Settings;
using NHibernate.Engine;

namespace Tests.NHibernate.Spatial
{
	// Copied and modified from NHibernate.Test/TestCase.cs
	public abstract class AbstractFixture
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(AbstractFixture));
		protected static readonly WKTReader Wkt = new WKTReader();
		protected Configuration configuration;
		protected ISessionFactory sessions;
		private ISpatialDialect spatialDialect;
		private ISession lastOpenedSession;
		private DebugConnectionProvider connectionProvider;

		protected abstract Type[] Mappings { get; }

		static AbstractFixture()
		{
			// Configure log4net here since configuration through an attribute doesn't always work.
			XmlConfigurator.Configure();
		}

		/// <summary>
		/// Creates the tables used in this TestCase
		/// </summary>
		[TestFixtureSetUp]
		public void TestFixtureSetUp()
		{
			try
			{
				Configure();
				BuildSessionFactory();
				ConfigureSpatialMetadata();

				bool created = true;
				try
				{
					CreateSchema();
				}
				catch
				{
					created = false;
				}
				if (!created)
				{
					DropSchema();
					CreateSchema();
				}
			}
			catch (Exception e)
			{
				Log.Error("Error while setting up the test fixture", e);
				throw;
			}
			OnTestFixtureSetUp();
		}

		private void ConfigureSpatialMetadata()
		{
			bool rebuildSessionFactory = false;

			if (spatialDialect.SupportsSpatialMetadata(MetadataClass.GeometryColumn))
			{
				Metadata.AddMapping(configuration, MetadataClass.GeometryColumn);
				rebuildSessionFactory = true;
			}

			if (spatialDialect.SupportsSpatialMetadata(MetadataClass.SpatialReferenceSystem))
			{
				Metadata.AddMapping(configuration, MetadataClass.SpatialReferenceSystem);
				rebuildSessionFactory = true;
			}

			if (rebuildSessionFactory)
			{
				sessions = configuration.BuildSessionFactory();
			}
		}

		protected virtual void OnTestFixtureSetUp()
		{
		}

		/// <summary>
		/// Removes the tables used in this TestCase.
		/// </summary>
		/// <remarks>
		/// If the tables are not cleaned up sometimes SchemaExport runs into
		/// Sql errors because it can't drop tables because of the FKs.  This 
		/// will occur if the TestCase does not have the same hbm.xml files
		/// included as a previous one.
		/// </remarks>
		[TestFixtureTearDown]
		public void TestFixtureTearDown()
		{
			OnTestFixtureTearDown();
			DropSchema();
			Cleanup();
		}

		protected virtual void OnTestFixtureTearDown()
		{
		}

		protected virtual void OnSetUp()
		{
		}

		/// <summary>
		/// Set up the test. This method is not overridable, but it calls
		/// <see cref="OnSetUp" /> which is.
		/// </summary>
		[SetUp]
		public void SetUp()
		{
			OnSetUp();
		}

		protected virtual void OnTearDown()
		{
		}

		/// <summary>
		/// Checks that the test case cleans up after itself. This method
		/// is not overridable, but it calls <see cref="OnTearDown" /> which is.
		/// </summary>
		[TearDown]
		public void TearDown()
		{
			OnTearDown();

			bool wasClosed = CheckSessionWasClosed();
			bool wasCleaned = !this.CheckDatabaseWasCleanedOnTearDown || CheckDatabaseWasCleaned();
			bool wereConnectionsClosed = CheckConnectionsWereClosed();
			bool fail = !wasClosed || !wasCleaned || !wereConnectionsClosed;

			if (fail)
			{
				Assert.Fail("Test didn't clean up after itself");
			}
		}

		public void DeleteMappings(ISession session)
		{
			foreach (Type type in this.Mappings)
			{
				session.Delete("from " + type.FullName);
			}
			session.Flush();
			session.Clear();
		}

		private bool CheckSessionWasClosed()
		{
			if (lastOpenedSession != null && lastOpenedSession.IsOpen)
			{
				Log.Error("Test case didn't close a session, closing");
				lastOpenedSession.Close();
				return false;
			}

			return true;
		}

		private bool CheckDatabaseWasCleaned()
		{
			if (sessions.GetAllClassMetadata().Count == 0)
			{
				// Return early in the case of no mappings, also avoiding
				// a warning when executing the HQL below.
				return true;
			}

			bool empty = false;
			using (ISession s = sessions.OpenSession())
			{
				foreach (Type type in this.Mappings)
				{
					IList objects = s.CreateQuery("from " + type.FullName).List();
					empty = objects.Count == 0;
					if (!empty)
						break;
				}
			}

			if (!empty)
			{
				Log.Error("Test case didn't clean up the database after itself, re-creating the schema");
				DropSchema();
				CreateSchema();
			}

			return empty;
		}

		private bool CheckConnectionsWereClosed()
		{
			if (connectionProvider == null || !connectionProvider.HasOpenConnections)
			{
				return true;
			}

			Log.Error("Test case didn't close all open connections, closing");
			connectionProvider.CloseAllConnections();
			return false;
		}

		private void Configure()
		{
			configuration = CreateConfiguration();
		}

		private Configuration CreateConfiguration()
		{
			Configuration configuration = new Configuration();

			Configure(configuration);

			foreach (Type type in this.Mappings)
			{
				configuration.AddClass(type);
			}

			ApplyCacheSettings(configuration);

			return configuration;
		}

		protected virtual void OnBeforeCreateSchema()
		{
		}

		private void CreateSchema()
		{
			OnBeforeCreateSchema();
			// Isolated configuration doesn't include SpatialReferenceSystem mapping,
			Configuration configuration = CreateConfiguration();
			configuration.AddAuxiliaryDatabaseObject(new SpatialAuxiliaryDatabaseObject(configuration));
			new SchemaExport(configuration).Create(Settings.Default.OutputDdl, true);
		}

		protected virtual void OnAfterDropSchema()
		{
		}

		private void DropSchema()
		{
			// Isolated configuration doesn't include SpatialReferenceSystem mapping,
			Configuration configuration = CreateConfiguration();
			configuration.AddAuxiliaryDatabaseObject(new SpatialAuxiliaryDatabaseObject(configuration));
			new SchemaExport(configuration).Drop(Settings.Default.OutputDdl, true);
			OnAfterDropSchema();
		}

		private void BuildSessionFactory()
		{
			sessions = configuration.BuildSessionFactory();
			spatialDialect = (ISpatialDialect)((ISessionFactoryImplementor)this.sessions).Dialect;
			connectionProvider = ((ISessionFactoryImplementor)this.sessions).ConnectionProvider as DebugConnectionProvider;
		}

		private void Cleanup()
		{
			sessions.Close();
			sessions = null;
			spatialDialect = null;
			connectionProvider = null;
			lastOpenedSession = null;
			configuration = null;
		}

		public int ExecuteStatement(string sql)
		{
			if (configuration == null)
			{
				configuration = new Configuration();
			}

			using (IConnectionProvider prov = ConnectionProviderFactory.NewConnectionProvider(configuration.Properties))
			{
				IDbConnection conn = prov.GetConnection();

				try
				{
					using (IDbTransaction tran = conn.BeginTransaction())
					using (IDbCommand comm = conn.CreateCommand())
					{
						comm.CommandText = sql;
						comm.Transaction = tran;
						comm.CommandType = CommandType.Text;
						int result = comm.ExecuteNonQuery();
						tran.Commit();
						return result;
					}
				}
				finally
				{
					prov.CloseConnection(conn);
				}
			}
		}

		protected ISession OpenSession()
		{
			lastOpenedSession = sessions.OpenSession();
			return lastOpenedSession;
		}

		protected void ApplyCacheSettings(Configuration configuration)
		{
			if (CacheConcurrencyStrategy == null)
			{
				return;
			}

			foreach (PersistentClass clazz in configuration.ClassMappings)
			{
				bool hasLob = false;
				foreach (global::NHibernate.Mapping.Property prop in clazz.PropertyClosureIterator)
				{
					if (prop.Value.IsSimpleValue)
					{
						IType type = ((SimpleValue)prop.Value).Type;
						if (type == NHibernateUtil.BinaryBlob)
						{
							hasLob = true;
						}
					}
				}
				if (!hasLob && !clazz.IsInherited)
				{
					configuration.SetCacheConcurrencyStrategy(clazz.MappedClass.Name, CacheConcurrencyStrategy);
				}
			}

			foreach (global::NHibernate.Mapping.Collection coll in configuration.CollectionMappings)
			{
				configuration.SetCacheConcurrencyStrategy(coll.Role, CacheConcurrencyStrategy);
			}
		}

		#region Properties overridable by subclasses

		protected virtual void Configure(Configuration configuration)
		{
		}

		protected virtual string CacheConcurrencyStrategy
		{
			//get { return "nonstrict-read-write"; }
			get { return null; }
		}

		protected virtual bool CheckDatabaseWasCleanedOnTearDown
		{
			get { return true; }
		}

		#endregion

	}
}
