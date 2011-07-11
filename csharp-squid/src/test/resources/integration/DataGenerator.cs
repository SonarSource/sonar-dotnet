using System;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;
using NHibernate;
using Tests.NHibernate.Spatial.RandomGeometries.Model;

namespace Tests.NHibernate.Spatial.RandomGeometries
{
	internal static class DataGenerator
	{
		private const int GeneratedRowsPerEntityCount = 500;
		private const int MinCoordValue = 0;
		private const int MaxCoordValue = 100000;
		private const int MaxNumGeom = 10;
		private const int MaxNumCoords = 20;
		private static readonly Random Random = new Random();

		public static void Generate(ISessionFactory factory)
		{
			GenerateData(factory, typeof(LineStringEntity), new LineStringCreator());
			GenerateData(factory, typeof(MultiLineStringEntity), new MultiLineStringCreator());
			GenerateData(factory, typeof(PolygonEntity), new PolygonCreator());
		}
		
		private static void GenerateData(ISessionFactory factory, Type entityClass, IGeometryCreator creator)
		{
			using (ISession session = factory.OpenSession())
			{
				using (ITransaction tx = session.BeginTransaction())
				{
					try
					{
						for (int i = 0; i < GeneratedRowsPerEntityCount; i++)
						{
							IGeometry geom = creator.Create();
							geom.SRID = 4326;
							object entity = Activator.CreateInstance(entityClass, i, "feature " + i, geom);
							session.Save(entity);
						}
					}
					catch (Exception e)
					{
						throw new ApplicationException("Failed loading data of type "
								+ entityClass.Name, e);
					}
					tx.Commit();
				}
			}

		}

		private static double GetRandomCoordinateValue()
		{
			return Random.Next(MinCoordValue, MaxCoordValue);
		}

		private static int GetRandomNumGeoms()
		{
			return Random.Next(1, MaxNumGeom);
		}

		private static int GetRandomNumCoords(int minValue)
		{
			return Random.Next(minValue, MaxNumCoords);
		}

		private static ICoordinate GetRandomCoordinate() {
			double x = GetRandomCoordinateValue();
			double y = GetRandomCoordinateValue();
			return new Coordinate(x, y);
		}

		private interface IGeometryCreator
		{
			IGeometry Create();
		}

		private class LineStringCreator : IGeometryCreator
		{
			public IGeometry Create()
			{
				int numCoords = GetRandomNumCoords(2);
				ICoordinate[] coordinates = new ICoordinate[numCoords];
				for (int i = 0; i < numCoords; i++) {
					coordinates[i] = GetRandomCoordinate();
				}
				return GeometryFactory.Default.CreateLineString(coordinates);
			}
		}

		private class MultiLineStringCreator : IGeometryCreator
		{
			public IGeometry Create()
			{
				// at least two linestrings, otherwise some databases like
				// Oracle will
				// store multilinestring as linestring in stead.
				int numGeoms = 1 + GetRandomNumGeoms();
				LineStringCreator lsc = new LineStringCreator();
				ILineString[] lines = new ILineString[numGeoms];
				for (int i = 0; i < numGeoms; i++) {
					lines[i] = (ILineString)lsc.Create();
				}
				return GeometryFactory.Default.CreateMultiLineString(lines);
			}
		}

		// This create small boxes
		// TODO -- find a better way to generate random linear rings
		private class LinearRingCreator : IGeometryCreator
		{
			public IGeometry Create()
			{

				const int numCoords = 4;
				ICoordinate[] coordinates = new ICoordinate[numCoords + 1];

				coordinates[0] = GetRandomCoordinate();
				coordinates[1] = new Coordinate(coordinates[0].X,
						coordinates[0].Y + 10.0d);
				coordinates[2] = new Coordinate(coordinates[0].X + 10.0d,
						coordinates[0].Y + 10.0d);
				coordinates[3] = new Coordinate(coordinates[0].X + 10.0d,
						coordinates[0].Y);
				coordinates[numCoords] = coordinates[0];
				ILinearRing lr = GeometryFactory.Default.CreateLinearRing(coordinates);
				return lr;
			}
		}

		private class PolygonCreator : IGeometryCreator
		{
			public IGeometry Create()
			{
				ILinearRing lr = (ILinearRing)(new LinearRingCreator()).Create();
				IPolygon pg = GeometryFactory.Default.CreatePolygon(lr, null);
				return pg;
			}
		}

		// TO DO -- add polygons with holes!!
	}
}
