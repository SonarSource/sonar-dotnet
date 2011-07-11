using System;
using System.Collections;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Spatial.Criterion;
using NUnit.Framework;
using Tests.NHibernate.Spatial.Model;

namespace Tests.NHibernate.Spatial
{
	public abstract class ProjectionsFixture : AbstractFixture
	{
		protected override Type[] Mappings
		{
			get
			{
				return new Type[] { 
					typeof(County)
				};
			}
		}

		private ISession session;

		protected override void OnSetUp()
		{
			session = sessions.OpenSession();

			session.Save(new County("aaaa", "AA", Wkt.Read("POLYGON((1 0, 2 0, 2 1, 1 1, 1 0))")));
			session.Save(new County("bbbb", "BB", Wkt.Read("POLYGON((1 1, 2 1, 2 2, 1 2, 1 1))")));
			session.Save(new County("cccc", "BB", Wkt.Read("POLYGON((2 1, 3 1, 3 2, 2 2, 2 1))")));
			session.Save(new County("dddd", "AA", Wkt.Read("POLYGON((2 0, 3 0, 3 1, 2 1, 2 0))")));
			session.Flush();
		}

		protected override void OnTearDown()
		{
			DeleteMappings(session);
			session.Close();
		}

		[Test]
		public void CountAndUnion()
		{
			IList results = session.CreateCriteria(typeof(County))
				.SetProjection(Projections.ProjectionList()
					.Add(Projections.RowCount())
					.Add(SpatialProjections.Union("Boundaries"))
					)
				.List();

			Assert.AreEqual(1, results.Count);

			object[] result = (object[])results[0];

			IGeometry expected = Wkt.Read("POLYGON((1 0, 1 1, 1 2, 2 2, 3 2, 3 1, 3 0, 2 0, 1 0))");
			IGeometry aggregated = (IGeometry)result[1];

			Assert.AreEqual(4, result[0]);
			Assert.IsTrue(expected.Equals(aggregated));

		}

		[Test]
		public void CountAndUnionByState()
		{
			IList results = session.CreateCriteria(typeof(County))
				.AddOrder(Order.Asc("State"))
				.SetProjection(Projections.ProjectionList()
					.Add(Projections.GroupProperty("State"))
					.Add(Projections.RowCount())
					.Add(SpatialProjections.Union("Boundaries"))
					)
				.List();

			Assert.AreEqual(2, results.Count);

			object[] resultAA = (object[])results[0];
			object[] resultBB = (object[])results[1];

			int countAA = (int)resultAA[1];
			int countBB = (int)resultBB[1];
			IGeometry aggregatedAA = (IGeometry)resultAA[2];
			IGeometry aggregatedBB = (IGeometry)resultBB[2];

			IGeometry expectedAA = Wkt.Read("POLYGON((1 0, 1 1, 3 1, 3 0, 1 0))");
			IGeometry expectedBB = Wkt.Read("POLYGON((1 1, 1 2, 3 2, 3 1, 1 1))");

			Assert.AreEqual(2, countAA);
			Assert.AreEqual(2, countBB);
			Assert.IsTrue(expectedAA.Equals(aggregatedAA));
			Assert.IsTrue(expectedBB.Equals(aggregatedBB));

		}

		[Test]
		public void EnvelopeAll()
		{
			IList results = session.CreateCriteria(typeof(County))
				.SetProjection(SpatialProjections.Envelope("Boundaries"))
				.List();

			Assert.AreEqual(1, results.Count);

			IGeometry aggregated = (IGeometry)results[0];
			IEnvelope expected = new Envelope(1, 3, 0, 2);

			Assert.IsTrue(expected.Equals(aggregated.EnvelopeInternal));

		}

		[Test]
		public void CollectAll()
		{
			IList results = session.CreateCriteria(typeof(County))
				.SetProjection(SpatialProjections.Collect("Boundaries"))
				.List();

			Assert.AreEqual(1, results.Count);

			IGeometry aggregated = (IGeometry)results[0];

			Assert.AreEqual(4, aggregated.NumGeometries);
			//Assert.AreEqual("GEOMETRYCOLLECTION", aggregated.GeometryType);
		}

		[Test]
		public void IntersectionAll()
		{
			IList results = session.CreateCriteria(typeof(County))
				.SetProjection(SpatialProjections.Intersection("Boundaries"))
				.List();

			Assert.AreEqual(1, results.Count);

			IGeometry aggregated = (IGeometry)results[0];
			IGeometry expected = new Point(2, 1);

			Assert.IsTrue(expected.Equals(aggregated));
		}


	}

}
