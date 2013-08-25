using UnityEngine;
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
    public float jumpImpulse = 5.0f;
    public float jumpWaterForce = 5.0f;
    public float jumpForce = 50.0f;
    public float jumpDelay = 0.15f;

    public bool jumpWall = false; //wall jump

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

    protected override bool CanMove() {
        float x = localVelocity.x;
        //disregard y (for better air controller)

        bool ret = x * x < moveMaxSpeed * moveMaxSpeed;

        //see if we are trying to move the opposite dir
        if(!ret) { //see if we are trying to move the opposite dir
            Vector3 velDir = rigidbody.velocity.normalized;
            ret = Vector3.Dot(moveDir, velDir) < moveCosCheck;
        }

        return ret;
    }

    protected override void RefreshCollInfo() {
        base.RefreshCollInfo();
                
        if(isSlopSlide) {
            //Debug.Log("sliding");
            mLastGround = false;
            mJumpCounter = jumpCounter;
        }
        else if(mLastGround != isGrounded) {
            if(!mLastGround) {
                //Debug.Log("landed");
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
            else if(!(isSlopSlide || mJumpingWall)) {
                //moveForward = moveY;
                moveSide = moveX;
            }

            //jump
            if(mJump) {
                if(isOnLadder) {
                    body.AddForce(dirRot * Vector3.up * ladderJumpForce);
                }
                else if(isUnderWater) {
                    body.AddForce(dirRot * Vector3.up * jumpWaterForce);
                }
                else {
                    if(Time.fixedTime - mJumpLastTime >= jumpDelay || (collisionFlags & CollisionFlags.Above) != 0) {
                        mJump = false;
                    }
                    else {
                        body.AddForce(dirRot * Vector3.up * jumpForce);
                    }
                }
            }
        }
        else {
            moveForward = 0.0f;
            moveSide = 0.0f;
            mJump = false;
            mJumpingWall = false;
        }

        //see if we are jumping wall and falling, then cancel jumpwall
        if(mJumpingWall && localVelocity.y < 0.0f)
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
            else if(!isSlopSlide) {
                if(jumpWall && collisionFlags == CollisionFlags.Sides) {
                    CollideInfo inf = new CollideInfo();

                    foreach(KeyValuePair<Collider, CollideInfo> pair in mColls) {
                        if(pair.Value.flag == CollisionFlags.Sides) {
                            inf = pair.Value;
                            break;
                        }
                    }

                    rigidbody.velocity = Vector3.zero;

                    Vector3 jumpDir = inf.normal + dirHolder.up;

                    if(jumpImpulse > 0.0f) {
                        PrepJumpVel();
                                                
                        rigidbody.AddForce(jumpDir * jumpImpulse, ForceMode.Impulse);
                    }

                    ClearCollFlags();
                    rigidbody.drag = airDrag;

                    mJumpingWall = true;
                    mJump = true;
                    mJumpLastTime = Time.fixedTime;
                    mJumpCounter = jumpCounter;
                }
                else if((mJumpCounter == 0 && isGrounded) || (mJumpCounter > 0 && mJumpCounter < jumpCounter)) {
                    if(jumpImpulse > 0.0f) {
                        PrepJumpVel();
                                                
                        rigidbody.AddForce(dirHolder.up * (isGrounded ? jumpImpulse : jumpAirImpulse), ForceMode.Impulse);
                    }

                    ClearCollFlags();
                    rigidbody.drag = airDrag;

                    mJumpCounter++;
                    mJumpingWall = false;
                    mJump = true;
                    mJumpLastTime = Time.fixedTime;
                }
            }
        }
        else if(dat.state == InputManager.State.Released) {
            mJump = false;
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
