using UnityEngine;
using System.Collections;

public class Player : EntityBase {
    public enum State {
        Invalid = -1,
        Normal,
        Hurt,
        Dead,
        Victory
    }

    public string gameoverScene;



    private static Player mInstance;

    private bool mIsGoal;
    private PlayerController mCtrl;
    private string mExitScene;

    public static Player instance { get { return mInstance; } }

    public PlayerController controller { get { return mCtrl; } }

    public bool isGoal { get { return mIsGoal; } set { mIsGoal = value; } }

    protected override void StateChanged() {
    }

    protected override void OnDespawned() {
        //reset stuff here

        base.OnDespawned();
    }

    protected override void OnDestroy() {
        if(mInstance == this)
            mInstance = null;

        //dealloc here

        base.OnDestroy();
    }

    public void ExitToScene(string scene) {
        mExitScene = scene;
        //start the exit animation
    }

    public override void Release() {
        state = (int)State.Invalid;

        mIsGoal = false;

        base.Release();
    }

    public override void SpawnFinish() {
        //start ai, player control, etc
        state = (int)State.Normal;
    }

    protected override void SpawnStart() {
        //initialize some things
    }

    protected override void Awake() {
        if(mInstance == null) {
            mInstance = this;

            mCtrl = GetComponent<PlayerController>();

            base.Awake();

            //initialize variables
            autoSpawnFinish = true;
        }
        else
            DestroyImmediate(gameObject);
    }

    // Use this for initialization
    protected override void Start() {
        base.Start();

        //initialize variables from other sources (for communicating with managers, etc.)
    }
}
