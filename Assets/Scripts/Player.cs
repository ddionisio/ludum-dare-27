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

    private float mCurTime = 0;
    private bool mTimerActive = false;

    private HUD mHUD;

    private M8.ImageEffects.WaveRGB mGameOverFX;

    public static Player instance { get { return mInstance; } }

    public PlayerController controller { get { return mCtrl; } }

    public float curTime { get { return mCurTime; } }

    public bool isGoal { get { return mIsGoal; } set { mIsGoal = value; } }

    public HUD HUD { get { return mHUD; } }
            
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

    public void GameOver() {
        StartCoroutine(GameOverDelay());
    }

    public void ExitToScene(string scene) {
        Main.instance.sceneManager.LoadScene(scene);
        //start the exit animation
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
        if(mInstance == null) {
            mInstance = this;

            mGameOverFX = Camera.main.GetComponent<M8.ImageEffects.WaveRGB>();

            mCtrl = GetComponent<PlayerController>();

            mHUD = HUD.GetHUD();

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
        Main.instance.input.AddButtonCall(0, InputAction.MenuEscape, OnInputMenu);
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
        SoundPlayerGlobal.instance.Play("explode");

        if(mGameOverFX)
            mGameOverFX.enabled = true;

        yield return new WaitForSeconds(2.0f);

        Main.instance.sceneManager.LoadScene(gameoverScene);
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
