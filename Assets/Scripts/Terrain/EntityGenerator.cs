namespace Terrain {

using System;
using Entities;
using Util;

[Serializable]
public class EntityGenerator : TimeAware {
    public int interval = 1;
    public int age;
    public int lastGeneratedAge = -1; // before start time
    public int limit = -1; // -1 => no limit
    public int count;

    public bool randomizePos;
    // TODO add enum describing behaviour for non-empty target position
    public Vector3IntRange randomPosRange;

    public Entity proto;
    // private Terrain3D terrain3D;

    public void onTick() {
        age++;
    }

    public bool shouldGenerateNow() =>
        ( limit < 0 || limit > count )
        && age >= lastGeneratedAge + interval;

    public Entity generate() {
        lastGeneratedAge = age;
        count++;
        Entity entity = proto.clone();
        if ( randomizePos ) {
            entity.pos = randomPosRange.random();
        }

        return entity;
    }
}

}
