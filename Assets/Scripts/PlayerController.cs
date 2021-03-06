﻿using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {
    public Transform attachPoint;
    public Transform throwPoint;
    public tk2dSpriteAnimator attachSpriteAnim;
    public AnimatorData attachAnimator;

    public GameObject bomb;
    public BombGrabber bombGrabber;
    //public float bombTimeRegen = 3.0f;

    public float throwAngle = 30;
    public float throwSpeed = 5;

    public float dropAngle = -45;
    public float dropSpeed = -5;

    public float hurtForce = 30.0f;
    public float hurtForceDelay;
    public float hurtInvulDelay = 2.0f;

    public float enemyJumpSpeed = 10.0f;

    public AnimatorData doubleJumpAnim;

    public SpriteColorBlink[] spriteBlinks;

    public LayerMask bombCollisionCheckMask;
    //public LayerMask bodyPenetrateCheckMask;
    //public float bodyPenetrateOfs;

    public float lookDelay = 1.0f;
    public float lookOfs = 64.0f / 24.0f;

    private enum ThrowMode {
        None,
        Up,
        Down
    }

    private const string attachSpriteClipEmpty = "empty";
    private const string attachSpriteClipBomb = "bomb";

    private Player mPlayer;
    private PlatformerController mBody;
    private PlatformerSpriteController mBodySpriteCtrl;
    private BombController mBombCtrl;

    private GameObject mTargetGO; //goal

    private ThrowMode mThrowMode = ThrowMode.None;

    private bool mInputEnabled = false;

    private Vector3 mHurtNormal;
    private float mHurtForceDelay;

    private float mLastMoveVerticalTime;
    private float mLastMoveVertical;
    private bool mVerticalMoveActive;

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
                        input.AddButtonCall(0, InputAction.Special, OnInputSpecial);
                    }
                    else {
                        input.RemoveButtonCall(0, InputAction.Action, OnInputAction);
                        input.RemoveButtonCall(0, InputAction.Special, OnInputSpecial);
                    }
                }
            }
        }
    }

    public PlatformerController body { get { return mBody; } }

    public bool hasAttach { get { return mPlayer.bombEnabled && !bomb.gameObject.activeSelf; } }

    public BombController bombCtrl { get { return mBombCtrl; } }

    public Player player { get { return mPlayer; } }

    bool CheckBombCollideAt(Vector3 pos) {
        return Physics.CheckSphere(pos, (bomb.collider as SphereCollider).radius * 0.5f, bombCollisionCheckMask);
    }

    void DoThrow(Vector3 pos, float speed, float angle, bool applyBodyVelocity) {
        mBody.ResetCollision();

        attachSpriteAnim.Play(attachSpriteClipEmpty);

        bomb.transform.position = pos;
        bomb.transform.rotation = throwPoint.rotation;
        bomb.rigidbody.angularVelocity = Vector3.zero;
        bomb.rigidbody.velocity = Vector3.zero;

        Vector3 newVel;

        if(applyBodyVelocity) {
            Vector3 bodyLVel = mBody.localVelocity;

            float velX = mBodySpriteCtrl.isLeft ? bodyLVel.x < 0.0f ? bodyLVel.x : 0.0f : bodyLVel.x > 0.0 ? bodyLVel.x : 0.0f;
            float velY = mBody.localVelocity.y < 0 ? 0.0f : mBody.localVelocity.y;

            newVel = bomb.transform.localToWorldMatrix.MultiplyVector(new Vector3(velX, velY));
        }
        else
            newVel = Vector3.zero;

        bomb.SetActive(true);
        mBombCtrl.Activate();

        if(speed != 0.0f) {
            Vector3 dir = mBodySpriteCtrl.isLeft ? -mBody.dirHolder.right : mBody.dirHolder.right;

            Quaternion rot = Quaternion.AngleAxis(angle, mBodySpriteCtrl.isLeft ? -Vector3.forward : Vector3.forward);

            dir = rot * dir;

            newVel += dir * speed;
        }

        bomb.rigidbody.AddForce(newVel, ForceMode.VelocityChange);

        StartCoroutine(DoBombCorrection(mBody.gravityController.up));

        tk2dBaseSprite bombSpr = bomb.GetComponentInChildren<tk2dBaseSprite>();
        if(bombSpr)
            bombSpr.FlipX = mBodySpriteCtrl.isLeft;

        mPlayer.HUD.targetOffScreen.gameObject.SetActive(false);

        if(bombGrabber)
            bombGrabber.gameObject.SetActive(true);
    }

    IEnumerator DoBombCorrection(Vector3 up) {
        yield return new WaitForFixedUpdate();

        if(bomb) {
            bomb.rigidbody.detectCollisions = true;

            GravityController bombGrav = bomb.GetComponent<GravityController>();
            bombGrav.up = up;
        }
    }

    public void ThrowAttach() {
        switch(mThrowMode) {
            case ThrowMode.None:
                if(!CheckBombCollideAt(throwPoint.position))
                    DoThrow(throwPoint.position, throwSpeed, throwAngle, true);
                else
                    attachAnimator.Stop();
                break;

            case ThrowMode.Up:
                if(!CheckBombCollideAt(throwPoint.position))
                    DoThrow(throwPoint.position, throwSpeed, 90.0f, false);
                else
                    attachAnimator.Stop();
                break;

            case ThrowMode.Down:
                float r = mBodySpriteCtrl.isLeft ? Mathf.PI * 0.5f : -Mathf.PI * 0.5f;
                Vector3 lpos = mBody.transform.worldToLocalMatrix.MultiplyPoint(throwPoint.position);

                //try downward
                Vector2 p2 = M8.MathUtil.Rotate(lpos, r * 2.0f);
                lpos.x = p2.x; lpos.y = p2.y;

                Matrix4x4 mtx = mBody.transform.localToWorldMatrix;
                Vector3 pos = mtx.MultiplyPoint(lpos);

                if(!CheckBombCollideAt(pos)) {
                    DoThrow(pos, throwSpeed, -90.0f, false);
                }
                else {
                    int i = 0;
                    for(; i < 3; i++) {
                        p2 = M8.MathUtil.Rotate(lpos, r);
                        lpos.x = p2.x; lpos.y = p2.y;
                        pos = mtx.MultiplyPoint(lpos);

                        if(!CheckBombCollideAt(pos))
                            break;
                    }

                    if(i < 3)
                        DoThrow(pos, 0.0f, 0.0f, true);
                }
                break;
        }
    }

    public void DropAttach() {
        if(hasAttach) {
            Vector3 lpos = mBody.transform.worldToLocalMatrix.MultiplyPoint(throwPoint.position);
            Vector3 pos = throwPoint.position;
            Matrix4x4 mtx = mBody.transform.localToWorldMatrix;

            for(int i = 0; i < 4; i++) {
                if(!CheckBombCollideAt(pos))
                    break;

                Vector2 p2 = M8.MathUtil.Rotate(lpos, Mathf.PI * 0.5f);
                lpos.x = p2.x; lpos.y = p2.y;
                pos = mtx.MultiplyPoint(lpos);

                dropAngle += 90.0f;
            }

            DoThrow(pos, dropSpeed, dropAngle, false);
        }
        else {
            bombGrabber.Revert();
        }
    }

    /// <summary>
    /// Put bomb back on git girl's head.
    /// </summary>
    public void BombActive() {
        if(mBody)
            mBody.ResetCollision();

        if(bomb) {
            bomb.rigidbody.detectCollisions = false;
            bomb.SetActive(false);
        }

        if(bombGrabber)
            bombGrabber.gameObject.SetActive(false);

        if(mPlayer.bombEnabled) {
            if(attachSpriteAnim)
                attachSpriteAnim.Play(attachSpriteClipBomb);

            mPlayer.HUD.targetOffScreen.gameObject.SetActive(true);
        }
        else {
            if(attachSpriteAnim)
                attachSpriteAnim.Play(attachSpriteClipEmpty);

            mPlayer.HUD.targetOffScreen.gameObject.SetActive(false);
        }

        if(attachAnimator)
            attachAnimator.Play("default");
    }

    public bool Hurt(Vector3 normal, bool forceBounce) {
        if(!mPlayer.isBlinking && mPlayer.state == (int)Player.State.Normal) {
            mPlayer.state = (int)Player.State.Hurt;

            SoundPlayerGlobal.instance.Play("hurt");

            bool startHurt = mHurtForceDelay <= 0.0f;
            mHurtNormal = normal;
            mHurtForceDelay = hurtForceDelay;
            if(startHurt)
                StartCoroutine(DoHurtForce());

            return true;
        }
        else if(forceBounce) {
            bool startHurt = mHurtForceDelay <= 0.0f;
            mHurtNormal = normal;
            mHurtForceDelay = hurtForceDelay;
            if(startHurt)
                StartCoroutine(DoHurtForce());
        }

        return false;
    }

    IEnumerator DoHurtForce() {

        mBody.enabled = false;
        mBody.rigidbody.velocity = Vector3.zero;
        mBody.rigidbody.drag = 0.0f;

        WaitForFixedUpdate wait = new WaitForFixedUpdate();

        while(mHurtForceDelay > 0.0f) {
            yield return wait;

            mBody.rigidbody.AddForce(mHurtNormal * hurtForce);

            mHurtForceDelay -= Time.fixedDeltaTime;
        }

        mBody.enabled = true;
        mBody.ResetCollision();
    }

    public void ResetData() {
        mVerticalMoveActive = false;
        mLastMoveVertical = 0.0f;

        mHurtForceDelay = 0.0f;

        if(bombGrabber)
            bombGrabber.gameObject.SetActive(false);

        foreach(SpriteColorBlink blink in spriteBlinks) {
            if(blink)
                blink.enabled = false;
        }

        if(attachAnimator) {
            attachAnimator.transform.localScale = Vector3.one;
            attachAnimator.Stop();
        }

        if(mBody) {
            mBody.ResetCollision();
            mBody.eyeOfs = Vector3.zero;
        }

        if(mBodySpriteCtrl)
            mBodySpriteCtrl.ResetAnimation();

        if(attachSpriteAnim)
            attachSpriteAnim.Sprite.FlipX = false;

        if(mBombCtrl)
            mBombCtrl.Init();

        if(mPlayer) {
            HUD hud = mPlayer.HUD;
            if(hud) {
                if(hud.tickerLabel)
                    hud.tickerLabel.gameObject.SetActive(false);

                if(hud.targetOffScreen)
                    hud.targetOffScreen.gameObject.SetActive(false);
            }
        }

        mThrowMode = ThrowMode.None;

        if(attachPoint)
            attachPoint.localRotation = Quaternion.identity;

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
        mBody.triggerEnterCallback += OnBodyTriggerEnter;

        mBodySpriteCtrl = mBody.GetComponent<PlatformerSpriteController>();
        mBodySpriteCtrl.flipCallback += OnFlipCallback;
        mBodySpriteCtrl.anim.AnimationCompleted += OnBodySpriteAnimFinish;

        mBombCtrl = bomb.GetComponent<BombController>();
        mBombCtrl.deathCallback += OnBombDeathCallback;

        mTargetGO = GameObject.FindGameObjectWithTag("Goal");
    }

    // Update is called once per frame
    void Update() {
        if(hasAttach && mBombCtrl.curDelay < mBombCtrl.deathDelay) {
            mBombCtrl.curDelay += Time.deltaTime;
            mPlayer.HUD.UpdateTicker(mBombCtrl.curDelay, mBombCtrl.deathDelay);
        }

        if(mPlayer.isGoal) {
            if(mPlayer.HUD.targetOffScreen.gameObject.activeSelf)
                mPlayer.HUD.targetOffScreen.gameObject.SetActive(false);
        }

        if(mInputEnabled) {
            //vertical movement
            InputManager input = Main.instance.input;

            float axisY = input.GetAxis(0, InputAction.MoveY);

            float moveVertical;

            //check if dropping
            if(axisY < -0.1f) {
                moveVertical = Mathf.Sign(axisY);
                mThrowMode = ThrowMode.Down;
            }
            else if(axisY > 0.1f) {
                moveVertical = Mathf.Sign(axisY);
                mThrowMode = ThrowMode.Up;
            }
            else {
                moveVertical = 0.0f;
                mThrowMode = ThrowMode.None;
            }

            if(moveVertical != mLastMoveVertical) {
                mLastMoveVertical = moveVertical;
                mLastMoveVerticalTime = Time.time;
                mVerticalMoveActive = true;
            }
        }
        else
            mThrowMode = ThrowMode.None;

        Vector3 a;
        switch(mThrowMode) {
            case ThrowMode.None:
                attachPoint.localRotation = Quaternion.identity;
                break;
            case ThrowMode.Down:
                a = attachPoint.localEulerAngles;
                a.z = mBodySpriteCtrl.isLeft ? 60 : -60;
                attachPoint.localEulerAngles = a;
                break;
            case ThrowMode.Up:
                a = attachPoint.localEulerAngles;
                a.z = mBodySpriteCtrl.isLeft ? -60 : 60;
                attachPoint.localEulerAngles = a;
                break;
        }

        if(mVerticalMoveActive) {
            if(Time.time - mLastMoveVerticalTime > lookDelay) {
                if(mLastMoveVertical == 0.0f) {
                    mBody.eyeOfs.y = 0.0f;
                }
                else if(mLastMoveVertical > 0.0f) {
                    mBody.eyeOfs.y = lookOfs;
                }
                else if(mLastMoveVertical < 0.0f) {
                    mBody.eyeOfs.y = -lookOfs;
                }

                mVerticalMoveActive = false;
            }
        }
    }

    void OnInputAction(InputManager.Info dat) {
        if(dat.state == InputManager.State.Pressed) {
            if(hasAttach) {
                if(!attachAnimator.isPlaying) {
                    attachAnimator.Play(mBodySpriteCtrl.isLeft ? "throwLeft" : "throw");
                }

                ThrowAttach();
            }
            else if(bombGrabber.canGrab) {
                bombGrabber.Grab();
            }
            else if(bombGrabber.grabState != BombGrabber.GrabState.None)
                bombGrabber.Revert();
        }
    }

    void OnInputSpecial(InputManager.Info dat) {
        /*if(dat.state == InputManager.State.Pressed) {
            if(bombGrabber.grabState == BombGrabber.GrabState.None) {
                int mode = (int)(bombGrabber.mode + 1);
                if(mode >= (int)BombGrabber.Mode.NumModes)
                    mode = 0;

                bombGrabber.mode = (BombGrabber.Mode)mode;
            }
        }*/
    }

    void OnPlayerSpawn(EntityBase ent) {
        bombGrabber.Init(this);

        ResetData();

        if(mTargetGO)
            mPlayer.HUD.targetOffScreen.SetPOI(mTargetGO.transform);

        BombActive();
    }

    void OnPlayerSetState(EntityBase ent) {
        switch((Player.State)ent.state) {
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
                ResetData();

                attachAnimator.Stop();

                mBodySpriteCtrl.animationActive = false;
                inputEnabled = false;

                mBombCtrl.StopTimer();
                break;

            case Player.State.Dead:
                ResetData();

                BombActive();

                mBodySpriteCtrl.animationActive = false;
                inputEnabled = false;

                if(mTargetGO) {
                    BombDropOffTrigger goalCtrl = mTargetGO.GetComponent<BombDropOffTrigger>();
                    goalCtrl.ResetData();
                }
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

        if(!b && mPlayer.state == (int)Player.State.Hurt) {
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
            /*else if(cp.otherCollider.gameObject == bomb) {
                //pick up bomb again
                //can't if we are hurt, bomb dropped at goal, currently animating for some reason.
                if(!mPlayer.isGoal
                    && (mBodySpriteCtrl.anim.CurrentClip == null || mBodySpriteCtrl.anim.CurrentClip.name != "hurt")
                    && !attachAnimator.isPlaying
                    && bombGrabber.grabState == BombGrabber.GrabState.RetractBomb) {
                    BombActive();
                }
            }*/
        }

        //check to see if we are squashed...
        //if(mBody.CheckPenetrate(bodyPenetrateOfs, bodyPenetrateCheckMask)) {
        //die
        //mPlayer.GameOver();
        //}
    }

    void OnBodyTriggerEnter(RigidBodyController ctrl, Collider col) {
        if(col.gameObject.CompareTag("Star")) {
            mPlayer.CollectStar(col);
        }
        else if(col.gameObject.CompareTag("Enemy")) {
            Vector3 playerPos = mBody.transform.position;
            Vector3 enemyPos = col.bounds.center;
            Vector3 dPos = playerPos - enemyPos;

            //check if enemy is at bottom
            Vector3 localDPos = mBody.transform.worldToLocalMatrix.MultiplyVector(dPos);
            bool isTop = Vector3.Angle(localDPos, Vector3.up) <= 55.0f;

            Enemy enemy = M8.Util.GetComponentUpwards<Enemy>(col.transform, true);

            if(enemy) {
                //Debug.Log("is top: " + isTop);
                //kill enemy and have a free jump
                if(isTop && enemy.playerJumpKill) {
                    if(enemy.FSM)
                        enemy.FSM.SendEvent(EntityEvent.Hit);

                    Vector3 localVel = mBody.localVelocity;
                    localVel.y = enemyJumpSpeed;
                    mBody.rigidbody.velocity = mBody.transform.rotation * localVel;
                    mBody.jumpCounterCurrent = 1;
                }
                else if(!mPlayer.isBlinking) {
                    Vector2 dir = dPos.normalized;
                    Hurt(dir, false);

                    if(enemy.state == (int)Enemy.State.Normal && enemy.FSM)
                        enemy.FSM.SendEvent(EntityEvent.Contact);
                }
            }
        }
        else if(col.gameObject.CompareTag("Harm") || col.gameObject.CompareTag("Goal")) { //Goal is the big monster thing
            if(!mPlayer.isBlinking) {
                Vector2 dir = (mBody.transform.position - col.bounds.center).normalized;
                Hurt(dir, false);
            }
        }
        else if(col.gameObject.CompareTag("Death")) {
            mPlayer.GameOver();
        }
        else if(col.gameObject.CompareTag("TriggerSave")) {
            TriggerCheckpoint triggerCP = col.GetComponent<TriggerCheckpoint>();
            if(triggerCP) {
                mPlayer.AddTriggerCheckpoint(triggerCP);
            }
        }
    }

    void OnBodySpriteAnimFinish(tk2dSpriteAnimator anim, tk2dSpriteAnimationClip clip) {
        if(clip.name == "hurt") {
            mBodySpriteCtrl.animationActive = true;
            inputEnabled = true;
        }
    }

    void OnBombDeathCallback(BombController ctrl) {
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

        mLastMoveVertical = 0.0f;
        mVerticalMoveActive = false;
        mBody.eyeOfs = Vector3.zero;
    }
}
