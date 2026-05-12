using System.Collections.Generic;
using System.Text;
using JingHongLu.Skills;
using UnityEngine;

namespace JingHongLu.SwordArts
{
    public sealed class StrokeRecorder : MonoBehaviour
    {
        [SerializeField] private PlayerSkillController skillController;
        [SerializeField] private float strokeLifetime = 3f;
        [SerializeField] private int maxStrokeCount = 8;
        [SerializeField] private bool logStrokeRecords = true;

        private readonly List<StrokeRecord> records = new List<StrokeRecord>();
        private bool warnedMissingSkillController;

        public IReadOnlyList<StrokeRecord> Records => records;
        public float StrokeLifetime => strokeLifetime;

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
            RemoveExpiredRecords();
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

            RemoveExpiredRecords();
            records.Add(new StrokeRecord(skill.StrokeType, skill, Time.time));

            while (records.Count > Mathf.Max(1, maxStrokeCount))
            {
                records.RemoveAt(0);
            }

            if (logStrokeRecords)
            {
                Debug.Log($"当前笔画：{BuildDebugText()}", this);
            }
        }

        private void RemoveExpiredRecords()
        {
            float now = Time.time;
            float lifetime = Mathf.Max(0f, strokeLifetime);

            for (int i = records.Count - 1; i >= 0; i--)
            {
                if (now - records[i].Time > lifetime)
                {
                    records.RemoveAt(i);
                }
            }
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
