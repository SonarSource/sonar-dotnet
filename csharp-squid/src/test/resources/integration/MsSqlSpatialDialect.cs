// Copyright 2007 - Ricardo Stuven (rstuven@gmail.com)
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
using System.Text;
using NHibernate.Dialect;
using NHibernate.Spatial.Dialect.Function;
using NHibernate.Spatial.Metadata;
using NHibernate.Spatial.Type;
using NHibernate.SqlCommand;
using NHibernate.Type;
using NHibernate.Util;

namespace NHibernate.Spatial.Dialect
{
	/// <summary>
	/// 
	/// </summary>
	public class MsSqlSpatialDialect : MsSql2005Dialect, ISpatialDialect
	{
		private static readonly IType geometryType = new CustomType(typeof(MsSqlSpatialGeometryType), null);
		
		private const string DialectPrefix = "ST.";

		/// <summary>
		/// Initializes a new instance of the <see cref="MsSqlSpatialDialect"/> class.
		/// </summary>
		public MsSqlSpatialDialect()
		{
			SpatialDialect.LastInstantiated = this;
			RegisterBasicFunctions();
			RegisterFunctions();
		}

		#region Functions registration

		private void RegisterBasicFunctions()
		{
			// Relations
			RegisterSpatialFunction(SpatialRelation.Contains);
			RegisterSpatialFunction(SpatialRelation.CoveredBy);
			RegisterSpatialFunction(SpatialRelation.Covers);
			RegisterSpatialFunction(SpatialRelation.Crosses);
			RegisterSpatialFunction(SpatialRelation.Disjoint);
			RegisterSpatialFunction(SpatialRelation.Equals);
			RegisterSpatialFunction(SpatialRelation.Intersects);
			RegisterSpatialFunction(SpatialRelation.Overlaps);
			RegisterSpatialFunction(SpatialRelation.Touches);
			RegisterSpatialFunction(SpatialRelation.Within);

			// Analysis
			RegisterSpatialFunction(SpatialAnalysis.Buffer);
			RegisterSpatialFunction(SpatialAnalysis.ConvexHull);
			RegisterSpatialFunction(SpatialAnalysis.Difference);
			RegisterSpatialFunction(SpatialAnalysis.Distance);
			RegisterSpatialFunction(SpatialAnalysis.Intersection);
			RegisterSpatialFunction(SpatialAnalysis.SymDifference);
			RegisterSpatialFunction(SpatialAnalysis.Union);

			// Validations
			RegisterSpatialFunction(SpatialValidation.IsClosed);
			RegisterSpatialFunction(SpatialValidation.IsEmpty);
			RegisterSpatialFunction(SpatialValidation.IsRing);
			RegisterSpatialFunction(SpatialValidation.IsSimple);
			RegisterSpatialFunction(SpatialValidation.IsValid);
		}

		private void RegisterFunctions()
		{
			RegisterSpatialFunction("Boundary");
			RegisterSpatialFunction("Centroid");
			RegisterSpatialFunction("EndPoint");
			RegisterSpatialFunction("Envelope");
			RegisterSpatialFunction("ExteriorRing");
			RegisterSpatialFunction("GeometryN", 2);
			RegisterSpatialFunction("InteriorRingN", 2);
			RegisterSpatialFunction("PointN", 2);
			RegisterSpatialFunction("PointOnSurface");
			RegisterSpatialFunction("Simplify", 2);
			RegisterSpatialFunction("StartPoint");
			RegisterSpatialFunction("Transform", 2);

			RegisterSpatialFunction("GeomCollFromText", 2);
			RegisterSpatialFunction("GeomCollFromWKB", 2);
			RegisterSpatialFunction("GeomFromText", 2);
			RegisterSpatialFunction("GeomFromWKB", 2);
			RegisterSpatialFunction("LineFromText", 2);
			RegisterSpatialFunction("LineFromWKB", 2);
			RegisterSpatialFunction("PointFromText", 2);
			RegisterSpatialFunction("PointFromWKB", 2);
            RegisterSpatialFunction("PolyFromText", "PolygonFromText", 2);
            RegisterSpatialFunction("PolyFromWKB", "PolygonFromWKB", 2);
            RegisterSpatialFunction("MLineFromText", 2);
			RegisterSpatialFunction("MLineFromWKB", 2);
			RegisterSpatialFunction("MPointFromText", 2);
			RegisterSpatialFunction("MPointFromWKB", 2);
			RegisterSpatialFunction("MPolyFromText", 2);
			RegisterSpatialFunction("MPolyFromWKB", 2);

			RegisterSpatialFunction("AsBinary", NHibernateUtil.Binary);

			RegisterSpatialFunction("AsText", NHibernateUtil.String);
			RegisterSpatialFunction("AsGML", NHibernateUtil.String);
			RegisterSpatialFunction("GeometryType", NHibernateUtil.String);

			RegisterSpatialFunction("Area", NHibernateUtil.Double);
			RegisterSpatialFunction("Length", NHibernateUtil.Double);
			RegisterSpatialFunction("X", NHibernateUtil.Double);
			RegisterSpatialFunction("Y", NHibernateUtil.Double);

			RegisterSpatialFunction("SRID", NHibernateUtil.Int32);
			RegisterSpatialFunction("Dimension", NHibernateUtil.Int32);
			RegisterSpatialFunction("NumGeometries", NHibernateUtil.Int32);
            RegisterSpatialFunction("NumInteriorRings", NHibernateUtil.Int32);
			RegisterSpatialFunction("NumPoints", NHibernateUtil.Int32);

			RegisterSpatialFunction("Relate", NHibernateUtil.Boolean, 3);
		}

		private void RegisterSpatialFunction(string standardName, string dialectName, IType returnedType, int allowedArgsCount)
		{
			RegisterFunction(SpatialDialect.HqlPrefix + standardName, new SpatialStandardSafeFunction(DialectPrefix + dialectName, returnedType, allowedArgsCount));
		}

		private void RegisterSpatialFunction(string standardName, string dialectName, IType returnedType)
		{
			RegisterSpatialFunction(standardName, dialectName, returnedType, 1);
		}

		private void RegisterSpatialFunction(string name, IType returnedType, int allowedArgsCount)
		{
			RegisterSpatialFunction(name, name, returnedType, allowedArgsCount);
		}

		private void RegisterSpatialFunction(string name, IType returnedType)
		{
			RegisterSpatialFunction(name, name, returnedType);
		}

		private void RegisterSpatialFunction(string name, int allowedArgsCount)
		{
			RegisterSpatialFunction(name, this.GeometryType, allowedArgsCount);
		}

		private void RegisterSpatialFunction(string name)
		{
			RegisterSpatialFunction(name, this.GeometryType);
		}

		private void RegisterSpatialFunction(string standardName, string dialectName, int allowedArgsCount)
		{
			RegisterSpatialFunction(standardName, dialectName, this.GeometryType);
		}

		private void RegisterSpatialFunction(SpatialRelation relation)
		{
			RegisterFunction(SpatialDialect.HqlPrefix + relation.ToString(), new SpatialRelationFunction(this, relation));
		}

		private void RegisterSpatialFunction(SpatialValidation validation)
		{
			RegisterFunction(SpatialDialect.HqlPrefix + validation.ToString(), new SpatialValidationFunction(this, validation));
		}

		private void RegisterSpatialFunction(SpatialAnalysis analysis)
		{
			RegisterFunction(SpatialDialect.HqlPrefix + analysis.ToString(), new SpatialAnalysisFunction(this, analysis));
		}



		#endregion

		#region ISpatialDialect Members

		/// <summary>
		/// Creates the geometry user type.
		/// </summary>
		/// <returns></returns>
		public IGeometryUserType CreateGeometryUserType()
		{
			return new MsSqlSpatialGeometryType();
		}

		/// <summary>
		/// Gets the type of the geometry.
		/// </summary>
		/// <value>The type of the geometry.</value>
		public IType GeometryType
		{
			get { return geometryType; }
		}

		/// <summary>
		/// Gets the spatial transform string.
		/// </summary>
		/// <param name="geometry">The geometry.</param>
		/// <param name="srid">The srid.</param>
		/// <returns></returns>
		public SqlString GetSpatialTransformString(object geometry, int srid)
		{
			return new SqlStringBuilder()
				.Add(DialectPrefix)
				.Add("Transform(")
				.AddObject(geometry)
				.Add(",")
				.Add(srid.ToString())
				.Add(")")
				.ToSqlString();
		}

		/// <summary>
		/// Gets the spatial aggregate string.
		/// </summary>
		/// <param name="geometry">The geometry.</param>
		/// <param name="aggregate">The aggregate.</param>
		/// <returns></returns>
		public SqlString GetSpatialAggregateString(object geometry, SpatialAggregate aggregate)
		{
			string aggregateFunction;
			switch (aggregate)
			{
				case SpatialAggregate.Collect:
					aggregateFunction = DialectPrefix + "Collect";
					break;
				case SpatialAggregate.Envelope:
					aggregateFunction = DialectPrefix + "EnvelopeAggregate";
					break;
				case SpatialAggregate.Intersection:
					aggregateFunction = DialectPrefix + "IntersectionAggregate";
					break;
				case SpatialAggregate.Union:
					aggregateFunction = DialectPrefix + "UnionAggregate";
					break;
				default:
					throw new ArgumentException("Invalid spatial aggregate argument");
			}
			return new SqlStringBuilder()
				.Add(aggregateFunction)
				.Add("(")
				.AddObject(geometry)
				.Add(")")
				.ToSqlString();
		}

		/// <summary>
		/// Gets the spatial validation string.
		/// </summary>
		/// <param name="geometry">The geometry.</param>
		/// <param name="validation">The validation.</param>
		/// <param name="criterion">if set to <c>true</c> [criterion].</param>
		/// <returns></returns>
		public SqlString GetSpatialValidationString(object geometry, SpatialValidation validation, bool criterion)
		{
			return new SqlStringBuilder()
				.Add(DialectPrefix)
				.Add(validation.ToString())
				.Add("(")
				.AddObject(geometry)
				.Add(")")
				.Add(criterion ? " = 1" : "")
				.ToSqlString();
		}

		public SqlString GetSpatialRelationString(object geometry, SpatialRelation relation, object anotherGeometry, bool criterion)
		{
			return new SqlStringBuilder(8)
				.Add(DialectPrefix)
				.Add("[")
				.Add(relation.ToString())
				.Add("](")
				.AddObject(geometry)
				.Add(", ")
				.AddObject(anotherGeometry)
				.Add(")")
				.Add(criterion ? " = 1" : "")
				.ToSqlString();
		}

		public SqlString GetSpatialRelateString(object geometry, object anotherGeometry, object pattern, bool isStringPattern, bool criterion)
		{
			SqlStringBuilder builder = new SqlStringBuilder();
			builder
				.Add(DialectPrefix)
				.Add("Relate");
			if (pattern == null)
			{
				builder.Add("Pattern");
			}
			builder
				.Add("(")
				.AddObject(geometry)
				.Add(", ")
				.AddObject(anotherGeometry);
			if (pattern != null)
			{
				builder.Add(", ");
				if (isStringPattern)
				{
					builder
						.Add("'")
						.Add((string)pattern)
						.Add("'");
				}
				else
				{
					builder.AddObject(pattern);
				}
			}
			return builder
				.Add(")")
				.Add(criterion ? " = 1" : "")
				.ToSqlString();
		}

		public SqlString GetSpatialFilterString(string tableAlias, string geometryColumnName, string primaryKeyColumnName, string tableName)
		{
			return new SqlStringBuilder(8)
				.Add(primaryKeyColumnName)
				.Add(" in (SELECT oid FROM ST.FilterQuery('")
				.Add(tableName)
				.Add("', '")
				.Add(geometryColumnName)
				.Add("', ")
				.AddParameter()
				.Add("))")
				.ToSqlString();
		}

		/// <summary>
		/// Gets the spatial analysis string.
		/// </summary>
		/// <param name="geometry">The geometry.</param>
		/// <param name="analysis">The analysis.</param>
		/// <param name="extraArgument">The extra argument.</param>
		/// <returns></returns>
		public SqlString GetSpatialAnalysisString(object geometry, SpatialAnalysis analysis, object extraArgument)
		{
			switch (analysis)
			{
				case SpatialAnalysis.Buffer:
					if (!(extraArgument is Parameter || SqlString.Parameter.Equals(extraArgument)))
					{
						extraArgument = Convert.ToString(extraArgument, System.Globalization.NumberFormatInfo.InvariantInfo);
					}
					return new SqlStringBuilder()
						.Add(DialectPrefix)
						.Add("Buffer(")
						.AddObject(geometry)
						.Add(", ")
						.AddObject(extraArgument)
						.Add(")")
						.ToSqlString();
				case SpatialAnalysis.ConvexHull:
					return new SqlStringBuilder()
						.Add(DialectPrefix)
						.Add("ConvexHull(")
						.AddObject(geometry)
						.Add(")")
						.ToSqlString();
				case SpatialAnalysis.Difference:
				case SpatialAnalysis.Distance:
				case SpatialAnalysis.Intersection:
				case SpatialAnalysis.SymDifference:
					return new SqlStringBuilder()
						.Add(DialectPrefix)
						.Add(analysis.ToString())
						.Add("(")
						.AddObject(geometry)
						.Add(",")
						.AddObject(extraArgument)
						.Add(")")
						.ToSqlString();
				case SpatialAnalysis.Union:
					return new SqlStringBuilder()
						.Add(DialectPrefix)
						.Add("GeomUnion(")
						.AddObject(geometry)
						.Add(",")
						.AddObject(extraArgument)
						.Add(")")
						.ToSqlString();
				default:
					throw new ArgumentException("Invalid spatial analysis argument");
			}
		}

		/// <summary>
		/// Gets the spatial create string.
		/// </summary>
		/// <param name="schema">The schema.</param>
		/// <returns></returns>
		public string GetSpatialCreateString(string schema)
		{
			return null;
		}

		/// <summary>
		/// Gets the spatial create string.
		/// </summary>
		/// <param name="schema">The schema.</param>
		/// <param name="table">The table.</param>
		/// <param name="column">The column.</param>
		/// <param name="srid">The srid.</param>
		/// <param name="subtype">The subtype.</param>
		/// <returns></returns>
		public string GetSpatialCreateString(string schema, string table, string column, int srid, string subtype)
		{
			StringBuilder builder = new StringBuilder();

			string quotedSchema = null;
			if (!string.IsNullOrEmpty(schema))
			{
				quotedSchema = this.QuoteForSchemaName(schema) + StringHelper.Dot;
			}

			builder.AppendFormat("ALTER TABLE {0}{1} DROP COLUMN {2}"
				, quotedSchema
				, this.QuoteForTableName(table)
				, this.QuoteForColumnName(column)
				);

			builder.Append(this.MultipleQueriesSeparator);

			builder.AppendFormat("EXECUTE ST.AddGeometryColumn '{0}','{1}','{2}',{3},'{4}'",
				schema, table, column, srid, subtype);

			builder.Append(this.MultipleQueriesSeparator);

			return builder.ToString();
		}

		/// <summary>
		/// Gets the spatial drop string.
		/// </summary>
		/// <param name="schema">The schema.</param>
		/// <returns></returns>
		public string GetSpatialDropString(string schema)
		{
			return null;
		}

		/// <summary>
		/// Gets the spatial drop string.
		/// </summary>
		/// <param name="schema">The schema.</param>
		/// <param name="table">The table.</param>
		/// <param name="column">The column.</param>
		/// <returns></returns>
		public string GetSpatialDropString(string schema, string table, string column)
		{
			StringBuilder builder = new StringBuilder();
			builder.AppendFormat("EXECUTE ST.DropGeometryColumn '{0}','{1}','{2}'",
				schema, table, column);
			builder.Append(this.MultipleQueriesSeparator);
			return builder.ToString();
		}

		/// <summary>
		/// Gets a value indicating whether it supports spatial metadata.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if it supports spatial metadata; otherwise, <c>false</c>.
		/// </value>
		public bool SupportsSpatialMetadata(MetadataClass metadataClass)
		{
			return true;
		}

		#endregion

		// TODO: Use ISessionFactory.ConnectionProvider.Driver.MultipleQueriesSeparator
		public string MultipleQueriesSeparator
		{
			get { return ";"; }
		}
	}
}
