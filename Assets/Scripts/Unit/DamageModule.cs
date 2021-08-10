using System;
using UnityEngine;

namespace Unit {
    public class DamageModule : MonoBehaviour {
        public float maxHealth = 3000f;
        public float health;
        
        public event EventHandler<OnDamageTakenArgs> OnDamageTaken;

        protected void InvokeOnDamageTaken(object sender, OnDamageTakenArgs args) {
            OnDamageTaken?.Invoke(sender, args);
        }
        public class OnDamageTakenArgs : EventArgs {
            public float damageTaken, healthRemaining, maxHealth;
            public OnDamageTakenArgs(float damageTaken, float healthRemaining, float maxHealth) {
                this.damageTaken = damageTaken;
                this.healthRemaining = healthRemaining;
                this.maxHealth = maxHealth;
            }
        }
    }
}