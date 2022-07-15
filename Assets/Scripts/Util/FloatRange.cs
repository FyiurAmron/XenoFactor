namespace Util {

using System;
using Random = UnityEngine.Random;

[Serializable]
public struct FloatRange {
    public float min, max;

    public FloatRange( float min, float max ) =>
        ( this.min, this.max )
        = ( min, max );

    public readonly float clamp( float value )
        => Math.Clamp( value, min, max );
    
    // [min,max] i.e. max inclusive, due to how Unity handles this partifular case
    public readonly float random()
        => Random.Range( min, max );

    public override readonly string ToString()
        => $"[{min},{max})";
}

}
