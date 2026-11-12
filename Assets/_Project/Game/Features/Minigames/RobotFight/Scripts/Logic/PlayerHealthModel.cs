using System;
using UnityEngine;

namespace ChainSawLeg.Features.Minigames
{
    /// <summary>
    /// Pure player-health state for the arena fight: damage accumulation with a
    /// post-death early-out, plus a normalized-health notification. No engine
    /// dependencies beyond math, so it can be unit-tested in isolation.
    /// </summary>
    public class PlayerHealthModel
    {
        private int current;

        /// <summary>Initial (maximum) health value.</summary>
        public int Max { get; }

        /// <summary>Current health value; may go negative on overkill damage.</summary>
        public int Current => current;

        /// <summary>True once health reached zero or below; further damage is ignored.</summary>
        public bool IsDepleted => current <= 0;

        /// <summary>
        /// Raised after each applied damage. The parameter is <c>Current / Max</c> as a raw
        /// fraction (not clamped, matching the original healthbar tween target).
        /// </summary>
        public event Action<float> HealthChanged;

        /// <summary>Creates the model with health starting (and maxing) at <paramref name="maxHealth"/>.</summary>
        public PlayerHealthModel(int maxHealth)
        {
            Max = maxHealth;
            current = maxHealth;
        }

        /// <summary>Applies damage unless already depleted.</summary>
        public void ApplyDamage(int damage)
        {
            if (IsDepleted) return;

            current -= damage;
            HealthChanged?.Invoke((float)current / Max);
        }
    }
}
