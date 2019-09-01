using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pointers : MonoBehaviour
{
    public float rotateSpeed = 10.0f;
    public Transform launcher;

    //开火
    public void LaunchBullet() {
        GameObject bullet = (GameObject)Instantiate(Resources.Load("Prefabs/Bullet"), launcher.position + new Vector3(0.5f ,0 ,0), launcher.rotation);
        bullet.GetComponent<Bullet>().Fire();
        Destroy(this.gameObject);
    }

    private void Update()
    {
        transform.transform.Rotate(0f, 0f, - (360 / rotateSpeed)*Time.deltaTime);
    }
}
