using UnityEngine;
using System.Collections;

public class VictoryTrigger : MonoBehaviour {
    public string victoryScene = "victory";

    void OnTriggerEnter(Collider col) {
        UserData.instance.SetInt(Application.loadedLevelName, 1);

        Player player = Player.instance;

        if(player.isGoal && col == player.controller.body.collider) {
            player.ExitToScene(victoryScene);
        }
    }
}
