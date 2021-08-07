using System;
using UnityEngine;

namespace ServiceLocator {
    public abstract class MonoBehaviourService : MonoBehaviour {
        protected virtual void Awake() {
            MonoBehaviourServiceLocator.Current.Register(this, GetType());
        }

        private void OnDestroy() {
            MonoBehaviourServiceLocator.Current.Deregister(this, GetType());
        }
    }
}