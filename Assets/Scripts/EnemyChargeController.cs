using UnityEngine;
using System.Collections;

public class EnemyChargeController : MonoBehaviour {
    public enum SeekMode {
        None,
        Wait,
        Towards,
        Return
    }

    public float seekRadius;
    public float seekDelay;
    public float seekWaitDelay;

    public LayerMask seekVisibleMask;

    public float returnDelay;

    public float orientSpeed = 90.0f;

    public float playerCheckDelay;

    private Enemy mEnemy;
    private Player mPlayer;

    private Vector3 mLastMoverPos;
    private Quaternion mLastMoverRot;
    private Vector3 mSeekToPos;

    private float mCurSeekTime;
    private SeekMode mCurSeekMode = SeekMode.None;

    private WaitForFixedUpdate mWait;
    private WaitForSeconds mCheckWait;
    private bool mRestorePosOnRevive;
    private bool mDefaultFacePlayer;

    void ApplyEnemyState() {
        StopAllCoroutines();

        switch((Enemy.State)mEnemy.state) {
            case Enemy.State.Normal:
                mEnemy.facePlayer = mDefaultFacePlayer;
                StartCoroutine(DoCheck());
                break;

            case Enemy.State.Attack:
                mEnemy.facePlayer = true;
                StartCoroutine(DoSeek());
                break;

            case Enemy.State.Dead:
                break;

            case Enemy.State.Reviving:
                break;
        }
    }

    void OnEnemyChangeState(EntityBase ent, int state) {
        if(mEnemy.prevState == (int)Enemy.State.Attack && state == (int)Enemy.State.Dead) {
            mRestorePosOnRevive = true;
        }

        if(state == (int)Enemy.State.Reviving) {
            if(mRestorePosOnRevive) {
                mEnemy.mover.position = mLastMoverPos;
                mRestorePosOnRevive = false;
            }
        }
        else if(state == (int)Enemy.State.Attack) {
            mLastMoverPos = mEnemy.mover.position;
            mLastMoverRot = mEnemy.mover.rotation;
            mSeekToPos = mPlayer.controller.body.transform.position;
            mCurSeekMode = SeekMode.Wait;
            mCurSeekTime = 0.0f;
        }

        ApplyEnemyState();
    }

    void OnEnemyActivatorWake() {
        ApplyEnemyState();
    }

    // Use this for initialization
    void Start() {
        mEnemy = GetComponent<Enemy>();

        mEnemy.setStateCallback += OnEnemyChangeState;

        mDefaultFacePlayer = mEnemy.facePlayer;

        if(mEnemy.activator)
            mEnemy.activator.awakeCallback += OnEnemyActivatorWake;

        mPlayer = Player.instance;

        mWait = new WaitForFixedUpdate();
        mCheckWait = new WaitForSeconds(playerCheckDelay);
    }

    IEnumerator DoCheck() {
        while(true) {
            //make sure player is normal or hurt
            if(mPlayer.state == (int)Player.State.Normal || mPlayer.state == (int)Player.State.Hurt) {
                Vector3 pos = mEnemy.mover.position;
                Vector2 dpos = (mPlayer.controller.body.transform.position - pos);
                float sqrDist = dpos.sqrMagnitude;
                if(sqrDist <= seekRadius * seekRadius) {
                    //check if not obscured
                    float dist = Mathf.Sqrt(sqrDist);
                    Vector3 dir = dpos / dist;
                    if(!Physics.Raycast(pos, dir, dist, seekVisibleMask)) {
                        mEnemy.state = (int)Enemy.State.Attack;
                        break;
                    }
                }
            }

            yield return mCheckWait;
        }
    }

    IEnumerator DoSeek() {
        Transform playerBodyTrans = mPlayer.controller.body.transform;

        while(true) {
            switch(mCurSeekMode) {
                case SeekMode.Wait:
                    DoOrientation(playerBodyTrans.rotation, Time.fixedDeltaTime);

                    mCurSeekTime += Time.fixedDeltaTime;
                    if(mCurSeekTime >= seekWaitDelay) {
                        mCurSeekMode = SeekMode.Towards;
                        mCurSeekTime = 0.0f;
                    }
                    break;

                case SeekMode.Towards:
                    DoOrientation(playerBodyTrans.rotation, Time.fixedDeltaTime);

                    mCurSeekTime += Time.fixedDeltaTime;
                    if(mCurSeekTime >= seekDelay) {
                        mCurSeekTime = 0.0f;
                        mCurSeekMode = SeekMode.Return;
                        mSeekToPos = mLastMoverPos;
                        mLastMoverPos = mEnemy.mover.position;
                    }
                    else {
                        float t = Holoville.HOTween.Core.Easing.Back.EaseInOut(mCurSeekTime, 0.0f, 1.0f, seekDelay, 0, 0);
                        mEnemy.mover.position = Vector3.Lerp(mLastMoverPos, mSeekToPos, t);
                    }
                    break;

                case SeekMode.Return:
                    DoOrientation(mLastMoverRot, Time.fixedDeltaTime);

                    mCurSeekTime += Time.fixedDeltaTime;
                    if(mCurSeekTime >= returnDelay) {
                        mEnemy.mover.rotation = mLastMoverRot;
                        mEnemy.state = (int)Enemy.State.Normal;
                        yield break;
                    }
                    else {
                        float t = Holoville.HOTween.Core.Easing.Quart.EaseOut(mCurSeekTime, 0.0f, 1.0f, returnDelay, 0, 0);
                        mEnemy.mover.position = Vector3.Lerp(mLastMoverPos, mSeekToPos, t);
                    }
                    break;
            }

            yield return mWait;
        }
    }

    void DoOrientation(Quaternion toRot, float timeDelta) {
        Quaternion curRot = mEnemy.mover.rotation;
        if(curRot != toRot) {
            mEnemy.mover.rotation = Quaternion.RotateTowards(curRot, toRot, orientSpeed * timeDelta);
        }
    }

    void OnDrawGizmos() {
        if(seekRadius > 0) {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, seekRadius);
        }
    }
}
