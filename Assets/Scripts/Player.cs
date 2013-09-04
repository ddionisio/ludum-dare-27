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

    public string gameoverScene = "gameover";

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

    private AnimatorData mAnim;

    private M8.ImageEffects.WaveRGB mGameOverFX;

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
        UIModalManager.instance.ModalOpen("levelComplete");
    }

    public void SetCheckPoint(Checkpoint c) {
        if(mCheckPointLast)
            mCheckPointLast.ResetState();

        mCheckPointLast = c;
        SetCheckpoint(c.point.position);
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
                if(!mTimerActive) {
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

        //dealloc here

        RemoveInput();

        base.OnDestroy();
    }

    public override void Release() {
        state = (int)State.Invalid;

        base.Release();
    }

    public override void SpawnFinish() {
        SetCheckpoint(mCtrl.body.transform.position);

        //start ai, player control, etc
        state = (int)State.Normal;
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

    void SetCheckpoint(Vector3 pos) {
        mCheckPointPos = pos;
        mCheckPointRot = mCtrl.body.transform.rotation;
        mCheckPointUp = mCtrl.body.gravityController.up;
    }

    void ApplyCheckpoint() {
        mCtrl.body.transform.position = mCheckPointPos;
        mCtrl.body.transform.rotation = mCheckPointRot;
        mCtrl.body.gravityController.up = mCheckPointUp;
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

        Vector2 ampR = mGameOverFX.amplitudeR;
        Vector2 ampG = mGameOverFX.amplitudeG;
        Vector2 rgR = mGameOverFX.rangeR;
        Vector2 rgG = mGameOverFX.rangeG;

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
