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

namespace NHibernate.Spatial.Criterion
{
	/// <summary>
	/// Factory class for NHibernate query projections
	/// </summary>
	/// <remarks>
	/// In the GIS context, this class name could be misleading,
	/// but it has nothing to do with cartographic planar projections.
	/// </remarks>
	public static class SpatialProjections
	{

		#region Aggregates

		/// <summary>
		/// Aggregates collection of the specified property name.
		/// </summary>
		/// <param name="propertyName">Name of the property.</param>
		/// <returns></returns>
		public static SpatialAggregateProjection Collect(string propertyName)
		{
			return new SpatialAggregateProjection(propertyName, SpatialAggregate.Collect);
		}

		/// <summary>
		/// Aggregates envelope of the specified property name.
		/// </summary>
		/// <param name="propertyName">Name of the property.</param>
		/// <returns></returns>
		public static SpatialAggregateProjection Envelope(string propertyName)
		{
			return new SpatialAggregateProjection(propertyName, SpatialAggregate.Envelope);
		}

		/// <summary>
		/// Aggregates intersection of the specified property name.
		/// </summary>
		/// <param name="propertyName">Name of the property.</param>
		/// <returns></returns>
		public static SpatialAggregateProjection Intersection(string propertyName)
		{
			return new SpatialAggregateProjection(propertyName, SpatialAggregate.Intersection);
		}

		/// <summary>
		/// Aggregates union of the specified property name.
		/// </summary>
		/// <param name="propertyName">Name of the property.</param>
		/// <returns></returns>
		public static SpatialAggregateProjection Union(string propertyName)
		{
			return new SpatialAggregateProjection(propertyName, SpatialAggregate.Union);
		}

		#endregion

		#region Analysis

		/// <summary>
		/// Buffers the specified property name.
		/// </summary>
		/// <param name="propertyName">Name of the property.</param>
		/// <param name="anotherPropertyName">Name of another property.</param>
		/// <returns></returns>
		public static SpatialProjection Buffer(string propertyName, double anotherPropertyName)
		{
			return new SpatialAnalysisProjection(propertyName, SpatialAnalysis.Buffer, anotherPropertyName);
		}

		/// <summary>
		/// ConvexHull for the specified property name.
		/// </summary>
		/// <param name="propertyName">Name of the property.</param>
		/// <returns></returns>
		public static SpatialProjection ConvexHull(string propertyName)
		{
			return new SpatialAnalysisProjection(propertyName, SpatialAnalysis.ConvexHull);
		}

		/// <summary>
		/// Difference of the specified property names.
		/// </summary>
		/// <param name="propertyName">Name of the property.</param>
		/// <param name="anotherPropertyName">Name of another property.</param>
		/// <returns></returns>
		public static SpatialProjection Difference(string propertyName, string anotherPropertyName)
		{
			return new SpatialAnalysisProjection(propertyName, SpatialAnalysis.Difference, anotherPropertyName);
		}

		/// <summary>
		/// Distance of the specified property names.
		/// </summary>
		/// <param name="propertyName">Name of the property.</param>
		/// <param name="anotherPropertyName">Name of another property.</param>
		/// <returns></returns>
		public static SpatialProjection Distance(string propertyName, string anotherPropertyName)
		{
			return new SpatialAnalysisProjection(propertyName, SpatialAnalysis.Distance, anotherPropertyName);
		}

		/// <summary>
		/// Intersection of the specified property names.
		/// </summary>
		/// <param name="propertyName">Name of the property.</param>
		/// <param name="anotherPropertyName">Name of another property.</param>
		/// <returns></returns>
		public static SpatialProjection Intersection(string propertyName, string anotherPropertyName)
		{
			return new SpatialAnalysisProjection(propertyName, SpatialAnalysis.Intersection, anotherPropertyName);
		}

		/// <summary>
		/// Symmetric difference of the specified property names.
		/// </summary>
		/// <param name="propertyName">Name of the property.</param>
		/// <param name="anotherPropertyName">Name of another property.</param>
		/// <returns></returns>
		public static SpatialProjection SymDifference(string propertyName, string anotherPropertyName)
		{
			return new SpatialAnalysisProjection(propertyName, SpatialAnalysis.SymDifference, anotherPropertyName);
		}

		/// <summary>
		/// Union of the specified property names.
		/// </summary>
		/// <param name="propertyName">Name of the property.</param>
		/// <param name="anotherPropertyName">Name of another property.</param>
		/// <returns></returns>
		public static SpatialProjection Union(string propertyName, string anotherPropertyName)
		{
			return new SpatialAnalysisProjection(propertyName, SpatialAnalysis.Union, anotherPropertyName);
		}

		#endregion

		#region Relations

		/// <summary>
		/// Determines whether the specified geometry property contains another geometry property.
		/// </summary>
		/// <param name="propertyName">Name of the property.</param>
		/// <param name="anotherPropertyName">Name of another property.</param>
		/// <returns></returns>
		public static SpatialProjection Contains(string propertyName, string anotherPropertyName)
		{
			return new SpatialRelationProjection(propertyName, SpatialRelation.Contains, anotherPropertyName);
		}

		/// <summary>
		/// Determines whether the specified geometry property is covered by another geometry property.
		/// </summary>
		/// <param name="propertyName">Name of the property.</param>
		/// <param name="anotherPropertyName">Name of another property.</param>
		/// <returns></returns>
		public static SpatialProjection CoveredBy(string propertyName, string anotherPropertyName)
		{
			return new SpatialRelationProjection(propertyName, SpatialRelation.CoveredBy, anotherPropertyName);
		}

		/// <summary>
		/// Determines whether the specified geometry property covers another geometry property.
		/// </summary>
		/// <param name="propertyName">Name of the property.</param>
		/// <param name="anotherPropertyName">Name of another property.</param>
		/// <returns></returns>
		public static SpatialProjection Covers(string propertyName, string anotherPropertyName)
		{
			return new SpatialRelationProjection(propertyName, SpatialRelation.Covers, anotherPropertyName);
		}

		/// <summary>
		/// Determines whether the specified geometry property crosses another geometry property.
		/// </summary>
		/// <param name="propertyName">Name of the property.</param>
		/// <param name="anotherPropertyName">Name of another property.</param>
		/// <returns></returns>
		public static SpatialProjection Crosses(string propertyName, string anotherPropertyName)
		{
			return new SpatialRelationProjection(propertyName, SpatialRelation.Crosses, anotherPropertyName);
		}

		/// <summary>
		/// Determines whether the specified geometry property is disjoint with another geometry property.
		/// </summary>
		/// <param name="propertyName">Name of the property.</param>
		/// <param name="anotherPropertyName">Name of another property.</param>
		/// <returns></returns>
		public static SpatialProjection Disjoint(string propertyName, string anotherPropertyName)
		{
			return new SpatialRelationProjection(propertyName, SpatialRelation.Disjoint, anotherPropertyName);
		}

		/// <summary>
		/// Determines whether the specified geometry property equals to another geometry property.
		/// </summary>
		/// <param name="propertyName">Name of the property.</param>
		/// <param name="anotherPropertyName">Name of another property.</param>
		/// <returns></returns>
		public static SpatialProjection Equals(string propertyName, string anotherPropertyName)
		{
			return new SpatialRelationProjection(propertyName, SpatialRelation.Equals, anotherPropertyName);
		}

		/// <summary>
		/// Determines whether the specified geometry property intersects another geometry property.
		/// </summary>
		/// <param name="propertyName">Name of the property.</param>
		/// <param name="anotherPropertyName">Name of another property.</param>
		/// <returns></returns>
		public static SpatialProjection Intersects(string propertyName, string anotherPropertyName)
		{
			return new SpatialRelationProjection(propertyName, SpatialRelation.Intersects, anotherPropertyName);
		}

		/// <summary>
		/// Determines whether the specified geometry property overlaps another geometry property.
		/// </summary>
		/// <param name="propertyName">Name of the property.</param>
		/// <param name="anotherPropertyName">Name of another property.</param>
		/// <returns></returns>
		public static SpatialProjection Overlaps(string propertyName, string anotherPropertyName)
		{
			return new SpatialRelationProjection(propertyName, SpatialRelation.Overlaps, anotherPropertyName);
		}

		/// <summary>
		/// Determines whether the specified geometry property touches another geometry property.
		/// </summary>
		/// <param name="propertyName">Name of the property.</param>
		/// <param name="anotherPropertyName">Name of another property.</param>
		/// <returns></returns>
		public static SpatialProjection Touches(string propertyName, string anotherPropertyName)
		{
			return new SpatialRelationProjection(propertyName, SpatialRelation.Touches, anotherPropertyName);
		}

		/// <summary>
		/// Determines whether the specified geometry property is within another geometry property.
		/// </summary>
		/// <param name="propertyName">Name of the property.</param>
		/// <param name="anotherPropertyName">Name of another property.</param>
		/// <returns></returns>
		public static SpatialProjection Within(string propertyName, string anotherPropertyName)
		{
			return new SpatialRelationProjection(propertyName, SpatialRelation.Within, anotherPropertyName);
		}


		/// <summary>
		/// Determines whether the specified geometry property relates to another geometry property.
		/// </summary>
		/// <param name="propertyName">Name of the property.</param>
		/// <param name="anotherPropertyName">Name of another property.</param>
		/// <returns></returns>
		public static SpatialProjection Relate(string propertyName, string anotherPropertyName)
		{
			return new SpatialRelateProjection(propertyName, anotherPropertyName);
		}

		/// <summary>
		/// Determines whether the specified geometry property relates to another geometry property.
		/// </summary>
		/// <param name="propertyName">Name of the property.</param>
		/// <param name="anotherPropertyName">Name of another property.</param>
		/// <param name="pattern">The pattern.</param>
		/// <returns></returns>
		public static SpatialProjection Relate(string propertyName, string anotherPropertyName, string pattern)
		{
			return new SpatialRelateProjection(propertyName, anotherPropertyName, pattern);
		}

		#endregion

		#region Validations

		/// <summary>
		/// Determines whether the specified geometry property is closed.
		/// </summary>
		/// <param name="propertyName">Name of the property.</param>
		/// <returns></returns>
		public static SpatialProjection IsClosed(string propertyName)
		{
			return new SpatialValidationProjection(propertyName, SpatialValidation.IsClosed);
		}

		/// <summary>
		/// Determines whether the specified geometry property is empty.
		/// </summary>
		/// <param name="propertyName">Name of the property.</param>
		/// <returns></returns>
		public static SpatialProjection IsEmpty(string propertyName)
		{
			return new SpatialValidationProjection(propertyName, SpatialValidation.IsEmpty);
		}

		/// <summary>
		/// Determines whether the specified geometry property is ring.
		/// </summary>
		/// <param name="propertyName">Name of the property.</param>
		/// <returns></returns>
		public static SpatialProjection IsRing(string propertyName)
		{
			return new SpatialValidationProjection(propertyName, SpatialValidation.IsRing);
		}

		/// <summary>
		/// Determines whether the specified geometry property is simple.
		/// </summary>
		/// <param name="propertyName">Name of the property.</param>
		/// <returns></returns>
		public static SpatialProjection IsSimple(string propertyName)
		{
			return new SpatialValidationProjection(propertyName, SpatialValidation.IsSimple);
		}

		/// <summary>
		/// Determines whether the specified geometry property is valid.
		/// </summary>
		/// <param name="propertyName">Name of the property.</param>
		/// <returns></returns>
		public static SpatialProjection IsValid(string propertyName)
		{
			return new SpatialValidationProjection(propertyName, SpatialValidation.IsValid);
		}

		#endregion

		#region Functions

		/// <summary>
		/// Transforms the coordinate reference system of the specified geometry property.
		/// </summary>
		/// <param name="propertyName">Name of the property.</param>
		/// <param name="srid">The srid.</param>
		/// <returns></returns>
		public static SpatialProjection Transform(string propertyName, int srid)
		{
			return new SpatialTransformProjection(propertyName, srid);
		}

		#endregion

	}

}
