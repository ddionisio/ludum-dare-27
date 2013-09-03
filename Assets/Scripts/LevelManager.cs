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

        public int stage { get { return mStageInd; } }
        public int level { get { return mInd; } }

        public bool completed { get { return mStars >= 0; } }
        public float time { get { return mTime; } }
        public int stars { get { return completed ? mTime < parTime ? mStars + 1 : mStars : 0; } }

        public string key { get { return string.Format("level{0}_{1}", mStageInd, mInd); } }

        public string userKey { get { return string.Format("l{0}_{1}", mStageInd, mInd); } }

        public string timeText { get { return completed ? GetTimeText(mTime) : "***.**"; } }

        public string parTimeText { get { return GetTimeText(parTime); } }

        public string levelText {
            get {
                //1 - 999.99 : 999.99
                return string.Format("{0} - {1} : {2}", mInd + 1, timeText, parTimeText);
            }
        }

        public void Load(int stageInd, int lvlInd) {
            mStageInd = stageInd;
            mInd = lvlInd;

            mTime = UserData.instance.GetFloat(userKey + "t", 99999.0f);
            mStars = UserData.instance.GetInt(userKey + "s", -1);
        }

        public void Complete(float timeElapsed, int starsCollected) {
            if(mStageInd >= 0 && mInd >= 0) {
                mTime = timeElapsed;
                mStars = starsCollected;

                UserData.instance.SetFloat(userKey + "t", timeElapsed);
                UserData.instance.SetInt(userKey + "s", starsCollected);
            }
        }
    }

    [System.Serializable]
    public class StageData {
        public LevelData[] levels;

        private int mStageInd = -1;

        private bool mUnlocked;

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

        public bool unlocked { get { return mUnlocked; } }

        public void Load(StageData prevStage, int stageInd) {
            mStageInd = stageInd;

            for(int i = 0; i < levels.Length; i++) {
                levels[i].Load(stageInd, i);
            }

            mUnlocked = prevStage == null || (prevStage.levels.Length > 0 && prevStage.completed == prevStage.levels.Length);
        }
    }

    public StageData[] stages;

    private int mCurStage;
    private int mCurLevel;

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
    /// Call once level is completed, will load to next scene
    /// </summary>
    public void LevelComplete(float timeElapsed, int starCollected) {

        curLevelData.Complete(timeElapsed, starCollected);

        //next level
        if(IsCurrentLevelLast()) {
            curStage = nextStage;
            mCurLevel = 0;

            //show stage end cutscene?

            Main.instance.sceneManager.LoadScene("levelSelect");
        }
        else {
            mCurLevel++;

            LoadCurrentLevel();
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
        }
    }
}
