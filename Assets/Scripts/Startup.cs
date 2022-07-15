using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class Startup : MonoBehaviour {
    public const float BASE_FIXED_DELTA_TIME = 1.0f / 48; // to match with game-hours

    public MainMap mainMap;
    public int seed;
    public bool useSeed;

    protected void Awake() {
        // QualitySettings.vSyncCount = 1;
        Time.fixedDeltaTime = BASE_FIXED_DELTA_TIME;

        if ( !useSeed ) {
            seed = (int) DateTime.Now.Ticks;
        }

        Random.InitState( seed );
        Debug.Log( $"seed {seed}" );
        mainMap.generate();
    }
}
