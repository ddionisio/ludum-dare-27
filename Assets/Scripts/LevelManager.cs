using UnityEngine;
using System.Collections;

public class LevelManager : MonoBehaviour {
    [System.Serializable]
    public class LevelData {
        public float parTime;

        private float mTime;

        private int mStars;

        private int mStageInd = -1;
        private int mInd = -1;

        private bool mCompleted;

        public bool completed { get { return mCompleted; } }
        public float time { get { return mTime; } }
        public int stars { get { return mTime < parTime ? mStars + 1 : mStars; } }

        public void Load(int stageInd, int lvlInd) {
        }

        public void Complete(float timeElapsed, int starsCollected) {
            if(mStageInd >= 0 && mInd >= 0) {
            }
        }
    }

    [System.Serializable]
    public class StageData {
        public LevelData[] levels;

        public int completed {
            get {
                int ret = 0;
                foreach(LevelData lvl in levels) {
                    if(lvl.completed)
                        ret++;
                }

                return ret;
            }
        }
    }

    public StageData[] stages;

    private int mCurStage;
    private int mCurLevel;

    private static LevelManager mInstance;

    public static LevelManager instance { get { return mInstance; } }

    public string curLevelKey { get { return string.Format("level_{0}_{1}", mCurStage, mCurLevel); } }

    public int curStage {
        get { return mCurStage; }

        set {
            if(mCurStage != value) {
                mCurStage = Mathf.Clamp(value, 0, stages.Length - 1);
            }
        }
    }

    public int curLevel {
        get { return mCurLevel; }

        set {
            if(mCurLevel != value) {
                mCurLevel = Mathf.Clamp(value, 0, curStageData.levels.Length - 1);
            }
        }
    }

    public StageData curStageData { get { return stages[mCurStage]; } }
    public LevelData curLevelData { get { return stages[mCurStage].levels[mCurLevel]; } }

    public bool IsCurrentLevelLast() {
        return mCurLevel == curStageData.levels.Length - 1;
    }

    public void LoadCurrentLevel() {
        Main.instance.sceneManager.LoadScene(curLevelKey);
    }

    void OnDestroy() {
        if(mInstance == this)
            mInstance = null;
    }

    void Awake() {
        if(mInstance == null) {
            mInstance = this;


        }
    }

    // Use this for initialization
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }
}
