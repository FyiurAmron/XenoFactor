namespace Terrain {

using System;
using System.Collections.Generic;
using Data;
using Entities;
using UnityEngine;
using Util;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

// ATM god-class, sort of
public class World3D : TimeAware {
    private readonly SortedMap<int, Entity>[,] entities;
    private readonly List<Entity> tickEntities = new();
    private readonly List<EntityGenerator> entityGenerators = new();

    private readonly HashSet<Entity> transformUpdateQueue = new();
    private readonly HashSet<Entity> disposeQueue = new();

    public int age;

    private readonly WorldGenPreset worldGenPreset;
    private readonly PrefabMap prefabMap;
    private readonly Transform rootTransform;

    public Vector3Int size { get; }

    // public Vector3IntRange sizeRange { get; }

    public World3D( WorldGenPreset worldGenPreset, PrefabMap prefabMap, Transform rootTransform )
        : this( worldGenPreset.size ) =>
        ( this.worldGenPreset, this.prefabMap, this.rootTransform )
        = ( worldGenPreset, prefabMap, rootTransform );

    private World3D( Vector3Int size ) {
        entities = new SortedMap<int, Entity>[size.x, size.y];
        foreach ( int x in ..size.x ) {
            foreach ( int y in ..size.y ) {
                entities[x, y] = new();
            }
        }

        this.size = size;
        // sizeRange = new( Vector3Int.zero, size );
    }

    public bool isValid( int x, int y = 0, int z = 0 )
        => x >= 0 && y >= 0 && z >= 0
            && x < size.x && y < size.y && z < size.z;

    public bool isValid( Vector3Int pos )
        => isValid( pos.x, pos.y, pos.z );

    public Entity get( Vector3Int pos )
        => isValid( pos ) ? entities[pos.x, pos.y][pos.z] : OutOfBoundsEntity.INSTANCE;

    public IReadOnlyDictionary<int, Entity> get( int x, int y )
        => isValid( x, y ) ? entities[x, y] : null;

    /*
    public void set( Vector3Int pos, Entity entity ) {
       entities[pos.x, pos.y][pos.z] = entity;
    }
    */

    private void add( Entity entity ) {
        if ( !isValid( entity.pos ) ) {
            throw new ArgumentOutOfRangeException();
        }

        Vector3Int pos = entity.pos;
        entities[pos.x, pos.y].Add( pos.z, entity );
        entity.world3D = this;
    }

    public void add( EntityGenerator entityGenerator ) {
        entityGenerators.Add( entityGenerator );
    }

    public void remove( Vector3Int pos ) {
        entities[pos.x, pos.y].Remove( pos.z );
    }

    public void bind( Entity entity ) {
        add( entity );
        // TODO row/col rootTransform?
        entity.createGameObject( rootTransform );
    }

    public void initialBind() {
        foreach ( int x in ..size.x ) {
            GameObject column = new( "col" + x ) { // TODO store them
                transform = {
                    parent = rootTransform,
                },
            }; // grouping only, no transform-related stuff here
            foreach ( int y in ..size.y ) {
                GameObject row = new( "row" + y ) { // TODO store them
                    transform = {
                        parent = column.transform,
                    },
                }; // grouping only, no transform-related stuff here
                // TODO create [x,y] => row/col map
                Transform parentTransform = row.transform;

                foreach ( ( int _, Entity entity ) in entities[x, y] ) {
                    entity.createGameObject( parentTransform );
                }
            } // end foreach y
        } // end foreach x
    }

    // for use of Entity class mainly
    public GameObject instantiateEntity( string subtypeName, Transform transform ) =>
        Object.Instantiate( prefabMap.get( subtypeName ), transform );

    public void generate() {
        Vector2Int size2D = new( size.x, size.y );
        int[,] rockHeightMap = generateTerrainHeightMap(
            size2D, worldGenPreset.rockHeightRange, worldGenPreset.rockSmoothRadius,
            worldGenPreset.rockPeakCount, worldGenPreset.rockPeakHeightRange,
            worldGenPreset.rockPeakLengthRange );
        int[,] soilHeightMap = generateTerrainHeightMap(
            size2D, worldGenPreset.soilHeightRange, worldGenPreset.soilSmoothRadius );

        foreach ( int x in ..size.x ) {
            foreach ( int y in ..size.y ) {
                // create rock
                int rockHeight = Math.Max( 1, rockHeightMap[x, y] ); // min is 1

                foreach ( int z in ..rockHeight ) {
                    add( new GroundEntity( GroundEntity.ROCK_GROUND, new( x, y, z ) ) );
                }

                // create soil etc.
                int soilHeight = soilHeightMap[x, y];

                if ( soilHeight > 0 && rockHeight < worldGenPreset.plantHeightAbsoluteMax ) {
                    int topSoilHeight = Math.Min( worldGenPreset.plantHeightAbsoluteMax, rockHeight + soilHeight )
                        - 1;
                    foreach ( int z in rockHeight..topSoilHeight ) {
                        GroundEntity soilEntity
                            = new( GroundEntity.SOIL_GROUND, new( x, y, z ) ) {
                                absorbedAmount = GroundEntity.DEFAULT_ABSORBED_MAX_FOR_SOIL_RANGE.clamp(
                                    worldGenPreset.initialSoilAbsorbedRange.random()
                                ),
                            };
                        add( soilEntity );
                    }

                    GroundEntity grassEntity
                        = new( GroundEntity.GRASS_GROUND, new( x, y, topSoilHeight ) ) {
                            absorbedAmount = GroundEntity.DEFAULT_ABSORBED_MAX_FOR_SOIL_RANGE.clamp(
                                worldGenPreset.initialSoilAbsorbedRange.random()
                            ),
                        };
                    add( grassEntity );

                    if ( Random.value <= worldGenPreset.treeChance ) {
                        TreeEntity treeEntity = new(
                            Random.value <= worldGenPreset.treeDeadChance
                                ? TreeEntity.DEAD_TREE
                                : TreeEntity.PINE_TREE,
                            new( x, y, topSoilHeight + 1 )
                        ) {
                            size = Random.value * worldGenPreset.treeSizeFactor,
                        };
                        treeEntity.clampSize();
                        treeEntity.setAbsorbedAmountToMax();
                        add( treeEntity );
                    }
                } // end if hasSoil
            } // end foreach y
        } // end foreach x

        // end generate()
    }

    public static int[,] generateTerrainHeightMap(
        Vector2Int size,
        IntRange heightRange,
        int blurRadius,
        int peakCount = 0, IntRange peakHeightRange = default, IntRange peakLengthRange = default
    ) {
        int srcLenX = size.x + 2 * blurRadius,
            srcLenY = size.y + 2 * blurRadius;
        int[,] src = new int[srcLenX, srcLenY];

        foreach ( int x in ..srcLenX ) {
            foreach ( int y in ..srcLenY ) {
                src[x, y] = heightRange.random();
            }
        }

        foreach ( int _ in ..peakCount ) {
            int x = Random.Range( 0, size.x ) + blurRadius,
                y = Random.Range( 0, size.y ) + blurRadius;
            foreach ( int __ in ..peakLengthRange.random() ) {
                src[x, y] += peakHeightRange.random();
                x = Math.Clamp( x + Random.Range( -1, 1 ), 0, srcLenX - 1 );
                y = Math.Clamp( y + Random.Range( -1, 1 ), 0, srcLenY - 1 );
            }
        }

        return LinearAlgebra.convolve( size, src, LinearAlgebra.generateDiamondSmoothingKernel( blurRadius ) );
    }

    public bool tryToFallBelow( Entity entity ) {
        Vector3Int pos = entity.pos;
        if ( pos.z == 0 ) {
            return false;
        }

        SortedMap<int, Entity> vert = entities[pos.x, pos.y];
        if ( vert[pos.z - 1] != null ) {
            return false;
        }

        vert.Remove( pos.z );
        pos.z--;
        vert[pos.z] = entity;
        entity.pos = pos;

        entity.scheduleTransformUpdate();

        return true;
    }

    /*
    public bool move( Entity entity, Vector3Int offset ) {
        Vector3Int oldPos = entity.pos,
                   newPos = oldPos + offset;
        IReadOnlyDictionary<int, Entity> vert = get( newPos.x, newPos.y );
        vert[oldPos.z]
        vert[oldPos.z - 1]
        throw new NotImplementedException();
        // TODO row/col GameObject transfer
    }
    */

    public void scheduleTransformUpdate( Entity entity ) {
        transformUpdateQueue.Add( entity );
    }

    public void scheduleDispose( Entity entity ) {
        disposeQueue.Add( entity );
    }

    public void onTick() {
        //// iterate ticks on entities in random order

        // supposedly one of the most performant ways to do it
        foreach ( int x in ..size.x ) {
            foreach ( int y in ..size.y ) {
                tickEntities.AddRange( entities[x, y].Values );
            }
        }

        tickEntities.shuffle();
        foreach ( Entity entity in tickEntities ) {
            entity.onTickGlobal();
        }

        tickEntities.Clear();

        //// generate new entities
        foreach ( EntityGenerator entityGenerator in entityGenerators ) {
            if ( entityGenerator.shouldGenerateNow() ) {
                Entity entity = entityGenerator.generate();
                if ( get( entity.pos ) == null ) {
                    bind( entity );
                } else {
                    // TODO possibly trigger push-out behaviour based on EntityGenerator flag 
                }
            }

            entityGenerator.onTick();
        }

        //// update transforms as needed
        foreach ( Entity entity in transformUpdateQueue ) {
            if ( entity.size >= ProtoEntity.MIN_SIZE ) {
                entity.updateTransform();
            } else {
                disposeQueue.Add( entity );
            }
        }

        transformUpdateQueue.Clear();

        //// dispose queued entities
        foreach ( Entity entity in disposeQueue ) {
            remove( entity.pos );
            entity.destroyGameObject();
        }

        disposeQueue.Clear();

        //// increase age
        age++;
    }
}

}
