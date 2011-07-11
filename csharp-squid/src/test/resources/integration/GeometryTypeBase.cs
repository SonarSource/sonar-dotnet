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
using System.Collections.Generic;
using System.Data;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;
using NHibernate.SqlTypes;
using NHibernate.Type;
using NHibernate.UserTypes;

namespace NHibernate.Spatial.Type
{
	/// <summary>
	/// 
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public abstract class GeometryTypeBase<T> : IGeometryUserType
	{
		/// <summary>
		/// Converts from GeoAPI geometry type to database geometry type.
		/// </summary>
		/// <param name="value">The GeoAPI geometry value.</param>
		/// <returns></returns>
		protected abstract T FromGeometry(object value);

		/// <summary>
		/// Converts to GeoAPI geometry type from database geometry type.
		/// </summary>
		/// <param name="value">The databse geometry value.</param>
		/// <returns></returns>
		protected abstract IGeometry ToGeometry(object value);

		private int srid = -1;
		private string subtype = "GEOMETRY";

		private readonly NullableType nullableType;
		private readonly SqlType sqlType;

		/// <summary>
		/// Initializes a new instance of the <see cref="GeometryTypeBase&lt;T&gt;"/> class.
		/// </summary>
		/// <param name="nullableType">Type of the nullable.</param>
		protected GeometryTypeBase(NullableType nullableType)
		{
			this.nullableType = nullableType;
			this.sqlType = nullableType.SqlType;
		}

		#region IGeometryUserType Members

		/// <summary>
		/// Return a deep copy of the persistent state, stopping at entities and at collections.
		/// </summary>
		/// <param name="value">generally a collection element or entity field</param>
		/// <returns>a copy</returns>
		public object DeepCopy(object value)
		{
			return this.ToGeometry(this.nullableType.DeepCopy(this.FromGeometry(value), EntityMode.Map, null));
		}

		/// <summary>
		/// Transform the object into its cacheable representation. At the very least this
		/// method should perform a deep copy if the type is mutable. That may not be enough
		/// for some implementations, however; for example, associations must be cached as
		/// identifier values. (optional operation)
		/// </summary>
		/// <param name="value">the object to be cached</param>
		/// <returns>a cacheable representation of the object</returns>
		public object Disassemble(object value)
		{
			return value;
		}

		/// <summary>
		/// Reconstruct an object from the cacheable representation. At the very least this
		/// method should perform a deep copy if the type is mutable. (optional operation)
		/// </summary>
		/// <param name="cached">the object to be cached</param>
		/// <param name="owner">the owner of the cached object</param>
		/// <returns>
		/// a reconstructed object from the cachable representation
		/// </returns>
		public object Assemble(object cached, object owner)
		{
			return cached;
		}

		/// <summary>
		/// Get a hashcode for the instance, consistent with persistence "equality"
		/// </summary>
		/// <param name="x"></param>
		/// <returns></returns>
		public int GetHashCode(object x)
		{
			return x.GetHashCode();
		}

		/// <summary>
		/// Are objects of this type mutable?
		/// </summary>
		/// <value></value>
		public bool IsMutable
		{
			get { return true; }
		}

		/// <summary>
		/// Retrieve an instance of the mapped class from a JDBC resultset.
		/// Implementors should handle possibility of null values.
		/// </summary>
		/// <param name="rs">a IDataReader</param>
		/// <param name="names">column names</param>
		/// <param name="owner">the containing entity</param>
		/// <returns></returns>
		/// <exception cref="T:NHibernate.HibernateException">HibernateException</exception>
		public object NullSafeGet(IDataReader rs, string[] names, object owner)
		{
			try
			{
				return this.ToGeometry((T)this.nullableType.NullSafeGet(rs, names));
			}
			catch
			{
				return null;
			}

		}

		/// <summary>
		/// Write an instance of the mapped class to a prepared statement.
		/// Implementors should handle possibility of null values.
		/// A multi-column type should be written to parameters starting from index.
		/// </summary>
		/// <param name="cmd">a IDbCommand</param>
		/// <param name="value">the object to write</param>
		/// <param name="index">command parameter index</param>
		/// <exception cref="T:NHibernate.HibernateException">HibernateException</exception>
		public void NullSafeSet(IDbCommand cmd, object value, int index)
		{
			this.nullableType.NullSafeSet(cmd, this.FromGeometry(value), index);
		}

		/// <summary>
		/// During merge, replace the existing (<paramref name="target"/>) value in the entity
		/// we are merging to with a new (<paramref name="original"/>) value from the detached
		/// entity we are merging. For immutable objects, or null values, it is safe to simply
		/// return the first parameter. For mutable objects, it is safe to return a copy of the
		/// first parameter. For objects with component values, it might make sense to
		/// recursively replace component values.
		/// </summary>
		/// <param name="original">the value from the detached entity being merged</param>
		/// <param name="target">the value in the managed entity</param>
		/// <param name="owner">the managed entity</param>
		/// <returns>the value to be merged</returns>
		public object Replace(object original, object target, object owner)
		{
			return original;
		}

		/// <summary>
		/// The type returned by <c>NullSafeGet()</c>
		/// </summary>
		/// <value></value>
		public System.Type ReturnedType
		{
			get { return typeof(IGeometry); }
		}

		/// <summary>
		/// The SQL types for the columns mapped by this type.
		/// </summary>
		/// <value></value>
		public virtual SqlType[] SqlTypes
		{
			get { return new SqlType[] { this.sqlType }; }
		}

		/// <summary>
		/// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
		/// </summary>
		/// <param name="obj">The <see cref="T:System.Object"/> to compare with the current <see cref="T:System.Object"/>.</param>
		/// <returns>
		/// true if the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>; otherwise, false.
		/// </returns>
		/// <exception cref="T:System.NullReferenceException">The <paramref name="obj"/> parameter is null.</exception>
		public override bool Equals(object obj)
		{
			return base.Equals(obj);
		}

		/// <summary>
		/// Determines whether the specified object is equals to another object.
		/// </summary>
		/// <param name="a">A.</param>
		/// <param name="b">The b.</param>
		/// <returns></returns>
		bool IUserType.Equals(object a, object b)
		{
			IGeometry ga = a as IGeometry;
			IGeometry gb = b as IGeometry;
			if (ga != null && gb != null)
			{
				try
				{
					return ga.SRID == gb.SRID && ga.Equals(gb);
				}
				catch (TopologyException)
				{
					return false;
				}
			}
			if (ga == null && gb == null)
			{
				return Util.EqualsHelper.Equals((T)a, (T)b);
			}
			return false;
		}

		/// <summary>
		/// Serves as a hash function for a particular type.
		/// </summary>
		/// <returns>
		/// A hash code for the current <see cref="T:System.Object"/>.
		/// </returns>
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		/// <summary>
		/// Gets called by Hibernate to pass the configured type parameters to
		/// the implementation.
		/// </summary>
		/// <param name="parameters"></param>
		public void SetParameterValues(IDictionary<string, string> parameters)
		{
			if (parameters != null)
			{
				string parameterSRID = parameters["srid"];
				string parameterSubtype = parameters["subtype"];

				if (!string.IsNullOrEmpty(parameterSRID))
				{
					Int32.TryParse(parameterSRID, out this.srid);
				}
				if (!string.IsNullOrEmpty(parameterSubtype))
				{
					this.subtype = parameterSubtype;
				}
			}
		}

		/// <summary>
		/// Gets the system reference identification
		/// </summary>
		/// <value></value>
		public int SRID
		{
			get { return this.srid; }
		}

		/// <summary>
		/// OGC geometry subtype name
		/// </summary>
		/// <value></value>
		public string Subtype
		{
			get { return this.subtype; }
		}

		/// <summary>
		/// Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
		/// </summary>
		/// <param name="x">The first object to compare.</param>
		/// <param name="y">The second object to compare.</param>
		/// <returns>
		/// Value Condition Less than zero <paramref name="x"/> is less than <paramref name="y"/>. Zero <paramref name="x"/> equals <paramref name="y"/>. Greater than zero <paramref name="x"/> is greater than <paramref name="y"/>.
		/// </returns>
		/// <exception cref="T:System.ArgumentException">Neither <paramref name="x"/> nor <paramref name="y"/> implements the <see cref="T:System.IComparable"/> interface.-or- <paramref name="x"/> and <paramref name="y"/> are of different types and neither one can handle comparisons with the other. </exception>
		public int Compare(object x, object y)
		{
			throw new InvalidOperationException("Invalid operation in geometry type");
		}

		#endregion

		/// <summary>
		/// Sets the default SRID.
		/// </summary>
		/// <param name="geometry">The geometry.</param>
		protected virtual void SetDefaultSRID(IGeometry geometry)
		{
			if (geometry.SRID <= 0 && this.SRID > 0)
			{
				geometry.SRID = this.SRID;
			}
		}
	}
}
