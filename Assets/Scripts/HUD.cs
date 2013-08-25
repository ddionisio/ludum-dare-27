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

    public static HUD GetHUD() {
        HUD ret = null;

        GameObject hudGO = GameObject.FindGameObjectWithTag("HUD");
        ret = hudGO.GetComponent<HUD>();

        return ret;
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
