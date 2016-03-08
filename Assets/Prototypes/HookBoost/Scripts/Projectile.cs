using System.Collections;
using UnityEngine;

namespace HookBoost
{
    public class Projectile : MonoBehaviour
    {
        public GameObject projector;

        public void OnCollisionEnter2D(Collision2D col)
        {
            var sphere = projector.GetComponent<Sphere2DController>();
            sphere.Hooked = true;
            GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            GetComponent<Rigidbody2D>().angularVelocity = 0;

            var direction = col.contacts[0].point - new Vector2(projector.transform.position.x, projector.transform.position.y);
            projector.GetComponent<Rigidbody2D>().AddForce(direction.normalized * sphere.hookSettings.hookForce, ForceMode2D.Impulse);
        }
    }
}
