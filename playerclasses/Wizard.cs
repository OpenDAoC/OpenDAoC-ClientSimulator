using System;
using System.Threading;
using DOL.Database.UniqueID;

namespace AtlasSimulator.playerclasses
{
    public class Wizard : IPlayerClass
    {
        public string accountName { get; }
        public string charname { get; }
        public string password { get; }
        public string sql { get; }
        private Client _client;
        private Timer _actionTimer;

        public Wizard(string acc, string pw, string cn, GLocation initialGLocation)
        {
            // Assign
            accountName = acc;
            password = pw;
            charname = cn;
            
            // setup client
            _client = new Client(acc, pw, cn);

            // handle adding to the database
            ClassHelpers.ClassData wizzyData = ClassHelpers.GetWizardData();

            sql = String.Format(
                "INSERT INTO `atlas`.`dolcharacters`(`HasGravestone`,`GravestoneRegion`,`Constitution`,`Dexterity`,`Strength`,`Quickness`,`Intelligence`,`Piety`,`Empathy`,`Charisma`,`BountyPoints`,`RealmPoints`,`RealmLevel`,`Experience`,`MaxEndurance`,`Health`,`Mana`,`Endurance`,`Concentration`,`AccountName`,`AccountSlot`,`CreationDate`,`LastPlayed`,`Name`,`GuildID`,`Gender`,`Race`,`Level`,`Class`,`Realm`,`CreationModel`,`CurrentModel`,`Region`,`ActiveWeaponSlot`,`Xpos`,`Ypos`,`Zpos`,`BindXpos`,`BindYpos`,`BindZpos`,`BindRegion`,`BindHeading`,`BindHouseXpos`,`BindHouseYpos`,`BindHouseZpos`,`BindHouseRegion`," +
                "`BindHouseHeading`,`DeathCount`,`ConLostAtDeath`,`Direction`,`MaxSpeed`,`Copper`,`Silver`,`Gold`,`Platinum`,`Mithril`,`SerializedCraftingSkills`,`SerializedAbilities`,`SerializedSpecs`,`SerializedRealmAbilities`,`DisabledSpells`,`DisabledAbilities`,`SerializedFriendsList`,`SerializedIgnoreList`,`IsCloakHoodUp`,`IsCloakInvisible`,`IsHelmInvisible`,`SpellQueue`,`IsLevelSecondStage`,`FlagClassName`,`Advisor`,`GuildRank`,`PlayedTime`,`DeathTime`,`RespecAmountAllSkill`,`RespecAmountSingleSkill`,`RespecAmountRealmSkill`,`RespecAmountDOL`,`RespecAmountChampionSkill`,`IsLevelRespecUsed`,`RespecBought`,`SafetyFlag`," +
                "`CraftingPrimarySkill`,`CancelStyle`,`IsAnonymous`,`CustomisationStep`,`EyeSize`,`LipSize`,`EyeColor`,`HairColor`,`FaceType`,`HairStyle`,`MoodType`,`UsedLevelCommand`,`CurrentTitleType`,`KillsAlbionPlayers`,`KillsMidgardPlayers`,`KillsHiberniaPlayers`,`KillsAlbionDeathBlows`,`KillsMidgardDeathBlows`,`KillsHiberniaDeathBlows`,`KillsAlbionSolo`,`KillsMidgardSolo`,`KillsHiberniaSolo`,`CapturedKeeps`,`CapturedTowers`,`CapturedRelics`,`KillsDragon`,`DeathsPvP`,`KillsLegion`,`KillsEpicBoss`,`GainXP`,`GainRP`,`Autoloot`,`LastFreeLeveled`,`LastFreeLevel`,`GuildNote`,`ShowXFireInfo`,`NoHelp`,`ShowGuildLogins`,`Champion`," +
                "`ChampionLevel`,`ChampionExperience`,`ML`,`MLExperience`,`MLLevel`,`MLGranted`,`RPFlag`,`IgnoreStatistics`,`NotDisplayedInHerald`,`ActiveSaddleBags`,`LastTimeRowUpdated`,`DOLCharacters_ID`,`LastLevelUp`,`PlayedTimeSinceLevel`,`ReceiveROG`,`HCFlag`,`HCCompleted`,`isBoosted`,`HideSpecializationAPI`)" +
                " VALUES('0','0','400','377','400','385','9955','400','400','400','0','0',1,171175293372,'100','652','4488','0','100','{0}','100','2000-01-01 00:00:00','2022-06-28 01:27:34','{1}','c97275ad-136f-4b36-ba4c-7b8a2cdc1a2f','1','1',50,'7','1','12352','12352','1','1','390452','748464','370','390452','748464','370','12352','0','0','0','0','0','0','0','0','0','191','84','63','257','0','0','1|1;2|1;3|1;4|1;6|1;7|1;8|1;9|1;10|1;11|1;12|1;13|1;14|1;15|1','Sprint|0;Weaponry: Staves|0;AlbArmor|1;QuickCast|0','Earth Magic|1;Cold Magic|50;Fire Magic|1','','','','','','0','0','0','1','0','0','0','0','358','0','0','0','0','0','0','0','0','1','0','0','0','0','0','0','0','0','0','0','0','0','','0','0','0','0','0','0','0','0','0','0','0','0','0','0','0','0','1','1','1','2022-06-28 00:42:57','0','','0','0','0','0','0','0','0','0','0','0','0','0','0','0','2022-06-28 05:27:34','{2}','2022-06-28 01:27:34','358','1','0','0','0','0');",
                accountName, charname, IDGenerator.GenerateID());
                

            // setup actions
            _actionTimer = new Timer(ActionCallback, null, 850, 1000);
        }
        public void login()
        {
            _client.Login();
        }

        public void ActionCallback(Object source)
        {
            // Cast PBAOE
            _client.SendUseSpell(0, 48, 5);
        }
    }
}