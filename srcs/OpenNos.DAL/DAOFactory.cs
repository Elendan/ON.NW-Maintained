/*
 * This file is part of the OpenNos Emulator Project. See AUTHORS file for Copyright information
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 */

using OpenNos.DAL.Interface;
using OpenNos.DAL.EF;

namespace OpenNos.DAL
{
    public class DaoFactory
    {
        #region Members

        private static IAccountDAO _accountDao;
        private static IBazaarItemDAO _bazaarItemDao;
        private static ICardDAO _cardDao;
        private static IBCardDAO _bcardDao;
        private static IRollGeneratedItemDAO _rollGeneratedItemDao;
        private static IEquipmentOptionDAO _equipmentOptionDao;
        private static ICharacterDAO _characterDao;
        private static ICharacterRelationDAO _characterRelationDao;
        private static ICharacterHomeDAO _characterHomeDao;
        private static ICharacterSkillDAO _characterskillDao;
        private static ICharacterQuestDAO _characterQuestDao;
        private static IComboDAO _comboDao;
        private static IDropDAO _dropDao;
        private static IExchangeLogDao _exchangeLogDao;
        private static IFamilyCharacterDAO _familycharacterDao;
        private static IFamilyDAO _familyDao;
        private static IFamilyLogDAO _familylogDao;
        private static IGeneralLogDAO _generallogDao;
        private static IItemDAO _itemDao;
        private static IItemInstanceDAO _iteminstanceDao;
        private static ILevelUpRewardsDAO _levelUpRewardsDao;
        private static ILogChatDAO _logChatDao;
        private static ILogCommandsDAO _logCommandsDao;
        private static ILogVIPDAO _logVipDao;
        private static IMailDAO _mailDao;
        private static IMapDAO _mapDao;
        private static IMapMonsterDAO _mapmonsterDao;
        private static IMapNpcDAO _mapnpcDao;
        private static IMapTypeDAO _maptypeDao;
        private static IMapTypeMapDAO _maptypemapDao;
        private static IMateDAO _mateDao;
        private static IMinilandObjectDAO _minilandobjectDao;
        private static INpcMonsterDAO _npcmonsterDao;
        private static INpcMonsterSkillDAO _npcmonsterskillDao;
        private static IPenaltyLogDAO _penaltylogDao;
        private static IPortalDAO _portalDao;
        private static IQuestDAO _questDao;
        private static IQuestLogDAO _questLogDao;
        private static IQuestRewardDAO _questRewardDao;
        private static IQuestObjectiveDAO _questObjectiveDao;
        private static IQuicklistEntryDAO _quicklistDao;
        private static IRaidLogDAO _raidLogDao;
        private static IRecipeDAO _recipeDao;
        private static IRecipeItemDAO _recipeitemDao;
        private static IRespawnDAO _respawnDao;
        private static IRespawnMapTypeDAO _respawnMapTypeDao;
        private static IScriptedInstanceDAO _scriptedinstanceDao;
        private static IShopDAO _shopDao;
        private static IShopItemDAO _shopitemDao;
        private static IShopSkillDAO _shopskillDao;
        private static ISkillDAO _skillDao;
        private static IStaticBonusDAO _staticBonusDao;
        private static IStaticBuffDAO _staticBuffDao;
        private static ITeleporterDAO _teleporterDao;
        private static IUpgradeLogDao _upgradeLogDao;
        private static IAntiBotLogDAO _antiBotLogDao;

        #endregion

        #region Instantiation

        #endregion

        #region Properties

        public static IAntiBotLogDAO AntiBotLogDao => _antiBotLogDao ?? (_antiBotLogDao = new AntiBotLogDAO());

        public static IAccountDAO AccountDao => _accountDao ?? (_accountDao = new AccountDAO());

        public static IBazaarItemDAO BazaarItemDao => _bazaarItemDao ?? (_bazaarItemDao = new BazaarItemDAO());

        public static ICardDAO CardDao => _cardDao ?? (_cardDao = new CardDAO());

        public static IEquipmentOptionDAO EquipmentOptionDao => _equipmentOptionDao ?? (_equipmentOptionDao = new EquipmentOptionDAO());

        public static ICharacterDAO CharacterDao => _characterDao ?? (_characterDao = new CharacterDAO());

        public static ICharacterHomeDAO CharacterHomeDao => _characterHomeDao ?? (_characterHomeDao = new CharacterHomeDAO());

        public static ICharacterRelationDAO CharacterRelationDao => _characterRelationDao ?? (_characterRelationDao = new CharacterRelationDAO());

        public static ICharacterSkillDAO CharacterSkillDao => _characterskillDao ?? (_characterskillDao = new CharacterSkillDAO());

        public static ICharacterQuestDAO CharacterQuestDao => _characterQuestDao ?? (_characterQuestDao = new CharacterQuestDAO());

        public static IComboDAO ComboDao => _comboDao ?? (_comboDao = new ComboDAO());

        public static IDropDAO DropDao => _dropDao ?? (_dropDao = new DropDAO());

        public static IFamilyCharacterDAO FamilyCharacterDao => _familycharacterDao ?? (_familycharacterDao = new FamilyCharacterDAO());

        public static IFamilyDAO FamilyDao => _familyDao ?? (_familyDao = new FamilyDAO());

        public static IFamilyLogDAO FamilyLogDao => _familylogDao ?? (_familylogDao = new FamilyLogDAO());

        public static IGeneralLogDAO GeneralLogDao => _generallogDao ?? (_generallogDao = new GeneralLogDAO());

        public static IItemDAO ItemDao => _itemDao ?? (_itemDao = new ItemDAO());

        public static IItemInstanceDAO IteminstanceDao => _iteminstanceDao ?? (_iteminstanceDao = new ItemInstanceDAO());

        public static ILevelUpRewardsDAO LevelUpRewardsDao => _levelUpRewardsDao ?? (_levelUpRewardsDao = new LevelUpRewardsDAO());

        public static ILogChatDAO LogChatDao => _logChatDao ?? (_logChatDao = new LogChatDAO());

        public static ILogCommandsDAO LogCommandsDao => _logCommandsDao ?? (_logCommandsDao = new LogCommandsDAO());

        public static ILogVIPDAO LogVipDao => _logVipDao ?? (_logVipDao = new LogVIPDAO());

        public static IMailDAO MailDao => _mailDao ?? (_mailDao = new MailDAO());

        public static IMapDAO MapDao => _mapDao ?? (_mapDao = new MapDAO());

        public static IMapMonsterDAO MapMonsterDao => _mapmonsterDao ?? (_mapmonsterDao = new MapMonsterDAO());

        public static IMapNpcDAO MapNpcDao => _mapnpcDao ?? (_mapnpcDao = new MapNpcDAO());

        public static IMapTypeDAO MapTypeDao => _maptypeDao ?? (_maptypeDao = new MapTypeDAO());

        public static IMapTypeMapDAO MapTypeMapDao => _maptypemapDao ?? (_maptypemapDao = new MapTypeMapDAO());

        public static IMateDAO MateDao => _mateDao ?? (_mateDao = new MateDAO());

        public static IMinilandObjectDAO MinilandObjectDao => _minilandobjectDao ?? (_minilandobjectDao = new MinilandObjectDAO());

        public static INpcMonsterDAO NpcMonsterDao => _npcmonsterDao ?? (_npcmonsterDao = new NpcMonsterDAO());

        public static INpcMonsterSkillDAO NpcMonsterSkillDao => _npcmonsterskillDao ?? (_npcmonsterskillDao = new NpcMonsterSkillDAO());

        public static IPenaltyLogDAO PenaltyLogDao => _penaltylogDao ?? (_penaltylogDao = new PenaltyLogDAO());

        public static IPortalDAO PortalDao => _portalDao ?? (_portalDao = new PortalDAO());

        public static IQuestDAO QuestDao => _questDao ?? (_questDao = new QuestDAO());

        public static IQuestLogDAO QuestLogDao => _questLogDao ?? (_questLogDao = new QuestLogDAO());

        public static IQuestObjectiveDAO QuestObjectiveDao => _questObjectiveDao ?? (_questObjectiveDao = new QuestObjectiveDAO());

        public static IQuestRewardDAO QuestRewardDao => _questRewardDao ?? (_questRewardDao = new QuestRewardDAO());

        public static IQuicklistEntryDAO QuicklistEntryDao => _quicklistDao ?? (_quicklistDao = new QuicklistEntryDAO());

        public static IRaidLogDAO RaidLogDao => _raidLogDao ?? (_raidLogDao = new RaidLogDAO());

        public static IRecipeDAO RecipeDao => _recipeDao ?? (_recipeDao = new RecipeDAO());

        public static IRecipeItemDAO RecipeItemDao => _recipeitemDao ?? (_recipeitemDao = new RecipeItemDAO());

        public static IRespawnDAO RespawnDao => _respawnDao ?? (_respawnDao = new RespawnDAO());

        public static IRespawnMapTypeDAO RespawnMapTypeDao => _respawnMapTypeDao ?? (_respawnMapTypeDao = new RespawnMapTypeDAO());

        public static IShopDAO ShopDao => _shopDao ?? (_shopDao = new ShopDAO());

        public static IShopItemDAO ShopItemDao => _shopitemDao ?? (_shopitemDao = new ShopItemDAO());

        public static IShopSkillDAO ShopSkillDao => _shopskillDao ?? (_shopskillDao = new ShopSkillDAO());

        public static ISkillDAO SkillDao => _skillDao ?? (_skillDao = new SkillDAO());

        public static IStaticBonusDAO StaticBonusDao => _staticBonusDao ?? (_staticBonusDao = new StaticBonusDAO());

        public static IStaticBuffDAO StaticBuffDao => _staticBuffDao ?? (_staticBuffDao = new StaticBuffDAO());

        public static ITeleporterDAO TeleporterDao => _teleporterDao ?? (_teleporterDao = new TeleporterDAO());

        public static IScriptedInstanceDAO ScriptedInstanceDao => _scriptedinstanceDao ?? (_scriptedinstanceDao = new ScriptedInstanceDAO());

        public static IBCardDAO BCardDao => _bcardDao ?? (_bcardDao = new BCardDAO());

        public static IRollGeneratedItemDAO RollGeneratedItemDao => _rollGeneratedItemDao ?? (_rollGeneratedItemDao = new RollGeneratedItemDAO());

        public static IExchangeLogDao ExchangeLogDao => _exchangeLogDao ?? (_exchangeLogDao = new ExchangeLogDao());

        public static IUpgradeLogDao UpgradeLogDao => _upgradeLogDao ?? (_upgradeLogDao = new UpgradeLogDao());

        #endregion
    }
}