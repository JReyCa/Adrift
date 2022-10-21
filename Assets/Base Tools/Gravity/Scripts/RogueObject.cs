using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gravity
{
    public class RogueObject : SpaceObject
    {
        #region Inspector Variables
        [SerializeField]
        private Vector3 initialVelocity = new(0.0f, 0.0f, 0.0f);
        #endregion

        protected override void Start()
        {
            base.Start();
            rbody.isKinematic = false;
            rbody.velocity = initialVelocity;
        }

        protected override void Motion()
        {
            Vector3 netGravity = Vector3.zero;

            foreach (var centre in GravityManager.Centres)
                netGravity += centre.ForceOnOther(this);

            rbody.AddForce(netGravity);
        }
    }
}
