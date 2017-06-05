using GTANetworkServer;
using GTANetworkShared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disorder_District.mission_manager
{
    class Mission : Script
    {
        private List<Vector3> objectiveLocations;
        private List<ObjectiveTypes> objectiveTypes;
        private List<Client> players;
        private bool pauseState = true;
        private bool checkingObjective = false;
        private int objectiveCompletion = 0;
        private DateTime objectiveCooldown; // ms
        private string missionName;
        
        /** Mission ObjectiveTypes that will be used clientside/serverside */
        public enum ObjectiveTypes
        {
            Location,
            Finish,
            Capture,
            FastCapture
        }

        /** Main Constructor */
        public Mission()
        {
            pauseState = true;
            objectiveLocations = new List<Vector3>();
            objectiveTypes = new List<ObjectiveTypes>();
            players = new List<Client>();
            objectiveCooldown = DateTime.Now;
        }

        public string MissionName
        {
            set
            {
                missionName = value;
            }
            get
            {
                return missionName;
            }
        }

        /** Should we run the mission now? */
        public bool PauseState
        {
            set
            {
                pauseState = value;
            }
            get
            {
                return pauseState;
            }
        }

        /** Add a player to the mission. */
        public void addPlayer(Client player)
        {
            if (players.Contains(player))
            {
                return;
            }

            players.Add(player);
            API.setEntitySyncedData(player, "Mission", true);

            API.sendChatMessageToPlayer(player, "Added Player" + player.name + " to mission " + missionName);
        }

        /** Abandon a mission. */
        public void abandonMission(Client player)
        {
            if (players.Contains(player))
            {
                players.Remove(player);
            }

            API.setEntitySyncedData(player, "Mission_Abandon", true);
        }

        /** Load in an objective location with a type. */
        public void addObjective(Vector3 objectiveLocation, ObjectiveTypes objectiveType)
        {
            objectiveLocations.Add(objectiveLocation);
            objectiveTypes.Add(objectiveType);
        }

        public void syncPlayers()
        {
            int syncedPlayers = 0;
            foreach (Client player in players)
            {
                API.setEntitySyncedData(player, "Mission_Location", objectiveLocations[0]);
                API.setEntitySyncedData(player, "Mission_Type", objectiveTypes[0].ToString());
                API.setEntitySyncedData(player, "Mission_Pause", PauseState);
                syncedPlayers++;
            }
            API.consoleOutput(syncedPlayers.ToString());
        }

        public void verifyObjective(string objectiveType, Client player)
        {
            /** Used to determine if another player is attempting to verify this objective. */
            if (checkingObjective)
            {
                return;
            }

            checkingObjective = true;

            /** Double check to make sure the player is apart of the mission. */
            if (!players.Contains(player))
            {
                checkingObjective = false;
                return;
            }

            /** Just to prevent an error from occuring where too many requests get sent. */
            if (objectiveTypes.Count <= 0)
            {
                checkingObjective = false;
                return;
            }

            /** Check if the player is on the right mission. */
            if (objectiveType != objectiveTypes[0].ToString())
            {
                checkingObjective = false;
                return;
            }

            if (!verifyObjective(player))
            {
                checkingObjective = false;
                return;
            }
           
            /** Pause the mission for a moment to update objectives. */
            PauseState = true;

            /** Remove the objective and its location. */
            objectiveLocations.RemoveAt(0);
            objectiveTypes.RemoveAt(0);

            /** Check the length of our objectives. */
            if (objectiveLocations.Count <= 0 && objectiveTypes.Count <= 0)
            {
                //** When all objectives are done go here. */
                API.setEntitySyncedData(player, "Mission_Complete", null);
                return;
            }

            switch (objectiveTypes[0])
            {
                case ObjectiveTypes.Location:
                    objectiveCompletion = 0;
                    break;
                case ObjectiveTypes.Capture:
                    objectiveCompletion = 0;
                    break;
            }

            /** Send a sound over to the player letting them know they hit it. */
            API.setEntitySyncedData(player, "Mission_Finish_Objective", "");

            /** Resume / Resync players. */
            PauseState = false;
            checkingObjective = false;
            syncPlayers();
        }

        /** Verify by objective type. */
        private bool verifyObjective(Client player)
        {
            switch (objectiveTypes[0])
            {
                case ObjectiveTypes.Location:
                    return objectiveLocation(player);
                case ObjectiveTypes.Capture:
                    return objectiveCapture(player, 5);
                case ObjectiveTypes.FastCapture:
                    return objectiveCapture(player, 20);
            }
            return false;   
        }

        // Location Objective Type
        private bool objectiveLocation(Client player)
        {
            if (player.position.DistanceTo(objectiveLocations[0]) <= 3)
            {
                return true;
            }
            return false;
        }

        // Capture Objective Type
        private bool objectiveCapture(Client player, int speed)
        {
            if (player.position.DistanceTo(objectiveLocations[0]) <= 5)
            {
                if (DateTime.Now < objectiveCooldown.AddMilliseconds(5000))
                {
                    return false;
                }
                objectiveCooldown = DateTime.Now;
                objectiveCompletion += speed;
                API.sendChatMessageToPlayer(player, objectiveCompletion.ToString());
                if (objectiveCompletion >= 100)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
