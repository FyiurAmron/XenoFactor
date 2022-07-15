namespace Util {

using System;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public struct Vector3IntRange {
    public Vector3Int min, max;

    public Vector3IntRange( Vector3Int max ) : this( Vector3Int.zero, max ) {
    }

    public Vector3IntRange( Vector3Int min, Vector3Int max )
        => ( this.min, this.max )
            = ( min, max );
    
    public Vector3IntRange setAsXRange( IntRange intRange ) {
        min.x = intRange.min;
        max.x = intRange.max;
        
        return this;
    }
    
    public Vector3IntRange setAsYRange( IntRange intRange ) {
        min.y = intRange.min;
        max.y = intRange.max;
        
        return this;
    }

    public Vector3IntRange setAsZRange( IntRange intRange ) {
        min.z = intRange.min;
        max.z = intRange.max;
        
        return this;
    }

    public readonly Vector3Int random()
        => new(
            Random.Range( min.x, max.x ),
            Random.Range( min.y, max.y ),
            Random.Range( min.z, max.z )
        );

    public readonly Vector2Int randomXY()
        => new(
            Random.Range( min.x, max.x ),
            Random.Range( min.y, max.y )
        );

    public override readonly string ToString()
        => $"[{min},{max})";
}

}
