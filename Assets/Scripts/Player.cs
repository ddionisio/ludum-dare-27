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
    private string mExitScene;

    private M8.ImageEffects.WaveRGB mGameOverFX;

    public static Player instance { get { return mInstance; } }

    public PlayerController controller { get { return mCtrl; } }

    public bool isGoal { get { return mIsGoal; } set { mIsGoal = value; } }

    protected override void StateChanged() {
        switch((State)state) {
            case State.Invalid:
                if(mGameOverFX)
                    mGameOverFX.enabled = false;
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
        mExitScene = scene;
        Main.instance.sceneManager.LoadScene(scene);
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

            mGameOverFX = Camera.main.GetComponent<M8.ImageEffects.WaveRGB>();

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
}
