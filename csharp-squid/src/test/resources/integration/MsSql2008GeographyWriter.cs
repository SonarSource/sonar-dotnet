// Copyright 2008 - Ricardo Stuven (rstuven@gmail.com)
//
// This file is part of NHibernate.Spatial.
// NHibernate.Spatial is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// NHibernate.Spatial is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with NHibernate.Spatial; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

using System;
using Microsoft.SqlServer.Types;
using GeoAPI.Geometries;

namespace NHibernate.Spatial.Type
{
	internal class MsSql2008GeographyWriter
	{
		private readonly SqlGeographyBuilder builder = new SqlGeographyBuilder();

		public SqlGeography Write(IGeometry geometry)
		{
			builder.SetSrid(geometry.SRID);
			AddGeometry(geometry);
			return builder.ConstructedGeography;
		}

		private void AddGeometry(IGeometry geometry)
		{
			if (geometry is IPoint)
			{
				AddPoint(geometry);
			}
			else if (geometry is ILineString)
			{
				AddLineString(geometry);
			}
			else if (geometry is IPolygon)
			{
				AddPolygon(geometry);
			}
			else if (geometry is IMultiPoint)
			{
				AddGeometryCollection(geometry, OpenGisGeographyType.MultiPoint);
			}
			else if (geometry is IMultiLineString)
			{
				AddGeometryCollection(geometry, OpenGisGeographyType.MultiLineString);
			}
			else if (geometry is IMultiPolygon)
			{
				AddGeometryCollection(geometry, OpenGisGeographyType.MultiPolygon);
			}
			else if (geometry is IGeometryCollection)
			{
				AddGeometryCollection(geometry, OpenGisGeographyType.GeometryCollection);
			}
		}

		private void AddGeometryCollection(IGeometry geometry, OpenGisGeographyType type)
		{
			builder.BeginGeography(type);
			IGeometryCollection coll = geometry as IGeometryCollection;
			Array.ForEach<IGeometry>(coll.Geometries, delegate(IGeometry g)
			{
				AddGeometry(g);
			});
			builder.EndGeography();
		}

		private void AddPolygon(IGeometry geometry)
		{
			builder.BeginGeography(OpenGisGeographyType.Polygon);
			IPolygon polygon = geometry as IPolygon;
			AddCoordinates(polygon.ExteriorRing.Coordinates);
			Array.ForEach<ILineString>(polygon.InteriorRings, delegate(ILineString ring)
			{
				AddCoordinates(ring.Coordinates);
			});
			builder.EndGeography();
		}

		private void AddLineString(IGeometry geometry)
		{
			builder.BeginGeography(OpenGisGeographyType.LineString);
			AddCoordinates(geometry.Coordinates);
			builder.EndGeography();
		}

		private void AddPoint(IGeometry geometry)
		{
			builder.BeginGeography(OpenGisGeographyType.Point);
			AddCoordinates(geometry.Coordinates);
			builder.EndGeography();
		}

		private void AddCoordinates(ICoordinate[] coordinates)
		{
			int points = 0;
			Array.ForEach<ICoordinate>(coordinates, delegate(ICoordinate coordinate)
			{
				double? z = null;
				if (!double.IsNaN(coordinate.Z) && !double.IsInfinity(coordinate.Z))
				{
					z = coordinate.Z;
				}
				if (points == 0)
				{
					builder.BeginFigure(coordinate.Y, coordinate.X, z, null);
				}
				else
				{
					builder.AddLine(coordinate.Y, coordinate.X, z, null);
				}
				points++;
			});
			if (points != 0)
			{
				builder.EndFigure();
			}
		}

		public SqlGeography ConstructedGeography
		{
			get { return builder.ConstructedGeography; }
		}
	}
}
