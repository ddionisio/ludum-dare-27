using UnityEngine;
using System.Collections;

public class VictoryTrigger : MonoBehaviour {
    public string victoryScene;
    public string levelSaveKey;

    void OnTriggerEnter(Collider col) {
        Player player = Player.instance;

        if(player.isGoal && col == player.controller.body.collider) {
            player.ExitToScene(victoryScene);
        }

        UserData.instance.SetInt(levelSaveKey, 1);
    }
}
