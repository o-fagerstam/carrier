using CommandMode;
using UnityEngine;

namespace Unit {
    public abstract class AiUnitController : MonoBehaviour {
        protected Order _currentOrder;

        public virtual void SetOrder(Order order) {
            _currentOrder = order;
        }

    }
}