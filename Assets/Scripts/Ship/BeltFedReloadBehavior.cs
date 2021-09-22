using UnityEngine;
namespace Ship {
	public class BeltFedReloadBehavior : MonoBehaviour, IReloadBehavior {
		public float roundsPerMinute = 10f;
		private float _timeOfLastFiring = float.MinValue;
		
		public void ConsumeAmmunition () {
			_timeOfLastFiring = Time.time;
		}
		public bool IsLoaded () => Time.time - _timeOfLastFiring > 60f / roundsPerMinute;
	}
}
