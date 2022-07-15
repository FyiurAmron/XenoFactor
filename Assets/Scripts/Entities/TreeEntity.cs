namespace Entities {

using System;
using UnityEngine;
using Util;
using Random = UnityEngine.Random;

[Serializable]
public class TreeEntity : Entity {
    public const string PINE_TREE = "PineTree";
    public const string DEAD_TREE = "DeadTree";

    // a live reference tree weighs about a 1t (1000 kg)
    // and is assumed to be 50% water
    // the possible water storage is thus 0.5m3 (1m3 = 0.01 height of base block)
    public const float DEFAULT_ABSORBED_MAX = 0.5E-3f;
    public const float DEFAULT_WATER_USAGE = 1E-5f;
    public static readonly FloatRange DEFAULT_ABSORBED_MAX_RANGE = new( 0, DEFAULT_ABSORBED_MAX );

    // public string someTreeProperty = "foo";

    public TreeEntity( string subtypeName, Vector3Int pos )
        : base( subtypeName, pos ) {
        if ( subtypeName != DEAD_TREE ) { // possibly different for different tree types
            absorbedMax = DEFAULT_ABSORBED_MAX; // note: this is small enough to not need to be scaled by tree size
        }

        rotation = Random.Range( 0, 360 );
    }

    protected float expGrowthFactor =>
        subtypeName switch {
            DEAD_TREE => -1E-4f, // no growth, decays with time, from 1.0 to 0.01 in ~5 years
            PINE_TREE => 1E-4f, // about *75 in 5 years if starting from MIN_SIZE
            _ => throw new NotSupportedException(),
        };

    public override void onTick() {
        if ( fallingThisTick ) {
            subtypeName = DEAD_TREE;
            // absorbedAmount = 0; // ?
            regenerateGameObject();
            return;
        }

        if ( subtypeName == DEAD_TREE ) {
            size *= 1 + expGrowthFactor; // should be negative
            scheduleTransformUpdate();
            return;
        }

        Entity belowEntity = find( Direction3D.BELOW );

        // note: the tree actually dies only if the total amount of available water is 0,
        // but not if the amount is just lower than STATIC_WATER_USAGE;
        // tree will still have a single tick before dying, which should buffer out possible anomalies with water flow
        float availableWater = absorbedAmount + belowEntity.absorbedAmount;
        if ( availableWater <= 0 ) {
            subtypeName = DEAD_TREE;
            regenerateGameObject();
            return;
        }

        float waterUsage = DEFAULT_WATER_USAGE; // TODO modify based on tree type

        if ( availableWater > waterUsage ) {
            float sizeGrowth = MathF.Min( absorbedAmount, expGrowthFactor * ( 1 - size ) * size );
            if ( sizeGrowth > 0 ) {
                waterUsage += sizeGrowth;
                size += sizeGrowth;
                absorbedAmount -= sizeGrowth;
                scheduleTransformUpdate();
            }
        }

        if ( waterUsage <= belowEntity.absorbedAmount ) {
            belowEntity.absorbedAmount -= waterUsage;
        } else {
            waterUsage -= belowEntity.absorbedAmount;
            belowEntity.absorbedAmount = 0;
            absorbedAmount -= waterUsage;
            if ( absorbedAmount < 0 ) {
                absorbedAmount = 0;
            }
        }
    }
}

}
