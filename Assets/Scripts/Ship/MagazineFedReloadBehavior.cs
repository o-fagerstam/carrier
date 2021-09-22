using UnityEngine;
namespace Ship {
	public class MagazineFedReloadBehavior : MonoBehaviour, IReloadBehavior {
		public float magazineReloadTime = 1f;
		public float roundsPerMinute = 200f;
		public int magazineSize = 10;
		private int _roundsInMagazine;
		private float _timeOfLastFiring = float.MinValue;
		private float _timeOfStartMagReload = float.MinValue;

		private void Awake () {
			_roundsInMagazine = magazineSize;
		}

		private void Update () {
			if (_roundsInMagazine <= 0 && Time.time - _timeOfStartMagReload > magazineReloadTime) {
				_roundsInMagazine = magazineSize;
			}
		}

		public void ConsumeAmmunition () {
			_roundsInMagazine--;
			_timeOfLastFiring = Time.time;

			if (_roundsInMagazine == 0) {
				_timeOfStartMagReload = Time.time;
			}
		}
		public bool IsLoaded () => _roundsInMagazine > 0 && Time.time - _timeOfLastFiring > 60f / roundsPerMinute;
	}
}
