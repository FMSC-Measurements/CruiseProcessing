using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace CruiseProcessing.Test
{
    public static class IEnumerableExtentions
    {
        public static IEnumerable<T> OrEmpty<T>(this IEnumerable<T> @this)
        {
            return @this ?? Enumerable.Empty<T>();
        }

        public static IEnumerable OrEmpty(this IEnumerable @this)
        {
            return @this ?? Enumerable.Empty<object>();
        }

        public static bool AnyAndNotNull<T>(this IEnumerable<T> @this)
        {
            return @this != null && @this.Any();
        }

        public static bool AnyAndNotNull<T>(this IEnumerable<T> @this, Func<T, bool> predicate)
        {
            return @this != null && @this.Any(predicate);
        }

        public static int MaxOrDefault<T>(this IEnumerable<T> @this, Func<T, int> selector, int dVal = default(int))
        {
            if (@this.AnyAndNotNull())
            { return @this.Max(selector); }
            else
            { return dVal; }
        }

        public static bool IsNullOrEmpty<T>(this IEnumerable<T> e)
        {
            return e == null || e.Any() == false;
        }

        public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> @this)
        {
            if (@this is ObservableCollection<T> c) { return c; }

            return new ObservableCollection<T>(@this);
        }

        // ToHashSet requires in netcore 2.0, net472 or netstd21
#if !NETCOREAPP3_1

        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> @this)
        {
            if (@this is HashSet<T> c) { return c; }

            return new HashSet<T>(@this);
        }

#endif
    }
}