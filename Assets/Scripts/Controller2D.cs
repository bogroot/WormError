using UnityEngine;
using System.Collections;


public class Controller2D : RaycastController
{
    public float maxClimpAngle = 75;
    public float maxDescendAngle = 60;


    public CollisionInfo collisions;
    [HideInInspector]
    public Vector2 playerInput;

    //重载
    public override void Start()
    {
        base.Start();
        collisions.faceDir = 1;
    }

    public void Move(Vector3 velocity, bool standingOnPlatform) {
        Move(velocity, Vector2.zero, standingOnPlatform);
    }

    //移动方法
    public void Move(Vector3 velocity, Vector2 input, bool standingOnPlatform = false)
    {
        collisions.Reset();
        UpdateRaycastOrigins();
        collisions.velocityOld = velocity;
        playerInput = input;

        if (velocity.y < 0)
        {
            DescendSlope(ref velocity);
        }

        //判断朝向
        if (velocity.x != 0) {
            collisions.faceDir = Mathf.Sign(velocity.x);
        }
        HorizontalCollisions(ref velocity);

        if (velocity.y != 0)
        {
            VerticalCollisions(ref velocity);
        }
        transform.Translate(velocity);

        //在platform上也能跳跃
        if(standingOnPlatform) {
            collisions.below = true;
        }       
    }


    //水平方向上的碰撞检测
    void HorizontalCollisions(ref Vector3 velocity)
    {
        float directionX = collisions.faceDir;
        float rayLength = Mathf.Abs(velocity.x) + skinWidth;

        //爬墙跳的检测
        if (Mathf.Abs(velocity.x ) < skinWidth) {
            rayLength = 2 * skinWidth;
        }

        for (int i = 0; i < horizontalRayCount; i++)
        {
            Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
            rayOrigin += Vector2.up * (horizontalRaySpacing * i);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);
            // Debug.DrawRay(rayOrigin, Vector2.right * directionX * rayLength * 10, Color.red);

            if (hit)
            {
                //当platform“挤压”player时，跳过检测，使之能够运动
                if(hit.distance == 0) {
                    continue;
                }
                //斜面倾斜角
                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                if (i == 0 && slopeAngle <= maxClimpAngle)
                {
                    //特殊情况，当player加载两个斜坡间的时候，可能既处于爬坡状态，也处于下坡状态
                    if (collisions.descendingSlopes)
                    {
                        collisions.descendingSlopes = false;
                        velocity = collisions.velocityOld;
                    }

                    float distanceToSlopeStart = 0;
                    //平面到坡或者坡到坡的过渡
                    if (slopeAngle != collisions.slopeAngleOld)
                    {
                        distanceToSlopeStart = hit.distance - skinWidth;
                        velocity.x -= distanceToSlopeStart * directionX;
                    }
                    ClimbSlope(ref velocity, slopeAngle);
                    velocity.x += distanceToSlopeStart * directionX;
                }

                //未达到爬坡条件
                if (!collisions.climbingSlope || slopeAngle > maxClimpAngle)
                {
                    velocity.x = (hit.distance - skinWidth) * directionX;
                    //避免重复检测多个碰撞体
                    rayLength = hit.distance;

                    //在坡上被障碍物阻隔，缓冲其速度，避免鬼畜
                    if (collisions.climbingSlope)
                    {
                        velocity.y = velocity.x * Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad);
                    }

                    collisions.left = directionX == -1;
                    collisions.right = directionX == 1;
                }

            }
        }
    }

    //竖直方向上的碰撞检测
    void VerticalCollisions(ref Vector3 velocity)
    {
        float directionY = Mathf.Sign(velocity.y);
        float rayLength = Mathf.Abs(velocity.y) + skinWidth;
        for (int i = 0; i < verticalRayCount; i++)
        {
            Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
            rayOrigin += Vector2.right * (verticalRaySpacing * i + velocity.x);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, collisionMask);
            // Debug.DrawRay(rayOrigin, Vector2.up * directionY * rayLength * 10, Color.red);

            if (hit)
            {
                //碰撞特殊处理
                if (hit.collider.tag == "Through") {
                    if (directionY == 1 || hit.distance == 0) {
                        continue;
                    } 
                    if (collisions.fallingThroughPlatform) {
                        continue;
                    }

                    if (playerInput.y == -1) {
                         collisions.fallingThroughPlatform = true;
                         Invoke("RsetFallingThroughPlatform", 0.5f);
                         continue;
                     }
                }

                velocity.y = (hit.distance - skinWidth) * directionY;
                //避免重复检测多个碰撞体
                rayLength = hit.distance;

                //下坡速度缓冲
                if (collisions.climbingSlope)
                {
                    velocity.x = velocity.y / Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Sign(velocity.x);
                }

                collisions.below = directionY == -1;
                collisions.above = directionY == 1;
            }
        }

        //针对两个斜坡相交处的缓冲处理(暂无觉得有处理的必要)
        if (collisions.climbingSlope)
        {
            float directionX = Mathf.Sign(velocity.x);
            rayLength = Mathf.Abs(velocity.x) + skinWidth;
            Vector2 Origin;
            if (directionX == -1)
            {
                Origin = raycastOrigins.bottomLeft + Vector2.up * velocity.y;

            }
            else
            {
                Origin = raycastOrigins.bottomRight + Vector2.up * velocity.y;

            }
            RaycastHit2D hitInfo = Physics2D.Raycast(Origin, Vector2.right * directionX, rayLength, collisionMask);
            if (hitInfo)
            {
                float slopeAngle = Vector2.Angle(hitInfo.normal, Vector2.up);
                if (slopeAngle != collisions.slopeAngle)
                {
                    velocity.x = (hitInfo.distance - skinWidth) * directionX;
                    collisions.slopeAngle = slopeAngle;
                }
            }
        }
    }


    //爬坡检测
    void ClimbSlope(ref Vector3 velocity, float slopeAngle)
    {
        //速度分解
        //float moveDistance = Mathf.Abs(Mathf.Sqrt(velocity.x * velocity.x + velocity.y * velocity.y));
        float moveDistance = Mathf.Abs(velocity.x);
        float climbVelocityY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;
        if (velocity.y <= climbVelocityY)
        {
            velocity.y = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;
            velocity.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(velocity.x);
            collisions.below = true;
            collisions.climbingSlope = true;
            collisions.slopeAngle = slopeAngle;
        }
        else
        {
            print("jump on the slope");
        }

    }

    //下坡检测
    void DescendSlope(ref Vector3 velocity)
    {
        float directionX = Mathf.Sign(velocity.x);
        Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomRight : raycastOrigins.bottomLeft;
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, Mathf.Infinity, collisionMask);
        if (hit)
        {
            float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
            if (slopeAngle != 0 && slopeAngle <= maxDescendAngle)
            {
                //判断操作方向是否和与理应下坡方向相同
                if (Mathf.Sign(hit.normal.x) == directionX)
                {
                    //保证与斜坡足够的近且Y轴速度足够
                    if (hit.distance - skinWidth <= Mathf.Tan(slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(velocity.x))
                    {
                        //速度分解
                        float moveDistance = Mathf.Abs(velocity.x);
                        float descendVelocityY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;
                        velocity.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(velocity.x);
                        velocity.y -= descendVelocityY;

                        //更新碰撞信息
                        collisions.slopeAngle = slopeAngle;
                        collisions.descendingSlopes = true;
                        collisions.below = true;
                    }
                }
            }
        }
    }

    void RsetFallingThroughPlatform() {
        collisions.fallingThroughPlatform = false;
    }

    //碰撞信息
    public struct CollisionInfo
    {
        public bool above, below;
        public bool left, right;

        public bool climbingSlope;
        public bool descendingSlopes;
        public float slopeAngle, slopeAngleOld;
        public Vector3 velocityOld;
        public float faceDir;
        public bool fallingThroughPlatform; //是否穿过platform

        public void Reset()
        {
            above = below = false;
            left = right = false;
            climbingSlope = false;
            descendingSlopes = false;
            slopeAngleOld = slopeAngle;
            slopeAngle = 0;
        }
    }
}
