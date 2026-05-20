using System;
using System.Collections.Generic;
using System.Text;
using JingHongLu.Skills;
using UnityEngine;

namespace JingHongLu.SwordArts
{
    public sealed class StrokeRecorder : MonoBehaviour
    {
        [SerializeField] private PlayerSkillController skillController;

        [Header("Stroke Slots")]
        [SerializeField] private int maxSlotCount = 7;
        [SerializeField] private float naturalRemoveInterval = 3f;

        [SerializeField] private bool logStrokeRecords = true;

        private readonly List<StrokeRecord> records = new List<StrokeRecord>();
        private bool warnedMissingSkillController;
        private float naturalRemoveTimer;

        public event Action<StrokeRecord> OnStrokeRecorded;
        public event Action<StrokeRecord> OnStrokeExpired;
        public event Action<StrokeRecord> OnStrokeOverflowed;
        public event Action<IReadOnlyList<StrokeRecord>> OnStrokesConsumed;
        public event Action OnRecordsChanged;
        public event Action<IReadOnlyList<StrokeRecord>> OnRecordsChangedDetailed;

        public IReadOnlyList<StrokeRecord> Records => records;
        public int MaxSlotCount => Mathf.Max(1, maxSlotCount);
        public float NaturalRemoveInterval => Mathf.Max(0.01f, naturalRemoveInterval);

        private void Awake()
        {
            ResolveReferences();
        }

        private void OnEnable()
        {
            ResolveReferences();

            if (skillController != null)
            {
                skillController.OnSkillExecuted += HandleSkillExecuted;
                return;
            }

            LogMissingSkillControllerWarning();
        }

        private void OnDisable()
        {
            if (skillController != null)
            {
                skillController.OnSkillExecuted -= HandleSkillExecuted;
            }
        }

        private void Update()
        {
            TickNaturalRemoveTimer();
        }

        public IReadOnlyList<StrokeRecord> GetActiveRecords()
        {
            return records;
        }

        public int GetStrokeCount(StrokeType strokeType)
        {
            int count = 0;

            for (int i = 0; i < records.Count; i++)
            {
                if (records[i].StrokeType == strokeType)
                {
                    count++;
                }
            }

            return count;
        }

        public bool HasEnoughStrokes(IReadOnlyList<StrokeType> requiredStrokes)
        {
            if (requiredStrokes == null || requiredStrokes.Count == 0)
            {
                return true;
            }

            Dictionary<StrokeType, int> ownedCounts = BuildStrokeCounts(records);
            Dictionary<StrokeType, int> requiredCounts =
                BuildStrokeCounts(requiredStrokes);

            foreach (KeyValuePair<StrokeType, int> pair in requiredCounts)
            {
                ownedCounts.TryGetValue(pair.Key, out int ownedCount);

                if (ownedCount < pair.Value)
                {
                    return false;
                }
            }

            return true;
        }

        public bool ConsumeRequiredStrokes(IReadOnlyList<StrokeType> requiredStrokes)
        {
            if (requiredStrokes == null || requiredStrokes.Count == 0)
            {
                return false;
            }

            if (!HasEnoughStrokes(requiredStrokes))
            {
                return false;
            }

            Dictionary<StrokeType, int> remainingRequiredCounts =
                BuildStrokeCounts(requiredStrokes);
            List<StrokeRecord> remainingRecords = new List<StrokeRecord>(records.Count);
            List<StrokeRecord> consumedRecords = new List<StrokeRecord>();

            for (int i = 0; i < records.Count; i++)
            {
                StrokeRecord record = records[i];

                if (remainingRequiredCounts.TryGetValue(
                        record.StrokeType,
                        out int remainingCount) &&
                    remainingCount > 0)
                {
                    consumedRecords.Add(record);
                    remainingRequiredCounts[record.StrokeType] = remainingCount - 1;
                    continue;
                }

                remainingRecords.Add(record);
            }

            records.Clear();
            records.AddRange(remainingRecords);

            OnStrokesConsumed?.Invoke(consumedRecords);
            ResetNaturalRemoveTimer();
            NotifyRecordsChanged();
            return true;
        }

        public void ResetNaturalRemoveTimer()
        {
            naturalRemoveTimer = 0f;
        }

        public void RecordStroke(StrokeType strokeType, SkillData sourceSkill = null)
        {
            if (strokeType == StrokeType.None)
            {
                return;
            }

            AddStroke(new StrokeRecord(strokeType, sourceSkill, Time.time));
        }

        public void AddStroke(StrokeRecord record)
        {
            if (record.StrokeType == StrokeType.None)
            {
                return;
            }

            int safeMaxSlotCount = MaxSlotCount;

            if (records.Count >= safeMaxSlotCount)
            {
                StrokeRecord overflowedRecord = records[0];
                records.RemoveAt(0);
                OnStrokeOverflowed?.Invoke(overflowedRecord);
            }

            records.Add(record);
            OnStrokeRecorded?.Invoke(record);
            NotifyRecordsChanged();

            if (logStrokeRecords)
            {
                Debug.Log($"当前笔画槽：{BuildDebugText()}", this);
            }
        }

        public void RemoveLastRecords(int count)
        {
            if (count <= 0 || records.Count == 0)
            {
                return;
            }

            if (count >= records.Count)
            {
                records.Clear();
            }
            else
            {
                for (int i = 0; i < count; i++)
                {
                    records.RemoveAt(records.Count - 1);
                }
            }

            if (records.Count == 0)
            {
                ResetNaturalRemoveTimer();
            }

            NotifyRecordsChanged();
        }

        private void ResolveReferences()
        {
            if (skillController == null)
            {
                TryGetComponent(out skillController);
            }
        }

        private void HandleSkillExecuted(SkillData skill)
        {
            if (skill == null || skill.StrokeType == StrokeType.None)
            {
                return;
            }

            RecordStroke(skill.StrokeType, skill);
        }

        private void TickNaturalRemoveTimer()
        {
            if (records.Count == 0)
            {
                ResetNaturalRemoveTimer();
                return;
            }

            naturalRemoveTimer += Time.deltaTime;

            if (naturalRemoveTimer < NaturalRemoveInterval)
            {
                return;
            }

            naturalRemoveTimer = 0f;
            StrokeRecord expiredRecord = records[0];
            records.RemoveAt(0);
            OnStrokeExpired?.Invoke(expiredRecord);
            NotifyRecordsChanged();
        }

        private void NotifyRecordsChanged()
        {
            OnRecordsChanged?.Invoke();
            OnRecordsChangedDetailed?.Invoke(records);
        }

        private string BuildDebugText()
        {
            if (records.Count == 0)
            {
                return "空";
            }

            StringBuilder builder = new StringBuilder();

            for (int i = 0; i < records.Count; i++)
            {
                if (i > 0)
                {
                    builder.Append(' ');
                }

                builder.Append(ToDisplayName(records[i].StrokeType));
            }

            return builder.ToString();
        }

        private void LogMissingSkillControllerWarning()
        {
            if (warnedMissingSkillController)
            {
                return;
            }

            warnedMissingSkillController = true;
            Debug.LogWarning(
                $"{nameof(StrokeRecorder)} requires a {nameof(PlayerSkillController)} on the same GameObject or an assigned reference.",
                this);
        }

        private static Dictionary<StrokeType, int> BuildStrokeCounts(
            IReadOnlyList<StrokeRecord> sourceRecords)
        {
            Dictionary<StrokeType, int> counts = new Dictionary<StrokeType, int>();

            for (int i = 0; i < sourceRecords.Count; i++)
            {
                AddStrokeCount(counts, sourceRecords[i].StrokeType);
            }

            return counts;
        }

        private static Dictionary<StrokeType, int> BuildStrokeCounts(
            IReadOnlyList<StrokeType> sourceStrokes)
        {
            Dictionary<StrokeType, int> counts = new Dictionary<StrokeType, int>();

            for (int i = 0; i < sourceStrokes.Count; i++)
            {
                AddStrokeCount(counts, sourceStrokes[i]);
            }

            return counts;
        }

        private static void AddStrokeCount(
            IDictionary<StrokeType, int> counts,
            StrokeType strokeType)
        {
            if (strokeType == StrokeType.None)
            {
                return;
            }

            counts.TryGetValue(strokeType, out int count);
            counts[strokeType] = count + 1;
        }

        private static string ToDisplayName(StrokeType strokeType)
        {
            return strokeType switch
            {
                StrokeType.Horizontal => "横",
                StrokeType.Vertical => "竖",
                StrokeType.LeftFalling => "撇",
                StrokeType.RightFalling => "捺",
                _ => "无"
            };
        }
    }
}
