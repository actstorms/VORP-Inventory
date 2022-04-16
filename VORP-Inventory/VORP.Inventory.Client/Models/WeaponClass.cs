﻿using CitizenFX.Core;
using CitizenFX.Core.Native;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;
using VORP.Inventory.Shared;

namespace VORP.Inventory.Client.Models
{
    [DataContract]
    public class WeaponClass : BaseScript
    {
        private long _hashkey;
        private string _name;

        [DataMember(Name = "id")]
        public int Id { get; set; }

        [DataMember(Name = "propietary")]
        public string Propietary { get; set; }

        [DataMember(Name = "name")]
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                _hashkey = GetHashKey(value);
            }
        }

        [DataMember(Name = "ammo")]
        public Dictionary<string, int> Ammo { get; set; }

        [DataMember(Name = "components")]
        public List<string> Components { get; set; }

        [DataMember(Name = "used")]
        public bool Used { get; set; }

        [DataMember(Name = "used2")]
        public bool Used2 { get; set; }

        [DataMember(Name = "weaponLabel")]
        public string WeaponLabel
        {
            get
            {
                int hashKey = API.GetHashKey(Name);
                string rtnName = Function.Call<string>((Hash)0x6D3AC61694A791C5, hashKey);

                if (rtnName == "WNS_INVALID")
                {
                    return Name;
                }

                return Configuration.GetWeaponLabel(rtnName);
            }
        }

        [JsonIgnore]
        public long Hash
        {
            get => _hashkey;
        }

        public void UnequipWeapon()
        {
            SetUsed(false);
            SetUsed2(false);
            TriggerServerEvent("vorpinventory:setUsedWeapon", Id, Used, Used2);
            //int hash = API.GetHashKey(Name);
            RemoveWeaponFromPed();
            Utils.CleanAmmo(Id);
        }

        public void RemoveWeaponFromPed()
        {
            API.RemoveWeaponFromPed(API.PlayerPedId(), (uint)Hash, true, 0);
        }

        public void LoadAmmo()
        {
            int playerPedId = PlayerPedId();

            if (Name.StartsWith("WEAPON_MELEE"))
            {
                Function.Call((Hash)0xB282DC6EBD803C75, playerPedId, Hash, 500, true, 0);
            }
            else
            {
                if (Used2)
                {
                    // GETTING THE EQUIPED WEAPON
                    uint currentPedWeaponHash = 0;
                    API.GetCurrentPedWeapon(playerPedId, ref currentPedWeaponHash, false, 0, false);

                    Function.Call((Hash)0x5E3BDDBCB83F3D84, playerPedId, currentPedWeaponHash, 1, 1, 1, 2, false, 0.5, 1.0, 752097756, 0, true, 0.0);
                    Function.Call((Hash)0x5E3BDDBCB83F3D84, playerPedId, Hash, 1, 1, 1, 3, false, 0.5, 1.0, 752097756, 0, true, 0.0);
                    Function.Call((Hash)0xADF692B254977C0C, playerPedId, currentPedWeaponHash, 0, 1, 0, 0);
                    Function.Call((Hash)0xADF692B254977C0C, playerPedId, Hash, 0, 0, 0, 0);

                }
                else
                {
                    API.GiveDelayedWeaponToPed(playerPedId, (uint)Hash, 0, true, 0);

                }
                API.SetPedAmmo(playerPedId, (uint)Hash, 0);
                foreach (KeyValuePair<string, int> ammos in Ammo)
                {
                    long ammoHash = GetHashKey(ammos.Key);
                    Function.Call((Hash)0x5FD1E1F011E76D7E, playerPedId, ammoHash, ammos.Value); // SetPedAmmoByType
                    Logger.Trace($"SetPedAmmoByType: {ammoHash}: {ammos.Key}, Amount: {ammos.Value}");
                }
            }

        }

        public void LoadComponents()
        {
            foreach (string component in Components)
            {
                Function.Call((Hash)0x74C9090FDD1BB48E, API.PlayerPedId(), (uint)API.GetHashKey(component), Hash, true);
            }
        }

        public void SetUsed(bool used)
        {
            Used = used;
            TriggerServerEvent("vorpinventory:setUsedWeapon", Id, used, Used2);
        }

        public void SetUsed2(bool used2)
        {
            Used2 = used2;
            TriggerServerEvent("vorpinventory:setUsedWeapon", Id, Used, used2); ;
        }

        public void QuitComponent(string component)
        {
            if (Components.Contains(component))
            {
                Components.Remove(component);
            }
        }

        public int GetAmmo(string type)
        {
            if (Ammo.ContainsKey(type))
            {
                return Ammo[type];
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Update ammo on server by client
        /// </summary>
        /// <param name="ammo"></param>
        /// <param name="type"></param>
        public void SetAmmo(int ammo, string type)
        {
            if (Ammo.ContainsKey(type))
            {
                Ammo[type] = ammo;
                TriggerServerEvent("vorpinventory:setWeaponBullets", Id, type, ammo);
            }
            else
            {
                Ammo.Add(type, ammo);
                TriggerServerEvent("vorpinventory:setWeaponBullets", Id, type, ammo);
            }
        }

        public void AddAmmo(int ammo, string type)
        {
            if (Ammo.ContainsKey(type))
            {
                Ammo[type] += ammo;
            }
            else
            {
                Ammo.Add(type, ammo);
            }
        }

        public void SubAmmo(int ammo, string type)
        {
            if (Ammo.ContainsKey(type))
            {
                Ammo[type] -= ammo;
                if (Ammo[type] == 0)
                {
                    Ammo.Remove(type);
                }
            }
        }
    }
}