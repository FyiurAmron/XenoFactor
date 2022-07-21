namespace Entities {

using System;
using UnityEngine;
using Util;
using Object = UnityEngine.Object;

// basically a 10m x 10m x 10m voxel cube, normalized to 1x1x1 uniform
[Serializable]
public class Entity : ProtoEntity, TimeAware {
    public GameObject gameObject;

    public Entity() {
    }

    public Entity( string subtypeName, Vector3Int pos )
        : base( subtypeName, pos ) {
    }

    public virtual void onTick() {
    }

    public /* virtual */ Entity clone()
        => MemberwiseClone() as Entity;

    public void onTickGlobal() {
        Debug.Assert( size >= 0, "size >= 0" );
        Debug.Assert( size <= 1.0f, "size <= 1.0f" );
        Debug.Assert( absorbedAmount >= 0, "absorbedAmount >= 0" );
        Debug.Assert( absorbedAmount <= absorbedMax, "absorbedAmount <= absorbedMax" );

        fallingThisTick = false;

        if ( !fixedPosition ) {
            fallingThisTick = world3D.tryToFallBelow( this );
        }

        onTick();
        age++;
    }

    // shouldn't be called outside of 1st step of tick processor in Terrain3D!
    public void updateTransform() {
        if ( size is < MIN_SIZE or > 1 ) {
            throw new ArgumentOutOfRangeException( $"size {size} < MIN_SIZE {MIN_SIZE} || > 1" );
        }

        Transform transform = gameObject.transform;
        transform.localPosition = new( pos.x, pos.z, pos.y ); // note the Z<=>Y twiddle
        float xzScale = sizeYOnly ? 1 : size;
        transform.localScale = new( xzScale, size, xzScale );
        transform.eulerAngles = new( 0, rotation, 0 );
    }

    public GameObject createGameObject( Transform parentTransform ) {
        GameObject go = world3D.instantiateEntity( subtypeName, parentTransform );

        gameObject = go;
        go.name += $"_[{pos.x},{pos.y},{pos.z}]";
        EntityComponent entityComponent = go.AddComponent<EntityComponent>();
        entityComponent.properties = this;
        updateTransform();
        
        /*
        if ( tint != null ) {
            go.transform.GetChild( 0 ).GetComponent<Renderer>().material
              .setColor( UnityConst.EMISSION_COLOR_KEYWORD, tint.Value );
        }
        */ 

        return go;
    }

    public void destroyGameObject() {
        Object.Destroy( gameObject );
    }

    public GameObject regenerateGameObject( Transform parentTransform = null ) {
        parentTransform ??= gameObject.transform.parent;
        destroyGameObject();
        return createGameObject( parentTransform );
    }

    public void scheduleTransformUpdate() {
        world3D.scheduleTransformUpdate( this );
    }

    public void scheduleDispose() {
        world3D.scheduleDispose( this );
    }
}

}
