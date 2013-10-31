using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Player : EntityBase {
    public enum State {
        Invalid = -1,
        Normal,
        Hurt,
        Dead,
        Victory
    }

    public delegate void OnGenericCall(Player player);

    public bool bombEnabled = true;

    public event OnGenericCall readyCallback; //finished entering the level

    //public string gameoverScene = "gameover";

    private static Player mInstance;

    private bool mIsGoal;
    private PlayerController mCtrl;

    private int mNumDeath = 0;
    private float mCurTime = 0;
    private bool mTimerActive = false;

    private HUD mHUD;

    private Vector3 mCheckPointPos;
    private Quaternion mCheckPointRot;
    private Vector3 mCheckPointUp;
    private Checkpoint mCheckPointLast;
    private GravityFieldBase mCheckPointGravField;

    private AnimatorData mAnim;

    private M8.ImageEffects.WaveRGB mGameOverFX;

    private List<AnimatorData> mLastStarsCollected = new List<AnimatorData>(3);
    private List<TriggerCheckpoint> mLastTriggers = new List<TriggerCheckpoint>(5);

    public static Player instance { get { return mInstance; } }

    public PlayerController controller { get { return mCtrl; } }

    public float curTime { get { return mCurTime; } }

    public int numDeath { get { return mNumDeath; } }

    public bool isGoal { get { return mIsGoal; } set { mIsGoal = value; } }

    public HUD HUD { get { return mHUD; } }

    public void GameOver() {
        state = (int)Player.State.Dead;

        StartCoroutine(GameOverDelay());
    }

    public void OpenLevelComplete() {
        //determine which modal to use

        //normal
        UIModalManager.instance.ModalOpen("levelComplete");

        //boss

        //other
    }

    public void SetCheckPoint(Checkpoint c) {
        if(mCheckPointLast)
            mCheckPointLast.ResetState();

        mCheckPointLast = c;
        SetCheckpoint(c.point.position, c.point.up);
    }

    public void CollectStar(Collider col) {
        col.enabled = false;
        AnimatorData anim = col.GetComponent<AnimatorData>();
        anim.Play("collect");

        mLastStarsCollected.Add(anim);

        mHUD.StarFill();
    }

    public void AddTriggerCheckpoint(TriggerCheckpoint trigger) {
        mLastTriggers.Add(trigger);
    }

    protected override void StateChanged() {
        switch((State)state) {
            case State.Invalid:
                if(mGameOverFX)
                    mGameOverFX.enabled = false;

                mCurTime = 0.0f;
                mTimerActive = false;

                mIsGoal = false;

                if(mHUD)
                    mHUD.ResetData();
                break;

            case State.Normal:
                if(mHUD.timerEnabled && !mTimerActive) {
                    mTimerActive = true;
                    StartCoroutine(Timer());
                }
                break;

            case State.Dead:
                mNumDeath++;
                mIsGoal = false;
                break;

            case State.Victory:
                RemoveInput();

                //save level complete info
                LevelManager.instance.LevelComplete(mCurTime, mHUD.starsFilled, mNumDeath);

                mAnim.Play("exit");

                mTimerActive = false;
                break;
        }
    }

    protected override void OnDespawned() {
        //reset stuff here

        base.OnDespawned();
    }

    protected override void OnDestroy() {
        if(mInstance == this)
            mInstance = null;

        readyCallback = null;

        //dealloc here

        RemoveInput();

        base.OnDestroy();
    }

    public override void Release() {
        state = (int)State.Invalid;

        base.Release();
    }

    public override void SpawnFinish() {
        SetCheckpoint(mCtrl.body.transform.position, mCtrl.body.gravityController.up);

        //start ai, player control, etc
        state = (int)State.Normal;

        if(readyCallback != null)
            readyCallback(this);
    }

    protected override void SpawnStart() {
        //initialize some things
        mAnim.Play("spawn");
    }

    protected override void Awake() {
        if(mInstance == null) {
            mInstance = this;

            mGameOverFX = Camera.main.GetComponent<M8.ImageEffects.WaveRGB>();

            mCtrl = GetComponent<PlayerController>();

            mHUD = HUD.GetHUD();

            mAnim = GetComponent<AnimatorData>();

            base.Awake();

            //initialize variables
            autoSpawnFinish = false;
        }
        else
            DestroyImmediate(gameObject);
    }

    // Use this for initialization
    protected override void Start() {
        base.Start();

        //initialize variables from other sources (for communicating with managers, etc.)
        Main.instance.input.AddButtonCall(0, InputAction.MenuEscape, OnInputMenu);
    }

    void SetCheckpoint(Vector3 pos, Vector3 up) {
        mCheckPointPos = pos;
        mCheckPointRot = mCtrl.body.transform.rotation;
        mCheckPointUp = up;
        mCheckPointGravField = mCtrl.body.gravityController.gravityField;

        //clear out undos
        mLastTriggers.Clear();
        mLastStarsCollected.Clear();
    }

    void ApplyCheckpoint() {
        mCtrl.body.transform.position = mCheckPointPos;
        mCtrl.body.transform.rotation = mCheckPointRot;
        mCtrl.body.gravityController.up = mCheckPointUp;
        mCtrl.body.rigidbody.velocity = Vector3.zero;

        if(mCheckPointGravField)
            mCheckPointGravField.Add(mCtrl.body.gravityController);

        //reset stars collected
        foreach(AnimatorData anim in mLastStarsCollected) {
            anim.collider.enabled = true;
            anim.PlayDefault();
        }

        mHUD.StarFillTo(mHUD.starsFilled - mLastStarsCollected.Count);

        mLastStarsCollected.Clear();

        //reset triggers
        foreach(TriggerCheckpoint trigger in mLastTriggers) {
            trigger.Revert();
        }

        mLastTriggers.Clear();
    }

    void RemoveInput() {
        if(Main.instance && Main.instance.input)
            Main.instance.input.RemoveButtonCall(0, InputAction.MenuEscape, OnInputMenu);
    }

    void OnInputMenu(InputManager.Info dat) {
        if(dat.state == InputManager.State.Pressed) {
            if(!UIModalManager.instance.ModalIsInStack("pause")) {
                UIModalManager.instance.ModalOpen("pause");
            }
        }
    }

    IEnumerator GameOverDelay() {
        WaitForFixedUpdate wait = new WaitForFixedUpdate();

        SoundPlayerGlobal.instance.Play("explode");

        if(mGameOverFX)
            mGameOverFX.enabled = true;

        Vector2 ampR = new Vector2(0.33f, 0f);
        Vector2 ampG = new Vector2(1f, 0f);
        Vector2 rgR = new Vector2(0f, 0.05f);
        Vector2 rgG = new Vector2(0f, 0.02f);

        mGameOverFX.amplitudeR = Vector2.zero;
        mGameOverFX.amplitudeG = Vector2.zero;
        mGameOverFX.rangeR = Vector2.zero;
        mGameOverFX.rangeG = Vector2.zero;

        float delay = 0.5f;
        float t = 0.0f;

        while(t < delay) {
            yield return wait;

            t += Time.fixedDeltaTime;
            if(t > delay) t = delay;

            mGameOverFX.amplitudeR = Vector2.Lerp(Vector2.zero, ampR, t / delay);
            mGameOverFX.amplitudeG = Vector2.Lerp(Vector2.zero, ampG, t / delay);
            mGameOverFX.rangeR = Vector2.Lerp(Vector2.zero, rgR, t / delay);
            mGameOverFX.rangeG = Vector2.Lerp(Vector2.zero, rgG, t / delay);
        }

        ApplyCheckpoint();

        state = (int)State.Normal;

        yield return new WaitForSeconds(delay);

        t = 0.0f;

        while(t < delay) {
            yield return wait;

            t += Time.fixedDeltaTime;
            if(t > delay) t = delay;

            mGameOverFX.amplitudeR = Vector2.Lerp(ampR, Vector2.zero, t / delay);
            mGameOverFX.amplitudeG = Vector2.Lerp(ampG, Vector2.zero, t / delay);
            mGameOverFX.rangeR = Vector2.Lerp(rgR, Vector2.zero, t / delay);
            mGameOverFX.rangeG = Vector2.Lerp(rgG, Vector2.zero, t / delay);
        }

        if(mGameOverFX) {
            mGameOverFX.enabled = false;

            mGameOverFX.amplitudeR = ampR;
            mGameOverFX.amplitudeG = ampG;
            mGameOverFX.rangeR = rgR;
            mGameOverFX.rangeG = rgG;
        }

        //Main.instance.sceneManager.LoadScene(gameoverScene);
    }

    IEnumerator Timer() {
        mCurTime = 0.0f;

        WaitForFixedUpdate wait = new WaitForFixedUpdate();

        while(mTimerActive) {
            yield return wait;

            mCurTime += Time.fixedDeltaTime;

            mHUD.RefreshTimer(mCurTime);
        }
    }
}
