using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;

namespace ChainSawLeg.Features.Minigames
{
    /// <summary>
    /// Runs attack waves: a scripted wave fires each attack at its absolute wave-time
    /// once; a looped wave fires attacks delta-seconds apart, repeating forever.
    /// Unifies the original duplicated scripted-sequence / looped-coroutine logic.
    /// </summary>
    public class AttackScheduler
    {
        private readonly MonoBehaviour host;
        private Coroutine loopedWaveCoroutine;

        /// <summary>Creates the scheduler. <paramref name="host"/> runs the looped-wave coroutine.</summary>
        public AttackScheduler(MonoBehaviour host)
        {
            this.host = host;
        }

        /// <summary>
        /// Plays a scripted wave once: every attack fires at its absolute wave-time,
        /// then <paramref name="onComplete"/> runs after <paramref name="endDelay"/> seconds.
        /// </summary>
        public void PlayScripted(RobotAttackConfig config, Action<RobotAttack> onAttack, float endDelay, Action onComplete)
        {
            Sequence wave = DOTween.Sequence();

            foreach (RobotAttack attack in config.attackList)
            {
                wave.Join(DOVirtual.DelayedCall(attack.time, () => onAttack(attack)));
            }

            // TweenCallback is not implicitly convertible from Action, so wrap it.
            wave.Append(DOVirtual.DelayedCall(endDelay, () => onComplete?.Invoke()));
        }

        /// <summary>Plays a wave on repeat; each attack fires delta-seconds after the previous one (config ordering defines deltas).</summary>
        public void PlayLooped(RobotAttackConfig config, Action<RobotAttack> onAttack)
        {
            StopLooped();
            loopedWaveCoroutine = host.StartCoroutine(LoopWave(config, onAttack));
        }

        /// <summary>Stops the looped wave if one is running. Scripted waves always play to completion (matching the original).</summary>
        public void StopLooped()
        {
            if (loopedWaveCoroutine != null)
            {
                host.StopCoroutine(loopedWaveCoroutine);
                loopedWaveCoroutine = null;
            }
        }

        private IEnumerator LoopWave(RobotAttackConfig config, Action<RobotAttack> onAttack)
        {
            while (true)
            {
                RobotAttack[] attacks = config.attackList;
                for (int i = 0; i < attacks.Length; i++)
                {
                    // Delta from the previous attack; the first attack waits its full wave-time.
                    float time = attacks[i].time;
                    if (i > 0) time -= attacks[i - 1].time;

                    yield return new WaitForSeconds(time);

                    onAttack(attacks[i]);
                }
            }
        }
    }
}
