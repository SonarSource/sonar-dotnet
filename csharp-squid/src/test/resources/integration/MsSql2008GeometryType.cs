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
using GeoAPI.Geometries;
using Microsoft.SqlServer.Types;
using NHibernate.Type;

namespace NHibernate.Spatial.Type
{
    /// <summary>
    /// 
    /// </summary>
    public class MsSql2008GeometryType : GeometryTypeBase<SqlGeometry>
    {
		private static readonly NullableType SqlGeometryType = new SqlGeometryType();
		
		/// <summary>
        /// Initializes a new instance of the <see cref="MsSql2008GeometryType"/> class.
        /// </summary>
        public MsSql2008GeometryType()
			: base(SqlGeometryType)
        {
        }

        /// <summary>
        /// Sets the default SRID.
        /// </summary>
        /// <param name="geometry">The geometry.</param>
        protected override void SetDefaultSRID(IGeometry geometry)
        {
            base.SetDefaultSRID(geometry);
            if (geometry.SRID == -1)
            {
                geometry.SRID = 0;
            }
        }

        /// <summary>
        /// Converts from GeoAPI geometry type to database geometry type.
        /// </summary>
        /// <param name="value">The GeoAPI geometry value.</param>
        /// <returns></returns>
        protected override SqlGeometry FromGeometry(object value)
        {
            IGeometry geometry = value as IGeometry;
            if (geometry == null)
            {
				return SqlGeometry.Null;
            }
        	SetDefaultSRID(geometry);

        	try
        	{
        		MsSql2008GeometryWriter writer = new MsSql2008GeometryWriter();
        		SqlGeometry sqlGeometry = writer.Write(geometry);
        		return sqlGeometry;
        	}
        	catch (FormatException ex)
        	{
        		if (ex.Message == "24117: The LineString input is not valid because it does not have enough distinct points. A LineString must have at least two distinct points." ||
        		    ex.Message == "24305: The Polygon input is not valid because the ring does not have enough distinct points. Each ring of a polygon must contain at least three distinct points.")
        		{
        			// TODO: Not sure what to do in these cases...
        			return null;
        		}
        		throw;
        	}
        }

        /// <summary>
        /// Converts to GeoAPI geometry type from database geometry type.
        /// </summary>
        /// <param name="value">The databse geometry value.</param>
        /// <returns></returns>
        protected override IGeometry ToGeometry(object value)
        {
			SqlGeometry sqlGeometry = value as SqlGeometry;

            if (sqlGeometry == null || sqlGeometry.IsNull)
            {
                return null;
            }
        	
			MsSql2008GeometryReader reader = new MsSql2008GeometryReader();
        	IGeometry geometry = reader.Read(sqlGeometry);
        	SetDefaultSRID(geometry);
        	return geometry;
        }

    }

}
