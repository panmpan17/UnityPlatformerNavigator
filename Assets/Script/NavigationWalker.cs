using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MPJamPack;
using MPJamPack.Aseprite;

public class NavigationWalker : MonoBehaviour
{
    private const string AKIdle = "Idle",
                         AKWalk = "Walk",
                         AKJump = "Jump",
                         AKLand = "Land",
                         AKClimb = "Climb",
                         AKClimbIdle = "ClimbIdle";

    private AseAnimator animator;
    private SmartBoxCollider boxCollider;
    private new Rigidbody2D rigidbody;
    private float gravityScale;

    [SerializeField]
    private bool spriteFaceRight;
    [SerializeField]
    private float walkSpeed, climbSpeed;
    [SerializeField]
    private float jumpForce;
    [SerializeField]
    private float airSpeedMultiplier;
    [SerializeField]
    private Timer landWait;
    private bool moving, jumping, jumpStart, climbing;

    private bool waitOnGround;
    private Vector3Int destination;

    [SerializeField]
    private GameObject destinationIndicator;

    private Route[] routes;
    private Vector3[] routePositions;
    private int routeI;

    private void Awake() {
        animator = GetComponent<AseAnimator>();
        boxCollider = GetComponent<SmartBoxCollider>();
        rigidbody = GetComponent<Rigidbody2D>();
        gravityScale = rigidbody.gravityScale;

        landWait.Running = false;
    }

    private void Turn(int direction) {
        if (direction == 0)
            return;

        if (spriteFaceRight)
        {
            bool nowFaceRight = transform.localScale.x > 0;
            if (nowFaceRight != direction > 0)
                transform.localScale = new Vector3(
                    direction > 0? Mathf.Abs(transform.localScale.x): -Mathf.Abs(transform.localScale.x),
                    transform.localScale.y,
                    transform.localScale.z);
        }
        else {
            bool nowFaceRight = transform.localScale.x < 0;
            if (nowFaceRight != direction > 0)
                transform.localScale = new Vector3(
                    direction > 0 ? -Mathf.Abs(transform.localScale.x) : Mathf.Abs(transform.localScale.x),
                    transform.localScale.y,
                    transform.localScale.z);
        }
    }

    public void TryWalkTo(Vector3Int _destination) {
        if (!boxCollider.DownTouched && !climbing)
        {
            waitOnGround = true;
            destination = _destination;
            return;
        }
        else
            WalkTo(_destination);
    }

    public void WalkTo(Vector3Int _destination) {
        Vector3Int position = NavigationManager.ins.EnvMap.WorldToCell(transform.position);

        if (!climbing)
            position.y -= 1;
        routes = NavigationManager.ins.FindNavigateRoute(position, _destination);

        for (int i = 0; i < routes.Length; i++) {
            Debug.LogFormat("{0} {1}", routes[i].Type, routes[i].Position);
        }

        if (routes.Length > 1)
        {
            routePositions = new Vector3[routes.Length];

            for (int i = 0; i < routes.Length; i++) {
                if (routes[i].Type == RouteType.Ground)
                    routes[i].Position.y += 1;

                routePositions[i] = NavigationManager.ins.EnvMap.CellToWorld(routes[i].Position) + new Vector3(0.5f, 0);
            }

            destinationIndicator.transform.position = routePositions[routePositions.Length - 1];
            destinationIndicator.SetActive(true);

            moving = true;
            routeI = 1;

            Turn(routePositions[routeI].x.CompareTo(routePositions[routeI - 1].x));

            if (routes[routeI - 1].Type == RouteType.Ground)
                animator.PlayAnimation(AKWalk);
            else
                animator.PlayAnimation(AKClimb);
        }
        else if (moving) {
            moving = false;
            destinationIndicator.SetActive(false);
            animator.PlayAnimation(AKIdle);
        }
    }

    private void FixedUpdate() {
        if (waitOnGround && boxCollider.DownTouched) {
            waitOnGround = false;
            WalkTo(destination);
            return;
        }

        if (landWait.Running) {
            if (landWait.FixedUpdateEnd) {
                landWait.Running = false;
                animator.PlayAnimation(moving? AKWalk: AKIdle);
            }
            return;
        }

        if (jumping) {
            if (jumpStart) {
                if (boxCollider.DownTouched) {
                    jumping = false;

                    animator.PlayAnimation(AKLand);
                    landWait.Reset();
                    return;
                }
            }
            else {
                if (!boxCollider.DownTouched)
                    jumpStart = true;
            }
        }
    
        if (moving)
            HandleMovement();
    }

    #region Movement Handling
    private Vector2 ResolveVelocity(Vector2 velocity) {
        if (velocity.x < 0 && boxCollider.LeftTouched) velocity.x = 0;
        if (velocity.x > 0 && boxCollider.RightTouched) velocity.x = 0;

        if (velocity.y < 0 && boxCollider.DownTouched) velocity.y = 0;
        if (velocity.y > 0 && boxCollider.UpTouched) velocity.y = 0;

        return velocity;
    }

    private void HandleMovement() {
        if (routes[routeI].Type == RouteType.Ground)
            HandleTwoGroundMoving();
        else if (routes[routeI].Type == RouteType.Ladder)
            HandleLadderClimbing();
    }
    
    private void HandleNextMovenment() {
        if (routeI + 1 >= routePositions.Length)
        {
            moving = false;
            destinationIndicator.SetActive(false);

            if (climbing)
                animator.PlayAnimation(AKClimbIdle);
            else
                animator.PlayAnimation(AKIdle);

            rigidbody.velocity = new Vector2(0, 0);
        }
        else {
            switch (routes[routeI].Type)
            {
                case RouteType.Ground:
                    if (routes[routeI + 1].Type == RouteType.Ladder){
                        animator.PlayAnimation(AKClimb);
                        rigidbody.velocity = Vector2.zero;
                        rigidbody.gravityScale = 0;
                        boxCollider.Collider.enabled = false;
                        climbing = true;
                    }
                    break;
                case RouteType.Ladder:
                    if (routes[routeI + 1].Type == RouteType.Ground) {
                        animator.PlayAnimation(AKWalk);
                        rigidbody.velocity = Vector2.zero;
                        rigidbody.gravityScale = gravityScale;
                        boxCollider.Collider.enabled = true;
                        climbing = false;
                    }
                    break;
            }

            routeI++;
            Turn(routePositions[routeI].x.CompareTo(routePositions[routeI - 1].x));
        }
    }

    private void HandleTwoGroundMoving() {
        float routeDelta = routePositions[routeI].x - routePositions[routeI - 1].x;
        float progress;

        if (routeDelta == 0)
            progress = 1;
        else
            progress = (transform.position.x - routePositions[routeI - 1].x) / routeDelta;

        if (progress < 1) {
            Vector2 velocity = rigidbody.velocity;
            velocity.x = routePositions[routeI].x > routePositions[routeI - 1].x? walkSpeed: -walkSpeed;
            if (jumping)
                velocity.x *= airSpeedMultiplier;

            if (routePositions[routeI].y > transform.position.y && !jumping && boxCollider.DownTouched) {
                velocity.y = jumpForce;
                jumping = true;
                jumpStart = false;
                animator.PlayAnimation(AKJump);
            }

            rigidbody.velocity = ResolveVelocity(velocity);
        }
        else
            HandleNextMovenment();
    }

    private void HandleLadderClimbing() {
        float routeDelta = routePositions[routeI].y - routePositions[routeI - 1].y;
        float posDelta = transform.position.y - routePositions[routeI - 1].y;
        float progress = posDelta / routeDelta;

        if (progress < 1) {
            Vector2 velocity = rigidbody.velocity;
            velocity.y = routePositions[routeI].y > routePositions[routeI - 1].y ? climbSpeed : -climbSpeed;
            rigidbody.velocity = velocity;
        }
        else
            HandleNextMovenment();
    }
    #endregion

    #if UNITY_EDITOR
    private void OnDrawGizmos() {
        if (NavigationManager.ins != null && NavigationManager.ins.EnvMap != null && moving) {
            Gizmos.color = Color.green;
            for (int i = routeI; i < routePositions.Length; i++) {
                Gizmos.DrawCube(routePositions[i], Vector3.one * 0.25f);
                Gizmos.DrawLine(
                    routePositions[i - 1],
                    routePositions[i]);
            }

            Gizmos.DrawCube(routePositions[routePositions.Length - 1], Vector3.one * 0.5f);
        }
    }
    #endif

}
