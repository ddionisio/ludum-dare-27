﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlatformerController : RigidBodyController {
    public delegate void Callback(PlatformerController ctrl);

    [SerializeField]
    Transform _eye;

    public float eyeLockOrientSpeed = 180.0f; //when we lock the eye again, this is the speed to re-orient based on dirHolder
    public float eyeLockPositionDelay = 1.0f; //reposition delay when we lock the eye again

    public int jumpCounter = 1;
    public float jumpAirImpulse = 2.5f;
    public float jumpWallImpulse = 5.0f;
    public float jumpImpulse = 5.0f;
    public float jumpWaterForce = 5.0f;
    public float jumpForce = 50.0f;
    public float jumpDelay = 0.15f;
    public float jumpReleaseDelay = 0.1f;

    public bool jumpWall = false; //wall jump
    public float jumpWallLockDelay = 0.2f;

    public float wallStickAngle = 160.0f; //what angle is acceptible to wall stick, usu. high angle
    public float wallStickDelay; //delay to stick to wall when moving against one
    public float wallStickUpDelay; //how long to move up the wall once you stick
    public float wallStickUpForce;

    public string ladderTag = "Ladder";
    public LayerMask ladderLayer;
    public float ladderOrientSpeed = 270.0f;
    public float ladderDrag = 20.0f;
    public float ladderJumpForce = 10.0f;

    public int player = 0;
    public int moveInputX = InputManager.ActionInvalid;
    public int moveInputY = InputManager.ActionInvalid;
    public int jumpInput = InputManager.ActionInvalid;

    public bool startInputEnabled = false;

    public event Callback landCallback;

    private bool mInputEnabled = false;

    private bool mJump = false;
    private int mJumpCounter = 0;
    private float mJumpLastTime = 0.0f;
    private bool mJumpingWall = false;

    private int mLadderCounter;
    private bool mLadderLastGravity;
    private Vector3 mLadderUp;
    private Quaternion mLadderRot;

    private bool mEyeLocked = true;
    private bool mEyeOrienting = false;
    private Vector3 mEyeOrientVel;
    private bool mLastGround;

    private bool mWallSticking = false;
    private float mWallStickLastTime = 0.0f;
    private CollideInfo mWallStickInfo;

    public bool inputEnabled {
        get { return mInputEnabled; }
        set {
            if(mInputEnabled != value) {
                mInputEnabled = value;

                InputManager input = Main.instance != null ? Main.instance.input : null;
                if(input != null) {
                    if(mInputEnabled) {
                        input.AddButtonCall(player, jumpInput, OnInputJump);
                    }
                    else {
                        input.RemoveButtonCall(player, jumpInput, OnInputJump);
                    }
                }
            }
        }
    }

    public bool isOnLadder { get { return mLadderCounter > 0; } }

    public Transform eye {
        get { return _eye; }
    }

    /// <summary>
    /// This determines whether or not the eye will be set to the dirHolder's transform.
    /// default: true. If false, input for looking up/down will be disabled.
    /// </summary>
    public bool eyeLocked {
        get { return _eye != null && mEyeLocked; }
        set {
            if(mEyeLocked != value && _eye != null) {
                mEyeLocked = value;

                if(mEyeLocked) {
                    //move eye orientation to dirHolder
                    EyeOrient();
                }
                else {
                    mEyeOrienting = false;
                }
            }
        }
    }

    public bool isJumpWall { get { return mJumpingWall; } }

    public override void ResetCollision() {
        base.ResetCollision();

        if(mLadderCounter > 0) {
            if(gravityController != null)
                gravityController.enabled = true;
            else
                rigidbody.useGravity = mLadderLastGravity;

            mLadderCounter = 0;
        }

        mLastGround = false;
        mJump = false;
        mJumpCounter = 0;
        mJumpingWall = false;

        lockDrag = false;

        mWallSticking = false;
    }

    protected override void WaterEnter() {
        mJumpCounter = 0;
        mJumpingWall = false;
    }

    protected override void WaterExit() {
        if(mJump) {
            if(jumpImpulse > 0.0f)
                rigidbody.AddForce(dirHolder.up * jumpImpulse, ForceMode.Impulse);

            mJumpLastTime = Time.fixedTime;
        }
    }

    protected override void OnTriggerEnter(Collider col) {
        base.OnTriggerEnter(col);

        if(M8.Util.CheckLayerAndTag(col.gameObject, ladderLayer, ladderTag)) {
            mLadderUp = col.transform.up;

            if(!M8.MathUtil.RotateToUp(mLadderUp, transform.right, transform.forward, ref mLadderRot))
                transform.up = mLadderUp;

            mLadderCounter++;
        }

        if(isOnLadder) {
            if(gravityController != null) {
                StartCoroutine(LadderOrientUp());
                gravityController.enabled = false;
            }
            else {
                mLadderLastGravity = rigidbody.useGravity;
                rigidbody.useGravity = false;
            }

            mJumpingWall = false;
        }
    }

    protected override void OnTriggerStay(Collider col) {
        base.OnTriggerStay(col);

        if(M8.Util.CheckLayerAndTag(col.gameObject, ladderLayer, ladderTag)) {
            if(mLadderCounter == 0) {
                mLadderCounter++;

                if(gravityController != null) {
                    StartCoroutine(LadderOrientUp());
                    gravityController.enabled = false;
                }
                else {
                    mLadderLastGravity = rigidbody.useGravity;
                    rigidbody.useGravity = false;
                }
            }

            if(mLadderUp != col.transform.up) {
                mLadderUp = col.transform.up;

                if(!M8.MathUtil.RotateToUp(mLadderUp, transform.right, transform.forward, ref mLadderRot))
                    transform.up = mLadderUp;
            }
        }
    }

    protected override void OnTriggerExit(Collider col) {
        base.OnTriggerExit(col);

        if(M8.Util.CheckLayerAndTag(col.gameObject, ladderLayer, ladderTag)) {
            mLadderCounter--;
        }

        if(!isOnLadder) {
            if(gravityController != null)
                gravityController.enabled = true;
            else
                rigidbody.useGravity = mLadderLastGravity;
        }
    }

    protected override bool CanMove(Vector3 dir, float maxSpeed) {
        
        //float x = localVelocity.x;
        float d = localVelocity.x * localVelocity.x;

        //disregard y (for better air controller)

        bool ret = d < maxSpeed * maxSpeed;

        //see if we are trying to move the opposite dir
        if(!ret) { //see if we are trying to move the opposite dir
            Vector3 velDir = rigidbody.velocity.normalized;
            ret = Vector3.Dot(dir, velDir) < moveCosCheck;
        }

        return ret;
    }

    protected override void RefreshCollInfo() {
        base.RefreshCollInfo();

        bool lastWallStick = mWallSticking;
        mWallSticking = false;

        if(isSlopSlide) {
            //Debug.Log("sliding");
            mLastGround = false;
            mJumpCounter = jumpCounter;
        }
        else if(!mJumpingWall && !isGrounded && collisionFlags == CollisionFlags.Sides) {
            foreach(KeyValuePair<Collider, CollideInfo> pair in mColls) {
                if(pair.Value.flag == CollisionFlags.Sides) {
                    mWallSticking = true;
                    mWallStickInfo = pair.Value;
                    mJump = false;
                    lockDrag = false;
                    break;
                }
            }

            if(mWallSticking && !lastWallStick)
                mWallStickLastTime = Time.fixedTime;
        }

        if(mLastGround != isGrounded) {
            if(!mLastGround) {
                //Debug.Log("landed");
                mJump = false;
                mJumpingWall = false;
                mJumpCounter = 0;

                if(landCallback != null)
                    landCallback(this);
            }

            mLastGround = isGrounded;
        }
    }

    protected override void OnDestroy() {
        inputEnabled = false;

        landCallback = null;

        base.OnDestroy();
    }

    protected override void OnDisable() {
        base.OnDisable();

        mEyeOrienting = false;
    }

    protected override void Awake() {
        base.Awake();
    }

    // Use this for initialization
    protected override void Start() {
        base.Start();

        inputEnabled = startInputEnabled;
    }

    // Update is called once per frame
    protected override void FixedUpdate() {
        Rigidbody body = rigidbody;
        Quaternion dirRot = dirHolder.rotation;

        if(mInputEnabled) {
            InputManager input = Main.instance.input;

            float moveX, moveY;

            moveX = moveInputX != InputManager.ActionInvalid ? input.GetAxis(player, moveInputX) : 0.0f;
            moveY = moveInputY != InputManager.ActionInvalid ? input.GetAxis(player, moveInputY) : 0.0f;

            //movement
            moveForward = 0.0f;
            moveSide = 0.0f;

            if(isOnLadder || (isUnderWater && !isGrounded)) {
                //move forward upwards
                Move(dirRot, Vector3.up, Vector3.right, new Vector2(moveX, moveY), moveForce);
            }
            else if(!(isSlopSlide || mJumpingWall || (mWallSticking && Time.fixedTime - mWallStickLastTime < wallStickDelay))) {
                //moveForward = moveY;
                moveSide = moveX;
            }
                        
            //jump
            if(mWallSticking) {
                float curT = Time.fixedTime - mWallStickLastTime;
                if(curT < wallStickUpDelay) {
                    Vector3 upDir = dirRot * Vector3.up;
                    upDir = M8.MathUtil.Slide(upDir, mWallStickInfo.normal);

                    if(localVelocity.y < airMaxSpeed)
                        body.AddForce(upDir * wallStickUpForce);
                }
            }
            else if(mJump) {
                if(isOnLadder) {
                    body.AddForce(dirRot * Vector3.up * ladderJumpForce);
                }
                else if(isUnderWater) {
                    body.AddForce(dirRot * Vector3.up * jumpWaterForce);
                }
                else {
                    if(Time.fixedTime - mJumpLastTime >= jumpDelay || collisionFlags == CollisionFlags.Above) {
                        mJump = false;
                        lockDrag = false;
                    }
                    else if(!input.IsDown(player, jumpInput) && Time.fixedTime - mJumpLastTime >= jumpReleaseDelay) {
                        lockDrag = false;
                        mJump = false;
                    }
                    else {
                        Vector3 upDir = dirRot * Vector3.up;

                        if(localVelocity.y > 0.0f && mWallSticking) {
                            upDir = M8.MathUtil.Slide(upDir, mWallStickInfo.normal);
                        }

                        if(localVelocity.y < airMaxSpeed)
                            body.AddForce(upDir * jumpForce);
                    }
                }
            }
        }
        else {
            moveForward = 0.0f;
            moveSide = 0.0f;
            mJump = false;
            mJumpingWall = false;

            lockDrag = false;
        }

        //see if we are jumping wall and falling, then cancel jumpwall
        if(mJumpingWall && Time.fixedTime - mJumpLastTime >= jumpWallLockDelay)
            mJumpingWall = false;

        //set eye rotation
        if(_eye != null && mEyeLocked && !mEyeOrienting) {
            Quaternion rot = dirHolder.rotation;
            Vector3 pos = dirHolder.position;

            //TODO: smoothing?

            _eye.rotation = rot;
            _eye.position = pos;
        }

        base.FixedUpdate();

        if(isOnLadder)
            rigidbody.drag = ladderDrag;
    }

    /*IEnumerator DoWallStick() {
        yield return new WaitForSeconds(wallStickDelay);

        //see if we still sticking
        WaitForFixedUpdate wait = new WaitForFixedUpdate();

        PrepJumpVel();
        
        while(mWallSticking) {
            rigidbody.AddForce(gravityController.up * wallStickUpForce, ForceMode.Force);

            yield return wait;
        }
    }*/

    void PrepJumpVel() {
        ComputeLocalVelocity();

        Vector3 newVel = localVelocity;
        newVel.y = 0.0f; //cancel 'falling down'
        newVel = dirHolder.transform.localToWorldMatrix.MultiplyVector(newVel);
        rigidbody.velocity = newVel;
    }

    void OnInputJump(InputManager.Info dat) {
        //jumpWall
        if(dat.state == InputManager.State.Pressed) {
            if(isUnderWater || isOnLadder) {
                mJumpingWall = false;
                mJump = true;
                mJumpCounter = 0;
            }
            else if(jumpWall && mWallSticking) {

                rigidbody.velocity = Vector3.zero;
                lockDrag = true;
                rigidbody.drag = airDrag;

                Vector3 impulse = mWallStickInfo.normal * jumpWallImpulse;
                impulse += dirHolder.up * jumpImpulse;

                PrepJumpVel();
                rigidbody.AddForce(impulse, ForceMode.Impulse);

                mJumpingWall = true;
                mJump = true;
                mWallSticking = false;
                mJumpLastTime = Time.fixedTime;
                mJumpCounter = Mathf.Clamp(mJumpCounter + 1, 0, jumpCounter);

                //TODO: remove me
                SoundPlayerGlobal.instance.Play("jump");
            }
            else if(!isSlopSlide) {
                if((mJumpCounter == 0 && isGrounded) || (mJumpCounter > 0 && mJumpCounter < jumpCounter)) {
                    lockDrag = true;
                    rigidbody.drag = airDrag;

                    PrepJumpVel();
                    rigidbody.AddForce(dirHolder.up * (isGrounded ? jumpImpulse : jumpAirImpulse), ForceMode.Impulse);

                    mJumpCounter++;
                    mJumpingWall = false;
                    mJump = true;
                    mJumpLastTime = Time.fixedTime;

                    //TODO: remove me
                    SoundPlayerGlobal.instance.Play("jump");
                }
            }
        }
    }

    void EyeOrient() {
        //move eye orientation to dirHolder
        if(!mEyeOrienting) {
            mEyeOrienting = true;
            StartCoroutine(EyeOrienting());
        }

        mEyeOrientVel = Vector3.zero;
    }

    IEnumerator LadderOrientUp() {
        WaitForFixedUpdate waitUpdate = new WaitForFixedUpdate();

        while(isOnLadder) {
            if(transform.up != mLadderUp) {
                float step = ladderOrientSpeed * Time.fixedDeltaTime;
                rigidbody.MoveRotation(Quaternion.RotateTowards(transform.rotation, mLadderRot, step));
            }

            yield return waitUpdate;
        }
    }

    IEnumerator EyeOrienting() {
        WaitForFixedUpdate waitUpdate = new WaitForFixedUpdate();

        while(mEyeOrienting) {
            yield return waitUpdate;

            bool posDone = _eye.position == dirHolder.position;
            if(!posDone) {
                _eye.position = Vector3.SmoothDamp(_eye.position, dirHolder.position, ref mEyeOrientVel, eyeLockPositionDelay, Mathf.Infinity, Time.fixedDeltaTime);
            }

            bool rotDone = _eye.rotation == dirHolder.rotation;
            if(!rotDone) {
                float step = eyeLockOrientSpeed * Time.fixedDeltaTime;
                _eye.rotation = Quaternion.RotateTowards(_eye.rotation, dirHolder.rotation, step);
            }

            mEyeOrienting = !rotDone;
        }
    }
}
