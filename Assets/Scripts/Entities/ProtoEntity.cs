namespace Entities {

using System;
using Terrain;
using UnityEngine;

// TODO encapsulation for the sake of debugging (setters assertions mostly)
[Serializable]
public class ProtoEntity {
    public const float MIN_SIZE = 0.01f;

    public string subtypeName;
    public Vector3Int pos;

    public float size = 1; // [0,1]
    public float rotation = 0; // degrees
    // public Color? tint = null;
    
    public int age; // in ticks-hours 

    public bool sizeYOnly;

    //public FixedToEnum fixedTo; // not sure if this makes sense, but maybe?
    public bool fixedPosition; // TEMP solution to the above problem

    public float absorbedMax;
    public float absorbedAmount;

    public bool fallingThisTick;

    public World3D world3D;

    public ProtoEntity() {
    }

    protected ProtoEntity( string subtypeName, Vector3Int pos )
        => ( this.subtypeName, this.pos )
            = ( subtypeName, pos );

    public float possibleAbsorbAmount
        => absorbedMax - absorbedAmount;

    public void clampSize() {
        size = Math.Clamp( size, MIN_SIZE, 1 );
    }

    public void setAbsorbedAmountToMax() {
        absorbedAmount = absorbedMax;
    }

    public Entity find( Vector3Int dist ) {
        Vector3Int target = pos + dist;
        return world3D.get( target );
    }
}

}
