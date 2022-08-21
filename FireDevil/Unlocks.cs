using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FireDevil
{
    public static class Unlocks
    {
        public static IEnumerable<DoctrineUpgradeSystem.DoctrineType> GetDoctrines()
        {
            for (int i = 1; i <= 4; i++)
            {
                yield return DoctrineUpgradeSystem.GetSermonReward(SermonCategory.Afterlife, i, true);
                yield return DoctrineUpgradeSystem.GetSermonReward(SermonCategory.Afterlife, i, false);
            }
            for (int i = 1; i <= 4; i++)
            {
                yield return DoctrineUpgradeSystem.GetSermonReward(SermonCategory.Food, i, true);
                yield return DoctrineUpgradeSystem.GetSermonReward(SermonCategory.Food, i, false);
            }
            for (int i = 1; i <= 4; i++)
            {
                yield return DoctrineUpgradeSystem.GetSermonReward(SermonCategory.LawAndOrder, i, true);
                yield return DoctrineUpgradeSystem.GetSermonReward(SermonCategory.LawAndOrder, i, false);
            }
            for (int i = 1; i <= 4; i++)
            {
                yield return DoctrineUpgradeSystem.GetSermonReward(SermonCategory.Possession, i, true);
                yield return DoctrineUpgradeSystem.GetSermonReward(SermonCategory.Possession, i, false);
            }
            for (int i = 1; i <= 4; i++)
            {
                yield return DoctrineUpgradeSystem.GetSermonReward(SermonCategory.WorkAndWorship, i, true);
                yield return DoctrineUpgradeSystem.GetSermonReward(SermonCategory.WorkAndWorship, i, false);
            }
        }

        public static void ToggleUnlock(DoctrineUpgradeSystem.DoctrineType doctrine)
        {
            if (!DoctrineUpgradeSystem.GetUnlocked(doctrine))
            {
                DoctrineUpgradeSystem.UnlockAbility(doctrine);
                var alt = GetAlternative(doctrine);
                if (DoctrineUpgradeSystem.GetUnlocked(alt))
                    ToggleUnlock(alt);
            }
            else
            {
                DoctrineUpgradeSystem.UnlockedUpgrades.Remove(doctrine);

                switch (doctrine)
                {
                    case DoctrineUpgradeSystem.DoctrineType.WorkWorship_FasterBuilding:
                        RemoveUnlockAbility(UpgradeSystem.Type.Ritual_FasterBuilding);
                        break;

                    case DoctrineUpgradeSystem.DoctrineType.WorkWorship_Enlightenment:
                        RemoveUnlockAbility(UpgradeSystem.Type.Ritual_Enlightenment);
                        break;

                    case DoctrineUpgradeSystem.DoctrineType.WorkWorship_FaithfulTrait:
                        RemoveAddCultTrait(FollowerTrait.TraitType.Faithful);
                        break;

                    case DoctrineUpgradeSystem.DoctrineType.WorkWorship_GoodWorkerTrait:
                        RemoveAddCultTrait(FollowerTrait.TraitType.Industrious);
                        break;

                    case DoctrineUpgradeSystem.DoctrineType.WorkWorship_WorkThroughNightRitual:
                        RemoveUnlockAbility(UpgradeSystem.Type.Ritual_WorkThroughNight);
                        break;

                    case DoctrineUpgradeSystem.DoctrineType.WorkWorship_HolidayRitual:
                        RemoveUnlockAbility(UpgradeSystem.Type.Ritual_Holiday);
                        JudgementMeter.ShowModify(-1);
                        break;

                    case DoctrineUpgradeSystem.DoctrineType.Possessions_MoreFaithFromHomes:
                        RemoveAddCultTrait(FollowerTrait.TraitType.ConstructionEnthusiast);
                        break;

                    case DoctrineUpgradeSystem.DoctrineType.Possessions_MoreFaithFromRituals:
                        RemoveAddCultTrait(FollowerTrait.TraitType.SermonEnthusiast);
                        break;

                    case DoctrineUpgradeSystem.DoctrineType.Possessions_TraitMaterialistic:
                        RemoveAddCultTrait(FollowerTrait.TraitType.Materialistic);
                        break;

                    case DoctrineUpgradeSystem.DoctrineType.Possessions_TraitFalseIdols:
                        RemoveAddCultTrait(FollowerTrait.TraitType.FalseIdols);
                        break;

                    case DoctrineUpgradeSystem.DoctrineType.Possessions_AlmsToPoorRitual:
                        RemoveUnlockAbility(UpgradeSystem.Type.Ritual_AlmsToPoor);
                        break;

                    case DoctrineUpgradeSystem.DoctrineType.Possessions_DonationRitual:
                        RemoveUnlockAbility(UpgradeSystem.Type.Ritual_DonationRitual);
                        break;

                    case DoctrineUpgradeSystem.DoctrineType.Sustenance_Fast:
                        RemoveUnlockAbility(UpgradeSystem.Type.Ritual_Fast);
                        JudgementMeter.ShowModify(1);
                        break;

                    case DoctrineUpgradeSystem.DoctrineType.Sustenance_Feast:
                        RemoveUnlockAbility(UpgradeSystem.Type.Ritual_Feast);
                        JudgementMeter.ShowModify(-1);
                        break;

                    case DoctrineUpgradeSystem.DoctrineType.Sustenance_TraitMushroomEncouraged:
                        RemoveAddCultTrait(FollowerTrait.TraitType.MushroomEncouraged);
                        break;

                    case DoctrineUpgradeSystem.DoctrineType.Sustenance_TraitMushroomBanned:
                        RemoveAddCultTrait(FollowerTrait.TraitType.MushroomBanned);
                        break;

                    case DoctrineUpgradeSystem.DoctrineType.Sustenance_TraitCannibal:
                        RemoveAddCultTrait(FollowerTrait.TraitType.Cannibal);
                        JudgementMeter.ShowModify(1);
                        break;

                    case DoctrineUpgradeSystem.DoctrineType.Sustenance_TraitGrassEater:
                        RemoveAddCultTrait(FollowerTrait.TraitType.GrassEater);
                        JudgementMeter.ShowModify(-1);
                        break;

                    case DoctrineUpgradeSystem.DoctrineType.Sustenance_TraitHarvestRitual:
                        RemoveUnlockAbility(UpgradeSystem.Type.Ritual_HarvestRitual);
                        break;

                    case DoctrineUpgradeSystem.DoctrineType.Sustenance_TraitFishingRitual:
                        RemoveUnlockAbility(UpgradeSystem.Type.Ritual_FishingRitual);
                        break;

                    case DoctrineUpgradeSystem.DoctrineType.DeathSacrifice_TraitSacrificeEnthusiast:
                        RemoveAddCultTrait(FollowerTrait.TraitType.SacrificeEnthusiast);
                        break;

                    case DoctrineUpgradeSystem.DoctrineType.DeathSacrifice_TraitDesensitisedToDeath:
                        RemoveAddCultTrait(FollowerTrait.TraitType.DesensitisedToDeath);
                        break;

                    case DoctrineUpgradeSystem.DoctrineType.DeathSacrifice_RessurectionRitual:
                        RemoveUnlockAbility(UpgradeSystem.Type.Ritual_Ressurect);
                        JudgementMeter.ShowModify(1);
                        break;

                    case DoctrineUpgradeSystem.DoctrineType.DeathSacrifice_Funeral:
                        RemoveUnlockAbility(UpgradeSystem.Type.Ritual_Funeral);
                        JudgementMeter.ShowModify(-1);
                        break;

                    case DoctrineUpgradeSystem.DoctrineType.DeathSacrifice_TraitRespectElders:
                        RemoveAddCultTrait(FollowerTrait.TraitType.LoveElderly);
                        JudgementMeter.ShowModify(-1);
                        break;

                    case DoctrineUpgradeSystem.DoctrineType.DeathSacrifice_TraitOldDieYoung:
                        RemoveAddCultTrait(FollowerTrait.TraitType.HateElderly);
                        JudgementMeter.ShowModify(1);
                        break;

                    case DoctrineUpgradeSystem.DoctrineType.DeathSacrifice_BuildingReturnToEarth:
                        RemoveUnlockAbility(UpgradeSystem.Type.Building_NaturalBurial);
                        JudgementMeter.ShowModify(1);
                        break;

                    case DoctrineUpgradeSystem.DoctrineType.DeathSacrifice_BuildingGoodGraves:
                        RemoveUnlockAbility(UpgradeSystem.Type.Building_Graves);
                        JudgementMeter.ShowModify(-1);
                        break;

                    case DoctrineUpgradeSystem.DoctrineType.LawOrder_MurderFollower:
                        JudgementMeter.ShowModify(1);
                        break;

                    case DoctrineUpgradeSystem.DoctrineType.LawOrder_AscendFollower:
                        RemoveUnlockAbility(UpgradeSystem.Type.Ritual_Ascend);
                        JudgementMeter.ShowModify(-1);
                        break;

                    case DoctrineUpgradeSystem.DoctrineType.LawOrder_FightPitRitual:
                        RemoveUnlockAbility(UpgradeSystem.Type.Ritual_Fightpit);
                        JudgementMeter.ShowModify(1);
                        break;

                    case DoctrineUpgradeSystem.DoctrineType.LawOrder_JudgementRitual:
                        RemoveUnlockAbility(UpgradeSystem.Type.Ritual_Wedding);
                        JudgementMeter.ShowModify(-1);
                        break;

                    case DoctrineUpgradeSystem.DoctrineType.LawOrder_AssignFaithEnforcerRitual:
                        RemoveUnlockAbility(UpgradeSystem.Type.Ritual_AssignFaithEnforcer);
                        break;

                    case DoctrineUpgradeSystem.DoctrineType.LawOrder_AssignTaxCollectorRitual:
                        RemoveUnlockAbility(UpgradeSystem.Type.Ritual_AssignTaxCollector);
                        break;

                    case DoctrineUpgradeSystem.DoctrineType.LawOrder_TraitDisciplinarian:
                        RemoveAddCultTrait(FollowerTrait.TraitType.Disciplinarian);
                        JudgementMeter.ShowModify(1);
                        break;

                    case DoctrineUpgradeSystem.DoctrineType.LawOrder_TraitLibertarian:
                        RemoveAddCultTrait(FollowerTrait.TraitType.Libertarian);
                        JudgementMeter.ShowModify(-1);
                        break;

                    case DoctrineUpgradeSystem.DoctrineType.Special_Brainwashed:
                        RemoveUnlockAbility(UpgradeSystem.Type.Ritual_Brainwashing);
                        break;

                    case DoctrineUpgradeSystem.DoctrineType.Special_Sacrifice:
                        RemoveUnlockAbility(UpgradeSystem.Type.Ritual_Sacrifice);
                        break;

                    case DoctrineUpgradeSystem.DoctrineType.Special_Consume:
                        RemoveUnlockAbility(UpgradeSystem.Type.Ritual_ConsumeFollower);
                        break;

                    case DoctrineUpgradeSystem.DoctrineType.Special_ReadMind:
                        DataManager.Instance.CanReadMinds = false;
                        break;

                    case DoctrineUpgradeSystem.DoctrineType.Special_Bonfire:
                        RemoveUnlockAbility(UpgradeSystem.Type.Ritual_FirePit);
                        break;
                }
            }
        }

        private static void RemoveUnlockAbility(UpgradeSystem.Type type)
        {
            UpgradeSystem.UnlockedUpgrades.Remove(type);
        }

        private static void RemoveAddCultTrait(FollowerTrait.TraitType TraitType)
        {
            DataManager.Instance.CultTraits.Remove(TraitType);
        }

        public static int GetLevel(DoctrineUpgradeSystem.DoctrineType doctrine)
        {
            switch (doctrine)
            {
                case DoctrineUpgradeSystem.DoctrineType.DeathSacrifice_TraitSacrificeEnthusiast:
                case DoctrineUpgradeSystem.DoctrineType.DeathSacrifice_TraitDesensitisedToDeath:
                case DoctrineUpgradeSystem.DoctrineType.Sustenance_Fast:
                case DoctrineUpgradeSystem.DoctrineType.Sustenance_Feast:
                case DoctrineUpgradeSystem.DoctrineType.LawOrder_MurderFollower:
                case DoctrineUpgradeSystem.DoctrineType.LawOrder_AscendFollower:
                case DoctrineUpgradeSystem.DoctrineType.Possessions_ExtortTithes:
                case DoctrineUpgradeSystem.DoctrineType.Possessions_Bribe:
                case DoctrineUpgradeSystem.DoctrineType.WorkWorship_FaithfulTrait:
                case DoctrineUpgradeSystem.DoctrineType.WorkWorship_GoodWorkerTrait:
                    return 1;

                case DoctrineUpgradeSystem.DoctrineType.DeathSacrifice_RessurectionRitual:
                case DoctrineUpgradeSystem.DoctrineType.DeathSacrifice_Funeral:
                case DoctrineUpgradeSystem.DoctrineType.Sustenance_TraitCannibal:
                case DoctrineUpgradeSystem.DoctrineType.Sustenance_TraitGrassEater:
                case DoctrineUpgradeSystem.DoctrineType.LawOrder_FightPitRitual:
                case DoctrineUpgradeSystem.DoctrineType.LawOrder_JudgementRitual:
                case DoctrineUpgradeSystem.DoctrineType.Possessions_TraitMaterialistic:
                case DoctrineUpgradeSystem.DoctrineType.Possessions_TraitFalseIdols:
                case DoctrineUpgradeSystem.DoctrineType.WorkWorship_Inspire:
                case DoctrineUpgradeSystem.DoctrineType.WorkWorship_Intimidate:
                    return 2;

                case DoctrineUpgradeSystem.DoctrineType.DeathSacrifice_TraitRespectElders:
                case DoctrineUpgradeSystem.DoctrineType.DeathSacrifice_TraitOldDieYoung:
                case DoctrineUpgradeSystem.DoctrineType.Sustenance_TraitHarvestRitual:
                case DoctrineUpgradeSystem.DoctrineType.Sustenance_TraitFishingRitual:
                case DoctrineUpgradeSystem.DoctrineType.LawOrder_TraitDisciplinarian:
                case DoctrineUpgradeSystem.DoctrineType.LawOrder_TraitLibertarian:
                case DoctrineUpgradeSystem.DoctrineType.Possessions_AlmsToPoorRitual:
                case DoctrineUpgradeSystem.DoctrineType.Possessions_DonationRitual:
                case DoctrineUpgradeSystem.DoctrineType.WorkWorship_FasterBuilding:
                case DoctrineUpgradeSystem.DoctrineType.WorkWorship_Enlightenment:
                    return 3;

                case DoctrineUpgradeSystem.DoctrineType.DeathSacrifice_BuildingReturnToEarth:
                case DoctrineUpgradeSystem.DoctrineType.DeathSacrifice_BuildingGoodGraves:
                case DoctrineUpgradeSystem.DoctrineType.Sustenance_TraitMushroomEncouraged:
                case DoctrineUpgradeSystem.DoctrineType.Sustenance_TraitMushroomBanned:
                case DoctrineUpgradeSystem.DoctrineType.LawOrder_AssignFaithEnforcerRitual:
                case DoctrineUpgradeSystem.DoctrineType.LawOrder_AssignTaxCollectorRitual:
                case DoctrineUpgradeSystem.DoctrineType.Possessions_MoreFaithFromHomes:
                case DoctrineUpgradeSystem.DoctrineType.Possessions_MoreFaithFromRituals:
                case DoctrineUpgradeSystem.DoctrineType.WorkWorship_WorkThroughNightRitual:
                case DoctrineUpgradeSystem.DoctrineType.WorkWorship_HolidayRitual:
                    return 4;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    
        public static DoctrineUpgradeSystem.DoctrineType GetAlternative(DoctrineUpgradeSystem.DoctrineType doctrine)
        {
            var sermon = DoctrineUpgradeSystem.GetCategory(doctrine);
            var level = GetLevel(doctrine);

            if (doctrine == DoctrineUpgradeSystem.GetSermonReward(sermon, level, true))
                return DoctrineUpgradeSystem.GetSermonReward(sermon, level, false);
            else
                return DoctrineUpgradeSystem.GetSermonReward(sermon, level, true);
        }
    }
}
