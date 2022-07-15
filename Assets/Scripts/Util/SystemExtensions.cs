namespace Util {

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public static class RangeExtensions {
    public static Enumerator GetEnumerator( this Range range ) => new( range );

    public struct Enumerator {
        private readonly int start;
        private readonly int end;

        public Enumerator( Range range ) {
            start = Current = range.Start.Value - 1;
            end = range.End.Value - 1;
        }

        public bool MoveNext() {
            if ( Current >= end ) {
                return false;
            }

            Current++;
            return true;
        }

        public void Reset() {
            Current = start;
        }

        public int Current { get; private set; }
    }
}

public static class IEnumeratorExtensions {
    public static IEnumerable ToIEnumerable( this IEnumerator enumerator ) {
        while ( enumerator.MoveNext() ) {
            yield return enumerator.Current;
        }
    }

    public static IEnumerable<T> ToIEnumerable<T>( this IEnumerator<T> enumerator ) {
        while ( enumerator.MoveNext() ) {
            yield return enumerator.Current;
        }
    }
}

public static class IEnumerableExtensions {
    public static bool empty<T>( this IEnumerable<T> list )
        => !list.Any();
    
    public static string toString<T>( this IEnumerable<T> list )
        => $"[{string.Join( ",", list )}]";

}

public static class IListExtensions {
    public static IList<T> shuffle<T>( this IList<T> list, Random rnd ) {
        return shuffle( list, rnd.Next );
    }

    public static IList<T> shuffle<T>( this IList<T> list, Func<int, int> rndFunc ) {
        for ( int i = list.Count - 1; i > 0; i-- ) {
            list.swap( i, rndFunc( i + 1 ) );
        }

        return list;
    }

    public static void swap<T>( this IList<T> list, int i, int j ) {
        ( list[i], list[j] ) = ( list[j], list[i] );
    }
}

}
