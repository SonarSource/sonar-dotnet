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
using NHibernate.UserTypes;
using NHibernate.Spatial.Dialect;

namespace NHibernate.Spatial.Type
{
	/// <summary>
	/// This type can be used in geometry columns mappings and will use the
	/// proper conversions according to the spatial database dialect used.
	/// </summary>
	/// <remarks>
	/// To make an explicit dialect selection, just use the specific 
	/// geometry type (eg. MsSqlSpatialGeometryType). See 
	/// <see cref="NHibernate.Spatial.Dialect.SpatialDialect.LastInstantiated"/>
	/// </remarks>
	public class GeometryType : IGeometryUserType
	{
		private readonly IGeometryUserType geometryUserType;

		/// <summary>
		/// Initializes a new instance of the <see cref="GeometryType"/> class.
		/// </summary>
		public GeometryType()
		{
			if (SpatialDialect.LastInstantiated == null)
			{
				throw new MappingException("A GeometryType column has been declared, but there is no spatial dialect configured");
			}
			this.geometryUserType = SpatialDialect.LastInstantiated.CreateGeometryUserType();
		}

		#region IGeometryType Members

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
			return this.geometryUserType.Assemble(cached, owner);
		}

		/// <summary>
		/// Return a deep copy of the persistent state, stopping at entities and at collections.
		/// </summary>
		/// <param name="value">generally a collection element or entity field</param>
		/// <returns>a copy</returns>
		public object DeepCopy(object value)
		{
			return this.geometryUserType.DeepCopy(value);
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
			return this.geometryUserType.Disassemble(value);
		}

		/// <summary>
		/// Get a hashcode for the instance, consistent with persistence "equality"
		/// </summary>
		/// <param name="x"></param>
		/// <returns></returns>
		public int GetHashCode(object x)
		{
			return this.geometryUserType.GetHashCode(x);
		}

		/// <summary>
		/// Are objects of this type mutable?
		/// </summary>
		/// <value></value>
		public bool IsMutable
		{
			get { return this.geometryUserType.IsMutable; }
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
			return this.geometryUserType.NullSafeGet(rs, names, owner);
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
			this.geometryUserType.NullSafeSet(cmd, value, index);
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
			return this.geometryUserType.Replace(original, target, owner);
		}

		/// <summary>
		/// The type returned by <c>NullSafeGet()</c>
		/// </summary>
		/// <value></value>
		public System.Type ReturnedType
		{
			get { return this.geometryUserType.ReturnedType; }
		}

		/// <summary>
		/// The SQL types for the columns mapped by this type.
		/// </summary>
		/// <value></value>
		public SqlTypes.SqlType[] SqlTypes
		{
			get { return this.geometryUserType.SqlTypes; }
		}

		/// <summary>
		/// Determines whether the specified object is equals to another object.
		/// </summary>
		/// <param name="a">A.</param>
		/// <param name="b">The b.</param>
		/// <returns></returns>
		bool IUserType.Equals(object a, object b)
		{
			return this.geometryUserType.Equals(a, b);
		}

		/// <summary>
		/// Gets called by Hibernate to pass the configured type parameters to
		/// the implementation.
		/// </summary>
		/// <param name="parameters"></param>
		public void SetParameterValues(IDictionary<string, string> parameters)
		{
			this.geometryUserType.SetParameterValues(parameters);
		}

		/// <summary>
		/// Gets the system reference identification
		/// </summary>
		/// <value></value>
		public int SRID
		{
			get { return this.geometryUserType.SRID; }
		}

		/// <summary>
		/// Gets the OGC geometry subtype name
		/// </summary>
		/// <value></value>
		public string Subtype
		{
			get { return this.geometryUserType.Subtype; }
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

	}
}
