namespace CatBrotato.Data
{
    public enum CatCategory
    {
        HomeCat,
        WildCat
    }

    public enum FurType
    {
        Short,
        Long,
        Hairless
    }

    public enum RoleClass
    {
        Balanced,
        Melee,
        Ranged,
        Mage,
        Tank,
        Support,
        Lucky,
        Special
    }

    public enum WeaponClass
    {
        Melee,
        Ranged,
        Magic
    }

    public enum WeaponSubType
    {
        Claw,
        Rod,
        Glove,
        Dart,
        Staff,
        Bell,
        Pointer,
        Orb
    }

    public enum DamageType
    {
        Physical,
        Magical,
        True
    }

    public enum TargetingType
    {
        NearestEnemy,
        RandomEnemy,
        AoE,
        Self
    }

    public enum Rarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }

    public enum ThreatCategory
    {
        Physical,
        Environmental,
        Imaginary
    }

    public enum BehaviorType
    {
        Chase,
        Patrol,
        Stationary,
        Flee,
        Circle
    }

    public enum ItemCategory
    {
        Food,
        Environment,
        Toy,
        Survival,
        Accessory
    }

    public enum EffectType
    {
        StatBoost,
        OnHit,
        OnKill,
        OnWaveStart,
        OnDamaged,
        Aura
    }

    public enum TriggerType
    {
        Passive,
        OnHit,
        OnKill,
        OnDamaged,
        OnWaveStart,
        Timed
    }

    public enum StackRule
    {
        Additive,
        Multiplicative,
        Unique
    }

    public enum GameState
    {
        Boot,
        Menu,
        CharacterSelect,
        Battle,
        Shop,
        Results
    }
}
