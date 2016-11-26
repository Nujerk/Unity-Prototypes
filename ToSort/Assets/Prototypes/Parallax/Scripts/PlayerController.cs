using UnityEngine;
using System.Collections;

namespace Parallax
{
    public class PlayerController : MonoBehaviour
    {
        void Update()
        {
            if(Input.GetKey(KeyCode.LeftArrow))
            {
                transform.position += Vector3.left * 10 * Time.deltaTime;
            }
            else if (Input.GetKey(KeyCode.RightArrow))
            {
                transform.position += Vector3.right * 10 * Time.deltaTime;
            }
        }
    }
}


