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
        private static WeaponData[] _data = Array.Empty<WeaponData>();

        public static WeaponData[] GetWeapons()
        {
            if (_data.Length != 0)
                return _data;

            if (EquipmentManager.WeaponsData == null)
                EquipmentManager.WeaponsData = Resources.LoadAll<WeaponData>("Data/Equipment Data/Weapons");

            _data = EquipmentManager.WeaponsData.Where(w => w.EquipmentType < EquipmentType.EnemyBlast).OrderBy(o => (int)o.EquipmentType).ToArray();
            return _data;
        }
    }
}
