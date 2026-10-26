using Terraria;
using Terraria.ModLoader;
using OdetteCharacter.Common.Compatibility;
using OdetteCharacter.Common.Drawing;

namespace OdetteCharacter;

public sealed class OdetteCharacter : Mod
{
    public override void Load()
    {
        if (!Main.dedServ) CharacterAtlas.Load(this);
    }

    public override void PostSetupContent() => AppearanceCompatibility.Initialize(this);

    public override void Unload()
    {
        CharacterAtlas.Unload();
        AppearanceCompatibility.Unload();
    }
}
