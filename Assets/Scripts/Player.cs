using UnityEngine;
using System.Collections;

[RequireComponent (typeof (Controller2D))]
public class Player : MonoBehaviour {

    public float maxJumpHeight = 4;//跳跃高度
	public float minJumpHeight = 1;
    public float timeTojumpApex = 0.4f;//跳到最高点所需时间
    private float gravity;//重力
    public float moveSpeed = 6;//移动速度
    private float maxJumpVelocity;
	private float minJumpVelocity;
    private float velocityXSmoothing;//X方向上的速度平滑
    private float accelerationTimeAirborne = 0.2f;//空中的加速度
    private float accelerationTimeGrounded = 0.1f;//地面上的加速度
	public float wallSildeMaxSpeed = 3; //粘墙时的最大速度
	public Vector2 wallJumpClimb = new Vector2(7.5f, 16f);
	public Vector2 wallJumpOff = new Vector2(8.5f, 7f);
	public Vector2 wallLeap = new Vector2(18f, 17f);
	public float wallSitckTime = 0.25f;
	float timeToWallUnStick;
    Vector3 velocity;
    Controller2D controller;
	Vector2 directionalInput;
	bool wallSliding;
	int wallDirX;
	private Transform Launcher;

	void Start() {
		controller = GetComponent<Controller2D> ();
		Launcher = transform.Find("Launcher");
		gravity = -(2 * maxJumpHeight) / Mathf.Pow (timeTojumpApex, 2);
		maxJumpVelocity = Mathf.Abs(gravity) * timeTojumpApex;
		minJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(gravity) * minJumpHeight);
	}

	public void SetDirectionalInput(Vector2 input) {
		directionalInput = input;
	}

	public void OnJumpInputDown() {
		if (wallSliding) {
				if (wallDirX == directionalInput.x) {
					velocity.x = -wallDirX * wallJumpClimb.x;
					velocity.y = wallJumpClimb.y;
				} else if (directionalInput.x == 0) {
					velocity.x = -wallDirX * wallJumpOff.x;
					velocity.y = wallJumpOff.y;
				} else {
					velocity.x = -wallDirX * wallLeap.x;
					velocity.y = wallLeap.y;
				}
			}

			//普通跳跃
			if (controller.collisions.below) {
				velocity.y = maxJumpVelocity;
			}
	}

	public void OnJumpInputUp() {
		if (velocity.y > minJumpVelocity) {
			velocity.y = minJumpVelocity;
		}
	}

	public void Fire() {
		GameObject bullet = (GameObject)Instantiate(Resources.Load("Prefabs/Bullet"), Launcher.position, Launcher.rotation);
		bullet.GetComponent<Bullet>().Fly();
	}

	//改变朝向
	public void Filp(Vector2 playerInput) {
		if (playerInput.x > 0) {
			if (GetComponent<SpriteRenderer>().flipX == false) {
				GetComponent<SpriteRenderer>().flipX = true;
			}
			
		} else if (playerInput.x < 0) {
			if (GetComponent<SpriteRenderer>().flipX == true) {
				GetComponent<SpriteRenderer>().flipX = false;
			}
		}
	}

	void Update() {
		CalculateVelocity();
		HandleWallSilding();
		directionalInput = new Vector2 (Input.GetAxisRaw ("Horizontal"), Input.GetAxisRaw ("Vertical"));
		Filp(directionalInput);

        if (Input.GetKeyDown(KeyCode.Space)) {
            OnJumpInputDown();
        }

        if (Input.GetKeyUp(KeyCode.Space)) {
            OnJumpInputUp();
        }

		 if (Input.GetKeyUp(KeyCode.Tab)) {
            Fire();
        }
		controller.Move (velocity * Time.deltaTime, directionalInput);

		//挤压缓冲
		if (controller.collisions.above || controller.collisions.below) {
			velocity.y = 0;
		}
	}

	//处理爬墙操作
	void HandleWallSilding() {
		wallDirX = (controller.collisions.left) ? -1 : 1;
		wallSliding = false;

		if ((controller.collisions.left || controller.collisions.right) && !controller.collisions.below && velocity.y < 0) {
			wallSliding = true;

			if (timeToWallUnStick > 0) {
				velocityXSmoothing = 0;
				velocity.x = 0;
				if (directionalInput.x != wallDirX && directionalInput.x != 0) {
					timeToWallUnStick -= Time.deltaTime;
				} else {
					timeToWallUnStick = wallSitckTime;
				}
			} 
			else {
				timeToWallUnStick = wallSitckTime;
			}

			if (velocity.y < - wallSildeMaxSpeed) {
				velocity.y = -wallSildeMaxSpeed;
			}
		}

	}

	//计算速度
	void CalculateVelocity() {
		//平滑速度
		float targetVelocityX = directionalInput.x * moveSpeed;
		velocity.x = Mathf.SmoothDamp (velocity.x, targetVelocityX, ref velocityXSmoothing, (controller.collisions.below)?accelerationTimeGrounded:accelerationTimeAirborne);
		velocity.y += gravity * Time.deltaTime;
	}
}
