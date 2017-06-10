using GTANetworkServer;
using GTANetworkShared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disorder_District.mission_manager
{
    class Objective : Script
    {
        private List<NetHandle> objectiveTargets;
        private List<NetHandle> objectiveVehicles;
        private Dictionary<Vector3, int> objectiveProgression;
        private Dictionary<Vector3, ObjectiveTypes> objectives;
        private int objectiveCount;
        private int objectivesComplete;
        private bool checkingObjective;
        private bool pauseState;
        private DateTime objectiveCooldown;

        public enum ObjectiveTypes
        {
            Location,
            Finish,
            Capture,
            FastCapture,
            Teleport,
            None
        }

        public Objective()
        {
            // Nothing
        }

        /** The main constructor for a new objective.
         * Adds a single new objective. Use addObjective to add additional objectives to this instance. */
        public Objective(Vector3 location, ObjectiveTypes type)
        {
            setupLists();
            objectives.Add(location, type);
            objectiveCount++;

            switch(type)
            {
                // Add our capture objective location to the objective progression list with a location.
                case ObjectiveTypes.Capture:
                    objectiveProgression.Add(location, 0);
                    return;
            }
        }

        private void setupLists()
        {
            objectiveCount = 0;
            objectivesComplete = 0;
            checkingObjective = false;
            pauseState = false;
            objectiveTargets = new List<NetHandle>();
            objectiveVehicles = new List<NetHandle>();
            objectiveProgression = new Dictionary<Vector3, int>();
            objectiveCooldown = DateTime.UtcNow;
            objectives = new Dictionary<Vector3, ObjectiveTypes>();
        }

        /***********************************************************
         * Adds an additional objective to this objective instance.
         * *********************************************************/
        public void addAdditionalObjective(Vector3 location, ObjectiveTypes type)
        {
            objectives.Add(location, type);
            objectiveCount++;

            switch (type)
            {
                // Add our capture objective location to the objective progression list with a location.
                case ObjectiveTypes.Capture:
                    objectiveProgression.Add(location, 0);
                    return;
            }
        }

        public void addObjectiveVehicle(Mission instance, Vector3 location, VehicleHash type)
        {
            NetHandle veh = API.createVehicle(type, location.Around(5), new Vector3(), 52, 52);
            API.setEntityData(veh, "Mission", instance);
            instance.addVehicle(veh);
        }

        public void syncObjectiveToPlayer(Client player)
        {
            foreach (Vector3 location in objectives.Keys)
            {
                API.triggerClientEvent(player, "Mission_New_Objective", location, objectives[location].ToString());
            }

            API.triggerClientEvent(player, "Mission_Setup_Objectives");
            API.triggerClientEvent(player, "Mission_Head_Notification", "~o~New Objective", "NewObjective");
        }

        private void updateObjectiveProgression(Client player, Vector3 location, int progression)
        {
            Mission mission = API.getEntityData(player, "Mission");
            mission.updateObjectiveProgressionForAll(location, progression);
        }

        public void verifyObjective(Client player)
        {
            // Used to determine if another player is attempting to verify this objective. */
            if (checkingObjective)
            {
                return;
            }

            checkingObjective = true;

            // Just to prevent an error from occuring where too many requests get sent. */
            if (objectives.Count <= 0)
            {
                checkingObjective = false;
                return;
            }

            // Get the closest objective to the player.
            Vector3 closestObjective = new Vector3();
            foreach (Vector3 location in objectives.Keys)
            {
                if (player.position.DistanceTo(location) <= 5)
                {
                    closestObjective = location;
                    break;
                }

                if (objectives[location] == ObjectiveTypes.Teleport)
                {
                    closestObjective = location;
                    break;
                }
            }

            if (closestObjective == new Vector3())
            {
                checkingObjective = false;
                return;
            }

            Tuple<bool, int, Vector3> completionCheck = checkForCompletion(player, closestObjective, objectives[closestObjective]);

            // Check if our tuple returned false.
            if (!completionCheck.Item1)
            {
                checkingObjective = false;
                return;
            }

            // Get the players mission instance.
            Mission mission = API.getEntityData(player, "Mission");
            mission.removeObjectiveForAll(completionCheck.Item3);

            API.triggerClientEvent(player, "Mission_Head_Notification", "~b~Minor Objective Complete", "Objective");

            // Remove dead objectives.
            objectives.Remove(completionCheck.Item3);

            // Check if all of our objectives are complete.
            if (objectives.Count >= 1)
            {
                checkingObjective = false;
                return;
            }

            mission.goToNextObjective();

            pauseState = false;
            checkingObjective = false;
        }

        /*********************************************
         * Verify based on objective type.
         * ******************************************/
        private Tuple<bool, int, Vector3> checkForCompletion(Client player, Vector3 location, ObjectiveTypes type)
        {
            switch (type)
            {
                case ObjectiveTypes.Location:
                    return objectiveLocation(player);
                case ObjectiveTypes.Teleport:
                    return objectiveTeleport(player, location);
                case ObjectiveTypes.Capture:
                    return objectiveCapture(player, location);
            }

            return Tuple.Create(false, 0, new Vector3());
        }

        /*************************************************
         *                OBJECTIVE TYPES
         * **********************************************/
        private Tuple<bool, int, Vector3> objectiveLocation(Client player)
        {
            int index = 0;
            foreach (Vector3 location in objectives.Keys)
            {
                if (player.position.DistanceTo(location) <= 5)
                {
                    return Tuple.Create(true, index, location);
                }
                index++;
            }
            return Tuple.Create(false, index, new Vector3());
        }

        private Tuple<bool, int, Vector3> objectiveTeleport(Client player, Vector3 location)
        {
            Mission mission = API.getEntityData(player, "Mission");
            mission.teleportAllPlayers(location);
            return Tuple.Create(true, -1, location);
        }

        private Tuple<bool, int, Vector3> objectiveCapture(Client player, Vector3 location)
        {
            if (player.position.DistanceTo(location) <= 8)
            {
                double sinceWhen = objectiveCooldown.TimeOfDay.TotalSeconds;
                double timeNow = DateTime.UtcNow.TimeOfDay.TotalSeconds;
                if (sinceWhen + 3 > timeNow)
                {
                    return Tuple.Create(false, 0, location);
                }
                objectiveCooldown = DateTime.UtcNow;
                objectiveProgression[location] += 5;
                updateObjectiveProgression(player, location, objectiveProgression[location]);
                if (objectiveProgression[location] > 100)
                {
                    return Tuple.Create(true, -1, location);
                }
                return Tuple.Create(false, 0, location);
            }
            return Tuple.Create(false, 0, new Vector3());
        }

        /** Get the total objective count. */
        public int ObjectiveCount
        {
            get
            {
                return objectiveCount;
            }
        }

        /** Get the total amount of objectives complete. */
        public int CompletedObjectives
        {
            get
            {
                return objectivesComplete;
            }
        }
    }
}
