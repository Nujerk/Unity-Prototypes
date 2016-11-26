using UnityEngine;
using System.Collections;

public class CollisionDectection : MonoBehaviour {

    public Vector2 direction = Vector2.right;
    public float collisionRange = 1f;

    protected BoxCollider2D _box2D;
    protected float _horizontalCheckDistance;
    protected float _verticalCheckDistance;

    void Start () {
        _box2D = GetComponent<BoxCollider2D>();
        if(_box2D == null)
        {
            Debug.LogError("(CollisionDectection) No Box Collider 2D on this GameObject !!!");
            DestroyImmediate(this);
        }

        // Store the collision distances
        _horizontalCheckDistance = _box2D.size.x / 2;
        _verticalCheckDistance = _box2D.size.y / 2;

    }
	
	void FixedUpdate () {
        // Check front collision
        var boxWidth = _box2D.size;
        var hitArray = Physics2D.BoxCastAll(((Vector2)gameObject.transform.position) + _box2D.offset, boxWidth, 0, direction, _horizontalCheckDistance);
        Collider2D collider;
        foreach (var hit in hitArray)
        {
            collider = hit.collider;
            if (collider != null && collider.gameObject != gameObject)
            {
                Debug.Log(gameObject.name);
            }
        }

        // Check bottom collision
        hitArray = Physics2D.BoxCastAll(((Vector2)gameObject.transform.position) + _box2D.offset - _box2D.size / 2, _box2D.size, 0, Vector2.down, _verticalCheckDistance);
        foreach (var hit in hitArray)
        {
            collider = hit.collider;
            if (collider != null && collider.gameObject != gameObject)
            {
                Debug.Log(gameObject.name);
            }
        }
    }
}
