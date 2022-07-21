namespace Entities {

using System;
using System.Collections.Generic;
using UnityEngine;
using Util;
using Random = UnityEngine.Random;

public class GroundEntity : Entity {
    public const string ROCK_GROUND = "RockGround";
    public const string SOIL_GROUND = "SoilGround";
    public const string GRASS_GROUND = "GrassGround";

    public const float DEFAULT_ABSORBED_MAX_FOR_SOIL = 0.9f;
    public const float GRASS_DEFAULT_WATER_USAGE = 1E-5f;

    public static readonly FloatRange DEFAULT_ABSORBED_MAX_FOR_SOIL_RANGE = new( 0, DEFAULT_ABSORBED_MAX_FOR_SOIL );

    protected float maximumFlowAmount
        => Math.Max( 0, absorbedAmount - getAbsorbedWaterFlowThreshold() );
    // the above may look quite high in comparison with trees, but it's mostly legit - forests are very efficient

    public GroundEntity( string subtypeName, Vector3Int pos )
        : base( subtypeName, pos ) {
        sizeYOnly = true;
        rotation = Random.Range( 0, 4 ) * 90;

        // tint = Random.ColorHSV( 0, 1 );

        if ( subtypeName != ROCK_GROUND ) {
            absorbedMax = DEFAULT_ABSORBED_MAX_FOR_SOIL;
        }
    }

    protected float getAbsorbedWaterFlowThreshold() {
        return subtypeName switch { // this should be strictly > SIZE_MIN for subtypes that have non-zero absorbedMax
            ROCK_GROUND => 0, // no absorption & no flow
            SOIL_GROUND => 0.45f,
            GRASS_GROUND => 0.6f,
            _ => throw new NotSupportedException(),
        };
    }

    public override void onTick() {
        if ( fallingThisTick ) {
            return;
        }

        if ( absorbedAmount > 0 ) {
            if ( subtypeName == GRASS_GROUND ) {
                absorbedAmount -= GRASS_DEFAULT_WATER_USAGE;
                if ( absorbedAmount < 0 ) {
                    absorbedAmount = 0;
                }
            }

            if ( maximumFlowAmount > 0 ) { // NOTE: no need to worry about SIZE_MIN, since the threshold > MIN
                tryToFlowBelow();

                if ( maximumFlowAmount > 0 ) {
                    tryToGetAbsorbed();

                    if ( maximumFlowAmount > 0 ) {
                        tryToFlowAbsorbedWaterOutside();
                    }
                }
            }
        } else { // absorbedAmount <= 0
            if ( subtypeName == GRASS_GROUND ) {
                subtypeName = SOIL_GROUND;
                regenerateGameObject();
            }
        }
    }

    // TODO refactor up due to similarities with WaterEntity
    private void tryToFlowBelow() {
        Entity belowEntity = find( Direction3D.BELOW );
        float flowAmount = MathF.Min(
            maximumFlowAmount, // NOTE this was changed vs WE
            belowEntity is WaterEntity waterEntityBelow
                ? 1 - waterEntityBelow.size
                : belowEntity.absorbedMax - belowEntity.absorbedAmount
        );

        if ( flowAmount <= 0 ) {
            return;
        }

        absorbedAmount -= flowAmount; // NOTE this was changed vs WE
        scheduleTransformUpdate();

        if ( belowEntity is WaterEntity ) {
            belowEntity.size += flowAmount;
        } else {
            belowEntity.absorbedAmount += flowAmount;
        }

        belowEntity.scheduleTransformUpdate();
    }

    // NOTE this can produces oscillatory behaviour, in the sense that the water will constantly flow out and in
    // in particular setups; it may be considered a feature for now XD
    // TODO possibly refactor this up, since it's very similar to WaterEntity
    private void tryToFlowAbsorbedWaterOutside() {
        List<WaterEntity> flowTargetEntities = new();
        float sizeSum = maximumFlowAmount; // diff to WE

        foreach ( Vector3Int direction in Direction3D.getRandomOrderedPlanarDirections() ) {
            Vector3Int targetPos = pos + direction;

            Entity found = world3D.get( targetPos );
            switch ( found ) {
                case null: // handled here completely to avoid null checks in default
                    if ( maximumFlowAmount < 2 * MIN_SIZE ) { // diff to WE
                        break;
                    }

                    WaterEntity newWaterEntity = new( WaterEntity.RAIN_WATER, targetPos ) { // TODO DIRTY_WATER
                        size = MIN_SIZE,
                    };
                    absorbedAmount -= MIN_SIZE;
                    flowTargetEntities.Add( newWaterEntity );
                    world3D.bind( newWaterEntity );
                    // sizeSum doesn't change, obviously
                    break;
                case WaterEntity waterEntity:
                    if ( found.size >= maximumFlowAmount ) { // diff to WE
                        break;
                    }

                    flowTargetEntities.Add( waterEntity );
                    sizeSum += found.size;
                    break;
                default:
                    // nothing ATM
                    break;
            }
        }

        if ( flowTargetEntities.empty() ) {
            return;
        }

        // flowTargetEntities.Add( this ); // diff to WE
        float sizeAvg = sizeSum / flowTargetEntities.Count;
        float overflow = 0;
        if ( sizeAvg > 1 ) { // shouldn't happen due to usual thresholds, but still 
            sizeAvg = 1;
            overflow = sizeSum - flowTargetEntities.Count;
        }

        foreach ( WaterEntity flowTargetEntity in flowTargetEntities ) {
            flowTargetEntity.size = sizeAvg;
            flowTargetEntity.scheduleTransformUpdate();
        }

        absorbedAmount -= ( maximumFlowAmount - overflow ); // diff to WE
        scheduleTransformUpdate(); // diff to WE
    }

    // TODO refactor up due to similarities with WaterEntity
    private void tryToGetAbsorbed() {
        HashSet<Entity> absorbTargetEntities = new();
        float minAbsorb = 1;
        Entity minAbsorbEntity = null;

        foreach ( Vector3Int direction in Direction3D.getRandomOrderedPlanarDirections() ) {
            Vector3Int targetPos = pos + direction;

            Entity found = world3D.get( targetPos );
            if ( found == null ) {
                continue;
            }

            // NOTE maximumFlowAmount may be also considered in some way here... ?
            float absorbPossible = found.possibleAbsorbAmount;
            if ( absorbPossible <= 0 ) {
                continue;
            }

            absorbTargetEntities.Add( found );
            if ( absorbPossible < minAbsorb ) {
                minAbsorb = absorbPossible;
                minAbsorbEntity = found;
            }
        }

        if ( absorbTargetEntities.empty() ) {
            return;
        }

        scheduleTransformUpdate();

        while ( absorbTargetEntities.Count > 0 && maximumFlowAmount > 0 ) {
            int cnt = absorbTargetEntities.Count;
            float sizeForEach = maximumFlowAmount / cnt;
            if ( sizeForEach < minAbsorb ) {
                foreach ( Entity entity in absorbTargetEntities ) {
                    entity.absorbedAmount += sizeForEach;
                }

                absorbedAmount = getAbsorbedWaterFlowThreshold(); // auto-dispose (if actually needed)
                break;
            }

            float newMinAbsorb = 1;
            foreach ( Entity entity in absorbTargetEntities ) {
                entity.absorbedAmount += minAbsorb;
                float absorbPossible = entity.possibleAbsorbAmount;
                // absorbPossible == 0 is a possible quick path here, but that requires different algo
                if ( absorbPossible < newMinAbsorb ) {
                    newMinAbsorb = absorbPossible;
                    minAbsorbEntity = entity;
                }
            }

            absorbedAmount -= cnt * minAbsorb;

            minAbsorb = newMinAbsorb;

            absorbTargetEntities.Remove( minAbsorbEntity );
        }
    }
}

}
