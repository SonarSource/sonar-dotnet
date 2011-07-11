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
	public class MySQLSpatialDialect : MySQL5Dialect, ISpatialDialect
	{
		private static readonly IType geometryType = new CustomType(typeof(MySQLGeometryType), null);

		/// <summary>
		/// Initializes a new instance of the <see cref="MySQLDialect"/> class.
		/// </summary>
		public MySQLSpatialDialect()
		{
			SpatialDialect.LastInstantiated = this;
			RegisterBasicFunctions();
			RegisterFunctions();
		}

		public override string ToBooleanValueString(bool value)
		{
			return value ? "true" : "false";
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
			RegisterSpatialFunction("PolyFromText", 2);
			RegisterSpatialFunction("PolyFromWKB", 2);
			RegisterSpatialFunction("MLineFromText", 2);
			RegisterSpatialFunction("MLineFromWKB", 2);
			RegisterSpatialFunction("MPointFromText", 2);
			RegisterSpatialFunction("MPointFromWKB", 2);
			RegisterSpatialFunction("MPolyFromText", 2);
			RegisterSpatialFunction("MPolyFromWKB", 2);

			RegisterSpatialFunction("AsBinary", NHibernateUtil.Binary);

			RegisterSpatialFunction("AsText", MySQLGeometryStringType.Instance);
			RegisterSpatialFunction("AsGML", NHibernateUtil.String);
			RegisterSpatialFunction("GeometryType", MySQLGeometryStringType.Instance);

			RegisterSpatialFunction("Area", NHibernateUtil.Double);
			RegisterSpatialFunction("Length", "GLength", NHibernateUtil.Double);
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
			RegisterFunction(SpatialDialect.HqlPrefix + standardName, new SpatialStandardSafeFunction(dialectName, returnedType, allowedArgsCount));
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

		private void RegisterSpatialFunction(SpatialRelation relation)
		{
			RegisterFunction(SpatialDialect.HqlPrefix + relation, new SpatialRelationFunction(this, relation));
		}

		private void RegisterSpatialFunction(SpatialValidation validation)
		{
			RegisterFunction(SpatialDialect.HqlPrefix + validation, new SpatialValidationFunction(this, validation));
		}

		private void RegisterSpatialFunction(SpatialAnalysis analysis)
		{
			RegisterFunction(SpatialDialect.HqlPrefix + analysis, new SpatialAnalysisFunction(this, analysis));
		}

		#endregion

		#region ISpatialDialect Members

		/// <summary>
		/// Creates the geometry user type.
		/// </summary>
		/// <returns></returns>
		public IGeometryUserType CreateGeometryUserType()
		{
			return new MySQLGeometryType();
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
				.Add("Transform(")
				.AddObject(geometry)
				.Add(",")
				.Add(srid.ToString())
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
				.Add(validation.ToString())
				.Add("(")
				.AddObject(geometry)
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
			// PostGIS aggregate functions do not need prefix
			string aggregateFunction;
			switch (aggregate)
			{
				case SpatialAggregate.Collect:
					aggregateFunction = "Collect";
					break;
				case SpatialAggregate.Envelope:
					aggregateFunction = "Extent";
					break;
				case SpatialAggregate.Intersection:
					aggregateFunction = "Intersection";
					break;
				case SpatialAggregate.Union:
					aggregateFunction = "GeomUnion";
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

		public SqlString GetSpatialRelationString(object geometry, SpatialRelation relation, object anotherGeometry, bool criterion)
		{
			switch (relation)
			{
				case SpatialRelation.Covers:
					string[] patterns = new string[] {
						"T*****FF*",
						"*T****FF*",
						"***T**FF*",
						"****T*FF*",
					};
					SqlStringBuilder builder = new SqlStringBuilder();
					builder.Add("(");
					for (int i = 0; i < patterns.Length; i++)
					{
						if (i > 0)
							builder.Add(" OR ");
						builder
							.Add("Relate")
							.Add("(")
							.AddObject(geometry)
							.Add(", ")
							.AddObject(anotherGeometry)
							.Add(", '")
							.Add(patterns[i])
							.Add("')")
							.ToSqlString();
					}
					builder.Add(")");
					return builder.ToSqlString();
				case SpatialRelation.CoveredBy:
					return GetSpatialRelationString(anotherGeometry, SpatialRelation.Covers, geometry, criterion);
				default:
					return new SqlStringBuilder(6)
						.Add(relation.ToString())
						.Add("(")
						.AddObject(geometry)
						.Add(", ")
						.AddObject(anotherGeometry)
						.Add(")")
						.ToSqlString();
			}
		}

		public SqlString GetSpatialRelateString(object geometry, object anotherGeometry, object pattern, bool isStringPattern, bool criterion)
		{
			SqlStringBuilder builder = new SqlStringBuilder();
			builder
				.Add("Relate(")
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
			return new SqlStringBuilder(7)
				.Add("MBRIntersects(")
				.Add(tableAlias)
				.Add(".")
				.Add(geometryColumnName)
				.Add(",")
				.AddParameter()
				.Add(")")
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
					return new SqlStringBuilder(6)
						.Add("Buffer(")
						.AddObject(geometry)
						.Add(", ")
						.AddObject(extraArgument)
						.Add(")")
						.ToSqlString();
				case SpatialAnalysis.ConvexHull:
					return new SqlStringBuilder()
						.Add("ConvexHull(")
						.AddObject(geometry)
						.Add(")")
						.ToSqlString();
				case SpatialAnalysis.Difference:
				case SpatialAnalysis.Distance:
				case SpatialAnalysis.Intersection:
				case SpatialAnalysis.SymDifference:
				case SpatialAnalysis.Union:
					return new SqlStringBuilder()
						.Add(analysis.ToString())
						.Add("(")
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
		/// Quotes the schema.
		/// </summary>
		/// <param name="schema">The schema.</param>
		/// <returns></returns>
		private string QuoteSchema(string schema)
		{
			if (string.IsNullOrEmpty(schema))
			{
				return null;
			}
			return this.QuoteForSchemaName(schema) + StringHelper.Dot;
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

			string quotedSchema = this.QuoteSchema(schema);
			string quoteForTableName = this.QuoteForTableName(table);
			string quoteForColumnName = this.QuoteForColumnName(column);

			builder.AppendFormat("ALTER TABLE {0}{1} DROP COLUMN {2}"
				, quotedSchema 
				, quoteForTableName
				, quoteForColumnName
				);

			builder.Append(this.MultipleQueriesSeparator);

			builder.AppendFormat("ALTER TABLE {0}{1} ADD {2} {3}"
				, quotedSchema
				, quoteForTableName
				, quoteForColumnName
				, subtype
				);

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
			return null;
		}

		/// <summary>
		/// Gets a value indicating whether it supports spatial metadata.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if it supports spatial metadata; otherwise, <c>false</c>.
		/// </value>
		public bool SupportsSpatialMetadata(MetadataClass metadataClass)
		{
			return false;
		}

		#endregion

		// TODO: Use ISessionFactory.ConnectionProvider.Driver.MultipleQueriesSeparator
		public string MultipleQueriesSeparator
		{
			get { return ";"; }
		}

	}
}
