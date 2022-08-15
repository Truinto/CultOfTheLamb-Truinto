using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static WeaponUpgradeSystem;

namespace FireDevil
{
    public static class WeaponWeight
    {
        public static IEnumerable<WeaponData> GetWeapons()
        {
            var data = EquipmentManager.WeaponsData;
            if (data == null)
                yield break;

            foreach (var weapon in data)
            {
                yield return weapon;
            }
        }
    }
}
