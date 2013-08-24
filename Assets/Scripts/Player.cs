using UnityEngine;
using System.Collections;

public class Player : EntityBase {
    public enum State {
        Invalid = -1,
        Normal,
        Hurt,
        Dead
    }

    protected override void StateChanged() {
    }

    protected override void OnDespawned() {
        //reset stuff here

        base.OnDespawned();
    }

    protected override void OnDestroy() {
        //dealloc here

        base.OnDestroy();
    }

    public override void Release() {
        state = (int)State.Invalid;

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
        base.Awake();

        //initialize variables
        autoSpawnFinish = true;
    }

    // Use this for initialization
    protected override void Start() {
        base.Start();

        //initialize variables from other sources (for communicating with managers, etc.)
    }
}
