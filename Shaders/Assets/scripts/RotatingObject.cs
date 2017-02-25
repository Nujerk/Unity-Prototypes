using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotatingObject : MonoBehaviour {

    public Quaternion localRotation;
    public Quaternion worldRotation;

    void Update () {
        gameObject.transform.Rotate(Vector3.up, 1);
    }
}
