using System;
using UnityEngine;
namespace Ship {
	public class AutoCannonMagazineFed : AutoCannon { 
		public int bulletsInMagazine;
		public int magazineSize;
		public float timeToReload;
		private float _timeOfStartReload;

		protected override void Awake () {
			if (bulletsInMagazine > magazineSize) {
				bulletsInMagazine = magazineSize;
			}
		}

		protected override void Update () {
			base.Update();
			if (bulletsInMagazine == 0 && Time.time - _timeOfStartReload > timeToReload) {
				bulletsInMagazine = magazineSize;
			}
		}

		protected override void Fire () {
			if (bulletsInMagazine == 0) {
				return;
			}

			bulletsInMagazine--;

			if (bulletsInMagazine == 0) {
				_timeOfStartReload = Time.time;
			}

			base.Fire();
		}
	}
}
