namespace Entities {

using System;
using UnityEngine;

// typed singleton null-object that conveys an out-of-bounds terrain semantic
public class OutOfBoundsEntity : Entity {
    private static bool instanceExists = false;
    public static readonly OutOfBoundsEntity INSTANCE = new( null, new( -1, -1, -1 ) );

    protected OutOfBoundsEntity( string subtypeName, Vector3Int pos ) : base( subtypeName, pos ) {
        if ( instanceExists ) {
            throw new NotSupportedException();
        }

        instanceExists = true;
    }
}

}
