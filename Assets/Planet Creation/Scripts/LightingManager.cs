using ReyToolkit;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlanetCreation
{
    [ExecuteAlways]
    public class LightingManager : SingletonComponent<LightingManager>
    {
        #region Inspector Variables
        [SerializeField]
        private Light directionalLight;

        [SerializeField]
        private MeshRenderer[] spaceObjectRenderers;
        #endregion

        private void UpdateDirectionalLight()
        {
            if (!directionalLight)
                return;

            foreach (var renderer in spaceObjectRenderers)
            {
                Material m;
                if (Application.isPlaying)
                    m = renderer.material;
                else
                    m = renderer.sharedMaterial;

                m.SetVector("_DirectionalLight", directionalLight.transform.forward);
                m.SetFloat("_DirectionalLight_Intensity", directionalLight.intensity);
                m.SetColor("_DirectionalLight_Colour", directionalLight.color);
            }
        }

        private void Update()
        {
            UpdateDirectionalLight();
        }
    }
}
