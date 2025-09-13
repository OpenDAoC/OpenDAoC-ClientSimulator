using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using DOL.Database.UniqueID;

namespace ClientSimulator.PlayerClass
{
    public class Wizard : IPlayerClass
    {
        public string accountName { get; }
        public string charname { get; }
        public string password { get; }
        public string sql { get; }
        private Client _client;
        public string AccountName { get; }
        public string CharName { get; }
        public string Password { get; }
        public string Sql { get; }

        public Wizard(string account, string password, string charName, GameLocation gameLocation)
        {
            // Assign
            AccountName = account;
            Password = password;
            CharName = charName;

            // setup client
            _client = new Client(account, password, charName, gameLocation);

            // handle adding to the database
            ClassHelpers.ClassData wizzyData = ClassHelpers.GetWizardData();

            Dictionary<string, string> characterData = new()
            {
                { "HasGravestone", "0" },
                { "GravestoneRegion", "0" },
                { "Constitution", "400" },
                { "Dexterity", "377" },
                { "Strength", "400" },
                { "Quickness", "385" },
                { "Intelligence", "9955" },
                { "Piety", "400" },
                { "Empathy", "400" },
                { "Charisma", "400" },
                { "BountyPoints", "0" },
                { "RealmPoints", "0" },
                { "RealmLevel", "1" },
                { "Experience", "171175293372" },
                { "MaxEndurance", "100" },
                { "Health", "652" },
                { "Mana", "4488" },
                { "Endurance", "0" },
                { "Concentration", "100" },
                { "AccountName", AccountName },
                { "AccountSlot", "100" },
                { "CreationDate", "2000-01-01 00:00:00" },
                { "LastPlayed", "2000-01-01 00:00:00" },
                { "Name", CharName },
                { "GuildID", "" },
                { "Gender", "1" },
                { "Race", "1" },
                { "Level", "50" },
                { "Class", "7" },
                { "Realm", "1" },
                { "CreationModel", "12352" },
                { "CurrentModel", "12352" },
                { "Region", "1" },
                { "ActiveWeaponSlot", "1" },
                { "Xpos", "390452" },
                { "Ypos", "748464" },
                { "Zpos", "370" },
                { "BindXpos", "390452" },
                { "BindYpos", "748464" },
                { "BindZpos", "370" },
                { "BindRegion", "1" },
                { "BindHeading", "0" },
                { "BindHouseXpos", "0" },
                { "BindHouseYpos", "0" },
                { "BindHouseZpos", "0" },
                { "BindHouseRegion", "0" },
                { "BindHouseHeading", "0" },
                { "DeathCount", "0" },
                { "ConLostAtDeath", "0" },
                { "Direction", "0" },
                { "MaxSpeed", "191" },
                { "Copper", "84" },
                { "Silver", "63" },
                { "Gold", "257" },
                { "Platinum", "0" },
                { "Mithril", "0" },
                { "SerializedCraftingSkills", "1|1;2|1;3|1;4|1;6|1;7|1;8|1;9|1;10|1;11|1;12|1;13|1;14|1;15|1" },
                { "SerializedAbilities", "Sprint|0;Weaponry: Staves|0;AlbArmor|1;QuickCast|0" },
                { "SerializedSpecs", "Earth Magic|1;Cold Magic|50;Fire Magic|1" },
                { "SerializedRealmAbilities", "" },
                { "DisabledSpells", "" },
                { "DisabledAbilities", "" },
                { "SerializedFriendsList", "" },
                { "SerializedIgnoreList", "" },
                { "IsCloakHoodUp", "0" },
                { "IsCloakInvisible", "0" },
                { "IsHelmInvisible", "0" },
                { "SpellQueue", "1" },
                { "IsLevelSecondStage", "0" },
                { "FlagClassName", "0" },
                { "Advisor", "0" },
                { "GuildRank", "0" },
                { "PlayedTime", "358" },
                { "DeathTime", "0" },
                { "RespecAmountAllSkill", "0" },
                { "RespecAmountSingleSkill", "0" },
                { "RespecAmountRealmSkill", "0" },
                { "RespecAmountDOL", "0" },
                { "RespecAmountChampionSkill", "0" },
                { "IsLevelRespecUsed", "0" },
                { "RespecBought", "0" },
                { "SafetyFlag", "1" },
                { "CraftingPrimarySkill", "0" },
                { "CancelStyle", "0" },
                { "IsAnonymous", "0" },
                { "CustomisationStep", "0" },
                { "EyeSize", "0" },
                { "LipSize", "0" },
                { "EyeColor", "0" },
                { "HairColor", "0" },
                { "FaceType", "0" },
                { "HairStyle", "0" },
                { "MoodType", "0" },
                { "UsedLevelCommand", "0" },
                { "CurrentTitleType", "" },
                { "KillsAlbionPlayers", "0" },
                { "KillsMidgardPlayers", "0" },
                { "KillsHiberniaPlayers", "0" },
                { "KillsAlbionDeathBlows", "0" },
                { "KillsMidgardDeathBlows", "0" },
                { "KillsHiberniaDeathBlows", "0" },
                { "KillsAlbionSolo", "0" },
                { "KillsMidgardSolo", "0" },
                { "KillsHiberniaSolo", "0" },
                { "CapturedKeeps", "0" },
                { "CapturedTowers", "0" },
                { "CapturedRelics", "0" },
                { "KillsDragon", "0" },
                { "DeathsPvP", "0" },
                { "KillsLegion", "0" },
                { "KillsEpicBoss", "0" },
                { "GainXP", "0" },
                { "GainRP", "0" },
                { "Autoloot", "1" },
                { "LastFreeLeveled", "2000-01-01 00:00:00" },
                { "LastFreeLevel", "1" },
                { "GuildNote", "2000-01-01 00:00:00" },
                { "ShowXFireInfo", "0" },
                { "ShowGuildLogins", "0" },
                { "Champion", "0" },
                { "ChampionLevel", "0" },
                { "ChampionExperience", "0" },
                { "ML", "0" },
                { "MLExperience", "0" },
                { "MLLevel", "0" },
                { "MLGranted", "0" },
                { "RPFlag", "0" },
                { "IgnoreStatistics", "0" },
                { "NotDisplayedInHerald", "0" },
                { "ActiveSaddleBags", "0" },
                { "LastTimeRowUpdated", "2000-01-01 00:00:00" },
                { "DOLCharacters_ID", IdGenerator.GenerateID() },
                { "LastLevelUp", "2000-01-01 00:00:00" },
                { "PlayedTimeSinceLevel", "358" },
                { "HCFlag", "0" },
                { "HCCompleted", "0" },
                { "HideSpecializationAPI", "0" }
            };

            string columns = string.Join(", ", characterData.Keys.Select(k => $"`{k}`"));
            string values = string.Join(", ", characterData.Values.Select(v => $"'{v}'"));
            Sql = $"INSERT INTO `{Program.DB}`.`dolcharacters` ({columns}) VALUES ({values});";

            _client.ActionTimer = new Timer(ActionCallback, null, 850, 1000);
        }

        public void Login()
        {
            _client.Login();
        }

        public void ActionCallback(object source)
        {
            // Cast PBAOE
            _client.SendUseSpell(0, 48, 5);
        }
    }
}
