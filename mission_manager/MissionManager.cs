﻿using GTANetworkServer;
using GTANetworkShared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Disorder_District.mission_manager
{
    class MissionManager : Script
    {
        public MissionManager()
        {
            API.onClientEventTrigger += API_onClientEventTrigger;
            API.onPlayerDisconnected += API_onPlayerDisconnected;
        }

        private void API_onPlayerDisconnected(Client player, string reason)
        {
            if (!checkIfInMission(player))
            {
                return;
            }

            Mission mission = API.getEntityData(player, "Mission");
            mission.abandonMission(player);
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
                    mission.verifyObjective(player);
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
        [Command("invite")]
        public void cmdInvitePlayerToMission(Client player, string target)
        {
            if (!checkIfInMission(player))
            {
                return;
            }

            Mission mission = API.getEntityData(player, "Mission");

            if (!mission.PauseState)
            {
                API.sendChatMessageToPlayer(player, "~r~A mission is currently running, you cannot invite others.");
                API.sendChatMessageToPlayer(player, "~r~You can always ~w~/leaveparty ~r~if you want to abandon your allies.");
                return;
            }

            Client targetPlayer = null;

            foreach (Client p in API.getAllPlayers())
            {
                if (p.name.Contains(target))
                {
                    targetPlayer = p;
                    break;
                }
            }

            if (targetPlayer == null)
            {
                API.triggerClientEvent(player, "Mission_Head_Notification", "~r~Player does not exist.", "Error");
                return;
            }

            API.sendChatMessageToPlayer(targetPlayer, string.Format("{0} invited you to a mission. Type '/joinparty' to join.", player.name));
            API.sendChatMessageToPlayer(player, string.Format("Invited {0} to your party.", targetPlayer.name));
            API.setEntityData(targetPlayer, "Mission", mission);
        }

        // Abandon any currently running mission a player is in.
        [Command("leaveparty")]
        public void cmdLeavePlayerMission(Client player)
        {
            if (API.hasEntityData(player, "Mission"))
            {
                Mission mission = API.getEntityData(player, "Mission");
                mission.abandonMission(player);
            } else {
                API.triggerClientEvent(player, "Mission_Head_Notification", "~r~You are not in a party.", "Error");
            }
        }

        // Add a player who has an invitation to a party instance.
        [Command("joinparty")]
        public void cmdAcceptMission(Client player)
        {
            if (!checkIfInMission(player))
            {
                return;
            }

            Mission mission = API.getEntityData(player, "Mission");
            mission.addPlayer(player);
        }

        [Command("createparty")]
        public void cmdCreateMission(Client player)
        {
            if (checkIfInMission(player))
            {
                API.sendChatMessageToPlayer(player, "~r~You're already in a party, ~w~/leaveparty ~r~if you want to abandon your allies.");
                return;
            }

            Mission mission = new Mission();
            API.setEntitySyncedData(player, "Mission_New_Instance", true);
            API.setEntityData(player, "Mission", mission);
            mission.addPlayer(player);
            API.triggerClientEvent(player, "Mission_Head_Notification", "New Party Created", "NewObjective");
        }

        [Command("newtestmission")]
        public void cmdTestMission2(Client player)
        {
            if (!checkIfInMission(player))
            {
                return;
            }

            Mission mission = API.getEntityData(player, "Mission");
            Objective objective;
            // Zero Objective
            objective = mission.CreateNewObjective(new Vector3(826.6046, 1291.703, 363.3745), Objective.ObjectiveTypes.Teleport);
            // First Objective
            objective = mission.CreateNewObjective(new Vector3(807.2759, 1276.154, 359.4603), Objective.ObjectiveTypes.Location);
            objective.addObjectiveVehicle(mission, new Vector3(807.2759, 1276.154, 359.4603), VehicleHash.Alpha);
            objective.setupObjective(new Vector3(857.6578, 1298.137, 356.9208), Objective.ObjectiveTypes.Location);
            objective.setupObjective(new Vector3(831.2583, 1358.375, 349.291), Objective.ObjectiveTypes.Location);
            objective.setupObjective(new Vector3(746.6826, 1363.2, 339.2456), Objective.ObjectiveTypes.Location);
            objective.setupObjective(new Vector3(683.0908, 1361.002, 327.9336), Objective.ObjectiveTypes.Location);
            objective.setupObjective(new Vector3(611.5548, 1408.151, 316.2294), Objective.ObjectiveTypes.Location);
            objective.setupObjective(new Vector3(543.8524, 1348.419, 294.1264), Objective.ObjectiveTypes.Location);
            objective.setupObjective(new Vector3(424.4767, 1294.409, 267.2835), Objective.ObjectiveTypes.Location);
            objective.setupObjective(new Vector3(411.289, 1225.015, 252.9696), Objective.ObjectiveTypes.Location);
            // Second Objective
            objective = mission.CreateNewObjective(new Vector3(422.066, 1197.878, 248.1305), Objective.ObjectiveTypes.Location);
            mission.startMission();
        }

        [Command("capture")]
        public void cmdCaptureTest(Client player)
        {
            if (!checkIfInMission(player))
            {
                return;
            }

            Mission mission = API.getEntityData(player, "Mission");
            Objective objective;
            // Zero Objective
            objective = mission.CreateNewObjective(new Vector3(826.6046, 1291.703, 363.3745), Objective.ObjectiveTypes.Teleport);
            // First Objective
            objective = mission.CreateNewObjective(new Vector3(807.2759, 1276.154, 359.4603), Objective.ObjectiveTypes.Location);
            objective.setupObjective(new Vector3(857.6578, 1298.137, 356.9208), Objective.ObjectiveTypes.Capture);
            objective.setupObjective(new Vector3(831.2583, 1358.375, 349.291), Objective.ObjectiveTypes.Capture);
            mission.startMission();
        }

        [Command("point2point")]
        public void cmdpoint2point(Client player)
        {
            if (!checkIfInMission(player))
            {
                return;
            }
            Mission mission = API.getEntityData(player, "Mission");
            Objective objective;
            // Zero Objective
            objective = mission.CreateNewObjective(new Vector3(659.4011, 593.0363, 128.0509), Objective.ObjectiveTypes.Teleport);
            // point b
            objective = mission.CreateNewObjective(new Vector3(662.4194, 566.8674, 128.0466), Objective.ObjectiveTypes.Location);
            objective = mission.CreateNewObjective(new Vector3(642.9566, 543.2855, 129.2457), Objective.ObjectiveTypes.Location);
            objective = mission.CreateNewObjective(new Vector3(609.6921, 502.1974, 138.8454), Objective.ObjectiveTypes.Location);
            objective = mission.CreateNewObjective(new Vector3(593.769, 482.1479, 143.6454), Objective.ObjectiveTypes.Location);
            objective = mission.CreateNewObjective(new Vector3(572.7003, 430.6943, 170.8436), Objective.ObjectiveTypes.Location);
            mission.startMission();

        }

        public float fetchGroundHeight(Client player, Vector3 location)
        {
            Vector3 oldPlayerPos = player.position;
            float ground = 0;
            API.setEntityPosition(player, location);
            API.resetEntityData(player, "Ground_Height");
            API.triggerClientEvent(player, "getGroundHeight", location);
            int timeout = DateTime.Now.Millisecond;
            while (!API.hasEntityData(player, "Ground_Height"))
            {
                if (DateTime.Now.Millisecond > timeout + 2000)
                {
                    API.resetEntityData(player, "Ground_Height");
                    return -1;
                }

                if (API.hasEntityData(player, "Ground_Height"))
                {
                    ground = API.getEntityData(player, "Ground_Height");
                    break;
                }
            }
            API.setEntityPosition(player, oldPlayerPos.Add(new Vector3(0, 0, 2)));
            API.resetEntityData(player, "Ground_Height");
            return ground;
        }

        [Command("trailblazer")]
        public void cmdTrailBlazer(Client player)
        {
            if (!checkIfInMission(player))
            {
                return;
            }

            Vector3 startPoint = new Vector3(-739.2295, 3662.618, 279.9295);
            Vector3 endPoint = new Vector3(-651.6451, 2956.092, 13.4562);
            Vector3 midPoint = Vector3.Lerp(startPoint, endPoint, 0.5f);

            Mission mission = API.getEntityData(player, "Mission");
            Objective objective;
            objective = mission.CreateNewObjective(startPoint, Objective.ObjectiveTypes.Teleport);
            int partyCount = mission.NumberOfPartyMembers;
            // Generate Bikes for the players.
            for (int i = 0; i < partyCount; i++)
            {
                objective.addObjectiveVehicle(mission, startPoint, VehicleHash.Scorcher);
            }
            // Last point we put in.
            Vector3 lastPoint = startPoint;
            // Control how often we're going to look for a point to add.
            bool lookingForGroundHeight = true;
            DateTime generateTimeStarted = DateTime.Now.AddMilliseconds(5000);

            int points = 10;
            double currentRange = 0.1;

            for (int i = 0; i < points; i++)
            {
                while (true)
                {
                    if (DateTime.Now > generateTimeStarted)
                    {
                        API.sendChatMessageToPlayer(player, "~r~Bad trail generation, retry.");
                        API.setEntityPosition(player, endPoint);
                        mission.forceEmptyMission();
                        return;
                    }

                    if (lookingForGroundHeight)
                    {
                        Vector3 newStart = Vector3.Lerp(lastPoint, endPoint, Convert.ToSingle(currentRange));
                        Vector3 newVector = Vector3.Lerp(getRandomVector3(startPoint, newStart), endPoint, Convert.ToSingle(currentRange));
                        float groundHeight = fetchGroundHeight(player, newVector);
                        newVector = new Vector3(newVector.X, newVector.Y, groundHeight);
                        if (newVector.Z < lastPoint.Z && newVector.DistanceTo2D(lastPoint) >= 10 && newVector.DistanceTo2D(lastPoint) <= 100 && newVector.Z > 0)
                        {
                            objective = mission.CreateNewObjective(newVector, Objective.ObjectiveTypes.Location);
                            lastPoint = newVector;
                            currentRange += 0.08;
                            break;
                        }
                    }
                }
            }
            objective = mission.CreateNewObjective(endPoint, Objective.ObjectiveTypes.Location);
            mission.startMission();
        }

        [Command("randomMission")]
        public void cmdRandomMission(Client player)
        {
            if (!checkIfInMission(player))
            {
                return;
            }

            Vector3 entry1 = new Vector3(858.0717, 1272.531, 358.4252);
            Vector3 entry2 = new Vector3(599.6312, 1338.299, 335.0936);

            Mission mission = API.getEntityData(player, "Mission");
            Objective objective = mission.CreateNewObjective(new Vector3(858.0717, 1272.531, 358.4252), Objective.ObjectiveTypes.Capture);

            int startTime = DateTime.Now.Millisecond;

            for (int i = 0; i < 10; i++)
            {
                Vector3 newPoint = getRandomVector3(entry1, entry2);
                API.triggerClientEvent(player, "getGroundHeight", newPoint);
                float groundHeight = 0;

                while (groundHeight == 0)
                {
                    if (API.hasEntityData(player, "Ground_Height"))
                    {
                        groundHeight = API.getEntityData(player, "Ground_Height");
                    }
                }

                newPoint = new Vector3(newPoint.X, newPoint.Y, groundHeight);

                objective.setupObjective(newPoint, Objective.ObjectiveTypes.Capture);

                API.resetEntityData(player, "Ground_Height");
            }
            mission.startMission();
        }


        private Vector3 getRandomVector3(Vector3 regionA, Vector3 regionB)
        {
            Vector3 result;
            int minX;
            int minY;
            int minZ;
            int maxX;
            int maxY;
            int maxZ;

            if (regionA.X > regionB.X)
            {
                maxX = Convert.ToInt32(regionA.X);
                minX = Convert.ToInt32(regionB.X);
            }
            else
            {
                minX = Convert.ToInt32(regionA.X);
                maxX = Convert.ToInt32(regionB.X);
            }

            if (regionA.Y > regionB.Y)
            {
                maxY = Convert.ToInt32(regionA.Y);
                minY = Convert.ToInt32(regionB.Y);
            }
            else
            {
                minY = Convert.ToInt32(regionA.Y);
                maxY = Convert.ToInt32(regionB.Y);
            }

            if (regionA.Z > regionB.Z)
            {
                maxZ = Convert.ToInt32(regionA.Z);
                minZ = Convert.ToInt32(regionB.Z);
            }
            else
            {
                minZ = Convert.ToInt32(regionA.Z);
                maxZ = Convert.ToInt32(regionB.Z);
            }

            int newX = new Random().Next(minX, maxX);
            int newY = new Random().Next(minY, maxY);
            int newZ = new Random().Next(minZ, maxZ);

            result = new Vector3(newX, newY, newZ);

            return result;
        }
    }
}
