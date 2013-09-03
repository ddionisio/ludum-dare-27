using UnityEngine;
using System.Collections;

public class HUD : MonoBehaviour {

    public NGUIAttach bombTimerAttach;
    public UILabel bombTimerCountLabel;

    public NGUIPointAt bombOffScreen;
    public UILabel bombOffScreenLabel;

    public NGUIPointAt bombOffScreenExit;
    public UILabel bombOffScreenExitLabel;

    public NGUIPointAt targetOffScreen;

    public UILabel timerLabel;

    public StarItem[] stars;

    private int mStarsFilled = 0;

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
    }

    public void RefreshTimer(float t) {
        if(timerLabel)
            timerLabel.text = LevelManager.GetTimeText(t);
    }

    public void StarFill() {
        if(mStarsFilled < stars.Length) {
            stars[mStarsFilled].Fill();
            mStarsFilled++;
        }
    }

    void Awake() {
        bombTimerAttach.gameObject.SetActive(false);
        bombOffScreen.gameObject.SetActive(false);
        bombOffScreenExit.gameObject.SetActive(false);
        targetOffScreen.gameObject.SetActive(false);
    }

    // Use this for initialization
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }
}
