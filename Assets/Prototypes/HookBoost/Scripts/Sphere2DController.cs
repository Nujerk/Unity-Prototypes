using UnityEngine;
using System.Collections;

namespace HookBoost
{
    public class Sphere2DController : MonoBehaviour
    {
        public GameObject projectile;

        // Update is called once per frame
        void Update()
        {
            if (Input.anyKey)
            {
                if (Input.GetKey(KeyCode.LeftArrow))
                {
                    gameObject.GetComponent<Rigidbody2D>().AddForce(Vector2.left);
                }
                else if (Input.GetKey(KeyCode.RightArrow))
                {
                    gameObject.GetComponent<Rigidbody2D>().AddForce(Vector2.right);
                }
            }

            if(Input.GetMouseButtonDown(0))
            {
                var direction = Camera.main.ScreenToWorldPoint(Input.mousePosition) - gameObject.transform.position;
                var instance = GameObject.Instantiate(projectile, transform.position, Quaternion.identity) as GameObject;
                projectile.GetComponent<Projectile>().projector = gameObject;
                // Use normalized vector to have a magnitude of 1 (this way even a little pressure on the joystick allow to shoot at full strength).
                instance.GetComponent<Rigidbody2D>().AddForce(direction.normalized * 10, ForceMode2D.Impulse);
            }
        }
    }
}

