﻿using UnityEngine;
using System.Collections;

public class ModalMain : UIController {
    public UIEventListener play;
    public UIEventListener options;
    public UIEventListener help;
    public UIEventListener credits;

    protected override void OnActive(bool active) {
        if(active) {
            play.onClick = OnPlay;
            options.onClick = OnOptions;
            help.onClick = OnHelp;
            credits.onClick = OnCredits;
        }
        else {
            play.onClick = null;
            options.onClick = null;
            help.onClick = null;
            credits.onClick = null;
        }
    }

    protected override void OnOpen() {
    }

    protected override void OnClose() {
    }

    void OnPlay(GameObject go) {
        Main.instance.sceneManager.LoadScene("levelSelect");
    }

    void OnOptions(GameObject go) {
        UIModalManager.instance.ModalOpen("options");
    }

    void OnHelp(GameObject go) {
        UIModalManager.instance.ModalOpen("help");
    }

    void OnCredits(GameObject go) {
        UIModalManager.instance.ModalOpen("credits");
    }
}
