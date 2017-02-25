using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour {

    public float moveSpeed = 5f;
    public Vector3 cameraDelta = new Vector3(0, 13, -22.5f);

    protected Camera _camera;
    protected Rigidbody _rb;
    protected Vector3 _direction;

	// Use this for initialization
	void Start () {
        _camera = Camera.main;
        _rb = GetComponent<Rigidbody>();
	}
	
	// Update is called once per frame
	void Update () {
        var xAxis = Input.GetAxis("Horizontal");
        var yAxis = Input.GetAxis("Vertical");

        if(Mathf.Abs(xAxis) > 0.1) {
            xAxis = Mathf.Sign(xAxis);
        }

        if (Mathf.Abs(yAxis) > 0.1) {
            yAxis = Mathf.Sign(yAxis);
        }

        _direction = new Vector3(xAxis, 0, yAxis);
    }

    void FixedUpdate() {
        _rb.velocity =  _direction * moveSpeed;
    }

    void LateUpdate() {
        _camera.gameObject.transform.position = gameObject.transform.position + cameraDelta;
    }
}
