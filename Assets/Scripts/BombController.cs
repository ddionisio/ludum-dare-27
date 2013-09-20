﻿using UnityEngine;
using System.Collections;

public class BombController : MonoBehaviour {
    public delegate void Callback(BombController ctrl);

    public float deathDelay = 10.0f;

    public tk2dSpriteAnimator anim;

    public GameObject highlightGO;

    public event Callback deathCallback;

    private float mCurDelay;

    private HUD mHUD;
    private GameObject mExitGO;
    private bool mTimerActive = false;
    private bool mConsumed = false;
    private float mRadius;

    public float radius { get { return mRadius; } }

    public bool isConsumed { get { return mConsumed; } }

    public float curDelay {
        get { return mCurDelay; }
        set {
            mCurDelay = Mathf.Clamp(value, 0.0f, deathDelay);
        }
    }

    public void Init() {
        mConsumed = false;
        mTimerActive = false;
        mCurDelay = deathDelay;
        collider.enabled = true;
        anim.gameObject.SetActive(true);

        mCurDelay = deathDelay;
    }

    /// <summary>
    /// Use for goal
    /// </summary>
    public void Consume() {
        mConsumed = true;
        mTimerActive = true;
        collider.enabled = false;
        anim.gameObject.SetActive(false);

        mCurDelay = deathDelay;
    }

    public void Activate() {
        mTimerActive = true;

        if(mHUD) {
            mHUD.bombTimerAttach.gameObject.SetActive(true);
        }
    }

    public void StopTimer() {
        mTimerActive = false;

        mHUD.bombTimerAttach.gameObject.SetActive(false);
        mHUD.bombOffScreen.gameObject.SetActive(false);
        mHUD.bombOffScreenExit.gameObject.SetActive(false);
    }

    void OnTriggerEnter(Collider col) {
        GameObject go = col.gameObject;

        if(go.CompareTag("Star")) {
            Player.instance.CollectStar(col);
        }
        else if(go.CompareTag("Enemy")) {
            //only hit enemy if player is not hurt
            Player player = Player.instance;
            if(player.state != (int)Player.State.Hurt) {
                Enemy enemy = M8.Util.GetComponentUpwards<Enemy>(go.transform, true);
                if(enemy && enemy.FSM)
                    enemy.FSM.SendEvent(EntityEvent.Hit);
            }

            Vector3 n = (transform.position - go.transform.position).normalized;
            rigidbody.velocity = Vector3.Reflect(rigidbody.velocity, n);
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

        mTimerActive = true;

        mRadius = (collider as SphereCollider).radius;
    }

    // Use this for initialization
    void Start() {

    }

    void Update() {
        if(Player.instance.isGoal) {
            mHUD.bombTimerAttach.target = mExitGO.transform;

            mHUD.bombOffScreen.gameObject.SetActive(false);
            mHUD.bombOffScreenExit.gameObject.SetActive(true);
        }
        else {
            mHUD.bombTimerAttach.target = transform;

            mHUD.bombOffScreen.gameObject.SetActive(true);
            mHUD.bombOffScreenExit.gameObject.SetActive(false);
        }

        if(mTimerActive) {
            mHUD.UpdateTicker(mCurDelay, deathDelay);

            if(mCurDelay > 0.0f) {
                mCurDelay = Mathf.Clamp(mCurDelay - Time.deltaTime, 0.0f, deathDelay);

                if(mCurDelay <= 0.0f && deathCallback != null)
                    deathCallback(this);
            }
        }
    }

    void OnProjectileHit(Projectile.HitInfo info) {
        Vector2 dir = (transform.position - info.projectile.transform.position).normalized;
        rigidbody.velocity = Vector3.Reflect(rigidbody.velocity, dir);
    }
}
