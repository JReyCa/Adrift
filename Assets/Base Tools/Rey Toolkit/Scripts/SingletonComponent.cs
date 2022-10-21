using System;
using System.Collections.Generic;
using UnityEngine;

namespace ReyToolkit
{
    public abstract class SingletonComponent<T> : MonoBehaviour
    {
        private static Dictionary<Type, object> singletons = new();

        protected static T Instance => (T)singletons[typeof(T)];

        // When the script is enabled, self-destruct if there was already an instance of this in the scene.
        private void OnEnable()
        {
            if (singletons.ContainsKey(GetType()))
                Destroy(this);
            else
                singletons.Add(GetType(), this);
        }
    }

}
