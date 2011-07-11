using System;
using System.Collections;
using System.Data;
using GeoAPI.Geometries;
using NHibernate;
using NHibernate.Spatial.Criterion;
using NHibernate.Spatial.Dialect;
using NUnit.Framework;
using Tests.NHibernate.Spatial.RandomGeometries.Model;

namespace Tests.NHibernate.Spatial.RandomGeometries
{
	/// <summary>
	/// Port of MAJAS Hibernate Spatial test suite.
	/// </summary>
	public abstract class SpatialQueriesFixture : AbstractFixture
	{
		private IGeometry filter;
		private const string FilterString = "POLYGON((0.0 0.0, 25000.0 0.0, 25000.0 25000.0, 0.0 25000.0, 0.0 0.0))";

		protected override Type[] Mappings
		{
			get
			{
				return new Type[] { 
					typeof(LineStringEntity),
					typeof(MultiLineStringEntity),
					typeof(MultiPointEntity),
					typeof(MultiPolygonEntity),
					typeof(PointEntity),
					typeof(PolygonEntity),
				};
			}
		}

		private ISession session;

		protected override bool CheckDatabaseWasCleanedOnTearDown
		{
			get { return false; }
		}

		protected override void OnTestFixtureSetUp()
		{
			DataGenerator.Generate(sessions);

			this.filter = Wkt.Read(FilterString);
			this.filter.SRID = 4326;
		}

		protected override void OnTestFixtureTearDown()
		{
			using (ISession session = sessions.OpenSession())
			{
				DeleteMappings(session);
				session.Close();
			}
		}

		protected override void OnSetUp()
		{
			session = sessions.OpenSession();
		}
		protected override void OnTearDown()
		{
			session.Clear();
			session.Close();
		}

		protected abstract string SqlLineStringFilter(string filterString);
		protected abstract string SqlPolygonFilter(string filterString);
		protected abstract string SqlMultiLineStringFilter(string filterString);
		protected abstract string SqlOvelapsLineString(string filterString);
		protected abstract string SqlIntersectsLineString(string filterString);
		protected abstract ISQLQuery SqlIsEmptyLineString(ISession session);
		protected abstract ISQLQuery SqlIsSimpleLineString(ISession session);
		protected abstract ISQLQuery SqlAsBinaryLineString(ISession session);

		[Test]
		public void LineStringFiltering()
		{
			IList results = session.CreateCriteria(typeof(LineStringEntity))
				.Add(SpatialRestrictions.Filter("Geometry", this.filter))
				.List();

			long count;
			using (IDbCommand command = session.Connection.CreateCommand())
			{
				command.CommandText = this.SqlLineStringFilter(FilterString);
				count = (long)command.ExecuteScalar();
			}

			Assert.AreEqual(count, results.Count);
		}

		[Test]
		public void PolygonFiltering()
		{
			IList results = session.CreateCriteria(typeof(PolygonEntity))
				.Add(SpatialRestrictions.Filter("Geometry", this.filter))
				.List();

			long count;
			using (IDbCommand command = session.Connection.CreateCommand())
			{
				command.CommandText = this.SqlPolygonFilter(FilterString);
				count = (long)command.ExecuteScalar();
			}

			Assert.AreEqual(count, results.Count);
		}

		[Test]
		public void MultiLineStringFiltering()
		{
			IList results = session.CreateCriteria(typeof(MultiLineStringEntity))
				.Add(SpatialRestrictions.Filter("Geometry", this.filter))
				.List();

			long count;
			using (IDbCommand command = session.Connection.CreateCommand())
			{
				command.CommandText = this.SqlMultiLineStringFilter(FilterString);
				count = (long)command.ExecuteScalar();
			}

			Assert.AreEqual(count, results.Count);
		}

		[Test]
		public void HqlAsTextLineString()  
		{
			IList results = session
				.CreateQuery("select NHSP.AsText(l.Geometry) from LineStringEntity as l")
				.SetMaxResults(10)
				.List();
			foreach (string item in results)
			{
				Assert.IsNotNull(item);
				Assert.AreNotEqual(string.Empty, item);
			}
		}

		[Test]
	    public void HqlDimensionLineString()
		{
			IList results1 = session
				.CreateQuery("select NHSP.Dimension(l.Geometry) from LineStringEntity as l where l.Geometry is not null")
				.SetMaxResults(10)
				.List();

			foreach (int dim in results1)
			{
				Assert.AreEqual(1, dim);
			}

			IList results2 = session
				.CreateQuery("select NHSP.Dimension(p.Geometry) from PolygonEntity as p where p.Geometry is not null")
				.SetMaxResults(10)
				.List();

			foreach (int dim in results2)
			{
				Assert.AreEqual(2, dim);
			}
		}

		[Test]
		public void HqlOverlapsLineString()
		{
			IList results = session
				.CreateQuery("select NHSP.Overlaps(?,l.Geometry) from LineStringEntity as l where l.Geometry is not null")
				.SetParameter(0, this.filter, SpatialDialect.GeometryTypeOf(session))
				.List();

			long countOverlapping = 0;
			foreach (bool isOverlapped in results)
			{
				if (isOverlapped) countOverlapping++;
			}

			long count;
			using (IDbCommand command = session.Connection.CreateCommand())
			{
				command.CommandText = this.SqlOvelapsLineString(FilterString);
				count = (long)command.ExecuteScalar();
			}

			Assert.AreEqual(countOverlapping, count);
		}

		[Test]
		public void HqlRelateLineString()
		{
			long count = session
				.CreateQuery("select count(*) from LineStringEntity l where l.Geometry is not null and NHSP.Relate(l.Geometry, ?, 'TT*******') = true")
				.SetParameter(0, this.filter, SpatialDialect.GeometryTypeOf(session))
				.UniqueResult<long>();

			Assert.Greater((int)count, 0);
		}

		[Test]
		public void HqlIntersectsLineString()
		{
			IList results = session
				.CreateQuery("select NHSP.Intersects(?,l.Geometry) from LineStringEntity as l where l.Geometry is not null")
				.SetParameter(0, this.filter, SpatialDialect.GeometryTypeOf(session))
				.List();

			long intersects = 0;
			foreach (bool b in results)
			{
				if (b) intersects++;
			}

			long altIntersects = session
				.CreateQuery("select count(*) from LineStringEntity as l where NHSP.Intersects(l.Geometry, ?) = true")
				.SetParameter(0, this.filter, SpatialDialect.GeometryTypeOf(session))
				.UniqueResult<long>();

			Assert.AreEqual(intersects, altIntersects);

			long count;
			using (IDbCommand command = session.Connection.CreateCommand())
			{
				command.CommandText = this.SqlIntersectsLineString(FilterString);
				count = (long)command.ExecuteScalar();
			}

			Assert.AreEqual(intersects, count);

			results = session
				.CreateQuery("from LineStringEntity as l where NHSP.Intersects(?,l.Geometry) = true")
				.SetParameter(0, this.filter, SpatialDialect.GeometryTypeOf(session))
				.List();

			Assert.AreEqual(count, results.Count);

		}

		[Test]
		public void HqlSRID()
		{
			IList results = session
				.CreateQuery("select NHSP.SRID(l.Geometry) from LineStringEntity as l where l.Geometry is not null")
				.List();

			foreach (object item in results)
			{
				int srid = (int)item;
				Assert.AreEqual(4326, srid);
			}
		}

		[Test]
		public void HqlGeometryType()
		{
			IList results = session
				.CreateQuery("select NHSP.GeometryType(l.Geometry) from LineStringEntity as l where l.Geometry is not null")
				.List();

			foreach (object item in results)
			{
				string gt = (string)item;
				Assert.AreEqual("LINESTRING", gt.ToUpper());
			}

			results = session
				.CreateQuery("select NHSP.GeometryType(p.Geometry) from PolygonEntity as p where p.Geometry is not null")
				.List();

			foreach (object item in results)
			{
				string gt = (string)item;
				Assert.AreEqual("POLYGON", gt.ToUpper());
			}
		}

		[Test]
		public void HqlEnvelope()
		{
			HqlEnvelope("LineStringEntity");
			HqlEnvelope("PolygonEntity");
		}

		private void HqlEnvelope(string entityName)
		{
			IList results = session
				.CreateQuery("select NHSP.Envelope(e.Geometry), e.Geometry from "
					+ entityName + " as e where e.Geometry is not null")
				.SetMaxResults(10)
				.List();
			foreach (object[] item in results)
			{
				IGeometry env = (IGeometry)item[0];
				IGeometry g = (IGeometry)item[1];
				Assert.IsTrue(g.Envelope.Equals(env));
			}
		}

		[Test]
		public void HqlIsEmpty()
		{
			IList results = session
				.CreateQuery("select l.Id, NHSP.IsEmpty(l.Geometry) from LineStringEntity as l where l.Geometry is not null")
				.List();

			ISQLQuery query = this.SqlIsEmptyLineString(session);

			foreach (object[] item in results)
			{
				long id = (long)item[0];
				bool isEmpty = (bool)item[1];
				query.SetInt64(0, id);
				bool expected = query.UniqueResult<bool>();
				Assert.AreEqual(expected, isEmpty);
			}
		}

		[Test]
		public void HqlIsSimple()
		{
			IList results = session
				.CreateQuery("select l.Id, NHSP.IsSimple(l.Geometry) from LineStringEntity as l where l.Geometry is not null")
				.List();

			ISQLQuery query = this.SqlIsSimpleLineString(session);

			foreach (object[] item in results)
			{
				long id = (long)item[0];
				bool isSimple = (bool)item[1];
				query.SetInt64(0, id);
				bool expected = query.UniqueResult<bool>();
				Assert.AreEqual(expected, isSimple);
			}
		}

		[Test]
		public void HqlBoundary()
		{
			IList results = session
				.CreateQuery("select p.Geometry, NHSP.Boundary(p.Geometry) from PolygonEntity as p where p.Geometry is not null")
				.List();
			foreach (object[] item in results)
			{
				IGeometry geom = (IGeometry)item[0];
				IGeometry bound = (IGeometry)item[1];
				Assert.IsTrue(geom.Boundary.Equals(bound));
			}
		}

		[Test]
		public void HqlAsBoundary()
		{
			IList results = session
				.CreateQuery("select l.Id, NHSP.AsBinary(l.Geometry) from LineStringEntity as l where l.Geometry is not null")
				.List();

			ISQLQuery query = this.SqlAsBinaryLineString(session);

			foreach (object[] item in results)
			{
				long id = (long)item[0];
				byte[] wkb = (byte[])item[1];
				query.SetInt64(0, id);
				byte[] expected = query.UniqueResult<byte[]>();
				Assert.AreEqual(expected, wkb);
			}
		}

		[Test]
		public void HqlDistance()
		{
			IList results = session
				.CreateQuery(@"
					select NHSP.Distance(l.Geometry, ?), l.Geometry
					from LineStringEntity as l
					where l.Geometry is not null")
				.SetParameter(0, this.filter, SpatialDialect.GeometryTypeOf(session))
				.SetMaxResults(100)
				.List();
			foreach (object[] item in results)
			{
				double distance = (double)item[0];
				IGeometry geom = (IGeometry)item[1];
				Assert.AreEqual(geom.Distance(this.filter), distance, 0.003);
			}
		}

		[Test]
		public void HqlDistanceMin()
		{
			const int minDistance = 40000;

			IList results = session
				.CreateQuery(@"
					select NHSP.Distance(l.Geometry, :filter), l.Geometry
					from LineStringEntity as l
					where l.Geometry is not null
					and NHSP.Distance(l.Geometry, :filter) > :minDistance
					order by NHSP.Distance(l.Geometry, :filter)")
				.SetParameter("filter", this.filter, SpatialDialect.GeometryTypeOf(session))
				.SetParameter("minDistance", minDistance)
				.SetMaxResults(100)
				.List();

			Assert.IsNotEmpty(results);
			foreach (object[] item in results)
			{
				double distance = (double)item[0];
				Assert.Greater(distance, minDistance);
				IGeometry geom = (IGeometry)item[1];
				Assert.AreEqual(geom.Distance(this.filter), distance, 0.003);
			}
		}

		[Test]
		public void HqlBuffer()
		{
			const double distance = 10.0;

			IList results = session
				.CreateQuery("select p.Geometry, NHSP.Buffer(p.Geometry, ?) from PolygonEntity as p where p.Geometry is not null")
				.SetDouble(0, distance)
				.SetMaxResults(100)
				.List();

			int count = 0;
			foreach (object[] item in results)
			{
				IGeometry geom = (IGeometry)item[0];
				IGeometry buffer = (IGeometry)item[1];
				IGeometry ntsBuffer = geom.Buffer(distance);

				buffer.Normalize();
				ntsBuffer.Normalize();

				if (IsApproximateCoincident(ntsBuffer, buffer, 0.05))
					count++;
			}
			Assert.Greater(count, 0);
		}

		[Test]
		public void HqlConvexHull()
		{
			IList results = session
				.CreateQuery("select m.Geometry, NHSP.ConvexHull(m.Geometry) from MultiLineStringEntity as m where m.Geometry is not null")
				.SetMaxResults(100)
				.List();

			int count = 0;
			foreach (object[] item in results)
			{
				IGeometry geom = (IGeometry)item[0];
				IGeometry cvh = (IGeometry)item[1];
				IGeometry ntsCvh = geom.ConvexHull();

				Assert.IsTrue(cvh.Contains(geom));

				cvh.Normalize();
				ntsCvh.Normalize();

				if (ntsCvh.EqualsExact(cvh, 0.5))
					count++;
			}
			Assert.Greater(count, 0);
		}

		[Test]
		public void HqlDifference()
		{
			IList results = session
				.CreateQuery("select e.Geometry, NHSP.Difference(e.Geometry, ?) from PolygonEntity as e where e.Geometry is not null")
				.SetParameter(0, this.filter, SpatialDialect.GeometryTypeOf(session))
				.SetMaxResults(100)
				.List();

			int count = 0;
			int countEmpty = 0;
			foreach (object[] item in results)
			{
				IGeometry geom = (IGeometry)item[0];
				IGeometry diff = (IGeometry)item[1];

				// some databases give a null object if the difference is the
				// null-set
				if (diff == null || diff.IsEmpty)
				{
					countEmpty++;
					continue;
				}

				diff.Normalize();
				IGeometry ntsDiff = geom.Difference(this.filter);
				ntsDiff.Normalize();

				if (ntsDiff.EqualsExact(diff, 0.5))
					count++;
			}
			Assert.Greater(count, 0);
		}

		[Test]
		public void HqlIntersection()
		{
			IList results = session
				.CreateQuery("select e.Geometry, NHSP.Intersection(e.Geometry, ?) from PolygonEntity as e where e.Geometry is not null")
				.SetParameter(0, this.filter, SpatialDialect.GeometryTypeOf(session))
				.SetMaxResults(100)
				.List();

			int count = 0;
			int countEmpty = 0;
			foreach (object[] item in results)
			{
				IGeometry geom = (IGeometry)item[0];
				IGeometry intersect = (IGeometry)item[1];

				// some databases give a null object if the difference is the
				// null-set
				if (intersect == null || intersect.IsEmpty)
				{
					countEmpty++;
					continue;
				}

				intersect.Normalize();
				IGeometry ntsIntersect = geom.Intersection(this.filter);
				ntsIntersect.Normalize();

				if (ntsIntersect.EqualsExact(intersect, 0.5))
					count++;
			}
			Assert.Greater(count, 0);
		}

		[Test]
		public void HqlSymDifference()
		{
			IList results = session
				.CreateQuery("select e.Geometry, NHSP.SymDifference(e.Geometry, ?) from PolygonEntity as e where e.Geometry is not null")
				.SetParameter(0, this.filter, SpatialDialect.GeometryTypeOf(session))
				.SetMaxResults(100)
				.List();

			int count = 0;
			int countEmpty = 0;
			foreach (object[] item in results)
			{
				IGeometry geom = (IGeometry)item[0];
				IGeometry symDiff = (IGeometry)item[1];

				// some databases give a null object if the difference is the
				// null-set
				if (symDiff == null || symDiff.IsEmpty)
				{
					countEmpty++;
					continue;
				}

				symDiff.Normalize();
				IGeometry ntsSymDiff = geom.SymmetricDifference(this.filter);
				ntsSymDiff.Normalize();

				if (ntsSymDiff.EqualsExact(symDiff, 0.5))
					count++;
			}
			Assert.Greater(count, 0);
		}

		[Test]
		public void HqlUnion()
		{
			IList results = session
				.CreateQuery("select e.Geometry, NHSP.Union(e.Geometry, ?) from PolygonEntity as e where e.Geometry is not null")
				.SetParameter(0, this.filter, SpatialDialect.GeometryTypeOf(session))
				.SetMaxResults(100)
				.List();

			int count = 0;
			int countEmpty = 0;
			foreach (object[] item in results)
			{
				IGeometry geom = (IGeometry)item[0];
				IGeometry union = (IGeometry)item[1];

				// some databases give a null object if the difference is the
				// null-set
				if (union == null || union.IsEmpty)
				{
					countEmpty++;
					continue;
				}

				union.Normalize();
				IGeometry ntsUnion = geom.Union(this.filter);
				ntsUnion.Normalize();

				if (ntsUnion.EqualsExact(union, 0.5))
					count++;
			}
			Assert.Greater(count, 0);
		}

		private static bool IsApproximateCoincident(IGeometry g1, IGeometry g2, double tolerance)
		{
			IGeometry symdiff;
			if (g1.Dimension < Dimensions.Surface && g2.Dimension < Dimensions.Surface)
			{
				g1 = g1.Buffer(tolerance);
				g2 = g2.Buffer(tolerance);
				symdiff = g1.SymmetricDifference(g2).Buffer(tolerance);
			}
			else
			{
				symdiff = g1.SymmetricDifference(g2);
			}
			double relError = symdiff.Area / (g1.Area + g2.Area);
			return relError < tolerance;

		}

	}
}
