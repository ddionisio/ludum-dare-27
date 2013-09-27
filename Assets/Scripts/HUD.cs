using UnityEngine;
using System.Collections;

public class HUD : MonoBehaviour {

    //
    public NGUIAttach bombTimerAttach;

    public NGUIPointAt bombOffScreen;

    public NGUIPointAt bombOffScreenExit;

    public NGUIPointAt targetOffScreen;

    //
    public UILabel tickerLabel;

    public Color[] tickerColors;

    public UILabel timerLabel;

    public UISprite timerStar;

    public StarItem[] stars;

    public GameObject[] grabInfo;

    //
    private int mStarsFilled = 0;
    private bool mStarTimeChanged;
    private string mPrevTimerStarSprite;

    public static HUD GetHUD() {
        HUD ret = null;

        GameObject hudGO = GameObject.FindGameObjectWithTag("HUD");
        ret = hudGO.GetComponent<HUD>();

        return ret;
    }

    public int starsFilled { get { return mStarsFilled; } }

    public void ResetData() {
        RefreshTimer(0.0f);

        foreach(StarItem star in stars) {
            if(star)
                star.ResetData();
        }

        timerStar.spriteName = mPrevTimerStarSprite;

        tickerLabel.gameObject.SetActive(false);
    }

    public void RefreshTimer(float t) {
        if(timerLabel)
            timerLabel.text = LevelManager.GetTimeText(t);

        //check par time
        if(!mStarTimeChanged) {
            LevelManager.LevelData lvl = LevelManager.instance.curLevelData;

            if(t > lvl.parTime) {
                timerStar.spriteName = "icons_star_empty";
                mStarTimeChanged = true;
            }
        }
    }

    public void StarFillTo(int amt) {
        for(int i = 0; i < amt; i++) {
            stars[i].Fill();
        }

        for(int i = amt; i < stars.Length; i++) {
            stars[i].ResetData();
        }

        mStarsFilled = amt;
    }

    public void StarFill() {
        if(mStarsFilled < stars.Length) {
            stars[mStarsFilled].Fill();
            mStarsFilled++;
        }
    }

    public void UpdateTicker(float curTime, float maxTime) {
        bool doUpdate = curTime < maxTime;
        tickerLabel.gameObject.SetActive(doUpdate);

        if(doUpdate) {
            tickerLabel.text = Mathf.CeilToInt(curTime).ToString();
            tickerLabel.color = M8.ColorUtil.Lerp(tickerColors, 1.0f - curTime / maxTime);
        }
    }

    void Awake() {
        bombTimerAttach.gameObject.SetActive(false);
        bombOffScreen.gameObject.SetActive(false);
        bombOffScreenExit.gameObject.SetActive(false);
        targetOffScreen.gameObject.SetActive(false);

        tickerLabel.gameObject.SetActive(false);

        mPrevTimerStarSprite = timerStar.spriteName;
    }

    // Use this for initialization
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }
}
