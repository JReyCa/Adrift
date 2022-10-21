using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gravity
{
    public class GravityCentre : MonoBehaviour
    {
        #region Inspector Variables
        [SerializeField]
        private float mass = 100.0f; // in kilograms
        #endregion

        private const float GRAV_CONSTANT = 0.00000000006674f;

        // The force (F) due to gravity of one object on another (according to the Newtonion model)
        // can be found by multiplying their two masses together (m1 & m2), then that result by the universal
        // gravitational constant (G), and finally dividing by the square of the distance between them (r^2).
        // This magnitude then needs to be multiplied by the direction of the force:
        // 
        // F = ((G * m1 * m2) / r^2) * dir
        // 
        // Note that this is only used to affect "rogue objects."
        public Vector3 ForceOnOther(RogueObject spaceObject)
        {
            Vector3 deltaPos = transform.position - spaceObject.transform.position;
            Vector3 direction = deltaPos.normalized;
            float distance = deltaPos.magnitude / 1000; // Divided by 1000 so it's in kilometres, not metres.
            float otherMass = spaceObject.GetComponent<Rigidbody>().mass;

            return (GRAV_CONSTANT * mass * otherMass) / (distance * distance) * direction;
        }
    }
}
