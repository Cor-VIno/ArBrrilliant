using System.Collections.Generic;
using UnityEngine;

namespace JingHongLu.Dialog
{
    [CreateAssetMenu(
        fileName = "DialogData",
        menuName = "JingHongLu/Dialog/Dialog Data")]
    public sealed class DialogData : ScriptableObject
    {
        [SerializeField] private List<DialogLine> lines = new();

        public IReadOnlyList<DialogLine> Lines => lines;
    }

    [System.Serializable]
    public sealed class DialogLine
    {
        [SerializeField] private string speakerName;
        [TextArea(2, 5)]
        [SerializeField] private string text;

        public string SpeakerName => speakerName;
        public string Text => text;
    }
}
