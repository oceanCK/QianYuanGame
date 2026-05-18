using System;
using CatBrotato.Data;

namespace CatBrotato.Shop
{
    public enum OfferKind
    {
        Weapon,
        Item
    }

    /// <summary>
    /// One slot in the shop's roll. Holds either a WeaponData or ItemData reference,
    /// along with the rolled final price, locked state, and sold-out state.
    /// </summary>
    [Serializable]
    public class ShopOffer
    {
        public OfferKind Kind { get; private set; }
        public WeaponData Weapon { get; private set; }
        public ItemData Item { get; private set; }
        public int FinalPrice { get; set; }
        public bool IsLocked { get; set; }
        public bool IsSold { get; set; }

        public int Id
        {
            get
            {
                if (Kind == OfferKind.Weapon) return Weapon != null ? Weapon.id : 0;
                return Item != null ? Item.id : 0;
            }
        }

        public string DisplayName
        {
            get
            {
                if (Kind == OfferKind.Weapon) return Weapon != null ? Weapon.displayName : string.Empty;
                return Item != null ? Item.displayName : string.Empty;
            }
        }

        public Rarity Rarity
        {
            get
            {
                if (Kind == OfferKind.Weapon) return Weapon != null ? Weapon.rarity : Rarity.Common;
                return Item != null ? Item.rarity : Rarity.Common;
            }
        }

        public static ShopOffer ForWeapon(WeaponData data, int price)
        {
            return new ShopOffer
            {
                Kind = OfferKind.Weapon,
                Weapon = data,
                FinalPrice = price
            };
        }

        public static ShopOffer ForItem(ItemData data, int price)
        {
            return new ShopOffer
            {
                Kind = OfferKind.Item,
                Item = data,
                FinalPrice = price
            };
        }
    }
}
