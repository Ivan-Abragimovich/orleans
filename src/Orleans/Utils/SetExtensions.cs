/*
Project Orleans Cloud Service SDK ver. 1.0
 
Copyright (c) Microsoft Corporation
 
All rights reserved.
 
MIT License

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and 
associated documentation files (the ""Software""), to deal in the Software without restriction,
including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense,
and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so,
subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED *AS IS*, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO
THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS
OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orleans
{
    internal static class SetExtensions
    {
        /// <summary>
        /// Shortcut to create HashSet from IEnumerable that supports type inference
        /// (which the standard constructor does not)
        /// </summary>
        /// <typeparam name="TK"></typeparam>
        /// <typeparam name="TV"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static HashSet<T> ToSet<T>(this IEnumerable<T> values)
        {
            if (values == null)
                return null;
            return new HashSet<T>(values);
        }

        public static bool ListEquals<T>(this List<T> a, List<T> b)
        {
            if (a.Count != b.Count) 
                return false;
            return new HashSet<T>(a).SetEquals(new HashSet<T>(b));
        }

        public static bool IEnumerableEquals<T>(this IEnumerable<T> a, IEnumerable<T> b)
        {
            return new HashSet<T>(a).SetEquals(new HashSet<T>(b));
        }

        public static bool IsSupersetOf<T>(this IEnumerable<T> a, IEnumerable<T> b)
        {
            return new HashSet<T>(a).IsSupersetOf(new HashSet<T>(b));
        }

        /// <summary>
        /// Synchronize contents of two dictionaries with mutable values
        /// </summary>
        /// <typeparam name="TKey">Key type</typeparam>
        /// <typeparam name="TValue">Value type</typeparam>
        /// <param name="a">Dictionary</param>
        /// <param name="b">Dictionary</param>
        /// <param name="copy">Return a copy of a value</param>
        /// <param name="sync">Synchronize two mutable values</param>
        private static void Synchronize<TKey, TValue>(this Dictionary<TKey, TValue> a, Dictionary<TKey, TValue> b, Func<TValue, TValue> copy, Action<TValue, TValue> sync)
        {
            var akeys = a.Keys.ToSet();
            var bkeys = b.Keys.ToSet();
            var aonly = akeys.Except(bkeys).ToSet();
            var bonly = bkeys.Except(akeys).ToSet();
            var both = akeys.Intersect(bkeys).ToSet();
            foreach (var ak in aonly)
            {
                b.Add(ak, copy(a[ak]));
            }
            foreach (var bk in bonly)
            {
                a.Add(bk, copy(b[bk]));
            }
            foreach (var k in both)
            {
                sync(a[k], b[k]);
            }
        }

        /// <summary>
        /// Synchronize contents of two dictionaries with immutable values
        /// </summary>
        /// <typeparam name="TKey">Key type</typeparam>
        /// <typeparam name="TValue">Value type</typeparam>
        /// <param name="a">Dictionary</param>
        /// <param name="b">Dictionary</param>
        /// <param name="sync">Synchronize two values, return synced value</param>
        private static void Synchronize<TKey, TValue>(this Dictionary<TKey, TValue> a, Dictionary<TKey, TValue> b, Func<TValue, TValue, TValue> sync)
        {
            var akeys = a.Keys.ToSet();
            var bkeys = b.Keys.ToSet();
            var aonly = akeys.Except(bkeys).ToSet();
            var bonly = bkeys.Except(akeys).ToSet();
            var both = akeys.Intersect(bkeys).ToSet();
            foreach (var ak in aonly)
            {
                b.Add(ak, a[ak]);
            }
            foreach (var bk in bonly)
            {
                a.Add(bk, b[bk]);
            }
            foreach (var k in both)
            {
                var s = sync(a[k], b[k]);
                a[k] = s;
                b[k] = s;
            }
        }

        /// <summary>
        /// Synchronize contents of two nested dictionaries with mutable values
        /// </summary>
        /// <typeparam name="TKey">Key type</typeparam>
        /// <typeparam name="TKey2">Nested key type</typeparam>
        /// <typeparam name="TValue">Value type</typeparam>
        /// <param name="a">Dictionary</param>
        /// <param name="b">Dictionary</param>
        /// <param name="copy">Return a copy of a value</param>
        /// <param name="sync">Synchronize two mutable values</param>
        private static void Synchronize2<TKey, TKey2, TValue>(this Dictionary<TKey, Dictionary<TKey2, TValue>> a, Dictionary<TKey, Dictionary<TKey2, TValue>> b, Func<TValue, TValue> copy, Action<TValue, TValue> sync)
        {
            a.Synchronize(b, d => d.Copy(copy), (d1, d2) => d1.Synchronize(d2, copy, sync));
        }

        /// <summary>
        /// Synchronize contents of two nested dictionaries with immutable values
        /// </summary>
        /// <typeparam name="TKey">Key type</typeparam>
        /// <typeparam name="TKey2">Nested key type</typeparam>
        /// <typeparam name="TValue">Value type</typeparam>
        /// <param name="a">Dictionary</param>
        /// <param name="b">Dictionary</param>
        /// <param name="copy">Return a copy of a value</param>
        /// <param name="sync">Synchronize two immutable values</param>
        private static void Synchronize2<TKey, TKey2, TValue>(this Dictionary<TKey, Dictionary<TKey2, TValue>> a, Dictionary<TKey, Dictionary<TKey2, TValue>> b, Func<TValue, TValue, TValue> sync)
        {
            a.Synchronize(b, d => new Dictionary<TKey2, TValue>(d), (d1, d2) => d1.Synchronize(d2, sync));
        }

        public static Dictionary<TKey, TValue> Copy<TKey, TValue>(this Dictionary<TKey, TValue> original)
        {
            return new Dictionary<TKey, TValue>(original);
        }

        /// <summary>
        /// Copy a dictionary with mutable values
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="original"></param>
        /// <param name="copy"></param>
        /// <returns></returns>
        private static Dictionary<TKey, TValue> Copy<TKey, TValue>(this Dictionary<TKey, TValue> original, Func<TValue, TValue> copy)
        {
            return original.ToDictionary(pair => pair.Key, pair => copy(pair.Value));
        }

        /// <summary>
        /// ToString every element of an enumeration
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="toString">Can supply null to use Object.ToString()</param>
        /// <param name="before">Before each element, or space if unspecified</param>
        /// <returns></returns>
        public static string ToStrings<T>(this IEnumerable<T> list, Func<T, object> toString = null, string separator = " ")
        {
            if (list == null) return "";
            toString = toString ?? (x => x);
            //Func<T, string> toStringPrinter = (x => 
            //    {
            //        object obj = toString(x);
            //        if(obj != null)
            //            return obj.ToString();
            //        else
            //            return "null";
            //    });
            //return Utils.IEnumerableToString(list, toStringPrinter, separator);
            //Do NOT use Aggregate for string concatenation. It is very inefficient, will reallocate and copy lots of intermediate strings.
            //toString = toString ?? (x => x);
            return list.Aggregate("", (s, x) => s + separator + toString(x));
        }

        public static List<T> Union<T>(List<T> list1, List<T> list2)
        {
            if (list1 == null && list2 == null)
                return null;
            if (list1 == null)
                return list2;
            if (list2 == null)
                return list1;
            list1.AddRange(list2);
            return list1;
        }
    }
}
