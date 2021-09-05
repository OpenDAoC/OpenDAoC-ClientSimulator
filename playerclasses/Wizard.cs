using System;
using System.Threading;
using System.Windows;

namespace AtlasSimulator.playerclasses
{
    public class Wizard : PlayerClass
    {

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
                "INSERT INTO `atlas`.`dolcharacters` (`Constitution`, `Dexterity`, `Strength`, `Quickness`, `Intelligence`," +
                " `Piety`, `Empathy`, `Charisma`, `MaxEndurance`, `Endurance`, `Concentration`, `AccountName`, `AccountSlot`," +
                "`Name`, `Race`, `Level`, `Class`, `Realm`, `CreationModel`, `CurrentModel`, `Region`, `Xpos`," +
                " `Ypos`, `Zpos`, `BindXpos`, `BindYpos`, `BindZpos`, `BindRegion`, `MaxSpeed`, `SerializedSpecs`, `DOLCharacters_ID`)" +
                " VALUES('400', '400', '400', '400', '10000', '400', '400', '400', '100', '100', '100'," +
                " '{0}', '1', '{1}', '{2}', '50', '{3}', '{4}', '{5}', '{5}', '{6}', '{7}'," +
                " '{8}', '{9}','{7}', '{8}', '{9}', '{5}', '191', '{10}', '{11}');",
                accountName, charname, wizzyData._validRaces[0], wizzyData._classID, wizzyData._realm, wizzyData._creationModel,
                initialGLocation.zone, initialGLocation.x,
                initialGLocation.y, initialGLocation.z, wizzyData._specString, Guid.NewGuid().ToString());

            // setup actions
            _actionTimer = new Timer(ActionCallback, null, 850, 1000);

        }

        override public void ActionCallback(Object source)
        {
            // Cast PBAOE
            if (isLoggedIn && actionEnabled)
            {
                _client.SendUseSpell(0, 48, 5);
            }
            
        }
    }
}