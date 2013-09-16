using UnityEngine;
using System.Collections;

public class EnemyShootController : MonoBehaviour {
    public delegate void OnShoot(EnemyShootController ctrl);

    public Transform spawnPoint;

    public string group;
    public string type;
    public bool seekTarget;
    public int maxCount = 1;

    public Vector3 visionDir = new Vector3(1, 0, 0);
    public float visionAngleLim = 360;
    public LayerMask visionObscureMask;

    public event OnShoot shootCallback;

    private bool mShootEnable;
    private int mCurCount;
    private Ray mVisionRay;

    public bool shootEnable {
        get { return mShootEnable; }
        set { mShootEnable = value; }
    }

    public void Shoot(Vector3 dir, Transform target) {
        Shoot(spawnPoint ? spawnPoint.position : transform.position, dir, target);
    }

    public void Shoot(Vector3 pos, Vector3 dir, Transform target) {
        if(string.IsNullOrEmpty(type))
            return;

        Projectile proj = Projectile.Create(group, type, pos, dir, target);

        if(proj) {
            mCurCount++;
            proj.releaseCallback += OnProjRelease;

            if(shootCallback != null)
                shootCallback(this);
        }
    }

    void OnTriggerStay(Collider col) {
        if(mShootEnable && mCurCount < maxCount) {
            Vector3 pos = spawnPoint ? spawnPoint.position : transform.position;
            Transform seek = col.transform;

            Vector3 dir = seek.position - pos;
            float dist = dir.magnitude;
            dir /= dist;

            //check if within angle
            if(visionAngleLim < 360) {
                Vector3 checkDir = transform.rotation * visionDir;
                if(Vector3.Angle(checkDir, dir) > visionAngleLim)
                    return;
            }

            //check walls
            mVisionRay.origin = pos;
            mVisionRay.direction = dir;
            if(Physics.Raycast(mVisionRay, dist, visionObscureMask))
                return;

            Shoot(pos, dir, seekTarget ? seek : null);
        }
    }

    void OnDestroy() {
        shootCallback = null;
    }

    void OnProjRelease(EntityBase ent) {
        ent.releaseCallback -= OnProjRelease;

        mCurCount--;
        if(mCurCount < 0)
            mCurCount = 0;
    }
}
