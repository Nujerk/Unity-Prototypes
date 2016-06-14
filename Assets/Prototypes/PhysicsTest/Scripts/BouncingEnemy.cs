using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PhysicsTest
{
    public class BouncingEnemy : MonoBehaviour
    {
        public bool isBouncing = false;

        void OnCollisionEnter2D(Collision2D collision)
        {
            if (!isBouncing)
                return;

            var velocity = collision.relativeVelocity;
            var normal = collision.contacts[0].normal;
            var direction = new Vector2(velocity.x * Mathf.Sign(normal.x), velocity.y * Mathf.Sign(normal.y));

            GetComponent<Rigidbody2D>().AddForce(direction, ForceMode2D.Impulse);
        }
    }
}
