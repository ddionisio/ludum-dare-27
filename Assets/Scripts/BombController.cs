using UnityEngine;
using System.Collections;

public class BombController : MonoBehaviour {
    public delegate void Callback(BombController ctrl);

    public float deathDelay = 10.0f;

    public event Callback deathCallback;

    private float mCurDelay;

    private HUD mHUD;
    private GameObject mExitGO;

    public float curDelay {
        get { return mCurDelay; }
        set {
            mCurDelay = Mathf.Clamp(value, 0.0f, deathDelay);
        }
    }

    public void Init() {
        mCurDelay = deathDelay;
    }

    public void Activate() {
        mCurDelay = deathDelay;

        if(mHUD) {
            mHUD.bombTimerAttach.gameObject.SetActive(true);
        }
    }

    void OnDisable() {
        if(mHUD) {
            mHUD.bombTimerAttach.gameObject.SetActive(false);
            mHUD.bombOffScreen.gameObject.SetActive(false);
            mHUD.bombOffScreenExit.gameObject.SetActive(false);
        }
    }

    void OnDestroy() {
        deathCallback = null;
    }

    void Awake() {
        mHUD = HUD.GetHUD();

        mExitGO = GameObject.FindGameObjectWithTag("Exit");

        mHUD.bombOffScreen.SetPOI(transform);
        mHUD.bombOffScreenExit.SetPOI(mExitGO.transform);
    }

    // Use this for initialization
    void Start() {

    }

    void Update() {
        UILabel countLabel = mHUD.bombTimerCountLabel;

        if(Player.instance.isGoal) {
            mHUD.bombTimerAttach.target = mExitGO.transform;

            mHUD.bombOffScreen.gameObject.SetActive(false);
            mHUD.bombOffScreenExit.gameObject.SetActive(true);

            if(mHUD.bombOffScreenExit.pointer.gameObject.activeSelf)
                countLabel = mHUD.bombOffScreenExitLabel;
        }
        else {
            mHUD.bombTimerAttach.target = transform;

            mHUD.bombOffScreen.gameObject.SetActive(true);
            mHUD.bombOffScreenExit.gameObject.SetActive(false);

            if(mHUD.bombOffScreen.pointer.gameObject.activeSelf)
                countLabel = mHUD.bombOffScreenLabel;
        }

        countLabel.text = Mathf.CeilToInt(mCurDelay).ToString();

        if(mCurDelay > 0.0f) {
            mCurDelay = Mathf.Clamp(mCurDelay - Time.deltaTime*0.7f, 0.0f, deathDelay);
                        
            if(mCurDelay <= 0.0f && deathCallback != null)
                deathCallback(this);
        }
    }
}
