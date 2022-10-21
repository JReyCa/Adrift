using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gravity
{
    [RequireComponent(typeof(Rigidbody))]
    public abstract class SpaceObject : MonoBehaviour
    {
        protected Rigidbody rbody;

        protected virtual void Start()
        {
            rbody = GetComponent<Rigidbody>();
            rbody.useGravity = false;
        }

        protected abstract void Motion();

        private void FixedUpdate()
        {
            Motion();
        }
    }
}
