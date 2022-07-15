namespace Util {

using System.Collections.Generic;
using System.Linq;

public class SortedMap<TKey, TValue> : SortedDictionary<TKey, TValue> where TKey : notnull {
    // public Map () : base() { }

    public new TValue this[ TKey key ] {
        get => TryGetValue( key, out TValue val ) ? val : default;
        set => base[key] = value;
    }

    public override string ToString() {
        return "{ " + string.Join( "; ", this.Select( x => $"{x.Key} => {x.Value}" ) ) + " }";
    }

    public void add( KeyValuePair<TKey, TValue> keyValuePair ) {
        Add( keyValuePair.Key, keyValuePair.Value );
    }
}

}
