using UnityEngine;
using System.Collections;

public class BombGrabber : MonoBehaviour {
    public enum GrabState {
        None,
        Seek,
        RetractBomb
    }

    public enum Mode {
        Push, //push yourself towards bomb
        Pull, //pull bomb to you

        NumModes
    }

    public Color[] modeHighlightColors;
    public Transform[] modeArrows;

    public float detachDistance;
    public float grabBombDistance;
    public float grabBombSpeedLimit;
    public Transform grabLine;

    public float seekSpeed;
    public float retractForce;
    public float pullDelay;

    public LayerMask collideMask;

    private Mode mMode = Mode.Pull;
    private GrabState mGrabState = GrabState.None;

    private Vector3 mGrabberStartPos;
    private Quaternion mGrabberStartRot;

    private PlayerController mPlayerCtrl;

    private Vector3 mGrabberSeekPos;

    private tk2dBaseSprite mHighlightSpr;

    private Vector3 mPullVel;

    private bool mIsInit = false;

    public Mode mode {
        get { return mMode; }
        set {
            if(mMode != value && mGrabState == GrabState.None) {
                modeArrows[(int)mMode].gameObject.SetActive(false);
                mPlayerCtrl.player.HUD.grabInfo[(int)mMode].SetActive(false);

                mMode = value;

                mPlayerCtrl.player.HUD.grabInfo[(int)mMode].SetActive(true);
                mHighlightSpr.color = modeHighlightColors[(int)mMode];
            }
        }
    }

    public GrabState grabState { get { return mGrabState; } }
    public bool canGrab { 
        get { return mGrabState == GrabState.None && !mPlayerCtrl.player.isBlinking && mPlayerCtrl.body.enabled && mPlayerCtrl.bombCtrl.highlightGO.activeSelf; } 
    }

    public void Init(PlayerController pc) {
        mPlayerCtrl = pc;

        mGrabberStartPos = pc.attachSpriteAnim.transform.localPosition;
        mGrabberStartRot = pc.attachSpriteAnim.transform.localRotation;

        mPlayerCtrl.bombCtrl.highlightGO.SetActive(false);

        mHighlightSpr = pc.bombCtrl.highlightGO.GetComponent<tk2dBaseSprite>();
        mHighlightSpr.color = modeHighlightColors[(int)mMode];

        for(int i = 0; i < (int)Mode.NumModes; i++) {
            pc.player.HUD.grabInfo[i].SetActive(i == (int)mMode);
        }

        mIsInit = true;
    }

    public void Grab() {
        modeArrows[(int)mMode].gameObject.SetActive(false);

        mPlayerCtrl.attachSpriteAnim.Play("seek");

        mGrabberSeekPos = mPlayerCtrl.attachPoint.position;
        mGrabState = GrabState.Seek;
    }

    public void Revert() {
        mGrabState = GrabState.None;
        if(mPlayerCtrl) {
            mPlayerCtrl.bombCtrl.highlightGO.SetActive(false);

            mPlayerCtrl.attachSpriteAnim.transform.localPosition = mGrabberStartPos;
            mPlayerCtrl.attachSpriteAnim.transform.localRotation = mGrabberStartRot;

            if(!mPlayerCtrl.body.enabled) {
                mPlayerCtrl.body.enabled = true;
                mPlayerCtrl.body.ResetCollision();
            }

            mPlayerCtrl.body.gravityController.gravityLocked = false;

            mPlayerCtrl.attachSpriteAnim.Play(mPlayerCtrl.hasAttach ? "bomb" : "empty");

            mPlayerCtrl.bomb.rigidbody.detectCollisions = true;
            mPlayerCtrl.bomb.rigidbody.isKinematic = false;
        }

        foreach(Transform t in modeArrows)
            t.gameObject.SetActive(false);

        grabLine.gameObject.SetActive(false);
        modeArrows[(int)mMode].gameObject.SetActive(false);
    }

    void OnDisable() {
        Revert();
    }

    /// <summary>
    /// Return true if intersect
    /// </summary>
    bool ApplyLine(Vector3 pos) {
        Vector3 dir = mPlayerCtrl.bomb.transform.position - pos;
        float d = dir.magnitude;

        if(d > 0) {
            Vector3 ls;

            dir /= d;

            RaycastHit hit;
            if(Physics.Raycast(pos, dir, out hit, d, collideMask) && hit.collider != mPlayerCtrl.bomb.collider) {
                modeArrows[(int)mMode].gameObject.SetActive(false);
                grabLine.gameObject.SetActive(false);
                return true;
            }

            Transform line = null;

            switch(mGrabState) {
                case GrabState.None:
                    line = modeArrows[(int)mMode];
                    break;

                case GrabState.Seek:
                case GrabState.RetractBomb:
                    line = grabLine;
                    break;
            }

            if(line) {
                line.up = new Vector3(dir.x, dir.y, 0.0f);

                ls = line.localScale;
                ls.y = d;

                line.localScale = ls;
                line.position = new Vector3(pos.x, pos.y, line.position.z);
            }

            return false;
        }
        else
            return false;
    }

    void OnTriggerStay(Collider col) {
        if(mGrabState == GrabState.None) {
            if(col.transform == mPlayerCtrl.bomb.transform) {
                if(mPlayerCtrl.body.enabled && mPlayerCtrl.inputEnabled && !mPlayerCtrl.player.isBlinking && !ApplyLine(mPlayerCtrl.attachPoint.position)) {
                    mPlayerCtrl.bombCtrl.highlightGO.SetActive(true);
                    modeArrows[(int)mMode].gameObject.SetActive(true);
                }
                else {
                    mPlayerCtrl.bombCtrl.highlightGO.SetActive(false);
                    modeArrows[(int)mMode].gameObject.SetActive(false);
                }
            }
        }
    }

    void OnTriggerExit(Collider col) {
        if(mGrabState == GrabState.None) {
            if(col.transform == mPlayerCtrl.bomb.transform) {
                mPlayerCtrl.bombCtrl.highlightGO.SetActive(false);
                modeArrows[(int)mMode].gameObject.SetActive(false);
            }
        }
    }

    // Update is called once per frame
    void FixedUpdate() {
        if(!mIsInit)
            return;

        if(mPlayerCtrl.bombCtrl.isConsumed)
            gameObject.SetActive(false);

        switch(mGrabState) {
            case GrabState.Seek:
                Vector3 attachPos = mPlayerCtrl.attachPoint.position;

                if(ApplyLine(attachPos))
                    Revert();
                else {
                    Transform bombT = mPlayerCtrl.bomb.transform;
                    Vector3 bombPos = bombT.position;

                    if((bombPos - attachPos).sqrMagnitude > detachDistance * detachDistance) {
                        Revert();
                    }
                    else {
                        grabLine.gameObject.SetActive(true);

                        Vector3 dir = bombPos - mGrabberSeekPos;
                        float dist = dir.magnitude;

                        if(dist < mPlayerCtrl.bombCtrl.radius) {
                            //Bomb is grabbed, now switch to retract
                            //depends on mode
                            switch(mMode) {
                                case Mode.Push:
                                    mPlayerCtrl.body.enabled = false;
                                    mPlayerCtrl.body.rigidbody.drag = 0.0f;

                                    mPlayerCtrl.body.gravityController.gravityLocked = true;
                                    break;

                                case Mode.Pull:
                                    mPlayerCtrl.bomb.rigidbody.detectCollisions = true;
                                    mPlayerCtrl.bomb.rigidbody.isKinematic = false;
                                    mPlayerCtrl.bomb.rigidbody.velocity = Vector3.zero;
                                    mPullVel = Vector3.zero;
                                    break;
                            }

                            mPlayerCtrl.bombCtrl.highlightGO.SetActive(false);

                            mGrabState = GrabState.RetractBomb;
                        }
                        else {
                            mGrabberSeekPos += dir * seekSpeed * Time.fixedDeltaTime;
                            mPlayerCtrl.attachSpriteAnim.transform.position = new Vector3(mGrabberSeekPos.x, mGrabberSeekPos.y, mPlayerCtrl.attachSpriteAnim.transform.position.z);
                            mPlayerCtrl.attachSpriteAnim.transform.up = new Vector3(dir.x, dir.y, 0.0f);
                        }
                    }
                }
                break;

            case GrabState.RetractBomb:
                Vector3 bodyPos = mPlayerCtrl.body.transform.position;

                if(ApplyLine(mPlayerCtrl.attachPoint.position))
                    Revert();
                else {
                    //check if bomb is too far, then cancel
                    //or if acquired, then attach
                    Vector3 bombPos = mPlayerCtrl.bomb.transform.position;

                    Vector3 dir = bombPos - bodyPos;

                    float bodyDist = dir.magnitude;
                    if(bodyDist > detachDistance) {
                        Revert();
                    }
                    else if(bodyDist <= grabBombDistance + mPlayerCtrl.bombCtrl.radius) {
                        if(mMode == Mode.Push) {
                            //slow player down once bomb is acquired
                            float spd = mPlayerCtrl.body.rigidbody.velocity.magnitude;
                            if(spd > mPlayerCtrl.body.airMaxSpeed) {
                                mPlayerCtrl.body.rigidbody.velocity = (mPlayerCtrl.body.rigidbody.velocity / spd) * mPlayerCtrl.body.airMaxSpeed;
                            }
                        }

                        mPlayerCtrl.BombActive();
                    }
                    else {
                        dir /= bodyDist;

                        //set head to bomb
                        mPlayerCtrl.attachSpriteAnim.transform.position = new Vector3(bombPos.x, bombPos.y, mPlayerCtrl.attachSpriteAnim.transform.position.z);
                        mPlayerCtrl.attachSpriteAnim.transform.up = new Vector3(dir.x, dir.y, 0.0f);

                        switch(mMode) {
                            case Mode.Push:
                                mPlayerCtrl.body.UpdateCamera(Time.fixedDeltaTime);

                                //move player towards bomb
                                if(Vector3.Angle(dir, mPlayerCtrl.body.rigidbody.velocity) > 45.0f) {
                                    mPlayerCtrl.body.rigidbody.velocity = Vector3.zero;
                                }

                                mPlayerCtrl.body.rigidbody.AddForce(dir * retractForce);

                                //slow the bomb down
                                float bombSpd = mPlayerCtrl.bomb.rigidbody.velocity.magnitude;
                                if(bombSpd > grabBombSpeedLimit) {
                                    mPlayerCtrl.bomb.rigidbody.velocity = grabBombSpeedLimit == 0 ? Vector3.zero : (mPlayerCtrl.bomb.rigidbody.velocity / bombSpd) * grabBombSpeedLimit;
                                }
                                break;

                            case Mode.Pull:
                                mPlayerCtrl.bomb.transform.position = Vector3.SmoothDamp(
                                    mPlayerCtrl.bomb.transform.position,
                                    mPlayerCtrl.body.transform.position,
                                    ref mPullVel, pullDelay, Mathf.Infinity, Time.fixedDeltaTime);
                                break;
                        }
                    }
                }
                break;
        }
    }

    void OnDrawGizmos() {
        if(detachDistance > 0) {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detachDistance);
        }

        if(grabBombDistance > 0) {
            Gizmos.color = Color.yellow * 0.5f;
            Gizmos.DrawWireSphere(transform.position, grabBombDistance);
        }
    }
}
