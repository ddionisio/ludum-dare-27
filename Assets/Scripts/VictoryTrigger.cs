using UnityEngine;
using System.Collections;

public class VictoryTrigger : MonoBehaviour {
    public string victoryScene = "victory";

    void OnTriggerEnter(Collider col) {
        int prev = UserData.instance.GetInt(Application.loadedLevelName, 0);

        UserData.instance.SetInt(Application.loadedLevelName, 1);

        if(prev <= 0) {
            int complete = SceneState.instance.GetGlobalValue("numLevelComplete");
            SceneState.instance.SetGlobalValue("numLevelComplete", complete + 1, false);
        }

        Player player = Player.instance;

        if(player.isGoal && col == player.controller.body.collider) {
            //player.ExitToScene(victoryScene);
            player.state = (int)Player.State.Victory;
        }
    }
}
