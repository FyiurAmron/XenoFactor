namespace Util {

using System;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public struct Vector3Range {
    public Vector3 min, max;

    public Vector3Range( Vector3 max ) : this( Vector3.zero, max ) {
    }

    public Vector3Range( Vector3 min, Vector3 max )
        => ( this.min, this.max )
            = ( min, max );
    
    public Vector3Range setAsXRange( FloatRange intRange ) {
        min.x = intRange.min;
        max.x = intRange.max;
        
        return this;
    }
    
    public Vector3Range setAsYRange( FloatRange intRange ) {
        min.y = intRange.min;
        max.y = intRange.max;
        
        return this;
    }

    public Vector3Range setAsZRange( FloatRange intRange ) {
        min.z = intRange.min;
        max.z = intRange.max;
        
        return this;
    }

    public readonly float clampX( float value )
        => Math.Clamp( value, min.x, max.x );

    public readonly float clampY( float value )
        => Math.Clamp( value, min.y, max.y );
    
    public readonly float clampZ( float value )
        => Math.Clamp( value, min.z, max.z );
    
    public readonly Vector3 clamp( Vector3 vector3 ) {
        vector3.x = clampX( vector3.x );
        vector3.y = clampY( vector3.y );
        vector3.z = clampZ( vector3.z );
        return vector3;
    }

    public readonly Vector3 random()
        => new(
            Random.Range( min.x, max.x ),
            Random.Range( min.y, max.y ),
            Random.Range( min.z, max.z )
        );

    public readonly Vector2 randomXY()
        => new(
            Random.Range( min.x, max.x ),
            Random.Range( min.y, max.y )
        );

    public override readonly string ToString()
        => $"[{min},{max})";
}

}
