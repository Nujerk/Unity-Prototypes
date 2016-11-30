using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PhysicManager : MonoBehaviour {

    #region SINGLETON
    public static PhysicManager Instance {
        get {
            return sInstance;
        }
    }
    protected static PhysicManager sInstance;
    #endregion

    public Vector2 gravityVector = new Vector2(0f, -9.81f);
    
    protected List<PhysicForce> _sceneForces;
    protected List<PhysicObject> _registeredObjects;
	
    public void RegisterObject(PhysicObject pObj) {
        if(!_registeredObjects.Contains(pObj))
            _registeredObjects.Add(pObj);
    }

    void Start () {
	    if(sInstance == null) {
            sInstance = this;
        }
        else {
            Debug.LogWarning("Trying to create a PhysicManager, but one already exists");
            DestroyImmediate(this);
        }

        _sceneForces = new List<PhysicForce>();
        _registeredObjects = new List<PhysicObject>();
	}

	void FixedUpdate () {
	    foreach(var pObj in _registeredObjects) {
            pObj.AddForce(gravityVector * pObj.gravityScale, false);
            foreach(var sceneForce in _sceneForces) {
                pObj.AddForce(sceneForce.Vector, sceneForce.ConstantForce);
            }
        }
	}
}
