namespace ClientSimulator.PlayerClass
{
    public class ClassHelpers
    {
        public class ClassData(int[] races, int creationModel, int classid, int realm, string specString)
        {
            public int[] ValidRaces { get; } = races;
            public int ClassID { get; } = classid;
            public string SpecString { get; } = specString;
            public int Realm { get; } = realm;
            public int CreationModel { get; } = creationModel;
        }

        public static ClassData GetPaladinData()
        {
            int[] paladinRaceSelection = [1];
            int creationModel = 37122;
            int realm = 1;
            int classid = 1;
            string specString = "Slash|50;Thrust|1;Crush|1;Two Handed|50;Chants|50;Shields|50;Parry|1";
            return new ClassData(paladinRaceSelection, creationModel,classid,realm,specString);
        }

        public static ClassData GetWizardData()
        {
            int[] wizardRaceSelection = [1];
            int creationModel = 12352;
            int realm = 1;
            int classid = 7;
            string specString = "Earth Magic|1;Cold Magic|50;Fire Magic|1";
            return new ClassData(wizardRaceSelection, creationModel,classid,realm,specString);
        }
    }
}
