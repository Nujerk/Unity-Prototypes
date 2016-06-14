using UnityEngine;
using System.Collections;

namespace PhysicsTest
{
    public class PlayerBumper : MonoBehaviour
    {
        public float moveSpeed = 5; // in pixels / s
        public float jumpForce = 5;
        public float pixelPerUnit = 10;

        private bool _touchingGround;

        void Start()
        {

        }

        void Update()
        {
            if(Input.GetKey(KeyCode.Space) && _touchingGround)
            {
                // Jump
                GetComponent<Rigidbody2D>().AddForce(new Vector2(0, jumpForce / pixelPerUnit), ForceMode2D.Impulse);
            }

            var direction = Input.GetAxis("Horizontal");
            if (Mathf.Abs(direction) > 0)
            {
                var forceX = moveSpeed / pixelPerUnit * Mathf.Sign(direction);
                GetComponent<Rigidbody2D>().velocity = new Vector2(forceX, GetComponent<Rigidbody2D>().velocity.y);
            }
        }

        void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
                _touchingGround = true;

            if(collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))
            {
                var contact = collision.contacts[0].point;
                var direction = contact - (Vector2)collision.gameObject.transform.position;
                var normal = direction.normalized;
                normal *= -20;
                collision.gameObject.GetComponent<Rigidbody2D>().velocity = normal;
                collision.gameObject.GetComponent<BouncingEnemy>().isBouncing = true;
            }
        }

        void OnCollisionExit2D(Collision2D collision)
        {
            if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
                _touchingGround = false;
        }
    }
}


