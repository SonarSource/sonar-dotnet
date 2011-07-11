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

using System.Collections;
using NHibernate.Type;
using NHibernate.Engine;
using NHibernate.SqlCommand;

namespace NHibernate.Spatial.Dialect.Function
{
	/// <summary>
	/// 
	/// </summary>
	public class SpatialStandardSafeFunction : SpatialStandardFunction
	{
		protected int allowedArgsCount = 1;

		/// <summary>
		/// Initializes a new instance of the <see cref="SpatialStandardSafeFunction"/> class.
		/// </summary>
		/// <param name="name">The name.</param>
		public SpatialStandardSafeFunction(string name)
			: base(name)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SpatialStandardSafeFunction"/> class.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="typeValue">The type value.</param>
		public SpatialStandardSafeFunction(string name, IType typeValue)
			: base(name, typeValue)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SpatialStandardSafeFunction"/> class.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="allowedArgsCount">The allowed args count.</param>
		public SpatialStandardSafeFunction(string name, int allowedArgsCount)
			: base(name)
		{
			this.allowedArgsCount = allowedArgsCount;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SpatialStandardSafeFunction"/> class.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="typeValue">The type value.</param>
		/// <param name="allowedArgsCount">The allowed args count.</param>
		public SpatialStandardSafeFunction(string name, IType typeValue, int allowedArgsCount)
			: base(name, typeValue)
		{
			this.allowedArgsCount = allowedArgsCount;
		}

		#region ISQLFunction Members

		/// <summary>
		/// Render the function call as SQL.
		/// </summary>
		/// <param name="args">List of arguments</param>
		/// <param name="factory"></param>
		/// <returns>SQL fragment for the function.</returns>
		public override SqlString Render(IList args, ISessionFactoryImplementor factory)
		{
			this.ValidateArgsCount(args);
			return base.Render(args, factory);
		}

		/// <summary>
		/// Validates the arguments count.
		/// </summary>
		/// <param name="args">The arguments.</param>
		protected void ValidateArgsCount(IList args)
		{
            if (args.Count != allowedArgsCount)
            {
				throw new QueryException(string.Format("function '{0}' requires {1} arguments.", this.name, this.allowedArgsCount));
			}
		}

		#endregion

		/// <summary>
		/// Gets the allowed arguments count.
		/// </summary>
		/// <value>The allowed arguments count.</value>
		public int AllowedArgsCount
		{
			get { return this.allowedArgsCount; }
		}

		/// <summary>
		/// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
		/// </returns>
		public override string ToString()
		{
			return name;
		}
	}
}
