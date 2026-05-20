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
        private readonly List<SwordArtData> availableSwordArts =
            new List<SwordArtData>();
        private bool warnedMissingStrokeRecorder;

        public event Action<SwordArtData> OnSwordArtTriggered;
        public event Action<IReadOnlyList<SwordArtData>> OnAvailableSwordArtsChanged;

        private void Awake()
        {
            ResolveReferences();
        }

        private void OnEnable()
        {
            ResolveReferences();

            if (strokeRecorder != null)
            {
                strokeRecorder.OnRecordsChanged += HandleRecordsChanged;
                RefreshAvailableSwordArts();
                return;
            }

            LogMissingStrokeRecorderWarning();
        }

        private void OnDisable()
        {
            if (strokeRecorder != null)
            {
                strokeRecorder.OnRecordsChanged -= HandleRecordsChanged;
            }
        }

        private void Update()
        {
            TickCooldowns();
        }

        public IReadOnlyList<SwordArtData> GetAvailableSwordArts()
        {
            availableSwordArts.Clear();

            if (strokeRecorder == null || swordArts == null)
            {
                return availableSwordArts;
            }

            for (int i = 0; i < swordArts.Length; i++)
            {
                SwordArtData swordArt = swordArts[i];

                if (swordArt == null || IsOnCooldown(swordArt))
                {
                    continue;
                }

                IReadOnlyList<StrokeType> requiredStrokes =
                    swordArt.RequiredSequence;

                if (requiredStrokes == null || requiredStrokes.Count == 0)
                {
                    continue;
                }

                if (IsSwordArtAvailable(swordArt))
                {
                    availableSwordArts.Add(swordArt);
                }
            }

            return availableSwordArts;
        }

        public bool RequestTriggerSwordArt(SwordArtData swordArt)
        {
            if (swordArt == null || strokeRecorder == null || IsOnCooldown(swordArt))
            {
                return false;
            }

            if (!IsSwordArtAvailable(swordArt))
            {
                return false;
            }

            IReadOnlyList<StrokeType> requiredStrokes = swordArt.RequiredSequence;

            if (swordArt.ConsumeMatchedStrokes)
            {
                bool consumed = swordArt.MatchMode switch
                {
                    SwordArtMatchMode.OrderedTail => ConsumeOrderedTail(requiredStrokes),
                    SwordArtMatchMode.UnorderedCounts =>
                        strokeRecorder.ConsumeRequiredStrokes(requiredStrokes),
                    _ => false
                };

                if (!consumed)
                {
                    return false;
                }
            }

            strokeRecorder.ResetNaturalRemoveTimer();
            StartCooldown(swordArt);
            StartCoroutine(DelayedTriggerSwordArtRoutine(swordArt));
            RefreshAvailableSwordArts();
            return true;
        }

        private void ResolveReferences()
        {
            if (strokeRecorder == null)
            {
                TryGetComponent(out strokeRecorder);
            }
        }

        private void HandleRecordsChanged()
        {
            RefreshAvailableSwordArts();
        }

        private void RefreshAvailableSwordArts()
        {
            IReadOnlyList<SwordArtData> available = GetAvailableSwordArts();
            OnAvailableSwordArtsChanged?.Invoke(available);
        }

        private bool IsSwordArtAvailable(SwordArtData swordArt)
        {
            if (swordArt == null || strokeRecorder == null)
            {
                return false;
            }

            IReadOnlyList<StrokeType> requiredStrokes =
                swordArt.RequiredSequence;

            if (requiredStrokes == null || requiredStrokes.Count == 0)
            {
                return false;
            }

            return swordArt.MatchMode switch
            {
                SwordArtMatchMode.OrderedTail => IsSequenceMatchedAtTail(
                    strokeRecorder.GetActiveRecords(),
                    requiredStrokes),
                SwordArtMatchMode.UnorderedCounts =>
                    strokeRecorder.HasEnoughStrokes(requiredStrokes),
                _ => false
            };
        }

        private bool ConsumeOrderedTail(IReadOnlyList<StrokeType> requiredStrokes)
        {
            if (requiredStrokes == null || requiredStrokes.Count == 0)
            {
                return false;
            }

            strokeRecorder.RemoveLastRecords(requiredStrokes.Count);
            strokeRecorder.ResetNaturalRemoveTimer();
            return true;
        }

        private static bool IsSequenceMatchedAtTail(
            IReadOnlyList<StrokeRecord> records,
            IReadOnlyList<StrokeType> sequence)
        {
            if (sequence == null || sequence.Count == 0)
            {
                return false;
            }

            if (records == null || records.Count < sequence.Count)
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
            bool changed = false;

            for (int i = cooldownSwordArts.Count - 1; i >= 0; i--)
            {
                SwordArtData swordArt = cooldownSwordArts[i];

                if (swordArt == null ||
                    !cooldownTimers.TryGetValue(swordArt, out float timer))
                {
                    cooldownSwordArts.RemoveAt(i);
                    changed = true;
                    continue;
                }

                timer -= Time.deltaTime;

                if (timer <= 0f)
                {
                    cooldownTimers.Remove(swordArt);
                    cooldownSwordArts.RemoveAt(i);
                    changed = true;
                    continue;
                }

                cooldownTimers[swordArt] = timer;
            }

            if (changed)
            {
                RefreshAvailableSwordArts();
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
