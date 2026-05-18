using System;
using System.Collections;
using UnityEngine;
using CatBrotato.Combat;

namespace CatBrotato.Player
{
    public class PlayerHealth : MonoBehaviour
    {
        public event Action<float> OnDamaged;
        public event Action<float> OnHealed;
        public event Action OnDeath;

        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Color damageFlashColor = Color.red;
        [SerializeField] private float flashDuration = 0.15f;

        public float CurrentHp { get; private set; }
        public float MaxHp { get; private set; }
        public bool IsInvulnerable { get; private set; }
        public bool IsDead { get; private set; }

        private Color originalColor;
        private Coroutine flashCoroutine;

        private void Awake()
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            }

            if (spriteRenderer != null)
            {
                originalColor = spriteRenderer.color;
            }
        }

        public void Init(float maxHp)
        {
            MaxHp = maxHp;
            CurrentHp = maxHp;
            IsDead = false;
            IsInvulnerable = false;
        }

        public void TakeDamage(float rawDamage)
        {
            if (IsDead || IsInvulnerable) return;

            // Check dodge
            PlayerStats stats = GetComponent<PlayerStats>();
            float dodgeChance = stats != null ? stats.Dodge : 0f;
            if (DamageCalculator.RollDodge(dodgeChance))
            {
                // Dodged - could trigger visual feedback here
                return;
            }

            // Apply armor reduction
            float armor = stats != null ? stats.Armor : 0f;
            float finalDamage = DamageCalculator.CalculateDamage(
                rawDamage, 0f, armor, 0f, out bool _);

            CurrentHp -= finalDamage;
            CurrentHp = Mathf.Max(CurrentHp, 0f);

            OnDamaged?.Invoke(finalDamage);
            FlashDamage();

            if (CurrentHp <= 0f)
            {
                Die();
            }
        }

        public void Heal(float amount)
        {
            if (IsDead) return;
            if (amount <= 0f) return;

            float previousHp = CurrentHp;
            CurrentHp = Mathf.Min(CurrentHp + amount, MaxHp);
            float actualHeal = CurrentHp - previousHp;

            if (actualHeal > 0f)
            {
                OnHealed?.Invoke(actualHeal);
            }
        }

        public void SetInvulnerable(bool invulnerable)
        {
            IsInvulnerable = invulnerable;
        }

        private void Die()
        {
            if (IsDead) return;
            IsDead = true;
            OnDeath?.Invoke();
        }

        private void FlashDamage()
        {
            if (spriteRenderer == null) return;

            if (flashCoroutine != null)
            {
                StopCoroutine(flashCoroutine);
            }
            flashCoroutine = StartCoroutine(FlashCoroutine());
        }

        private IEnumerator FlashCoroutine()
        {
            spriteRenderer.color = damageFlashColor;
            yield return new WaitForSeconds(flashDuration);
            spriteRenderer.color = originalColor;
            flashCoroutine = null;
        }
    }
}
