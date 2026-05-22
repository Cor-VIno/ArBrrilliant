using JingHongLu.SwordArts;
using UnityEngine;

namespace JingHongLu.Player
{
    public sealed class PlayerSwordArtArmorGuard : MonoBehaviour
    {
        [SerializeField] private SwordArtExecutor swordArtExecutor;
        [SerializeField] private PlayerSuperArmorController superArmorController;
        [SerializeField] private bool logSwordArtArmor = true;

        private readonly object swordArtArmorSource = new object();
        private bool armorApplied;

        private void Awake()
        {
            ResolveReferences();
        }

        private void OnEnable()
        {
            ResolveReferences();

            if (swordArtExecutor != null)
            {
                swordArtExecutor.OnSwordArtExecutionStarted += HandleSwordArtExecutionStarted;
                swordArtExecutor.OnSwordArtExecutionFinished += HandleSwordArtExecutionFinished;
            }
        }

        private void OnDisable()
        {
            if (swordArtExecutor != null)
            {
                swordArtExecutor.OnSwordArtExecutionStarted -= HandleSwordArtExecutionStarted;
                swordArtExecutor.OnSwordArtExecutionFinished -= HandleSwordArtExecutionFinished;
            }

            ClearSwordArtArmor(null);
        }

        private void ResolveReferences()
        {
            if (swordArtExecutor == null)
            {
                TryGetComponent(out swordArtExecutor);
            }

            if (superArmorController == null)
            {
                TryGetComponent(out superArmorController);
            }
        }

        private void HandleSwordArtExecutionStarted(SwordArtData swordArt)
        {
            if (superArmorController == null)
            {
                return;
            }

            armorApplied = true;
            superArmorController.AddSuperArmor(swordArtArmorSource);

            if (logSwordArtArmor)
            {
                Debug.Log(
                    $"[SwordArtArmor] Super armor started. SwordArt={swordArt?.DisplayName ?? "Unknown"}",
                    this);
            }
        }

        private void HandleSwordArtExecutionFinished(SwordArtData swordArt)
        {
            ClearSwordArtArmor(swordArt);
        }

        private void ClearSwordArtArmor(SwordArtData swordArt)
        {
            if (!armorApplied)
            {
                return;
            }

            armorApplied = false;

            if (superArmorController != null)
            {
                superArmorController.RemoveSuperArmor(swordArtArmorSource);
            }

            if (logSwordArtArmor)
            {
                Debug.Log(
                    $"[SwordArtArmor] Super armor ended. SwordArt={swordArt?.DisplayName ?? "Unknown"}",
                    this);
            }
        }
    }
}
