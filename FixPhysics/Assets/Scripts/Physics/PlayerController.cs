using UnityEngine;
using System.Collections;
using System;

public class PlayerController : CollisionDectection {

    public override void OnBottomCollision(RaycastHit2D collision) {
        var pObj = GetComponent<PhysicObject>();
        pObj.SetVelocity(pObj.Velocity.x, -(collision.distance / Time.fixedDeltaTime));
        pObj.AddForce(new Vector2(0, 9.81f), false);
    }

    public override void OnFrontCollision(RaycastHit2D collision) {

    }

    public override void OnTopCollision(RaycastHit2D collision) {

    }
}
