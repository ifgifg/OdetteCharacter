using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace OdetteCharacter.Common.Config;

public sealed class OdetteCharacterConfig : ModConfig
{
    public override ConfigScope Mode => ConfigScope.ClientSide;

    [DefaultValue(true)]
    public bool Enabled { get; set; } = true;

    [DefaultValue(false)]
    public bool GentleExpression { get; set; }
}
