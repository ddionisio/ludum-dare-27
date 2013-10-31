using UnityEngine;
using System.Collections;

public class Boss : EntityBase {
    public const int maxHP = 3;

    private Player mPlayer;

    private int mCurHP = maxHP;

    public int curHP {
        get { return mCurHP; }
        set {
            if(mCurHP != value) {
                mCurHP = Mathf.Clamp(value, 0, maxHP);
                //update hud

                //if 0, dead
                if(mCurHP == 0) {
                    mPlayer.state = (int)Player.State.Victory;

                    //it is up to the FSM or whatever animation to call OpenLevelComplete from player
                    //"EntityKill"
                    FSM.SendEvent(EntityEvent.Kill);
                }
                else {
                    FSM.SendEvent(EntityEvent.Hurt);

                    Hurt();
                }
            }
        }
    }

    protected override void OnDespawned() {
        //reset stuff here

        base.OnDespawned();
    }

    protected override void OnDestroy() {
        //dealloc here

        base.OnDestroy();
    }

    public override void SpawnFinish() {
        //start ai, player control, etc
    }

    protected override void SpawnStart() {
        //initialize some things
    }

    protected override void Awake() {
        base.Awake();

        //initialize variables
    }

    // Use this for initialization
    protected override void Start() {
        base.Start();

        //initialize variables from other sources (for communicating with managers, etc.)
        mPlayer = Player.instance;
        mPlayer.readyCallback += OnPlayerReady;
        mPlayer.controller.bombCtrl.consumeCallback += OnBombConsume;
    }

    /////////////////////////
    //interfaces

    protected virtual void Hurt() {
    }

    /////////////////////////
    //internal

    //player is ready, start entering
    void OnPlayerReady(Player player) {
        //this is the actual entry-point, make sure FSM is waiting to receive "EntityActionEnter", then from there do fancy level entry
        //then do SpawnFinish
        FSM.SendEvent(EntityEvent.ActionEnter);
    }

    void OnBombConsume(BombController ctrl) {
        curHP--;

        if(curHP > 0) {
            //reset bomb
            ctrl.Init();
            mPlayer.controller.BombActive();
        }
    }
}
