using UnityEngine;

namespace CatBrotato.Combat
{
    public class AutoAttackSystem : MonoBehaviour
    {
        [SerializeField] private float detectionRange = 10f;
        [SerializeField] private float targetSwitchThreshold = 1.5f;
        [SerializeField] private LayerMask enemyLayerMask = ~0;

        private WeaponHolder weaponHolder;
        private Transform currentTarget;

        public Transform CurrentTarget => currentTarget;

        private void Awake()
        {
            weaponHolder = GetComponent<WeaponHolder>();
        }

        private void Update()
        {
            UpdateTarget();

            if (currentTarget != null && weaponHolder != null)
            {
                weaponHolder.TryAttackAll(currentTarget);
            }
        }

        private void UpdateTarget()
        {
            // If current target is still valid and in range, keep it
            if (currentTarget != null)
            {
                float distToTarget = Vector2.Distance(
                    transform.position, currentTarget.position);

                // Check if target is still alive (active in hierarchy)
                if (!currentTarget.gameObject.activeInHierarchy ||
                    distToTarget > detectionRange * targetSwitchThreshold)
                {
                    currentTarget = null;
                }
            }

            // Find new target if needed
            if (currentTarget == null)
            {
                currentTarget = FindNearestEnemy();
            }
        }

        private Transform FindNearestEnemy()
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(
                transform.position, detectionRange, enemyLayerMask);

            Transform nearest = null;
            float nearestDist = float.MaxValue;

            for (int i = 0; i < hits.Length; i++)
            {
                // Skip self and non-enemy objects
                if (hits[i].transform == transform) continue;
                if (!hits[i].CompareTag("Enemy")) continue;

                float dist = Vector2.Distance(
                    transform.position, hits[i].transform.position);

                if (dist < nearestDist)
                {
                    nearestDist = dist;
                    nearest = hits[i].transform;
                }
            }

            return nearest;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRange);
        }
    }
}
