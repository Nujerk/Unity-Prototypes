using UnityEngine;
using System.Collections;
using System;

namespace HookBoost
{
    [Serializable]
    public class HookSettings
    {
        [Range(1,20)]
        public float maxDistanceHook = 10;
        [Range(1, 50)]
        public float hookSpeed = 20;
        [Range(1, 50)]
        public float hookForce = 20;
    }

    /// <summary>
    /// To use the hook :
    /// - Hold left mouse button to launch the hook, when it collides to a wall, it drags the sphere
    /// - Once hooked, click again to remove the hook
    /// - Now you can remove the hook while the sphere is dragged, and immediatly launch a new hook while the sphere is in the air. This will accumulate
    /// the speed and make the sphere accelerate.
    /// </summary>
    public class Sphere2DController : MonoBehaviour
    {
        public GameObject projectile;

        public bool Hooked { get; set; }
        public HookSettings hookSettings;

        private Vector2 _lastProjectileDirection;
        private bool _hookLaunched = false;
        private bool _hookReturn = false;

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

            if (_hookReturn && Vector2.Distance(projectile.transform.position, gameObject.transform.position) < 0.5)
            {
                projectile.SetActive(false);
                _hookLaunched = false;
            }

            if (Input.GetMouseButtonDown(0) && Hooked || !Hooked && Input.GetMouseButtonUp(0) || Vector2.Distance(projectile.transform.position, transform.position) > hookSettings.maxDistanceHook || _hookReturn)
            {
                projectile.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
                projectile.transform.position = Vector2.MoveTowards(projectile.transform.position, gameObject.transform.position, 1);
                gameObject.GetComponent<Rigidbody2D>().isKinematic = false;
                Hooked = false;
                _hookReturn = true;
            }

            if (Input.GetMouseButtonDown(0) && !_hookLaunched)
            {
                Hooked = false;
                _lastProjectileDirection = Camera.main.ScreenToWorldPoint(Input.mousePosition) - gameObject.transform.position;
                projectile.transform.position = gameObject.transform.position;
                projectile.SetActive(true);
                // Use normalized vector to have a magnitude of 1 (this way even a little pressure on the joystick allow to shoot at full strength).
                projectile.GetComponent<Rigidbody2D>().AddForce(_lastProjectileDirection.normalized * hookSettings.hookSpeed, ForceMode2D.Impulse);
                _hookLaunched = true;
                _hookReturn = false;
            }

            if(Hooked && Vector2.Distance(projectile.transform.position, gameObject.transform.position) < 2)
            {
                gameObject.GetComponent<Rigidbody2D>().isKinematic = true;
                gameObject.transform.position = Vector2.MoveTowards(gameObject.transform.position, projectile.transform.position, 0.5f);
            }
        }
    }
}

