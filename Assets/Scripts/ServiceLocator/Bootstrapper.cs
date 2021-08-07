using UnityEngine;

namespace ServiceLocator {
    public static class Bootstrapper {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Initialize() {
            MonoBehaviourServiceLocator.Initialize();
        }
    }
}