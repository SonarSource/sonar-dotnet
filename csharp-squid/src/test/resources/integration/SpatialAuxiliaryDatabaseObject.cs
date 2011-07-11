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

using System.Text;
using NHibernate.Cfg;
using NHibernate.Type;
using NHibernate.Mapping;
using NHibernate.Engine;
using NHibernate.Spatial.Type;
using NHibernate.Spatial.Dialect;
using GeoAPI.Geometries;

namespace NHibernate.Spatial.Mapping
{
	/// <summary>
	/// 
	/// </summary>
	/// 
	/// <example>
	/// 
	/// Sample usage:
	///  
	/// Declare a geometry property specifing SRID (system reference ID):
	/// 
	/// <code>
	/// 	 <class name="MyGeoTableA">
	/// 		 <property name="MyGeoColumn">
	/// 			 <type name="NHibernate.Spatial.Type.GeometryType, NHibernate.Spatial">
	/// 				 <param name="srid">1234</param>
	/// 			 </type>
	/// 		 </property>
	/// 	 </class>
	/// 	 <class name="MyGeoTableB">
	/// 		 <property name="MyGeoColumn">
	/// 			 <type name="NHibernate.Spatial.Type.GeometryType, NHibernate.Spatial">
	/// 				 <param name="subtype">POLYGON</param>
	/// 			 </type>
	/// 		 </property>
	///  	 </class>
	///  	 </class>
	/// </code>
	/// 
	///  * SpatialAuxiliaryDatabaseObject generates spatial schema for all configured classes
	/// 
	/// <code>
	/// 	<database-object>
	/// 		<definition class="NHibernate.Spatial.Mapping.SpatialAuxiliaryDatabaseObject, NHibernate.Spatial" />
	/// 	</database-object>
	/// </code>
	/// 
	///  * For example, for PostGIS the schema generation code would look like:
	/// 
	/// <code>
	/// 		ALTER TABLE dbo.MyGeoTableA DROP COLUMN MyGeoColumn;
	/// 		SELECT AddGeometryColumn('dbo', 'MyGeoTableA', 'MyGeoColumn', 1234, 'GEOMETRY');
	/// 		 
	/// 		ALTER TABLE dbo.MyGeoTableB DROP COLUMN MyGeoColumn;
	/// 		SELECT AddGeometryColumn('dbo', 'MyGeoTableB', 'MyGeoColumn', -1, 'POLYGON');
	/// </code>
	/// 
	///  (Compare the values defined in the property type parameters with generated SQL code)
	/// 
	///  </example>

	public class SpatialAuxiliaryDatabaseObject : AbstractAuxiliaryDatabaseObject
	{

		/// <summary>
		/// Initializes a new instance of the <see cref="SpatialAuxiliaryDatabaseObject"/> class.
		/// </summary>
		/// <param name="configuration">The configuration.</param>
		/// <remarks>
		/// To programmatically configure the auxiliary object, we follow
		/// the advice of passing the configuration in the constructor given at:
		/// http://forum.hibernate.org/viewtopic.php?p=2320370#2320370
		/// However, this approach doesn't allow to configure it fully declaratively
		/// using the &lt;database-object&gt; element plus the option
		/// "hibernate.hbm2ddl.auto" set to "create" or "create-drop",
		/// Alternative solutions would be:
		/// 1) Add a Configuration property to IMapping, or
		/// 2) Add a Configuration parameter to IAuxiliaryDatabaseObject::SqlCreateString, or
		/// 3) Add a SetConfiguration method to IAuxiliaryDatabaseObject.
		/// The same should apply to IAuxiliaryDatabaseObject::SqlDropString.
		/// </remarks>
		public SpatialAuxiliaryDatabaseObject(Configuration configuration)
		{
			this.configuration = configuration;
		}

		private readonly Configuration configuration;

		private delegate void VisitGeometryColumnDelegate(Table table, Column column);

		private void VisitGeometryColumns(VisitGeometryColumnDelegate visitGeometryColumn)
		{

			// This would be quite illegal, because "configuration" is a private 
			// field of Mapping class, the most probable implementation of IMapping.
			//
			//this.configuration = mapping.configuration;

			foreach (PersistentClass persistentClass in this.configuration.ClassMappings)
			{
				Table table = persistentClass.Table;
				foreach (Column column in table.ColumnIterator)
				{
					if (column.Value.Type.ReturnedClass == typeof(IGeometry))
					{
						visitGeometryColumn(table, column);
					}
				}
			}
		}

		/// <summary>
		/// Creates SQL to create auxiliary database objects.
		/// </summary>
		/// <param name="dialect">The dialect.</param>
		/// <param name="mapping">The mapping.</param>
		/// <param name="defaultCatalog">The default catalog.</param>
		/// <param name="defaultSchema">The default schema.</param>
		/// <returns></returns>
		public override string SqlCreateString(NHibernate.Dialect.Dialect dialect, IMapping mapping, string defaultCatalog, string defaultSchema)
		{
			ISpatialDialect spatialDialect = (ISpatialDialect)dialect;
			StringBuilder builder = new StringBuilder();

			// Create general objects
			builder.Append(spatialDialect.GetSpatialCreateString(defaultSchema));

			// Create objects per column
			VisitGeometryColumns(delegate(Table table, Column column)
			{
				// Ugly trick: We use Comparator property to get the column instance of IGeometryUserType.
				// (Comparator is mainly used in IVersionType comparisons)
				// Maybe it will require to implement IComparer in IGeometryUserType, just to comply.
				//
				// It would be nicer if CustomType made UserType property public (today is protected).
				IGeometryUserType geometryType = (IGeometryUserType)((CustomType)column.Value.Type).Comparator;

				// The previous trick allows to get geometry type properties, such as SRID.
				int srid = geometryType.SRID;
				string subtype = geometryType.Subtype;

				builder.Append(spatialDialect.GetSpatialCreateString(defaultSchema, table.Name, column.Name, srid, subtype));
			});

			return builder.ToString();
		}

		/// <summary>
		/// Creates SQL to drop auxiliary database objects.
		/// </summary>
		/// <param name="dialect">The dialect.</param>
		/// <param name="defaultCatalog">The default catalog.</param>
		/// <param name="defaultSchema">The default schema.</param>
		/// <returns></returns>
		public override string SqlDropString(NHibernate.Dialect.Dialect dialect, string defaultCatalog, string defaultSchema)
		{
			ISpatialDialect spatialDialect = (ISpatialDialect)dialect;
			StringBuilder builder = new StringBuilder();

			// Drop objects per column
			VisitGeometryColumns(delegate(Table table, Column column)
			{
				builder.Append(spatialDialect.GetSpatialDropString(defaultSchema, table.Name, column.Name));
			});

			// Drop general objects
			builder.Append(spatialDialect.GetSpatialDropString(defaultSchema));

			return builder.ToString();
		}

	}
}
