using GTANetworkServer;
using GTANetworkShared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disorder_District.mission_manager
{
    class MissionManager : Script
    {
        public MissionManager()
        {
            API.onClientEventTrigger += API_onClientEventTrigger;
        }

        private void API_onClientEventTrigger(Client player, string eventName, params object[] arguments)
        {
            if (!checkIfInMission(player))
            {
                return;
            }

            Mission mission = API.getEntityData(player, "Mission");

            switch (eventName)
            {
                case "checkObjective":
                    mission.verifyObjective(arguments[0].ToString(), player);
                    return;
            }
        }

        /** Check if our player is in a mission. */
        private bool checkIfInMission(Client player)
        {
            if(!API.hasEntityData(player, "Mission")) {
                return false;
            }

            return true;
        }

        // Invite a player to a mission.
        [Command("invitemission")]
        public void cmdInvitePlayerToMission(Client player, string target)
        {
            if (!checkIfInMission(player))
            {
                return;
            }

            Mission mission = API.getEntityData(player, "Mission");

            if (!mission.PauseState)
            {
                API.sendChatMessageToPlayer(player, "~r~Your mission is currently running, you cannot invite others.");
                API.sendChatMessageToPlayer(player, "~r~You can always ~w~/abandonmission ~r~ if you want to abandon your mission.");
                return;
            }

            Client targetPlayer = API.getPlayerFromName(target);

            API.sendChatMessageToPlayer(targetPlayer, string.Format("{0} invited you to a mission. Type '/acceptMission' to join.", player.name));
            API.sendChatMessageToPlayer(player, "Invited player to mission.");
            API.setEntityData(targetPlayer, "Mission", mission);
        }

        // Abandon any currently running mission a player is in.
        [Command("abandonmission")]
        public void cmdLeavePlayerMission(Client player)
        {
            if (checkIfInMission(player))
            {
                Mission mission = API.getEntityData(player, "Mission");
                mission.abandonMission(player);
            }

            API.resetEntityData(player, "Mission");
            API.resetEntitySyncedData(player, "Mission");
            API.sendChatMessageToPlayer(player, "~r~Abandoned any mission the player is currently on.");
        }

        // Add a player who has an invitation to a mission instance.
        [Command("acceptmission")]
        public void cmdAcceptMission(Client player)
        {
            if (!checkIfInMission(player))
            {
                return;
            }

            Mission mission = API.getEntityData(player, "Mission");
            mission.addPlayer(player);
        }

        [Command("createmission")]
        public void cmdCreateMission(Client player)
        {
            Mission mission = new Mission();
            API.setEntityData(player, "Mission", mission);
            API.sendChatMessageToPlayer(player, "Setup new mission instance.");
        }

        // Used as a debug tool for the moment.
        [Command("testmission")]
        public void cmdTestMission(Client player)
        {
            if(!checkIfInMission(player))
            {
                return;
            }

            Mission mission = API.getEntityData(player, "Mission");
            /** Setup a local instance of the 'mission' data for the player */
            API.setEntityData(player, "Mission", mission);
            API.setEntitySyncedData(player, "Mission", true);
            /** Add some objectives to our mission. */
            mission.addPlayer(player);
            mission.MissionName = "TestMission1234";
            mission.addObjective(new Vector3(849.026, 1283.348, 358.531), Mission.ObjectiveTypes.Location);
            mission.addObjective(new Vector3(859.6832, 1311.694, 355.547), Mission.ObjectiveTypes.Location);
            mission.addObjective(new Vector3(849.026, 1283.348, 358.531), Mission.ObjectiveTypes.Location);
            mission.addObjective(new Vector3(859.6832, 1311.694, 355.547), Mission.ObjectiveTypes.Location);
            mission.addObjective(new Vector3(849.026, 1283.348, 358.531), Mission.ObjectiveTypes.Location);
            mission.addObjective(new Vector3(859.6832, 1311.694, 355.547), Mission.ObjectiveTypes.Location);
            mission.addObjective(new Vector3(849.026, 1283.348, 358.531), Mission.ObjectiveTypes.Location);
            mission.addObjective(new Vector3(859.6832, 1311.694, 355.547), Mission.ObjectiveTypes.FastCapture);
            mission.addObjective(new Vector3(849.026, 1283.348, 358.531), Mission.ObjectiveTypes.Capture);
            mission.addObjective(new Vector3(), Mission.ObjectiveTypes.Finish);
            mission.PauseState = false;
            mission.syncPlayers();
        }
    }
}
