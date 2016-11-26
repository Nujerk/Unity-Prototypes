using UnityEngine;
using System.Collections;

namespace Parallax
{
    public class ParallaxBackground : MonoBehaviour
    {
        [Range(1, 10)]
        public float parallaxSpeed = 5;

        private GameObject _player;

        void Start()
        {
            _player = GameObject.FindGameObjectWithTag("Player");
        }

        void Update()
        {
            transform.position = new Vector3(_player.transform.position.x / (11 - parallaxSpeed), transform.position.y, transform.position.z);
        }
    }
}


