using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    Rigidbody2D rig;
    public float velocity = 5;
    private GameController gameController;
    private void Start()
    {   
        gameController = GameObject.Find("GameController").GetComponent<GameController>();
        rig = GetComponent<Rigidbody2D>();
    }
   public void Fire() {
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
       else if (other.gameObject.tag == "Obstacle") {
           Debug.Log("hit the Obstacle");
           Destroy(this.gameObject);
       }
   }

   private void Update() {
       transform.Translate(Vector3.right * velocity * Time.deltaTime);
   }

  
}
