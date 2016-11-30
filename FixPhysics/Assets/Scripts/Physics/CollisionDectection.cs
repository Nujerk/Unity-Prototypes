using UnityEngine;
using System.Collections;

public abstract class CollisionDectection : MonoBehaviour {

    // !!! Exposed for test purpose !!!
    public Vector2 direction = Vector2.right;

    protected BoxCollider2D _box2D;
    protected PhysicObject _pobj;
    protected float _horizontalCheckDistance;
    protected float _verticalCheckDistance;

    void Start() {
        _box2D = GetComponent<BoxCollider2D>();
        if (_box2D == null) {
            Debug.LogError("(CollisionDectection) Box Collider 2D missing on this GameObject !!!");
            DestroyImmediate(this);
        }

        _pobj = GetComponent<PhysicObject>();
        if(_pobj == null) {
            Debug.LogError("(CollisionDectection) PhysicObject missing on this GameObject !!!");
            DestroyImmediate(this);
        }

        // Store the collision distances
        _horizontalCheckDistance = _box2D.size.x / 2;
        _verticalCheckDistance = _box2D.size.y / 2;

    }

    void FixedUpdate() {
        // predict next position
        var deltaTime = Time.fixedDeltaTime;
        var cumulatedForces = _pobj.GetCumulatedForces();
        var nextAppliedForce = deltaTime * cumulatedForces;
        float nextAccX = deltaTime * cumulatedForces.x;
        float nextAccY = deltaTime * cumulatedForces.y;
        float nextVeloX = _pobj.Velocity.x + nextAccX;
        float nextVeloY = _pobj.Velocity.y + nextAccY;
        float nextDeltaX = nextVeloX * deltaTime;
        float nextDeltaY = nextVeloY * deltaTime;
        var currentPosition = (Vector2)gameObject.transform.position;
        var boxWidth = _box2D.size;
        var boxOffset = _box2D.offset;

        // Check front collision
        var hitArray = Physics2D.BoxCastAll(currentPosition + boxOffset, boxWidth, 0, direction, Mathf.Abs(nextDeltaX));
        Collider2D collider;
        foreach (var hit in hitArray) {
            collider = hit.collider;
            if (collider != null && collider.gameObject != gameObject) {
                OnFrontCollision(hit);
            }
        }

        // Check bottom collision
        if(nextDeltaY < 0) {
            hitArray = Physics2D.BoxCastAll(currentPosition + boxOffset, boxWidth, 0, Vector2.down, Mathf.Abs(nextDeltaY));
            foreach (var hit in hitArray) {
                collider = hit.collider;
                if (collider != null && collider.gameObject != gameObject) {
                    OnBottomCollision(hit);
                }
            }
        }

        // Check top collision
        if(nextDeltaY > 0) {
            hitArray = Physics2D.BoxCastAll(currentPosition + boxOffset, boxWidth, 0, Vector2.up, Mathf.Abs(nextDeltaY));
            foreach (var hit in hitArray) {
                collider = hit.collider;
                if (collider != null && collider.gameObject != gameObject) {
                    OnTopCollision(hit);
                }
            }
        }
    }

    public abstract void OnFrontCollision(RaycastHit2D collision);
    public abstract void OnTopCollision(RaycastHit2D collision);
    public abstract void OnBottomCollision(RaycastHit2D collision);
}
