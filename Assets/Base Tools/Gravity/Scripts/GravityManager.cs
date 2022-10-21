using ReyToolkit;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gravity
{
    public class GravityManager : SingletonComponent<GravityManager>
    {
        #region Inspector Variables
        [SerializeField]
        private GravityCentre[] centres;
        #endregion

        public static GravityCentre[] Centres => Instance.centres;

        public static GravityCentre GetNearestCentre(Transform someSpaceObject)
        {
            float shortestDist = float.MaxValue;
            GravityCentre[] centres = Instance.centres;
            GravityCentre nearestCentre = centres[0];

            for (int i = 0; i < centres.Length; i++)
            {
                float dist = Vector3.Distance(someSpaceObject.position, centres[i].transform.position);

                if (dist < shortestDist)
                {
                    shortestDist = dist;
                    nearestCentre = centres[i];
                }
            }

            return nearestCentre;
        }
    }
}
