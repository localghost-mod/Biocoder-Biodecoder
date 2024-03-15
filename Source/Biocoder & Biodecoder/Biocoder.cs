using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace Biocoder
{
    public class CompUnbiocoded : CompTargetable
    {
        protected override bool PlayerChoosesTarget => true;

        public override IEnumerable<Thing> GetTargets(Thing targetChosenByPlayer = null)
        {
            yield return targetChosenByPlayer;
        }

        protected override TargetingParameters GetTargetingParameters() =>
            new TargetingParameters
            {
                canTargetItems = true,
                mapObjectTargetsMustBeAutoAttackable = false,
                validator = target => target.Thing.TryGetComp<CompBladelinkWeapon>() == null && (!target.Thing.TryGetComp<CompBiocodable>()?.Biocoded ?? false)
            };
    }

    public class CompProperties_Unbiocoded : CompProperties_Targetable
    {
        public CompProperties_Unbiocoded() => compClass = typeof(CompUnbiocoded);
    }

    public class JobDriver_Biocode : JobDriver
    {
        private Thing Item
        {
            get => job.GetTarget(TargetIndex.A).Thing;
        }
        private Thing Target
        {
            get => job.GetTarget(TargetIndex.B).Thing;
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed) =>
            pawn.Reserve(Target, job, 1, 1, null, errorOnFailed) && pawn.Reserve(Item, job, 1, 1, null, errorOnFailed);

        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch).FailOnDespawnedOrNull(TargetIndex.A).FailOnDespawnedOrNull(TargetIndex.B);
            yield return Toils_Haul.StartCarryThing(TargetIndex.A);
            yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.Touch).FailOnDespawnedOrNull(TargetIndex.B);
            Toil toil = Toils_General.Wait(600, TargetIndex.None);
            toil.WithProgressBarToilDelay(TargetIndex.B, false, -0.5f);
            toil.FailOnDespawnedOrNull(TargetIndex.B);
            toil.FailOnCannotTouch(TargetIndex.B, PathEndMode.Touch);
            yield return toil;
            yield return Toils_General.Do(new Action(Biocode));
        }

        private void Biocode()
        {
            var compQuality = Target.TryGetComp<CompQuality>();
            if (Biocoder.settings.qualityOffset && compQuality?.Quality < QualityCategory.Legendary)
            {
                var targetQualityCategory = compQuality.Quality + 1;
                Messages.Message(
                    "Biocoder.BiocoderUsedGainQuality".Translate(pawn.NameShortColored, Target.Label, targetQualityCategory.GetLabel()),
                    MessageTypeDefOf.NeutralEvent
                );
                compQuality.SetQuality(targetQualityCategory, ArtGenerationContext.Colony);
            }
            else
                Messages.Message("Biocoder.BiocoderUsed".Translate(pawn.NameShortColored, Target.Label), MessageTypeDefOf.NeutralEvent);
            Item.SplitOff(1).Destroy(DestroyMode.Vanish);
            Target.TryGetComp<CompBiocodable>().CodeFor(pawn);
        }
    }
}
