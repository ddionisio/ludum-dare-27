using UnityEngine;
using System.Collections;

public class ModalIngame : UIController {
    public UIEventListener options;
    public UIEventListener help;
    public UIEventListener restart;
    public UIEventListener exit;

    protected override void OnActive(bool active) {
        if(active) {
            options.onClick = OnOptions;
            help.onClick = OnHelp;
            restart.onClick = OnRestart;
            exit.onClick = OnExit;
        }
        else {
            options.onClick = null;
            help.onClick = null;
            restart.onClick = null;
            exit.onClick = null;
        }
    }

    protected override void OnOpen() {
        Main.instance.sceneManager.Pause();
    }

    protected override void OnClose() {
        Main.instance.sceneManager.Resume();
    }

    void OnOptions(GameObject go) {
        UIModalManager.instance.ModalOpen("options");
    }

    void OnHelp(GameObject go) {
        UIModalManager.instance.ModalOpen("help");
    }

    void OnRestart(GameObject go) {
        UIModalConfirm.Open("RESTART", null,
            delegate(bool yes) {
                if(yes)
                    Main.instance.sceneManager.Reload();
            });
    }

    void OnExit(GameObject go) {
        UIModalConfirm.Open("EXIT", null,
            delegate(bool yes) {
                if(yes)
                    Main.instance.sceneManager.LoadScene("levelSelect");
            });
    }
}
