namespace Util {

using System;
using Random = UnityEngine.Random;

[Serializable]
public struct IntRange {
    public int min, max;

    public IntRange( int min, int max ) =>
        ( this.min, this.max )
        = ( min, max );
    
    public readonly int clamp( int value ) {
        return Math.Clamp( value, min, max );
    }
    
    public readonly int random()
        => Random.Range( min, max );

    public override readonly string ToString()
        => $"[{min},{max})";
}

}
