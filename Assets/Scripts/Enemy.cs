using UnityEngine;
using System.Collections;

public class Enemy : EntityBase {
    public enum State {
        Invalid = -1,
        Normal,
        Hurt,
        Dead,
        Reviving
    }

    public enum BodySpriteState {
        Invalid = -1,
        Idle,
        Move,
    }

    public Collider bodyCollider;
    public bool facePlayer;
    public tk2dSpriteAnimator bodySpriteAnim;
    public tk2dBaseSprite bodySprite;

    public string bodyClipIdle = "idle";
    public string bodyClipMove = "move";
    public float bodyClipMoveThreshold = 0.015f;

    private int mBodySpriteState = (int)BodySpriteState.Invalid;
    private float mBodySpriteHorizontal = 0.0f;

    private Transform mPlayerTrans;
    private Transform mBodyTrans;

    private tk2dSpriteAnimationClip mBodyClipIdle;
    private tk2dSpriteAnimationClip mBodyClipMove;

    public float bodySpriteHorizontal {
        get { return mBodySpriteHorizontal; }
        set {
            float delta = value - mBodySpriteHorizontal;
            if(delta != 0) {
                mBodySpriteHorizontal = value;

                if(bodySprite) {
                    bodySprite.FlipX = delta < 0.0f;
                }
            }
        }
    }

    public int bodySpriteState {
        get { return mBodySpriteState; }
        set {
            if(mBodySpriteState != value) {
                mBodySpriteState = value;

                if(!Application.isPlaying)
                    return;

                switch((BodySpriteState)mBodySpriteState) {
                    case BodySpriteState.Idle:
                        if(bodySpriteAnim && mBodyClipIdle != null) {
                            bodySpriteAnim.Play(mBodyClipIdle);
                        }
                        break;

                    case BodySpriteState.Move:
                        if(bodySpriteAnim && mBodyClipMove != null) {
                            bodySpriteAnim.Play(mBodyClipMove);
                        }
                        break;
                }
            }
        }
    }

    protected override void StateChanged() {
        switch((State)state) {
            case State.Dead:
            case State.Reviving:
                if(bodyCollider)
                    bodyCollider.rigidbody.detectCollisions = false;
                //bodyCollider.enabled = false;

                mBodySpriteState = (int)BodySpriteState.Invalid;
                break;

            case State.Invalid:
                mBodySpriteState = (int)BodySpriteState.Invalid;
                break;

            default:
                if(bodyCollider)
                    bodyCollider.rigidbody.detectCollisions = true;
                //bodyCollider.enabled = true;
                break;
        }
    }

    public override void Release() {
        state = StateInvalid;

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

    public override void SpawnFinish() {
        //start ai, player control, etc
        state = (int)State.Normal;
    }

    protected override void SpawnStart() {
        //initialize some things
        if(facePlayer) {
            mPlayerTrans = Player.instance.controller.body.transform;
        }
    }

    protected override void Awake() {
        base.Awake();

        if(bodySpriteAnim != null) {
            mBodyClipIdle = string.IsNullOrEmpty(bodyClipIdle) ? null : bodySpriteAnim.GetClipByName(bodyClipIdle);
            mBodyClipMove = string.IsNullOrEmpty(bodyClipMove) ? null : bodySpriteAnim.GetClipByName(bodyClipMove);
        }

        if(bodySprite == null && bodySpriteAnim != null)
            bodySprite = bodySpriteAnim.Sprite;

        if(bodyCollider)
            mBodyTrans = bodyCollider.transform;
    }

    // Use this for initialization
    protected override void Start() {
        base.Start();

        //initialize variables from other sources (for communicating with managers, etc.)
    }

    void Update() {
        switch((State)state) {
            case State.Normal:
                if(facePlayer && bodySprite && mBodyTrans) {
                    Vector3 dir = mPlayerTrans.transform.position - mBodyTrans.position;
                    dir = mBodyTrans.worldToLocalMatrix.MultiplyVector(dir);
                    bodySprite.FlipX = dir.x < 0.0f;
                }
                break;
        }
    }
}
