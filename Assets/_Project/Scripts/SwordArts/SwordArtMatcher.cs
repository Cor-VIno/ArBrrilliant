using System;
using System.Collections;
using System.Collections.Generic;
using JingHongLu.Skills;
using UnityEngine;

namespace JingHongLu.SwordArts
{
    public sealed class SwordArtMatcher : MonoBehaviour
    {
        [SerializeField] private StrokeRecorder strokeRecorder;
        [SerializeField] private SwordArtData[] swordArts;
        [SerializeField] private bool logMatchedSwordArt = true;

        private readonly Dictionary<SwordArtData, float> cooldownTimers =
            new Dictionary<SwordArtData, float>();
        private readonly List<SwordArtData> cooldownSwordArts =
            new List<SwordArtData>();
        private bool warnedMissingStrokeRecorder;

        public event Action<SwordArtData> OnSwordArtTriggered;

        private void Awake()
        {
            ResolveReferences();
        }

        private void OnEnable()
        {
            ResolveReferences();

            if (strokeRecorder != null)
            {
                strokeRecorder.OnStrokeRecorded += HandleStrokeRecorded;
                return;
            }

            LogMissingStrokeRecorderWarning();
        }

        private void OnDisable()
        {
            if (strokeRecorder != null)
            {
                strokeRecorder.OnStrokeRecorded -= HandleStrokeRecorded;
            }
        }

        private void Update()
        {
            TickCooldowns();
        }

        private void ResolveReferences()
        {
            if (strokeRecorder == null)
            {
                TryGetComponent(out strokeRecorder);
            }
        }

        private void HandleStrokeRecorded(StrokeRecord record)
        {
            TryMatchSwordArts();
        }

        private void TryMatchSwordArts()
        {
            if (strokeRecorder == null || swordArts == null)
            {
                return;
            }

            IReadOnlyList<StrokeRecord> records = strokeRecorder.GetActiveRecords();

            if (records.Count == 0)
            {
                return;
            }

            for (int i = 0; i < swordArts.Length; i++)
            {
                SwordArtData swordArt = swordArts[i];

                if (swordArt == null || IsOnCooldown(swordArt))
                {
                    continue;
                }

                IReadOnlyList<StrokeType> sequence = swordArt.RequiredSequence;

                if (!IsSequenceMatchedAtTail(records, sequence))
                {
                    continue;
                }

                int sequenceLength = sequence.Count;

                bool shouldConsumeMatchedStrokes =
                    swordArt.ConsumeMatchedStrokes &&
                    !IsPrefixOfLongerConfiguredSequence(sequence);

                if (shouldConsumeMatchedStrokes)
                {
                    strokeRecorder.RemoveLastRecords(sequenceLength);
                }

                StartCooldown(swordArt);
                StartCoroutine(DelayedTriggerSwordArtRoutine(swordArt));
                break;
            }
        }

        private static bool IsSequenceMatchedAtTail(
            IReadOnlyList<StrokeRecord> records,
            IReadOnlyList<StrokeType> sequence)
        {
            if (sequence == null || sequence.Count == 0)
            {
                return false;
            }

            if (records.Count < sequence.Count)
            {
                return false;
            }

            int recordStartIndex = records.Count - sequence.Count;

            for (int i = 0; i < sequence.Count; i++)
            {
                if (records[recordStartIndex + i].StrokeType != sequence[i])
                {
                    return false;
                }
            }

            return true;
        }

        private bool IsPrefixOfLongerConfiguredSequence(
            IReadOnlyList<StrokeType> sequence)
        {
            if (sequence == null || swordArts == null)
            {
                return false;
            }

            for (int i = 0; i < swordArts.Length; i++)
            {
                SwordArtData candidate = swordArts[i];

                if (candidate == null || IsOnCooldown(candidate))
                {
                    continue;
                }

                IReadOnlyList<StrokeType> candidateSequence =
                    candidate.RequiredSequence;

                if (candidateSequence == null ||
                    candidateSequence.Count <= sequence.Count)
                {
                    continue;
                }

                if (IsSequencePrefix(sequence, candidateSequence))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsSequencePrefix(
            IReadOnlyList<StrokeType> prefix,
            IReadOnlyList<StrokeType> sequence)
        {
            if (prefix == null || sequence == null || prefix.Count > sequence.Count)
            {
                return false;
            }

            for (int i = 0; i < prefix.Count; i++)
            {
                if (prefix[i] != sequence[i])
                {
                    return false;
                }
            }

            return true;
        }

        private void TriggerSwordArt(SwordArtData swordArt)
        {
            OnSwordArtTriggered?.Invoke(swordArt);

            if (logMatchedSwordArt)
            {
                Debug.Log($"触发剑招：{swordArt.DisplayName}", this);
            }
        }

        private IEnumerator DelayedTriggerSwordArtRoutine(SwordArtData swordArt)
        {
            float delay = Mathf.Max(0f, swordArt.ExecutionDelay);

            if (delay > 0f)
            {
                yield return new WaitForSeconds(delay);
            }

            TriggerSwordArt(swordArt);
        }

        private void StartCooldown(SwordArtData swordArt)
        {
            if (swordArt.Cooldown <= 0f)
            {
                return;
            }

            cooldownTimers[swordArt] = swordArt.Cooldown;

            if (!cooldownSwordArts.Contains(swordArt))
            {
                cooldownSwordArts.Add(swordArt);
            }
        }

        private bool IsOnCooldown(SwordArtData swordArt)
        {
            return cooldownTimers.ContainsKey(swordArt);
        }

        private void TickCooldowns()
        {
            for (int i = cooldownSwordArts.Count - 1; i >= 0; i--)
            {
                SwordArtData swordArt = cooldownSwordArts[i];

                if (swordArt == null ||
                    !cooldownTimers.TryGetValue(swordArt, out float timer))
                {
                    cooldownSwordArts.RemoveAt(i);
                    continue;
                }

                timer -= Time.deltaTime;

                if (timer <= 0f)
                {
                    cooldownTimers.Remove(swordArt);
                    cooldownSwordArts.RemoveAt(i);
                    continue;
                }

                cooldownTimers[swordArt] = timer;
            }
        }

        private void LogMissingStrokeRecorderWarning()
        {
            if (warnedMissingStrokeRecorder)
            {
                return;
            }

            warnedMissingStrokeRecorder = true;
            Debug.LogWarning(
                $"{nameof(SwordArtMatcher)} requires a {nameof(StrokeRecorder)} on the same GameObject or an assigned reference.",
                this);
        }
    }
}
