using Data;
using Terrain;
using UnityEngine;
using Util;

public class MainMap : MonoBehaviour {
    public PrefabMap terrainPrefabMap;
    public WorldGenPreset worldGenPreset;

    public float gfxTimeScale = 1.0f;
    public int engineTimeScale = 1;

    private int fixedUpdateCounter = 0;

    private World3D world3D;

    protected void Awake() {
    }

    // Start is called before the first frame update
    protected void Start() {
    }

    // Update is called once per frame
    protected void Update() {
    }

    protected void FixedUpdate() {
        Time.timeScale = gfxTimeScale; // this also affects fixed updates, as intended

        // every 2nd update, to leave time for physics etc.
        if ( fixedUpdateCounter % 2 == 0 ) {
            foreach ( int _ in ..engineTimeScale ) {
                world3D.onTick();
            }
        }

        fixedUpdateCounter++;
    }

    // called on Startup#Awake ; TODO move to correct event queue later on
    public void generate() {
        world3D = new( worldGenPreset, terrainPrefabMap, transform );
        world3D.generate();
        world3D.initialBind();

        /*

        Vector3IntRange rainPosRange = new( world3D.size );
        rainPosRange.min.z = rainPosRange.max.z / 2;
        GameObject rainGenerator = new( "rainGenerator" );
        EntityGeneratorComponent entityGeneratorComponent = rainGenerator.AddComponent<EntityGeneratorComponent>();
        EntityGenerator entityGenerator = new() {
            proto = new WaterEntity( WaterEntity.RAIN_WATER, new( 8, 8, 32 ) ) {
                // size = 2 * ProtoEntity.MIN_SIZE,
                size = 0.8f,
            },
            interval = 2,
            // randomizePos = true,
            randomPosRange = rainPosRange,
        };
        entityGeneratorComponent.properties = entityGenerator;

        world3D.add( entityGenerator );
        
        */
    }
}
