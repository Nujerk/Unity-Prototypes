using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PhysicObject : MonoBehaviour {

    public float gravityScale = 1f;

    public Vector2 Velocity { get; private set; }
    protected List<PhysicForce> _forceList;

    void Start() {
        Velocity = Vector2.zero;
        _forceList = new List<PhysicForce>();
        PhysicManager.Instance.RegisterObject(this);
    }

    void FixedUpdate() {
        // Compute sum of applied forces
        Vector2 cumulatedForces = Vector2.zero;
        var tmpList = new List<PhysicForce>(_forceList);
        foreach (var force in tmpList) {
            cumulatedForces += force.Vector;
            if (!force.ConstantForce)
                _forceList.Remove(force);
        }

        // Compute acceleration to add (since last FixedUpdate)
        Velocity += (Time.fixedDeltaTime * cumulatedForces);

        // Add covered distance since last FixedUpdate
        gameObject.transform.position += (Vector3)(Time.fixedDeltaTime * Velocity);
    }

    public Vector2 GetCumulatedForces() {
        Vector2 cumulatedForces = Vector2.zero;
        foreach (var force in _forceList) {
            cumulatedForces += force.Vector;
        }

        return cumulatedForces;
    }

    public void SetVelocity(float xVelocity, float yVelocity) {
        Velocity = new Vector2(xVelocity, yVelocity);
    }

    public void AddForce(Vector2 vector, bool constant) {
        var newForce = new PhysicForce();
        newForce.Vector = vector;
        newForce.ConstantForce = constant;
        _forceList.Add(newForce);
    }
}
