using System.Numerics;

namespace AtlasSimulator.playerclasses
{
    public class ClassHelpers
    {
        public class ClassData
        {
            public int[] _validRaces;
            public int _classID;
            public string _specString;
            public int _realm;
            public int _creationModel;
            
            public ClassData(int[] races, int creationModel, int classid, int realm, string specString)
            {
                _validRaces = races;
                _classID = classid;
                _specString = specString;
                _realm = realm;
                _creationModel = creationModel;
            }
        }

        public static ClassData GetPaladinData()
        {
            int[] paladinRaceSelection = new int[] {1};
            int creationModel = 37122;
            int realm = 1;
            int classid = 1;
            string specString = "Slash|50;Thrust|1;Crush|1;Two Handed|50;Chants|50;Shields|50;Parry|1";
            return new ClassData(paladinRaceSelection, creationModel,classid,realm,specString);
        }
        
        public static ClassData GetWizardData()
        {
            int[] wizardRaceSelection = new int[] {1};
            int creationModel = 12352;
            int realm = 1;
            int classid = 7;
            string specString = "Earth Magic|50;Cold Magic|50;Fire Magic|50";
            return new ClassData(wizardRaceSelection, creationModel,classid,realm,specString);
        }

    }
}