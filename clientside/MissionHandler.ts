var currentMission: Mission = null;
var missionMarker = null;
var markerR = 0;
var markerG = 204;
var markerB = 255;
var markerAlpha = 100;
var screenX = API.getScreenResolutionMantainRatio().Width;
var screenY = API.getScreenResolutionMantainRatio().Height;
// Used to sync data between server / client. One way ticket.
API.onEntityDataChange.connect(function (player, data, oldValue) {
    /** Check if the data recieved is a player. */
    if (API.getEntityType(player) !== Enums.EntityType.Player) {
        return;
    }
    /** Check if the data that changed is the local player. */
    if (player.Value !== API.getLocalPlayer().Value) {
        return;
    }
    /** Check if they have a mission key. */
    if (!API.hasEntitySyncedData(player, "Mission")) {
        return;
    }
    /** Check if the mission is running. */
    if (!API.getEntitySyncedData(player, "Mission")) {
        return;
    }

    checkMissionNullState();
    API.sendChatMessage("RECIEVED MISSION INFORMATION");

    ///*** Now we can check to see what mission information needs to be recieved. */
    switch (data) {
        case "Mission_Location":
            currentMission.Location = API.getEntitySyncedData(player, "Mission_Location");
            return;
        case "Mission_Type":
            currentMission.ObjectiveType = API.getEntitySyncedData(player, "Mission_Type");
            return;
        case "Mission_Pause":
            currentMission.PauseState = API.getEntitySyncedData(player, "Mission_Pause");
            return;
        /** We just use this one to play a sound. */
        case "Mission_Finish_Objective":
            API.playSoundFrontEnd("Checkpoint_Hit", "GTAO_FM_Events_Soundset");
            API.displaySubtitle("~g~Objective Complete", 3000);
            return;
        /** We just use this to cleanup a player mission if they choose to leave. */
        case "Mission_Abandon":
            currentMission.cleanupMission();
            API.displaySubtitle("~r~Abandoned Mission", 3000);
            API.playSoundFrontEnd("LOSER", "HUD_AWARDS");
            return;
    }
});
// Mission Class //
class Mission {
    private missionLocation: Vector3;
    private missionType: string;
    private missionPause: boolean;

    const() {
        this.missionLocation = null;
        this.missionType = null;
        this.missionPause = true;
        return this;
    }

    /**
     * Used to run the main functionality of our mission.
     */
    public run() {
        if (this.missionPause) {
            return;
        }

        this.missionOverlays();
        this.missionObjectives();
    }

    /** Get / Set the current mission location. */
    set Location(value: Vector3) {
        this.missionLocation = value;
    }

    get Location(): Vector3 {
        return this.missionLocation;
    }

    /** Get / Set the current mission type. */
    set ObjectiveType(value: string) {
        this.missionType = value;
        /** Setup a new marker for the objective type. */
        this.setupMarker();
    }

    get ObjectiveType(): string {
        return this.missionType;
    }

    /** Get / Set the current mission pause state. */
    set PauseState(value: boolean) {
        this.missionPause = value;
    }

    get PauseState(): boolean {
        return this.missionPause;
    }

    /**
     *  Used to cleanup any leftover mission objectives and clear out the current Mission. Normally used in the final step of a mission.
     */
    public cleanupMission() {
        currentMission = null;
        if (missionMarker !== null) {
            API.deleteEntity(missionMarker);
        }
        return;
    }

    private setupMarker() {
        /** Delete the existing marker if it exists. */
        if (missionMarker !== null) {
            API.deleteEntity(missionMarker);
        }
        /** Setup markers based on objective types. */
        switch (this.missionType) {
            case "Location":
                missionMarker = API.createMarker(Enums.MarkerType.VerticalCylinder, this.Location, new Vector3(), new Vector3(), new Vector3(3, 3, 3), markerR, markerG, markerB, markerAlpha);
                break;
            case "Capture":
                missionMarker = API.createMarker(Enums.MarkerType.VerticalCylinder, this.Location, new Vector3(), new Vector3(), new Vector3(5, 5, 5), markerR, markerG, markerB, markerAlpha);
                break;
            case "FastCapture":
                missionMarker = API.createMarker(Enums.MarkerType.VerticalCylinder, this.Location, new Vector3(), new Vector3(), new Vector3(5, 5, 5), markerR, markerG, markerB, markerAlpha);
                break;
        }
    }

    private missionOverlays() {
        // Don't leave this on release.
        API.drawText('Type: ' + this.missionType, 900, 75, 0.5, 255, 255, 255, 255, 4, 0, false, false, 600);
        API.drawText('Pause State: ' + this.missionPause, 900, 100, 0.5, 255, 255, 255, 255, 4, 0, false, false, 600);
    }

    private missionObjectives() {
        /** Handles how our mission interacts. */
        switch (this.missionType) {
            /** This handles point to point objectives. */
            case "Finish":
                this.cleanupMission();
                API.displaySubtitle("~g~Mission Complete", 5000);
                API.playSoundFrontEnd("WIN", "HUD_AWARDS");
                return;
            case "Location":
                objectiveLocation();
                return;
            case "Capture":
                objectiveCapture();
                return;
            case "FastCapture":
                objectiveCapture();
                return;
        }
    }
}
/** OnUpdate Event */
API.onUpdate.connect(function () {
    if (currentMission === null) {
        return;
    }
    currentMission.run();
});
//// MISC FUNCTIONS
/**  Used to determine if the mission exists or not. */
function checkMissionNullState() {
    if (currentMission === null) {
        currentMission = new Mission();
        API.sendChatMessage("Generated Mission for Player");
    }
}

/** Attempt to verify our objective with the server to ensure that it's true. */
function checkObjective(currentType) {
    API.triggerServerEvent("checkObjective", currentType);
}
//// OBJECTIVE TYPES
/** This is a point to point location objective type. */
function objectiveLocation() {
    if (API.getEntityPosition(API.getLocalPlayer()).DistanceTo(currentMission.Location) <= 3) {
        checkObjective(currentMission.ObjectiveType);
    }
}

/** Basic capture point type */
function objectiveCapture() {
    if (API.getEntityPosition(API.getLocalPlayer()).DistanceTo(currentMission.Location) <= 5) {
        checkObjective(currentMission.ObjectiveType);
    }
}