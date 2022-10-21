using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ReyToolkit
{
    [RequireComponent(typeof(Camera))]
    [ExecuteAlways]
    public class DepthCamera : MonoBehaviour
    {
        private void OnEnable()
        {
            GetComponent<Camera>().depthTextureMode = DepthTextureMode.Depth;
        }
    }
}
