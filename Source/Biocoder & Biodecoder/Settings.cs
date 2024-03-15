using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Biocoder
{
    [StaticConstructorOnStartup]
    public static class Startup
    {
        static Startup()
        {
            if (Biocoder.settings.weaponsBiocode)
                DefDatabase<ThingDef>
                    .AllDefsListForReading.Where(def => !(def.weaponClasses.NullOrEmpty() || def.HasComp(typeof(CompBiocodable)) || def.HasComp(typeof(CompBladelinkWeapon))))
                    .ToList()
                    .ForEach(weapon => weapon.comps.Add(new CompProperties_Biocodable()));
            if (Biocoder.settings.apparelsBiocode)
                DefDatabase<ThingDef>
                    .AllDefsListForReading.Where(def => def.thingClass == typeof(Apparel) && !def.HasComp(typeof(CompBiocodable)))
                    .ToList()
                    .ForEach(apparel => apparel.comps.Add(new CompProperties_Biocodable()));
        }
    }

    public class Settings : ModSettings
    {
        public bool weaponsBiocode;
        public bool apparelsBiocode;
        public bool qualityOffset;

        public void DoWindowContents(Rect inRect)
        {
            var height = 28f;
            var ls = new Listing_Standard();
            ls.Begin(inRect);
            {
                var rowRect = ls.GetRect(height);
                var row = new WidgetRow(rowRect.x, rowRect.y, UIDirection.RightThenDown, ls.ColumnWidth);
                row.Label("Biocoder.EnableWeaponBiocode".Translate());
                var rowRight = new WidgetRow(ls.ColumnWidth, row.FinalY, UIDirection.LeftThenDown);
                if (rowRight.ButtonIcon(weaponsBiocode ? Widgets.CheckboxOnTex : Widgets.CheckboxOffTex))
                    weaponsBiocode = !weaponsBiocode;
            }
            {
                var rowRect = ls.GetRect(height);
                var row = new WidgetRow(rowRect.x, rowRect.y, UIDirection.RightThenDown, ls.ColumnWidth);
                row.Label("Biocoder.EnableApparelBiocode".Translate());
                var rowRight = new WidgetRow(ls.ColumnWidth, row.FinalY, UIDirection.LeftThenDown);
                if (rowRight.ButtonIcon(apparelsBiocode ? Widgets.CheckboxOnTex : Widgets.CheckboxOffTex))
                    apparelsBiocode = !apparelsBiocode;
            }
            {
                var rowRect = ls.GetRect(height);
                var row = new WidgetRow(rowRect.x, rowRect.y, UIDirection.RightThenDown, ls.ColumnWidth);
                row.Label("Biocoder.RestartRequired".Translate());
            }
            ls.Gap();
            {
                var rowRect = ls.GetRect(height);
                var row = new WidgetRow(rowRect.x, rowRect.y, UIDirection.RightThenDown, ls.ColumnWidth);
                row.Label("Biocoder.EnableQualityOffset".Translate());
                var rowRight = new WidgetRow(ls.ColumnWidth, row.FinalY, UIDirection.LeftThenDown);
                if (rowRight.ButtonIcon(qualityOffset ? Widgets.CheckboxOnTex : Widgets.CheckboxOffTex))
                    qualityOffset = !qualityOffset;
            }
            {
                var rowRect = ls.GetRect(height);
                var row = new WidgetRow(rowRect.x, rowRect.y, UIDirection.RightThenDown, ls.ColumnWidth);
                row.Label("Biocoder.TakeEffectImmediately".Translate());
            }
            ls.End();
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref weaponsBiocode, "weaponsBiocode");
            Scribe_Values.Look(ref apparelsBiocode, "apparelsBiocode");
            Scribe_Values.Look(ref qualityOffset, "qualityOffset");
        }
    }

    public class Biocoder : Mod
    {
        public static Settings settings;

        public Biocoder(ModContentPack content)
            : base(content) => settings = GetSettings<Settings>();

        public override void DoSettingsWindowContents(Rect inRect) => settings.DoWindowContents(inRect);

        public override string SettingsCategory() => "Biocoder".Translate();
    }
}
