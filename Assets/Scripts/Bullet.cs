using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    Rigidbody2D rig;
    public float velocity = 3;
    private GameController gameController;
    private void Start()
    {   
        gameController = GameObject.Find("GameController").GetComponent<GameController>();
        rig = GetComponent<Rigidbody2D>();
    }
   public void Fly() {
       GetComponent<Rigidbody2D>().velocity = new Vector3(velocity, 0, 0);
       if (this.gameObject == true) {
            Destroy(this.gameObject, 5);
       }
       
   }

   private void OnCollisionEnter2D(Collision2D  other)
   {
       if (other.gameObject.tag == "Enemy") {
           Debug.Log("hit the target");
           gameController.changePlayer(other.gameObject);
           Destroy(this.gameObject);
       }
   }

  
}
