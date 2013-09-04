using UnityEngine;
using System.Collections;

public class LevelManager : MonoBehaviour {
    [System.Serializable]
    public class LevelData {
        public float parTime;

        private float mTime;

        private int mStars = -1;

        private int mStageInd = -1;
        private int mInd = -1;
        private int mNumDeath = -1;

        private bool mIsNewTime;
        private bool mIsNewDeath;
        private bool mIsNewStar;

        public int stage { get { return mStageInd; } }
        public int level { get { return mInd; } }

        public bool completed { get { return mStars >= 0; } }
        public float time { get { return mTime; } }
        public int deathCount { get { return mNumDeath; } }
        public int stars {
            get {
                if(completed) {
                    int ret = mStars;

                    if(mTime < parTime)
                        ret++;

                    if(mNumDeath == 0)
                        ret++;

                    return ret;
                }
                else
                    return 0;
            }
        }

        /// <summary>
        /// After completing this level, is this the new best time?
        /// </summary>
        public bool isNewTime { get { return mIsNewTime; } }

        /// <summary>
        /// After completing this level, is this the new best death?
        /// </summary>
        public bool isNewDeath { get { return mIsNewDeath; } }

        /// <summary>
        /// After completing this level, is this the new best time?
        /// </summary>
        public bool isNewStar { get { return mIsNewStar; } }

        public string key { get { return string.Format("level{0}_{1}", mStageInd, mInd); } }

        public string userKey { get { return string.Format("l{0}_{1}", mStageInd, mInd); } }

        public string timeText { get { return completed ? GetTimeText(mTime) : "---.--"; } }

        public string parTimeText { get { return GetTimeText(parTime); } }

        public string levelTextTitle {
            get {
                //1 - 999.99 : 999.99
                return string.Format("{0}. {1}", mInd + 1, GameLocalize.GetText(key));
            }
        }

        public string levelTextInfo {
            get {
                return string.Format("{0} : {1}", timeText, parTimeText);
            }
        }

        public void Load(int stageInd, int lvlInd) {
            mStageInd = stageInd;
            mInd = lvlInd;

            mTime = UserData.instance.GetFloat(userKey + "t", 99999.0f);
            mStars = UserData.instance.GetInt(userKey + "s", -1);
            mNumDeath = UserData.instance.GetInt(userKey + "d", -1);
        }

        public void Complete(float timeElapsed, int starsCollected, int numDeath) {
            if(mStageInd >= 0 && mInd >= 0) {
                mIsNewTime = timeElapsed < mTime;
                if(mIsNewTime) {
                    mTime = timeElapsed;
                    UserData.instance.SetFloat(userKey + "t", timeElapsed);
                }

                mIsNewStar = mStars < starsCollected;
                if(mIsNewStar) {
                    mStars = starsCollected;
                    UserData.instance.SetInt(userKey + "s", starsCollected);
                }

                mIsNewDeath = mNumDeath == -1 || numDeath < mNumDeath;
                if(mIsNewDeath) {
                    mNumDeath = numDeath;
                    UserData.instance.SetInt(userKey + "d", numDeath);
                }
            }
        }
    }

    [System.Serializable]
    public class StageData {
        public int starRequire = 0;
        public LevelData[] levels;

        private int mStageInd = -1;

        public int stage { get { return mStageInd; } }

        public string planetRef { get { return stage.ToString(); } }

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

        //stage_{0}_title
        public string title { get { return GameLocalize.GetText(string.Format("stage_{0}_title", stage)); } }

        public string desc { get { return GameLocalize.GetText(string.Format("stage_{0}_desc", stage)); } }

        public bool unlocked {
            get {
                return LevelManager.instance.mTotalStars >= starRequire;
            }
        }

        public void Load(StageData prevStage, int stageInd) {
            mStageInd = stageInd;

            for(int i = 0; i < levels.Length; i++) {
                levels[i].Load(stageInd, i);
            }
        }
    }

    public float scorePerStar = 1000.0f;
    public float scorePerLevelComplete = 200.0f;
    public float scorePerUnderParSecond = 500.0f;

    public StageData[] stages;

    private int mCurStage;
    private int mCurLevel;

    private int mTotalStars = 0;
    private float mHiScore = 0;

    private static LevelManager mInstance;

    public static LevelManager instance { get { return mInstance; } }

    public string curLevelKey { get { return curLevelData.key; } }

    public int curStage {
        get { return mCurStage; }

        set {
            if(mCurStage != value) {
                mCurStage = Mathf.Clamp(value, 0, stages.Length - 1);
            }
        }
    }

    public int prevStage {
        get {
            int ind = mCurStage - 1;
            if(ind < 0) ind = stages.Length - 1;
            return ind;
        }
    }

    public int nextStage {
        get {
            int ind = mCurStage + 1;
            if(ind >= stages.Length) ind = 0;
            return ind;
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

    public int totalScore { get { return Mathf.RoundToInt(mHiScore); } }
    public int totalStars { get { return mTotalStars; } }

    public static string GetTimeText(float time) {
        int centi = Mathf.FloorToInt((time - Mathf.Floor(time)) * 100.0f);
        int sec = Mathf.FloorToInt(time);

        return string.Format("{0:D3}.{1:D2}", sec, centi);
    }

    public StageData curStageData { get { return stages[mCurStage]; } }

    public StageData prevStageData { get { return stages[prevStage]; } }

    public StageData nextStageData { get { return stages[nextStage]; } }

    public LevelData curLevelData { get { return stages[mCurStage].levels[mCurLevel]; } }

    public bool IsCurrentLevelLast() {
        return mCurLevel == curStageData.levels.Length - 1;
    }

    public void LoadCurrentLevel() {
        Main.instance.sceneManager.LoadScene(curLevelKey);
    }

    /// <summary>
    /// Save data
    /// </summary>
    public void LevelComplete(float timeElapsed, int starCollected, int numDeath) {

        curLevelData.Complete(timeElapsed, starCollected, numDeath);

        ComputeData();
    }

    /// <summary>
    /// Call this after LevelComplete when ready to go to next stage, or intermission, or ending
    /// </summary>
    public void LevelGotoNext() {
        //ending?
        if(mCurStage == stages.Length - 1) {
            mCurStage = 0;
            mCurLevel = 0;

            Main.instance.sceneManager.LoadScene("ending");
        }
        else {
            //next level
            if(IsCurrentLevelLast()) {
                curStage = nextStage;
                mCurLevel = 0;

                //show stage end cutscene?

                LoadCurrentLevel();
            }
            else {
                mCurLevel++;

                LoadCurrentLevel();
            }
        }
    }

    void OnDestroy() {
        if(mInstance == this)
            mInstance = null;
    }

    void Awake() {
        if(mInstance == null) {
            mInstance = this;

            StageData prevStage = null;

            for(int i = 0; i < stages.Length; i++) {
                StageData stage = stages[i];

                stage.Load(prevStage, i);

                prevStage = stage;
            }

            ComputeData();

#if UNITY_EDITOR
            string lvlStr = Application.loadedLevelName;
            int lvlInd = lvlStr.LastIndexOf('l');
            if(lvlInd > 0) {
                int midInd = lvlStr.LastIndexOf('_');
                if(midInd > 0) {
                    mCurStage = int.Parse(lvlStr.Substring(lvlInd + 1, midInd - lvlInd - 1));
                    mCurLevel = int.Parse(lvlStr.Substring(midInd + 1, lvlStr.Length - midInd - 1));
                }
            }
#endif
        }
    }

    void ComputeData() {
        mHiScore = 0;
        mTotalStars = 0;

        foreach(StageData stage in stages) {
            foreach(LevelData level in stage.levels) {
                if(level.stars > 0) {
                    mTotalStars += level.stars;
                    mHiScore += level.stars * scorePerStar;
                }

                if(level.time > 0.0f && level.time < level.parTime) {
                    mHiScore += (level.parTime - level.time) * scorePerUnderParSecond;
                }

                if(level.completed)
                    mHiScore += scorePerLevelComplete;
            }
        }
    }
}
