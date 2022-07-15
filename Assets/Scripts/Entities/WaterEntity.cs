namespace Entities {

using System;
using System.Collections.Generic;
using UnityEngine;
using Util;

public class WaterEntity : Entity {
    public const string RAIN_WATER = "RainWater";

    public WaterEntity( string subtypeName, Vector3Int pos )
        : base( subtypeName, pos ) =>
        sizeYOnly = true;

    public override void onTick() {
        if ( fallingThisTick ) {
            return;
        }

        // a silent assumption we're still above MIN_SIZE :D
        tryToFlowBelow();

        if ( size > MIN_SIZE ) {
            tryToFlowAround();
        }

        if ( size > MIN_SIZE ) {
            tryToGetAbsorbed();
        }
    }

    private void tryToFlowBelow() {
        Entity belowEntity = find( Direction3D.BELOW );
        float flowAmount = MathF.Min(
            size,
            belowEntity is WaterEntity waterEntityBelow
                ? 1 - waterEntityBelow.size
                : belowEntity.absorbedMax - belowEntity.absorbedAmount
        );

        if ( flowAmount <= 0 ) {
            return;
        }

        size -= flowAmount;
        scheduleTransformUpdate();

        if ( belowEntity is WaterEntity ) {
            belowEntity.size += flowAmount;
        } else {
            belowEntity.absorbedAmount += flowAmount;
        }

        belowEntity.scheduleTransformUpdate();
    }

    private void tryToFlowAround() {
        List<WaterEntity> flowTargetEntities = new();
        float sizeSum = size;

        foreach ( Vector3Int direction in Direction3D.getRandomOrderedPlanarDirections() ) {
            Vector3Int targetPos = pos + direction;

            Entity found = world3D.get( targetPos );
            switch ( found ) {
                case null: // handled here completely to avoid null checks in default
                    if ( size < 2 * MIN_SIZE ) {
                        break;
                    }

                    WaterEntity newWaterEntity = new( subtypeName, targetPos ) {
                        size = MIN_SIZE,
                    };
                    size -= MIN_SIZE;
                    flowTargetEntities.Add( newWaterEntity );
                    world3D.bind( newWaterEntity );
                    // sizeSum doesn't change, obviously
                    break;
                case WaterEntity waterEntity:
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

        flowTargetEntities.Add( this );
        float sizeAvg = sizeSum / flowTargetEntities.Count;
        foreach ( WaterEntity flowTargetEntity in flowTargetEntities ) {
            flowTargetEntity.size = sizeAvg;
            flowTargetEntity.scheduleTransformUpdate();
        }
    }

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

        while ( absorbTargetEntities.Count > 0 && size > 0 ) {
            int cnt = absorbTargetEntities.Count;
            float sizeForEach = size / cnt;
            if ( sizeForEach < minAbsorb ) {
                foreach ( Entity entity in absorbTargetEntities ) {
                    entity.absorbedAmount += sizeForEach;
                }

                size = 0; // auto-dispose (if actually needed)
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

            size -= cnt * minAbsorb;

            minAbsorb = newMinAbsorb;

            absorbTargetEntities.Remove( minAbsorbEntity );
        }
    }
}

}
