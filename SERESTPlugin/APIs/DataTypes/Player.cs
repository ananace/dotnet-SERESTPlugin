using System.Runtime.Serialization;

namespace SERESTPlugin.APIs.DataTypes
{
    [DataContract]
    internal class PlayerStatus
    {
        public PlayerStatus() { }
        public PlayerStatus(Sandbox.Game.Entities.Character.MyCharacter character)
        {
            Health = character.StatComp.HealthRatio;

            if (character.OxygenComponent != null)
            {
                Oxygen = character.OxygenLevel;
                var gas = character.OxygenComponent;
                Hydrogen = gas.GetGasFillLevel(Sandbox.Game.Entities.Character.Components.MyCharacterOxygenComponent.HydrogenId);

                Sandbox.Game.Entities.MyEntityStat stat;
                if (character.StatComp.TryGetStat(VRage.Utils.MyStringHash.Get("player_oxygen_bottles"), out stat))
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