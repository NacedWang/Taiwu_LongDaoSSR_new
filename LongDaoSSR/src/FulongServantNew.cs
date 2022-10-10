using TaiwuModdingLib.Core.Plugin;
using GameData.Utilities;
using HarmonyLib;
using GameData.Common;
using System;
using GameData.Domains.Character;
using GameData.Domains.TaiwuEvent.EventHelper;
using GameData.Domains;
using GameData.Domains.Organization;
using GameData.Domains.Character.Creation;
using GameData.Domains.Map;
using GameData.Domains.TaiwuEvent;
using System.Collections.Generic;
using Config;

namespace LongDaoSSR.src
{

    [PluginConfig("LongDaoSSR", "Naced", "1.4.0")]
    public class FulongServantSSR : TaiwuRemakePlugin
    {

        Harmony harmony;

        public override void Dispose()
        {
            if (harmony != null)
            {
                harmony.UnpatchSelf();
            }

        }

        public override void Initialize()
        {
            harmony = Harmony.CreateAndPatchAll(typeof(FulongServantSSR));
            AdaptableLog.Info("Initialize  LongDaoSSR");
        }

        // 配置项
        private static bool SameMorality;
        private static bool SameGender;
        private static bool FeatureDream;
        private static bool FeatureBattle;
        private static bool FeatureRead;
        private static bool FeatureOld;
        private static bool FeatureDao;
        private static bool FeatureDead;
        private static int SkillAdd;
        private static int SkillMin;
        private static int SkillRandom;
        private static int MainAttributeMin;
        private static int MainAttributeRandom;
        private static int BasicFeatures;
        private static bool HusbandOrWife;

        public override void OnModSettingUpdate()
        {
            AdaptableLog.Info("LongDaoSSR OnModSettingUpdate");
            DomainManager.Mod.GetSetting(base.ModIdStr, "SameMorality", ref FulongServantSSR.SameMorality);
            DomainManager.Mod.GetSetting(base.ModIdStr, "SameGender", ref FulongServantSSR.SameGender);
            DomainManager.Mod.GetSetting(base.ModIdStr, "FeatureDream", ref FulongServantSSR.FeatureDream);
            DomainManager.Mod.GetSetting(base.ModIdStr, "FeatureBattle", ref FulongServantSSR.FeatureBattle);
            DomainManager.Mod.GetSetting(base.ModIdStr, "FeatureRead", ref FulongServantSSR.FeatureRead);
            DomainManager.Mod.GetSetting(base.ModIdStr, "FeatureOld", ref FulongServantSSR.FeatureOld);
            DomainManager.Mod.GetSetting(base.ModIdStr, "FeatureDao", ref FulongServantSSR.FeatureDao);
            DomainManager.Mod.GetSetting(base.ModIdStr, "FeatureDead", ref FulongServantSSR.FeatureDead);
            DomainManager.Mod.GetSetting(base.ModIdStr, "SkillAdd", ref FulongServantSSR.SkillAdd);
            DomainManager.Mod.GetSetting(base.ModIdStr, "SkillMin", ref FulongServantSSR.SkillMin);
            DomainManager.Mod.GetSetting(base.ModIdStr, "SkillRandom", ref FulongServantSSR.SkillRandom);
            DomainManager.Mod.GetSetting(base.ModIdStr, "MainAttributeMin", ref FulongServantSSR.MainAttributeMin);
            DomainManager.Mod.GetSetting(base.ModIdStr, "MainAttributeRandom", ref FulongServantSSR.MainAttributeRandom);
            DomainManager.Mod.GetSetting(base.ModIdStr, "BasicFeatures", ref FulongServantSSR.BasicFeatures);
            DomainManager.Mod.GetSetting(base.ModIdStr, "HusbandOrWife", ref FulongServantSSR.HusbandOrWife);
            AdaptableLog.Info(string.Format("LongDaoSSR setting : \n SameMorality :{0} \n, FeatureDao : {1} \n, BasicFeatures: {2} ", SameMorality, FeatureDao, BasicFeatures));
        }

        [HarmonyPrefix, HarmonyPatch(typeof(EventHelper), "CreateFulongServant")]
        public unsafe static bool preCreateFulongServant()
        {
            AdaptableLog.Info("preCreateFulongServant start");
            TaiwuEventDomain domain = DomainManager.TaiwuEvent;
            DataContext context = domain.MainThreadDataContext;
            GameData.Domains.Character.Character taiwuChar = DomainManager.Taiwu.GetTaiwu();
            Location location = taiwuChar.GetLocation();
            OrganizationInfo orgInfo = taiwuChar.GetOrganizationInfo();
            orgInfo.Grade = 0;
            sbyte gender = 0;
            // 性别
            AdaptableLog.Info("taiwuGender " + Convert.ToString(taiwuChar.GetGender()));
            if (SameGender)
            {
                gender = taiwuChar.GetGender();
            }
            else
            {
                // 改变性别
                gender = (sbyte)(taiwuChar.GetGender() == 0 ? 1 : 0);
            }
            sbyte growingSectId = OrganizationDomain.GetRandomSectOrgTemplateId(context.Random, gender);
            sbyte growingGrade = (sbyte)context.Random.Next(6);
            sbyte stateTemplateId = DomainManager.Map.GetStateTemplateIdByAreaId(location.AreaId);
            short charTemplateId = OrganizationDomain.GetCharacterTemplateId(growingSectId, stateTemplateId, gender);
            AdaptableLog.Info("原TemplateId " + Convert.ToString(charTemplateId));

            // 年龄
            short age = (short)16;
            IntelligentCharacterCreationInfo info = new IntelligentCharacterCreationInfo(location, orgInfo, charTemplateId)
            {
                Age = age,
                GrowingSectId = growingSectId,
                GrowingSectGrade = growingGrade,
                LifeSkillsAdjustBonus = { },
                CombatSkillsAdjustBonus = { },
                InitializeSectSkills = false,
                //魅力下限
                BaseAttraction = 900
            };
            GameData.Domains.Character.Character character = DomainManager.Character.CreateIntelligentCharacter(context, ref info);
            int charId = character.GetId();
            DomainManager.Character.CompleteCreatingCharacter(charId);
            // 年龄
            character.SetActualAge(16, domain.MainThreadDataContext);

            // 样貌处理,去掉奇怪的减魅力的项
            character.GetAvatar().Feature1Id = (short)context.Random.Next(7);
            character.GetAvatar().Feature2Id = 1;

            // 特性
            HashSet<short> origFeatureSet = new HashSet<short>();
            List<short> orgFeatureList = new List<short>(character.GetFeatureIds());
            orgFeatureList.ForEach(featureId =>
            {
                // 删除旧特质
                if (featureId <= 167)
                {
                    character.RemoveFeature(domain.MainThreadDataContext, featureId);
                }
                if (featureId >= 171 && featureId <= 182)
                {
                    AdaptableLog.Info("原抓周特性 " + Convert.ToString(featureId));
                    character.RemoveFeature(domain.MainThreadDataContext, featureId);
                }
            });
            // 抓周特质
            character.AddFeature(domain.MainThreadDataContext, (short)(171 + context.Random.Next(11)), false);

            // 龙岛忠仆特性
            character.AddFeature(domain.MainThreadDataContext, 199, false);



            int count = 0;
            int tryFeatureTime = 0;
            HashSet<short> featureSet = new HashSet<short>();
            // 各项配置  max 164 ,%6 ,min 0,
            if (BasicFeatures > 0)
            {
                // 最多尝试 50 次
                while (count < BasicFeatures && tryFeatureTime < 50)
                {
                    tryFeatureTime++;
                    // 防重复配置
                    short featureId = (short)(context.Random.Next(27) * 6 + context.Random.Next(3));
                    if (featureSet.Contains(featureId))
                    {
                        continue;
                    }
                    character.AddFeature(domain.MainThreadDataContext, featureId, false);
                    featureSet.Add(featureId);
                    count++;
                }
            }
            if (FeatureDream)
            {
                character.AddFeature(domain.MainThreadDataContext, CharacterFeature.DefKey.DreamLover, false);
            }
            if (FeatureBattle)
            {
                character.AddFeature(domain.MainThreadDataContext, CharacterFeature.DefKey.CombatSkillLearning, false);
            }
            if (FeatureRead)
            {
                character.AddFeature(domain.MainThreadDataContext, CharacterFeature.DefKey.LifeSkillLearning, false);
            }
            if (FeatureOld)
            {
                character.AddFeature(domain.MainThreadDataContext, CharacterFeature.DefKey.Longevity, false);
            }
            // 健康
            character.SetHealth(character.GetMaxHealth(), domain.MainThreadDataContext);
            if (FeatureDao)
            {
                character.AddFeature(domain.MainThreadDataContext, CharacterFeature.DefKey.ProtectedByPrayer, false);
            }
            //活死人
            if (FeatureDead)
            {
                character.AddFeature(domain.MainThreadDataContext, CharacterFeature.DefKey.WalkingDead, false);
            }

            //关系
            if (HusbandOrWife)
            {
                DomainManager.Character.AddHusbandOrWifeRelations(domain.MainThreadDataContext, charId, taiwuChar.GetId(), int.MinValue);
            }
           
            // 成长 均衡 不好使
            //Traverse.Create(character).Field("_combatSkillQualificationGrowthType").SetValue((sbyte)0);
            //Traverse.Create(character).Field("_lifeSkillQualificationGrowthType").SetValue((sbyte)0);

            // 立场
            if (SameMorality)
            {
                character.SetBaseMorality(taiwuChar.GetBaseMorality(), domain.MainThreadDataContext);
            }

            // 主属性
            MainAttributes mainAttributes = character.GetBaseMainAttributes();
            for (int i = 0; i < 6; i++)
            {
                // 随机资质增加量
                int randomBonus = 0;
                if (MainAttributeRandom > 0)
                {
                    randomBonus = context.Random.Next(MainAttributeRandom);
                }
                // 最终资质值
                short finalMainAttribute = (short)(MainAttributeMin + randomBonus);
                mainAttributes.Items[i] = finalMainAttribute;
            }
            character.SetBaseMainAttributes(mainAttributes, domain.MainThreadDataContext);
            character.SetCurrMainAttributes( character.GetMaxMainAttributes(), domain.MainThreadDataContext);

            // 技艺
            for (int i = 0; i < 16; i++)
            {
                decimal taiwuBonus = 0;
                // 太吾资质增加量
                if (SkillAdd > 0)
                {
                    decimal bonusPercent = decimal.Round((decimal)SkillAdd / (decimal)100, 2);
                    taiwuBonus = bonusPercent * taiwuChar.GetBaseLifeSkillQualifications().Items[i];
                }
                // 随机资质增加量
                int randomBonus = 0;
                if (SkillRandom > 0)
                {
                    randomBonus = context.Random.Next(SkillRandom);
                }
                // 最终资质值
                short finalSkill = (short)(SkillMin + randomBonus + taiwuBonus);
                AdaptableLog.Info("太吾技艺资质加成: " + Convert.ToString(taiwuBonus));
                character.GetBaseLifeSkillQualifications().Items[i] = finalSkill;
            }
            // 永久更新资质
            character.SetBaseLifeSkillQualifications(ref character.GetBaseLifeSkillQualifications(), domain.MainThreadDataContext);

            // 武学
            for (int i = 0; i < 14; i++)
            {
                decimal taiwuBonus = 0;
                // 太吾资质增加量
                if (SkillAdd > 0)
                {
                    decimal bonusPercent = decimal.Round((decimal)SkillAdd / (decimal)100, 2);
                    taiwuBonus = bonusPercent * taiwuChar.GetBaseCombatSkillQualifications().Items[i];
                }
                // 随机资质增加量
                int randomBonus = 0;
                if (SkillRandom > 0)
                {
                    randomBonus = context.Random.Next(SkillRandom);
                }
                // 最终资质值
                short finalSkill = (short)(SkillMin + randomBonus + taiwuBonus);
                AdaptableLog.Info("太吾战斗资质加成: " + Convert.ToString(taiwuBonus));
                character.GetBaseCombatSkillQualifications().Items[i] = finalSkill;
            }
            // 永久更新资质
            character.SetBaseCombatSkillQualifications(ref character.GetBaseCombatSkillQualifications(), domain.MainThreadDataContext);

            // 原基础操作
            character.SetIdealSect(growingSectId, context);
            DomainManager.Character.TryCreateGeneralRelation(context, taiwuChar, character);
            DomainManager.Character.ChangeFavorability(context, character, taiwuChar, 10000);
            DomainManager.Taiwu.JoinGroup(context, charId, true);
            DomainManager.Taiwu.ReceiveCharacters(context, new int[]
            {
                charId
            });
            AdaptableLog.Info("preCreateFulongServant end");
            return false;
        }


        /*
        [HarmonyPrefix, HarmonyPatch(typeof(SkillBook), "SetCurrDurability")]
        public static bool postAddFeature(ref SkillBook __instance,ref short currDurability)
        {
            currDurability = __instance.GetMaxDurability();
            return true;
            //    AdaptableLog.Info("instance.GetId " + Convert.ToString( __instance.GetId()));
            //AdaptableLog.Info("DataContext " + context.ToString());
            //   AdaptableLog.Info("featureId " + Convert.ToString(featureId));
        }
        */
    }
}
