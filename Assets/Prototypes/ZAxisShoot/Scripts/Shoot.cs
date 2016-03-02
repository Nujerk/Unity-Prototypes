using UnityEngine;
using System.Collections;

public class Shoot : MonoBehaviour {

    public GameObject projectile;

    private float lastShot;
    public float timeBetweenShots = 0.25f;
    public float forceFactor = 10;

    void Start()
    {
        Debug.Log("*** PROTO Z AXIS SHOT : use your second joystick to shoot.");
    }

	// Update is called once per frame
	void Update () {
        // Gather the Z-axis values
        var axisHZ = Input.GetAxis("Horizontal Z");
        var axisVZ = Input.GetAxis("Vertical Z");
        if (axisHZ != 0 || axisVZ != 0)
        {
            // Time based shoot frequency
            if (Time.realtimeSinceStartup - lastShot > timeBetweenShots)
            {
                Vector2 direction = new Vector2(axisHZ, axisVZ);
                var instance = GameObject.Instantiate(projectile, transform.position, transform.rotation) as GameObject;
                // Use normalized vector to have a magnitude of 1 (this way even a little pressure on the joystick allow to shoot at full strength).
                instance.GetComponent<Rigidbody2D>().AddForce(direction.normalized * forceFactor, ForceMode2D.Impulse);
                lastShot = Time.realtimeSinceStartup;
            }
        }
	}
}
