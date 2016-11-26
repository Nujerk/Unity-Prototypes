using UnityEngine;
using System.Collections;

namespace ScreenShake
{
    public class ScreenShakeOnContact : MonoBehaviour
    {

        public GameObject target;
        public Camera camera;

        public void OnCollisionEnter2D(Collision2D coll)
        {
            if (coll.gameObject == target)
                ScreenShake();
        }

        private void ScreenShake()
        {
            camera.GetComponent<Animator>().PlayInFixedTime("Shake");
        }

    }
}
