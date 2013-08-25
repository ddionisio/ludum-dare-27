public class ModalHelp : UIController {

    protected override void OnActive(bool active) {
        if(active) {
            UICamera.selectedObject = gameObject;
        }
        else {
        }
    }

    protected override void OnOpen() {
    }

    protected override void OnClose() {
    }

    void OnClick() {
        UIModalManager.instance.ModalCloseTop();
    }
}
