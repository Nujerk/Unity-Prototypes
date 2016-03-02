using UnityEngine;
using System.Collections;

public class Projectile : MonoBehaviour {

    private float birthDate;
    public float lifeDuration = 2;

	// Use this for initialization
	void Start () {
        birthDate = Time.realtimeSinceStartup;
	}
	
	// Update is called once per frame
	void Update () {
        if (Time.realtimeSinceStartup - birthDate > lifeDuration)
            Destroy(gameObject);
	}
}
