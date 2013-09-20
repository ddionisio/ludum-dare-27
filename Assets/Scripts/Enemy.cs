using UnityEngine;
using System.Collections;

public class Enemy : EntityBase {
    public enum State {
        Invalid = -1,
        Normal,
        Hurt,
        Dead,
        Reviving,
        Attack
    }

    public enum BodySpriteState {
        idle,
        move
    }

    public Collider bodyCollider;
    public bool facePlayer;
    public tk2dSpriteAnimator bodySpriteAnim;
    public tk2dBaseSprite bodySprite;
    public Transform mover;

    public bool attackUseNormalUpdate = false;

    public EnemyShootController shoot;
    public float shootCooldown = 1.0f;

    public float bodyClipMoveThreshold = 0.015f;

    private BodySpriteState mBodySpriteState = BodySpriteState.idle;

    private Transform mPlayerTrans;
    private Transform mBodyTrans;

    private tk2dSpriteAnimationClip[] mBodySpriteClips;
    private bool mNormalUpdateActive;

    public Transform playerTransform { get { return mPlayerTrans; } }

    public void SetCollisionActive(bool yes) {
        if(bodyCollider) {
            bodyCollider.enabled = yes;

            if(bodyCollider.rigidbody)
                bodyCollider.rigidbody.detectCollisions = yes;
        }
    }

    public BodySpriteState bodySpriteState {
        get { return mBodySpriteState; }
        set {
            //Debug.Log("f: " + value);

            mBodySpriteState = value;

            if(!Application.isPlaying || bodySpriteAnim == null)
                return;

            if(mBodySpriteClips[(int)mBodySpriteState] != null)
                bodySpriteAnim.Play(mBodySpriteClips[(int)mBodySpriteState]);
        }
    }

    void ApplyState() {
        switch((State)state) {
            case State.Attack:
                SetCollisionActive(true);

                if(attackUseNormalUpdate && !mNormalUpdateActive) {
                    StartCoroutine(DoNormalMoverUpdate());
                }
                break;

            case State.Normal:
                SetCollisionActive(true);

                if(!mNormalUpdateActive)
                    StartCoroutine(DoNormalMoverUpdate());
                break;

            case State.Dead:
            case State.Reviving:
                SetCollisionActive(false);
                break;

            case State.Invalid:
                break;
        }
    }

    protected override void StateChanged() {
        switch((State)prevState) {
            case State.Normal:
                if(shoot)
                    shoot.shootEnable = false;
                break;
        }

        ApplyState();
    }

    public override void Release() {
        state = StateInvalid;
        mNormalUpdateActive = false;

        base.Release();
    }

    protected override void OnDespawned() {
        //reset stuff here

        base.OnDespawned();
    }

    protected override void OnDestroy() {
        //dealloc here

        base.OnDestroy();
    }

    protected override void ActivatorWakeUp() {
        base.ActivatorWakeUp();

        ApplyState();
    }

    protected override void ActivatorSleep() {
        base.ActivatorSleep();

        mNormalUpdateActive = false;
    }

    public override void SpawnFinish() {
        //start ai, player control, etc
        state = (int)State.Normal;
    }

    protected override void SpawnStart() {
        //initialize some things
        mPlayerTrans = Player.instance.controller.body.transform;
    }

    protected override void Awake() {
        base.Awake();

        if(bodySpriteAnim != null) {
            mBodySpriteClips = M8.tk2dUtil.GetSpriteClips(bodySpriteAnim, typeof(BodySpriteState));
        }

        if(bodySprite == null && bodySpriteAnim != null)
            bodySprite = bodySpriteAnim.Sprite;

        if(bodyCollider)
            mBodyTrans = bodyCollider.transform;

        if(shoot)
            shoot.shootCallback += OnShoot;
    }

    // Use this for initialization
    protected override void Start() {
        base.Start();

        //initialize variables from other sources (for communicating with managers, etc.)
    }

    void OnShoot(EnemyShootController ctrl) {
        state = (int)State.Attack;
    }

    IEnumerator DoNormalMoverUpdate() {
        mNormalUpdateActive = true;

        WaitForSeconds waitDelay = new WaitForSeconds(0.1f);

        Vector3 lastMoverPos = mover ? mover.position : transform.position;

        float lastShootTime = Time.fixedTime;

        while((State)state == State.Normal || ((State)state == State.Attack && attackUseNormalUpdate)) {
            if(shoot && !shoot.shootEnable) {
                if(Time.fixedTime - lastShootTime >= shootCooldown) {
                    lastShootTime = Time.fixedTime;
                    shoot.shootEnable = true;
                }
            }

            Vector3 moverPos = mover ? mover.position : transform.position;
            Vector3 delta;

            if(lastMoverPos != moverPos) {
                delta = moverPos - lastMoverPos;

                delta = mBodyTrans.worldToLocalMatrix.MultiplyVector(delta);
            }
            else
                delta = Vector3.zero;

            lastMoverPos = moverPos;

            //determine animation
            if(bodySpriteAnim) {
                if(Mathf.Abs(delta.x) < bodyClipMoveThreshold) {
                    bodySpriteState = BodySpriteState.idle;
                }
                else {
                    bodySpriteState = BodySpriteState.move;
                }
            }

            //determine facing
            if(bodySprite) {
                if(facePlayer) {
                    Vector3 dir = mPlayerTrans.transform.position - mBodyTrans.position;
                    dir = mBodyTrans.worldToLocalMatrix.MultiplyVector(dir);

                    if(dir.x != 0.0f) {
                        bodySprite.FlipX = dir.x < 0.0f;

                        if(shoot)
                            shoot.visionDir.x = dir.x < 0.0f ? -Mathf.Abs(shoot.visionDir.x) : Mathf.Abs(shoot.visionDir.x);
                    }
                }
                else if(delta.x != 0.0f) {
                    bodySprite.FlipX = delta.x < 0.0f;

                    if(shoot)
                        shoot.visionDir.x = delta.x < 0.0f ? -Mathf.Abs(shoot.visionDir.x) : Mathf.Abs(shoot.visionDir.x);
                }
            }

            yield return waitDelay;
        }

        mNormalUpdateActive = false;
    }
}
