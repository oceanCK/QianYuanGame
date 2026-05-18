using UnityEngine;
using CatBrotato.Core;

namespace CatBrotato.Player
{
    [RequireComponent(typeof(CircleCollider2D))]
    public class MaterialCollector : MonoBehaviour
    {
        [SerializeField] private float basePickupRange = 1.5f;
        [SerializeField] private float harvestingRangeScale = 0.05f;
        [SerializeField] private float harvestingBonusScale = 0.1f;
        [SerializeField] private AudioClip pickupSound;
        [SerializeField] private GameObject pickupEffectPrefab;

        private CircleCollider2D pickupCollider;
        private PlayerStats playerStats;
        private AudioSource audioSource;

        private void Awake()
        {
            pickupCollider = GetComponent<CircleCollider2D>();
            pickupCollider.isTrigger = true;
            pickupCollider.radius = basePickupRange;

            playerStats = GetComponentInParent<PlayerStats>();
            if (playerStats == null)
            {
                playerStats = GetComponent<PlayerStats>();
            }

            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
            }
        }

        private void Update()
        {
            // Adjust pickup range based on harvesting stat
            float harvesting = playerStats != null ? playerStats.Harvesting : 0f;
            pickupCollider.radius = basePickupRange + harvesting * harvestingRangeScale;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Material")) return;

            MaterialPickup pickup = other.GetComponent<MaterialPickup>();
            if (pickup == null) return;

            // Calculate bonus materials from harvesting stat
            float harvesting = playerStats != null ? playerStats.Harvesting : 0f;
            int baseAmount = pickup.MaterialAmount;
            int bonusAmount = Mathf.FloorToInt(baseAmount * harvesting * harvestingBonusScale);
            int totalAmount = baseAmount + bonusAmount;

            // Add to game manager
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddMaterials(totalAmount);
            }

            // Visual feedback
            if (pickupEffectPrefab != null)
            {
                Instantiate(pickupEffectPrefab, other.transform.position, Quaternion.identity);
            }

            // Audio feedback
            if (pickupSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(pickupSound);
            }

            // Destroy the pickup
            Destroy(other.gameObject);
        }
    }

    /// <summary>
    /// Attach to material pickup objects in the scene.
    /// </summary>
    public class MaterialPickup : MonoBehaviour
    {
        [SerializeField] private int materialAmount = 1;

        public int MaterialAmount => materialAmount;

        public void SetAmount(int amount)
        {
            materialAmount = amount;
        }
    }
}
