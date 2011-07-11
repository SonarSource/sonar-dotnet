using System;
using System.Collections;
using System.IO;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Spatial.Criterion;
using NUnit.Framework;
using Open.Topology.TestRunner;
using Tests.NHibernate.Spatial.NtsTestCases.Model;

namespace Tests.NHibernate.Spatial.NtsTestCases
{
	/// <summary>
	/// This fixture reuses some NTS test runner data.
	/// </summary>
	public abstract class NtsTestCasesFixture : AbstractFixture
	{
		protected override Type[] Mappings
		{
			get
			{
				return new Type[]{
					typeof(NtsTestCase),
				}; 
			}
		}

		protected override void OnTestFixtureSetUp()
		{
			using (ISession session = sessions.OpenSession())
			{
				string basePath = Path.Combine(
					AppDomain.CurrentDomain.BaseDirectory,
					@"..\..\..\Tests.NHibernate.Spatial\NtsTestCases\Data");

				string[] filenames = new string[] {

					@"vivid\TestFunctionAA.xml",
					@"vivid\TestFunctionAAPrec.xml",

					@"vivid\TestRelateAA.xml",
					@"vivid\TestRelateAC.xml",

					@"vivid\TestRectanglePredicate.xml",

					@"vivid\TestSimple.xml",
					@"vivid\TestValid.xml",
				};

				long id = 0;
				for (int i = 0; i < filenames.Length; i++)
				{
					LoadTestCases(session, ref id, Path.Combine(basePath, filenames[i]));
				}
				session.Flush();
			}
		}

		private static void LoadTestCases(ISession session, ref long id, string filename)
		{
			XmlTestDocument document = new XmlTestDocument();
			document.LoadFile(filename);
			foreach (XmlTestCollection testCase in document.Tests)
			{
				foreach (XmlTest test in testCase)
				{
					NtsTestCase ntsTestCase = new NtsTestCase();
					switch (test.TestType)
					{
						case XmlTestType.Intersection:
						case XmlTestType.Union:
						case XmlTestType.Difference:
						case XmlTestType.SymmetricDifference:
						case XmlTestType.Boundary:
						case XmlTestType.Centroid:
						case XmlTestType.ConvexHull:
						case XmlTestType.Envelope:
						case XmlTestType.InteriorPoint:
							ntsTestCase.GeometryResult = (IGeometry)test.Result;
							break;
						case XmlTestType.Contains:
						case XmlTestType.CoveredBy:
						case XmlTestType.Covers:
						case XmlTestType.Crosses:
						case XmlTestType.Disjoint:
						case XmlTestType.Equals:
						case XmlTestType.Intersects:
						case XmlTestType.IsEmpty:
						case XmlTestType.IsSimple:
						case XmlTestType.IsValid:
						case XmlTestType.Touches:
						case XmlTestType.Within:
							ntsTestCase.BooleanResult = (bool)test.Result;
							break;
						case XmlTestType.Relate:
							ntsTestCase.RelatePattern = (string)test.Argument2;
							ntsTestCase.BooleanResult = (bool)test.Result;
							break;
						default:
							continue;
					}
					ntsTestCase.Operation = test.TestType.ToString();
					ntsTestCase.Description = testCase.Name + ": " + test.Description;

					if (test.IsDefaultTarget)
					{
						ntsTestCase.GeometryA = test.A;
						ntsTestCase.GeometryB = test.B;
					}
					else
					{
						ntsTestCase.GeometryA = test.B;
						ntsTestCase.GeometryB = test.A;
					}

					ntsTestCase.Id = ++id;

					Prepare(ntsTestCase);

					session.Save(ntsTestCase);
				}
			}
		}

		/// <summary>
		/// Prepares an entity for saving.
		/// </summary>
		/// <param name="ntsTestCase"></param>
		private static void Prepare(NtsTestCase ntsTestCase)
		{
			ntsTestCase.GeometryA = Prepare(ntsTestCase.GeometryA);
			ntsTestCase.GeometryB = Prepare(ntsTestCase.GeometryB);
			ntsTestCase.GeometryResult = Prepare(ntsTestCase.GeometryResult);
		}

		/// <summary>
		/// Prepares a geometry for saving.
		/// </summary>
		/// <param name="geometry"></param>
		/// <returns></returns>
		private static IGeometry Prepare(IGeometry geometry)
		{
			if (geometry == null)
			{
				geometry = GeometryCollection.Empty;
			}
			else
			{
				geometry = ConvertToSqlGeometryType(geometry);
			}
			geometry.SRID = -1;
			return geometry;
		}

		/// <summary>
		/// Some geometries are not OGC SQL Geometry Types, 
		/// so we convert them to .
		/// </summary>
		/// <param name="geometry"></param>
		/// <returns></returns>
		private static IGeometry ConvertToSqlGeometryType(IGeometry geometry)
		{
			if (geometry is ILinearRing)
			{
				return new Polygon((ILinearRing)geometry, null);
			}
			return geometry;
		}

		protected override void OnTestFixtureTearDown()
		{
			using (ISession session = sessions.OpenSession())
			{
				DeleteMappings(session);
			}
		}

		private ISession session;

		protected override void OnSetUp()
		{
			session = sessions.OpenSession();
		}

		protected override void OnTearDown()
		{
			session.Clear();
			session.Close();
			session.Dispose();
			session = null;
		}

		protected override bool CheckDatabaseWasCleanedOnTearDown
		{
			get { return false; }
		}

		#region Supporting test functions

		private delegate SpatialProjection SpatialProjectionBinaryDelegate(string propertyName, string anotherPropertyName);
		private delegate AbstractCriterion SpatialRelationCriterionDelegate(string propertyName, object anotherGeometry);
		private delegate SpatialProjection SpatialProjectionUnaryDelegate(string propertyName);
		private delegate AbstractCriterion SpatialCriterionUnaryDelegate(string propertyName);

		private void TestGeometryBinaryOperation(string operationCriterion, SpatialProjectionBinaryDelegate projection)
		{
			IList results = session.CreateCriteria(typeof(NtsTestCase))
				.Add(Restrictions.Eq("Operation", operationCriterion))
				.SetProjection(Projections.ProjectionList()
					.Add(Projections.Property("Description"))
					.Add(Projections.Property("GeometryResult"))
					.Add(projection("GeometryA", "GeometryB"))
					)
				.List();

			Assert.Greater(results.Count, 0);

			foreach (object[] result in results)
			{
				string description = (string)result[0];
				IGeometry expected = (IGeometry)result[1];
				IGeometry operation = (IGeometry)result[2];

				expected.Normalize();
				operation.Normalize();

				bool equals = expected.EqualsExact(operation, 1.5);
				if (!equals)
					Console.WriteLine(operationCriterion + ": " + description);
				//Assert.IsTrue(equals, description);
			}
		}

		private void TestGeometryUnaryOperation(string operationCriterion, SpatialProjectionUnaryDelegate projection)
		{
			IList results = session.CreateCriteria(typeof(NtsTestCase))
				.Add(Restrictions.Eq("Operation", operationCriterion))
				.SetProjection(Projections.ProjectionList()
					.Add(Projections.Property("Description"))
					.Add(Projections.Property("GeometryResult"))
					.Add(projection("GeometryA"))
					)
				.List();

			Assert.Greater(results.Count, 0);

			foreach (object[] result in results)
			{
				string description = (string)result[0];
				IGeometry expected = (IGeometry)result[1];
				IGeometry operation = (IGeometry)result[2];

				expected.Normalize();
				operation.Normalize();

				bool equals = expected.EqualsExact(operation, 1.5);
				if (!equals)
					Console.WriteLine(operationCriterion + ": " + description);
				//Assert.IsTrue(equals, description);
			}
		}

		private void TestBooleanBinaryOperation(string operationCriterion, SpatialProjectionBinaryDelegate projection, SpatialRelationCriterionDelegate criterion)
		{
			IList results = session.CreateCriteria(typeof(NtsTestCase))
				.Add(Restrictions.Eq("Operation", operationCriterion))
				.SetProjection(Projections.ProjectionList()
					.Add(Projections.Property("Description"))
					.Add(Projections.Property("BooleanResult"))
					.Add(projection("GeometryA", "GeometryB"))
					)
				.List();

			Assert.Greater(results.Count, 0);

			long countTrue = 0;

			foreach (object[] result in results)
			{
				string description = (string)result[0];
				bool expected = (bool)result[1];
				bool operation = (bool)result[2];

				Assert.AreEqual(expected, operation);

				if (operation)
					countTrue++;
			}

			// RowCount uses "count(*)" which in PostgreSQL returns Int64 and
			// in MS SQL Server return Int32.
			long countRows = Convert.ToInt64(session.CreateCriteria(typeof(NtsTestCase))
				.Add(Restrictions.Eq("Operation", operationCriterion))
				.Add(criterion("GeometryA", "GeometryB"))
				.SetProjection(Projections.RowCount())
				.UniqueResult());

			Assert.AreEqual(countTrue, countRows);
		}

		private void TestBooleanUnaryOperation(string operationCriterion, SpatialProjectionUnaryDelegate projection, SpatialCriterionUnaryDelegate criterion)
		{
			IList results = session.CreateCriteria(typeof(NtsTestCase))
				.Add(Restrictions.Eq("Operation", operationCriterion))
				.SetProjection(Projections.ProjectionList()
					.Add(Projections.Property("Description"))
					.Add(Projections.Property("BooleanResult"))
					.Add(projection("GeometryA"))
					)
				.List();

			Assert.Greater(results.Count, 0);

			long countTrue = 0;

			foreach (object[] result in results)
			{
				string description = (string)result[0];
				bool expected = (bool)result[1];
				bool operation = (bool)result[2];

				Assert.AreEqual(expected, operation, description);

				if (operation)
					countTrue++;
			}

			// RowCount uses "count(*)" which in PostgreSQL returns Int64 and
			// in MS SQL Server return Int32.
			long countRows = Convert.ToInt64(session.CreateCriteria(typeof(NtsTestCase))
				.Add(Restrictions.Eq("Operation", operationCriterion))
				.Add(criterion("GeometryA"))
				.SetProjection(Projections.RowCount())
				.UniqueResult());

			Assert.AreEqual(countTrue, countRows);
		}

		#endregion

		#region Analysis

		[Test]
		public void Intersection()
		{
			TestGeometryBinaryOperation("Intersection", SpatialProjections.Intersection);
		}

		[Test]
		public void Union()
		{
			TestGeometryBinaryOperation("Union", SpatialProjections.Union);
		}

		[Test]
		public void Difference()
		{
			TestGeometryBinaryOperation("Difference", SpatialProjections.Difference);
		}

		[Test]
		public void SymmetricDifference()
		{
			TestGeometryBinaryOperation("SymmetricDifference", SpatialProjections.SymDifference);
		}

		[Test]
		public void ConvexHull()
		{
			TestGeometryUnaryOperation("ConvexHull", SpatialProjections.ConvexHull);
		}

		#endregion

		#region Relations

		[Test]
		public void BooleanRelate()
		{
			IList results = session.CreateCriteria(typeof(NtsTestCase))
				.Add(Restrictions.Eq("Operation", "Relate"))
				.SetProjection(Projections.ProjectionList()
					.Add(Projections.Property("Description"))
					.Add(Projections.Property("BooleanResult"))
					.Add(SpatialProjections.Relate("GeometryA", "GeometryB", "RelatePattern"))
					)
				.List();

			Assert.Greater(results.Count, 0);

			foreach (object[] result in results)
			{
				string description = (string)result[0];
				bool expected = (bool)result[1];
				bool operation = (bool)result[2];

				Assert.AreEqual(expected, operation);
			}
		}

		[Test]
		public void StringRelate()
		{
			IList results = session.CreateCriteria(typeof(NtsTestCase))
				.Add(Restrictions.Eq("Operation", "Relate"))
				.SetProjection(Projections.ProjectionList()
					.Add(Projections.Property("Description"))
					.Add(Projections.Property("RelatePattern"))
					.Add(SpatialProjections.Relate("GeometryA", "GeometryB"))
					)
				.List();

			Assert.Greater(results.Count, 0);

			foreach (object[] result in results)
			{
				string description = (string)result[0];
				string expected = (string)result[1];
				string operation = (string)result[2];

				Assert.AreEqual(expected, operation);
			}
		}

		[Test]
		public void Contains()
		{
			TestBooleanBinaryOperation("Contains", SpatialProjections.Contains, SpatialRestrictions.Contains);
		}

		[Test]
		public void CoveredBy()
		{
			TestBooleanBinaryOperation("CoveredBy", SpatialProjections.CoveredBy, SpatialRestrictions.CoveredBy);
		}

		[Test]
		public void Covers()
		{
			TestBooleanBinaryOperation("Covers", SpatialProjections.Covers, SpatialRestrictions.Covers);
		}

		[Test]
		[Ignore("No data to test")]
		public void Crosses()
		{
			TestBooleanBinaryOperation("Crosses", SpatialProjections.Crosses, SpatialRestrictions.Crosses);
		}

		[Test]
		[Ignore("No data to test")]
		public void Disjoint()
		{
			TestBooleanBinaryOperation("Disjoint", SpatialProjections.Disjoint, SpatialRestrictions.Disjoint);
		}

		[Test]
		[Ignore("No data to test")]
		public void Equals()
		{
			TestBooleanBinaryOperation("Equals", SpatialProjections.Equals, SpatialRestrictions.Eq);
		}

		[Test]
		public void Intersects()
		{
			TestBooleanBinaryOperation("Intersects", SpatialProjections.Intersects, SpatialRestrictions.Intersects);
		}

		[Test]
		[Ignore("No data to test")]
		public void Overlaps()
		{
			TestBooleanBinaryOperation("Overlaps", SpatialProjections.Overlaps, SpatialRestrictions.Overlaps);
		}

		[Test]
		[Ignore("No data to test")]
		public void Touches()
		{
			TestBooleanBinaryOperation("Touches", SpatialProjections.Touches, SpatialRestrictions.Touches);
		}

		[Test]
		public virtual void Within()
		{
			TestBooleanBinaryOperation("Within", SpatialProjections.Within, SpatialRestrictions.Within);
		}

		#endregion

		#region Validations

		[Test]
		[Ignore("No data to test")]
		public void IsClosed()
		{
			TestBooleanUnaryOperation("IsClosed", SpatialProjections.IsClosed, SpatialRestrictions.IsClosed);
		}

		[Test]
		[Ignore("No data to test")]
		public void IsEmpty()
		{
			TestBooleanUnaryOperation("IsEmpty", SpatialProjections.IsEmpty, SpatialRestrictions.IsEmpty);
		}

		[Test]
		[Ignore("No data to test")]
		public void IsRing()
		{
			TestBooleanUnaryOperation("IsRing", SpatialProjections.IsRing, SpatialRestrictions.IsRing);
		}

		[Test]
		public void IsSimple()
		{
			TestBooleanUnaryOperation("IsSimple", SpatialProjections.IsSimple, SpatialRestrictions.IsSimple);
		}

		[Test]
		public virtual void IsValid()
		{
			TestBooleanUnaryOperation("IsValid", SpatialProjections.IsValid, SpatialRestrictions.IsValid);
		}

		#endregion

	}
}
