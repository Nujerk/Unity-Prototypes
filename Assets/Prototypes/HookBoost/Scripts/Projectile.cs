using System.Collections;
using UnityEngine;

namespace HookBoost
{
    public class Projectile : MonoBehaviour
    {
        public GameObject projector;

        public void OnCollisionEnter2D(Collision2D col)
        {
            foreach(var comp in gameObject.GetComponents<DistanceJoint2D>())
            {
                Destroy(comp);
            }
            projector.GetComponent<Rigidbody2D>().AddForce(Vector2.up * 2, ForceMode2D.Impulse);
            var joint = projector.AddComponent<DistanceJoint2D>();
            joint.connectedBody = col.rigidbody;
            var x = (col.contacts[0].point.x - col.transform.position.x) / col.transform.localScale.x;
            joint.connectedAnchor = new Vector2(x, 0);
        }
    }
}
