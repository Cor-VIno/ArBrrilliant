using UnityEngine;

namespace JingHongLu.Dialog
{
    public sealed class DialogTrigger2D : MonoBehaviour
    {
        [SerializeField] private DialogController dialogController;
        [SerializeField] private DialogData dialogData;
        [SerializeField] private bool triggerOnce = true;
        [SerializeField] private string playerTag = "Player";

        private bool hasTriggered;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (triggerOnce && hasTriggered)
            {
                return;
            }

            if (!IsPlayerCollider(other))
            {
                return;
            }

            if (dialogController == null)
            {
                dialogController = FindAnyObjectByType<DialogController>();
            }

            if (dialogController == null)
            {
                Debug.LogWarning("[Dialog] Trigger could not find DialogController.", this);
                return;
            }

            dialogController.StartDialog(dialogData);
            hasTriggered = true;
        }

        private bool IsPlayerCollider(Collider2D other)
        {
            if (other == null)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(playerTag))
            {
                return true;
            }

            if (other.CompareTag(playerTag))
            {
                return true;
            }

            Transform parent = other.transform.parent;
            while (parent != null)
            {
                if (parent.CompareTag(playerTag))
                {
                    return true;
                }

                parent = parent.parent;
            }

            return false;
        }
    }
}
