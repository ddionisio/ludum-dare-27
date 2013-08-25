using UnityEngine;
using System.Collections;

public class ModalLevelSelect : UIController {

    protected override void OnActive(bool active) {
        if(active) {
            Main.instance.input.AddButtonCall(0, InputAction.MenuEscape, OnEsc);
        }
        else {
            Main.instance.input.RemoveButtonCall(0, InputAction.MenuEscape, OnEsc);
        }
    }

    protected override void OnOpen() {
    }

    protected override void OnClose() {
    }

    void OnEsc(InputManager.Info dat) {
        if(dat.state == InputManager.State.Pressed) {
            UIModalManager.instance.ModalOpen("menu");
        }
    }
}
