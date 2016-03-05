using UnityEngine;
using System.Collections;

namespace LightTest
{
    public class Sphere2DController : MonoBehaviour
    {

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
        }

    }
}
