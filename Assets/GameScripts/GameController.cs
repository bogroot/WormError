using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public CameraFollow cameraController;
    private GameObject[] players;
    private GameObject mainPlayer;
    public GameObject firstPlayer;
    public GameObject secondPlayer;
    void Start()
    {
        mainPlayer = firstPlayer;
    }

    //切换角色
    public void changePlayer(GameObject newPlayer) {
        mainPlayer.tag = "Enemy";
        Destroy(mainPlayer.GetComponent<Player>());
        Destroy(mainPlayer.GetComponent<Controller2D>());
        if (!mainPlayer.GetComponent<Rigidbody2D>()) {
            mainPlayer.AddComponent<Rigidbody2D>();
        }
        mainPlayer = newPlayer;
        mainPlayer.tag = "Player";
        if (mainPlayer.GetComponent<Rigidbody2D>()) {
            Destroy(mainPlayer.GetComponent<Rigidbody2D>());
        }
        newPlayer.AddComponent<Player>();
        newPlayer.AddComponent<Controller2D>();
        cameraController.target = mainPlayer.GetComponent<Controller2D>();
    }
}
