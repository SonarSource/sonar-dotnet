﻿//
// JavaScriptConverter.cs
//
// Author:
//   Igor Zelmanovich <igorz@mainsoft.com>
//
// (C) 2007 Mainsoft, Inc.  http://www.mainsoft.com
//
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
namespace Nancy.Json
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Abstracr base class for javascript converter operations.
    /// </summary>
    public abstract class JavaScriptConverter
	{
        /// <summary>
        /// Initializes a new instance of the <see cref="JavaScriptConverter"/> class.
        /// </summary>
        protected JavaScriptConverter () { }

        /// <summary>
        /// Gets the supported types.
        /// </summary>
        /// <value>The supported types.</value>
        public abstract IEnumerable<Type> SupportedTypes { get; }

        /// <summary>
        /// Deserializes the specified dictionary.
        /// </summary>
        /// <param name="dictionary">The dictionary.</param>
        /// <param name="type">The type.</param>
        /// <returns>The deserialized <paramref name="dictionary"/></returns>
        public virtual object Deserialize(IDictionary<string, object> dictionary, Type type)
        {
            return Deserialize(dictionary, type, null);
        }

        /// <summary>
        /// Deserializes the specified dictionary.
        /// </summary>
        /// <param name="dictionary">The dictionary.</param>
        /// <param name="type">The type.</param>
        /// <param name="serializer">The serializer.</param>
        /// <returns>An <see cref="object"/> representing <paramref name="dictionary"/></returns>
        public abstract object Deserialize (IDictionary<string, object> dictionary, Type type, JavaScriptSerializer serializer);

        /// <summary>
        /// Serializes the specified object.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>A <see cref="Dictionary{TKey,TValue}"/> instance</returns>
        public IDictionary<string, object> Serialize(object obj)
        {
            return Serialize(obj, null);
        }

        /// <summary>
        /// Serializes the specified object.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="serializer">The serializer.</param>
        /// <returns>A <see cref="Dictionary{TKey,TValue}"/> instance</returns>
        public abstract IDictionary<string, object> Serialize (object obj, JavaScriptSerializer serializer);
	}
}