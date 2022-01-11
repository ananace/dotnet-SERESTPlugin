using System.Runtime.Serialization;

namespace SERESTPlugin.APIs.DataTypes
{
    [DataContract]
    public class PlayerInformation
    {
        [DataMember(Name = "name")]
        public string Name { get; set; }
        [DataMember(Name = "id")]
        public long ID { get; set; }
    }

    [DataContract]
    public class PlayerStatus : PlayerInformation
    {
        public PlayerStatus() { }
        public PlayerStatus(Sandbox.Game.Entities.Character.MyCharacter character)
        {
            ID = character.GetPlayerIdentityId();
            Name = character.GetFriendlyName();
            Health = character.StatComp.HealthRatio;

            if (character.OxygenComponent != null)
            {
                Oxygen = character.OxygenLevel;
                var gas = character.OxygenComponent;
                Hydrogen = gas.GetGasFillLevel(Sandbox.Game.Entities.Character.Components.MyCharacterOxygenComponent.HydrogenId);

                if (character.StatComp.TryGetStat(VRage.Utils.MyStringHash.Get("player_oxygen_bottles"), out Sandbox.Game.Entities.MyEntityStat stat))
                    OxygenBottles = (int)stat.Value;
                if (character.StatComp.TryGetStat(VRage.Utils.MyStringHash.Get("player_hydrogen_bottles"), out stat))
                    HydrogenBottles = (int)stat.Value;
            }
        }

        [DataMember(Name = "health")]
        public float Health { get; set; }
        [DataMember(Name = "hydrogen", EmitDefaultValue = false)]
        public float? Hydrogen { get; set; }
        [DataMember(Name = "hydrogen_bottles", EmitDefaultValue = false)]
        public int? HydrogenBottles { get; set; }
        [DataMember(Name = "oxygen", EmitDefaultValue = false)]
        public float? Oxygen { get; set; }
        [DataMember(Name = "oxygen_bottles", EmitDefaultValue = false)]
        public int? OxygenBottles { get; set; }
    }

    [DataContract]
    internal class PlayerList
    {
        [DataMember(Name = "players")]
        public string[] Names { get; set; }
    }
}