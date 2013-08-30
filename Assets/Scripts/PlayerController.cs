using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {
    public Transform attachPoint;
    public tk2dSpriteAnimator attachSpriteAnim;
    public AnimatorData attachAnimator;

    public GameObject bomb;
    //public float bombTimeRegen = 3.0f;

    public float throwAngle = 30;
    public float throwImpulse = 30;

    public float dropAngle = 60;
    public float dropImpulse = -20;

    public float hurtForce = 30.0f;
    public float hurtForceDelay;
    public float hurtInvulDelay = 2.0f;

    public AnimatorData doubleJumpAnim;

    public SpriteColorBlink[] spriteBlinks;

    public LayerMask bombCollisionCheckMask;

    private Player mPlayer;
    private PlatformerController mBody;
    private PlatformerSpriteController mBodySpriteCtrl;
    private BombController mBombCtrl;

    private HUD mHUD;

    private GameObject mTargetGO; //goal

    private bool mInputEnabled = false;

    public bool inputEnabled {
        get { return mInputEnabled; }
        set {
            if(mInputEnabled != value) {
                mInputEnabled = value;

                mBody.inputEnabled = mInputEnabled;

                InputManager input = Main.instance ? Main.instance.input : null;
                if(input) {
                    if(mInputEnabled) {
                        input.AddButtonCall(0, InputAction.Action, OnInputAction);
                    }
                    else {
                        input.RemoveButtonCall(0, InputAction.Action, OnInputAction);
                    }
                }
            }
        }
    }

    public PlatformerController body { get { return mBody; } }

    public bool hasAttach { get { return attachPoint.gameObject.activeSelf; } }

    public Player player { get { return mPlayer; } }

    bool CheckBombCollideAt(Vector3 pos) {
        return Physics.CheckSphere(pos, (bomb.collider as SphereCollider).radius, bombCollisionCheckMask);
    }

    void DoThrow(Vector3 pos, float impulse, float angle) {
        attachPoint.gameObject.SetActive(false);
        mBody.ResetCollision();

        attachSpriteAnim.Play("empty");

        bomb.transform.position = pos;
        bomb.transform.rotation = attachPoint.rotation;
        bomb.rigidbody.angularVelocity = Vector3.zero;

        bomb.rigidbody.velocity = mBody.rigidbody.velocity;

        bomb.SetActive(true);
        mBombCtrl.Activate();

        if(impulse != 0.0f) {
            Vector3 dir = mBodySpriteCtrl.isLeft ? -mBody.dirHolder.right : mBody.dirHolder.right;

            Quaternion rot = Quaternion.AngleAxis(angle, mBodySpriteCtrl.isLeft ? -Vector3.forward : Vector3.forward);

            dir = rot * dir;

            bomb.rigidbody.AddForce(dir * impulse, ForceMode.Impulse);
        }

        StopCoroutine("DoBombCorrection");
        StartCoroutine(DoBombCorrection(mBody.gravityController.up));

        tk2dBaseSprite bombSpr = bomb.GetComponentInChildren<tk2dBaseSprite>();
        if(bombSpr)
            bombSpr.FlipX = mBodySpriteCtrl.isLeft;

        mHUD.targetOffScreen.gameObject.SetActive(false);
    }

    IEnumerator DoBombCorrection(Vector3 up) {
        yield return new WaitForFixedUpdate();

        if(bomb) {
            GravityController bombGrav = bomb.GetComponent<GravityController>();
            bombGrav.up = up;
        }
    }

    public void ThrowAttach() {
        if(!CheckBombCollideAt(attachPoint.position))
            DoThrow(attachPoint.position, throwImpulse, throwAngle);
        else
            attachAnimator.Stop();
    }

    public void DropAttach() {
        if(hasAttach) {
            Vector3 lpos = mBody.transform.worldToLocalMatrix.MultiplyPoint(attachPoint.position);
            Vector3 pos = attachPoint.position;
            Matrix4x4 mtx = mBody.transform.localToWorldMatrix;

            for(int i = 0; i < 4; i++) {
                if(!CheckBombCollideAt(pos))
                    break;

                Vector2 p2 = M8.MathUtil.Rotate(lpos, Mathf.PI * 0.5f);
                lpos.x = p2.x; lpos.y = p2.y;
                pos = mtx.MultiplyPoint(lpos);

                dropAngle += 90.0f;
            }

            DoThrow(pos, dropImpulse, dropAngle);
        }
    }

    public void BombActive() {
        if(attachPoint)
            attachPoint.gameObject.SetActive(true);

        if(mBody)
            mBody.ResetCollision();

        if(bomb)
            bomb.SetActive(false);

        if(attachSpriteAnim)
            attachSpriteAnim.Play("bomb");


        mHUD.targetOffScreen.gameObject.SetActive(true);
    }

    public bool Hurt(Vector3 dir, bool forceBounce) {
        if(!mPlayer.isBlinking && mPlayer.state != (int)Player.State.Hurt) {
            mPlayer.state = (int)Player.State.Hurt;

            SoundPlayerGlobal.instance.Play("hurt");

            StopCoroutine("DoHurtForce");
            StartCoroutine(DoHurtForce(dir));

            return true;
        }
        else if(forceBounce) {
            StopCoroutine("DoHurtForce");
            StartCoroutine(DoHurtForce(dir));
        }

        return false;
    }

    IEnumerator DoHurtForce(Vector3 dir) {
        mBody.ResetCollision();
        mBody.lockDrag = true;
        mBody.rigidbody.drag = 0.0f;

        WaitForFixedUpdate wait = new WaitForFixedUpdate();
        float t = 0.0f;

        while(t < hurtForceDelay) {
            yield return wait;

            mBody.rigidbody.AddForce(dir * hurtForce);

            t += Time.fixedDeltaTime;
        }

        mBody.ResetCollision();
    }

    void ResetData() {
        StopCoroutine("DoHurtForce");
        StopCoroutine("DoBombCorrection");

        if(attachPoint)
            attachPoint.gameObject.SetActive(true);

        foreach(SpriteColorBlink blink in spriteBlinks) {
            if(blink)
                blink.enabled = false;
        }

        if(attachAnimator) {
            attachAnimator.transform.localScale = Vector3.one;
            attachAnimator.Stop();
        }

        if(mBody)
            mBody.ResetCollision();

        if(mBodySpriteCtrl)
            mBodySpriteCtrl.ResetAnimation();

        if(attachSpriteAnim)
            attachSpriteAnim.Sprite.FlipX = false;

        if(mBombCtrl)
            mBombCtrl.Init();

        if(mHUD) {
            if(mHUD.targetOffScreen)
                mHUD.targetOffScreen.gameObject.SetActive(false);
        }

        doubleJumpAnim.Stop();

    }

    void OnDestroy() {
        inputEnabled = false;
    }

    void Awake() {
        mPlayer = GetComponent<Player>();
        mPlayer.spawnCallback += OnPlayerSpawn;
        mPlayer.setStateCallback += OnPlayerSetState;
        mPlayer.setBlinkCallback += OnPlayerBlink;

        mBody = GetComponentInChildren<PlatformerController>();

        mBody.player = 0;
        mBody.moveInputX = InputAction.MoveX;
        mBody.moveInputY = InputAction.MoveY;
        mBody.jumpInput = InputAction.Jump;

        mBody.jumpCallback += OnBodyJump;
        mBody.collisionStayCallback += OnBodyCollisionStay;

        mBodySpriteCtrl = mBody.GetComponent<PlatformerSpriteController>();
        mBodySpriteCtrl.flipCallback += OnFlipCallback;
        mBodySpriteCtrl.anim.AnimationCompleted += OnBodySpriteAnimFinish;

        mBombCtrl = bomb.GetComponent<BombController>();
        mBombCtrl.deathCallback += OnBombDeathCallback;

        mTargetGO = GameObject.FindGameObjectWithTag("Goal");

        mHUD = HUD.GetHUD();
        mHUD.targetOffScreen.SetPOI(mTargetGO.transform);

        ResetData();
    }

    // Use this for initialization
    void Start() {

    }

    // Update is called once per frame
    void Update() {
        /*if(hasAttach && mBombCtrl.curDelay < mBombCtrl.deathDelay) {
            mBombCtrl.curDelay += bombTimeRegen * Time.deltaTime;
        }*/

        if(mPlayer.isGoal) {
            if(mHUD.targetOffScreen.gameObject.activeSelf)
                mHUD.targetOffScreen.gameObject.SetActive(false);
        }
    }

    void OnInputAction(InputManager.Info dat) {
        if(dat.state == InputManager.State.Pressed) {
            if(hasAttach && !attachAnimator.isPlaying && !CheckBombCollideAt(attachPoint.position)) {
                attachAnimator.Play(mBodySpriteCtrl.isLeft ? "throwLeft" : "throw");
            }
        }
    }

    void OnPlayerSpawn(EntityBase ent) {
        BombActive();
    }

    void OnPlayerSetState(EntityBase ent, int state) {
        switch((Player.State)state) {
            case Player.State.Normal:
                mBodySpriteCtrl.animationActive = true;
                inputEnabled = true;
                break;

            case Player.State.Hurt:
                attachAnimator.Stop();

                mBodySpriteCtrl.animationActive = false;
                inputEnabled = false;

                mBodySpriteCtrl.anim.Play("hurt");

                mPlayer.Blink(hurtInvulDelay);

                DropAttach();
                break;

            case Player.State.Victory:
            case Player.State.Dead:
                //TODO: animation?

                attachAnimator.Stop();

                mBodySpriteCtrl.animationActive = false;
                inputEnabled = false;
                break;

            case Player.State.Invalid:
                inputEnabled = false;

                ResetData();
                break;
        }
    }

    void OnPlayerBlink(EntityBase ent, bool b) {
        foreach(SpriteColorBlink blink in spriteBlinks)
            blink.enabled = b;

        if(!b) {
            mPlayer.state = (int)Player.State.Normal;
        }
    }

    void OnFlipCallback(PlatformerSpriteController ctrl) {
        attachSpriteAnim.Sprite.FlipX = ctrl.isLeft;

        if(attachAnimator.isPlaying) {
            float t = attachAnimator.currentPlayingTake.sequence.elapsed;
            attachAnimator.PlayAtTime(mBodySpriteCtrl.isLeft ? "throwLeft" : "throw", t);
        }


        Transform attach = attachAnimator.transform;

        Vector3 s = attach.localScale;

        s.x = ctrl.isLeft ? -Mathf.Abs(s.x) : Mathf.Abs(s.x);

        attach.localScale = s;
    }

    void OnBodyCollisionStay(RigidBodyController controller, Collision col) {
        //Debug.Log("hi");
        foreach(ContactPoint cp in col.contacts) {
            if(cp.otherCollider.gameObject.tag == "Harm") {
                Hurt(cp.normal, false);
            }
            else if(cp.otherCollider.gameObject == bomb) {
                //pick up bomb again
                //can't if we are hurt, bomb dropped at goal, currently animating for some reason.
                if(!mPlayer.isGoal && (mBodySpriteCtrl.anim.CurrentClip == null || mBodySpriteCtrl.anim.CurrentClip.name != "hurt") && !attachAnimator.isPlaying)
                    BombActive();
            }
        }
    }

    void OnBodySpriteAnimFinish(tk2dSpriteAnimator anim, tk2dSpriteAnimationClip clip) {
        if(clip.name == "hurt") {
            mBodySpriteCtrl.animationActive = true;
            mBody.inputEnabled = true;
        }
    }

    void OnBombDeathCallback(BombController ctrl) {
        mPlayer.state = (int)Player.State.Dead;
        mPlayer.GameOver();
    }

    void OnUIModalActive() {
        inputEnabled = false;
    }

    void OnUIModalInactive() {
        if(mPlayer.state == (int)Player.State.Normal) {
            inputEnabled = true;
        }
    }

    void OnBodyJump(PlatformerController ctrl) {
        SoundPlayerGlobal.instance.Play("jump");

        if(mBody.jumpCounterCurrent > 1 && !mBody.isJumpWall && !doubleJumpAnim.isPlaying) {
            doubleJumpAnim.transform.rotation = mBody.transform.rotation;
            doubleJumpAnim.transform.position = mBody.transform.position;// -mBody.transform.up * mBody.collider.bounds.extents.y;
            doubleJumpAnim.Play("boost");
        }
    }
}
