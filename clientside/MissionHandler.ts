// Screen Stuff
var screenX = API.getScreenResolution().Width;
var screenY = API.getScreenResolution().Height;

// Mission PauseState
var missionPauseState = true;

// Stylesheet Properties
var backgroundRGBA = [0, 0, 0, 100];
var textRGBA = [255, 255, 255, 255];
var overlayRGBA = [1, 87, 155, 255];
var markerRGBA = [79, 195, 247, 175];
var blipColor = 77;

// Used for over-head notifications.
var headNotification: PlayerHeadNotification = null;

// Array of mission objectives / locations.
var objectiveLocations: Vector3[] = new Array <Vector3>();
var objectiveTypes = [];

// Array of mission blips / markers.
var objectiveMarkers = [];
var objectiveBlips = [];

// Array of mission progress arrays.
var objectiveProgression: ObjectiveProgression[] = new Array <ObjectiveProgression>();

// Array of current allied players on team.
var allies = [];


API.onServerEventTrigger.connect(function (event, args) {
    if (event === "getGroundHeight") {
        var groundHeight = API.getGroundHeight(args[0]);
        API.triggerServerEvent("recieveGroundHeight", groundHeight);
        return;
    }

    if (!event.includes("Mission")) {
        return;
    }
    switch (event) {
        // Team Removal / Updates
        case "Mission_Add_Player":
            addPlayer(args[0]);
            return;
        case "Mission_Remove_Player":
            removePlayer(args[0]);
            return;
        // Mission Instance - Objectives
        case "Mission_New_Objective":
            objectiveLocations.push(args[0]);
            objectiveTypes.push(args[1]);
            return;
       // Mission Instance - Setup Objective Markers
        case "Mission_Setup_Objectives":
            setupMarkers();
            setupBlips();
            return;
       // Mission Instance - Remove Objective
        case "Mission_Remove_Objective":
            removeObjective(args[0]);
            return;
        // Mission Instance
        case "Mission_New_Instance":
            fullCleanup();
            return;
        case "Mission_Abandon":
            fullCleanup();
            return;
        case "Mission_Finish":
            partialCleanup();
            return;
        case "Mission_Pause_State":
            missionPauseState = args[0];
            return;
        case "Mission_Head_Notification":
            headNotification = new PlayerHeadNotification(args[0]);

            if (args.Count <= 1) {
                return;
            }
            switch (args[1]) {
                case "Objective":
                    API.playSoundFrontEnd("On_Call_Player_Join", "DLC_HEISTS_GENERAL_FRONTEND_SOUNDS");
                    return;
                case "Finish":
                    API.playSoundFrontEnd("Mission_Pass_Notify", "DLC_HEISTS_GENERAL_FRONTEND_SOUNDS");
                    return;
                case "Fail":
                    API.playSoundFrontEnd("Hack_Failed", "DLC_HEIST_BIOLAB_PREP_HACKING_SOUNDS");
                    return;
                case "ObjectivesComplete":
                    API.playSoundFrontEnd("Highlight_Move", "DLC_HEIST_PLANNING_BOARD_SOUNDS");
                    return;
                case "NewObjective":
                    API.playSoundFrontEnd("FocusIn", "HintCamSounds");
                    return;
                case "Error":
                    API.playSoundFrontEnd("Highlight_Error", "DLC_HEIST_PLANNING_BOARD_SOUNDS");
                    return;
            }

            return;
        case "Mission_Update_Progression":
            var vector: Vector3 = args[0];
            var exists = false;
            var whatIndex = 0;
            for (var i = 0; i < objectiveProgression.length; i++) {
                if (objectiveProgression[i].ObjectiveLocation.ToString() == vector.ToString()) {
                    whatIndex = i;
                    exists = true;
                    break;
                }
            }
            if (exists) {
                objectiveProgression[whatIndex].ObjectiveProgress = args[1];
            } else {
                var obj = new ObjectiveProgression(args[0]);
                obj.ObjectiveProgress = args[1];
                objectiveProgression.push(obj);
            }
            return;
    }
});
/**
 * Cleans up anything / everything but ALLIES.
 */
function partialCleanup() {
    objectiveLocations = [];
    objectiveTypes = [];
    cleanupMarkers();
    cleanupBlips();
    cleanupProgression();
}

/**
 * Cleans up anything / everything.
 */
function fullCleanup() {
    allies = [];
    objectiveLocations = [];
    objectiveTypes = [];
    cleanupProgression();
    cleanupMarkers();
    cleanupBlips();
}

/**
 *  Called when we need to setup all the markers for our objectives.
 */
function setupMarkers() {
    // Deletes any existing markers and then creates a clean array.
    cleanupMarkers();
    // Get all of our objective locations, loop through and determine the type of marker we need.
    for (var i = 0; i < objectiveLocations.length; i++) {
        let newMarker = null;
        switch (objectiveTypes[i]) {
            case "Location":
                newMarker = API.createMarker(Enums.MarkerType.ChevronUpX1, objectiveLocations[i].Add(new Vector3(0, 0, 2)), new Vector3(), new Vector3(), new Vector3(1, 1, 1), markerRGBA[0], markerRGBA[1], markerRGBA[2], markerRGBA[3]);
                break;
            case "Capture":
                newMarker = API.createMarker(Enums.MarkerType.VerticalCylinder, objectiveLocations[i], new Vector3(), new Vector3(), new Vector3(5, 5, 5), markerRGBA[0], markerRGBA[1], markerRGBA[2], markerRGBA[3]);
                break;
            case "FastCapture":
                newMarker = API.createMarker(Enums.MarkerType.VerticalCylinder, objectiveLocations[i], new Vector3(), new Vector3(), new Vector3(5, 5, 5), markerRGBA[0], markerRGBA[1], markerRGBA[2], markerRGBA[3]);
                break;
            default:
                return;
        }
        if (newMarker !== null) {
            objectiveMarkers.push(newMarker);
        }
    }
}
/**
 *  Called when we need to setup all the blips for our objectives.
 */
function setupBlips() {
    // Deleted any existing blips and then created a clean array.
    cleanupBlips();
    // Get all of our objective locations, loop through and determine the type of blip we need.
    for (var i = 0; i < objectiveLocations.length; i++) {
        let newBlip = API.createBlip(objectiveLocations[i]);
        switch (objectiveTypes[i]) {
            case "Capture":
                API.setBlipSprite(newBlip, 164);
                API.setBlipColor(newBlip, blipColor);
                break;
            case "FastCapture":
                API.setBlipSprite(newBlip, 164);
                API.setBlipColor(newBlip, blipColor);
                break;
            case "Location":
                API.setBlipSprite(newBlip, 1);
                API.setBlipColor(newBlip, blipColor);
                break;
            case "Teleport":
                API.deleteEntity(newBlip);
                return;
        }
        objectiveBlips.push(newBlip);
    }
}
/**
 * Cleanup blips.
 */
function cleanupBlips() {
    if (objectiveBlips.length <= 0) {
        return;
    }

    for (var i = 0; i < objectiveBlips.length; i++) {
        API.deleteEntity(objectiveBlips[i]);
    }

    objectiveBlips = [];
}
/**
 * Cleanup Markers.
 */
function cleanupMarkers() {
    if (objectiveMarkers.length <= 0) {
        return;
    }

    for (var i = 0; i < objectiveMarkers.length; i++) {
        API.deleteEntity(objectiveMarkers[i]);
    }

    objectiveMarkers = [];
}
/**
* Cleanup Text
*/
function cleanupProgression() {
    objectiveProgression = [];
}


/**
 * Remove an objective based on location.
 */
function removeObjective(location: Vector3) {
    var index = -1;
    for (var i = 0; i < objectiveLocations.length; i++) {
        if (objectiveLocations[i].ToString() === location.ToString()) {
            index = i;
            break;
        }
    }

    if (objectiveMarkers.length > 0) {
        API.deleteEntity(objectiveMarkers[index]);
    }

    if (objectiveBlips.length > 0) {
        API.deleteEntity(objectiveBlips[index]);
    }

    objectiveMarkers.splice(index, 1);
    objectiveLocations.splice(index, 1);
    objectiveTypes.splice(index, 1);
    objectiveBlips.splice(index, 1);

    // Delete Progression Text
    var index = -1;
    for (var i = 0; i < objectiveProgression.length; i++) {
        if (objectiveProgression[i].objectiveLocation.ToString() === location.ToString()) {
            index = i;
            break;
        }
    }

    if (index === -1) {
        return;
    }

    objectiveProgression.splice(index, 1);
}

class ObjectiveProgression {
    objectiveLocation: Vector3;
    objectiveProgress: number;

    constructor(value: Vector3) {
        this.objectiveLocation = value;
        this.objectiveProgress = 0;
    }

    // Get the objective location.
    get ObjectiveLocation(): Vector3 {
        return this.objectiveLocation;
    }

    set ObjectiveProgress(value: number) {
        this.objectiveProgress = value;
    }

    run() {
        if (API.getEntityPosition(API.getLocalPlayer()).DistanceTo(this.objectiveLocation) <= 15) {
            var pointer = Point.Round(API.worldToScreenMantainRatio(this.objectiveLocation));
            API.drawText(`${this.objectiveProgress}%`, pointer.X, pointer.Y - 10, 0.5, textRGBA[0], textRGBA[1], textRGBA[2], textRGBA[3], 4, 1, false, true, 600);
        }
    }
}

class PlayerHeadNotification {
    headText: string;
    headAlpha: number;
    headAddon: number;

    constructor(value: string) {
        this.headText = value;
        this.headAlpha = 255;
        this.headAddon = 0;
    }

    run() {
        if (this.headAlpha <= 0) {
            headNotification = null;
            return;
        }

        var location = API.getEntityPosition(API.getLocalPlayer()).Add(new Vector3(0, 0, 1.2));
        var pointer = Point.Round(API.worldToScreenMantainRatio(location));
        API.drawText(`${this.headText}`, pointer.X, Math.round(pointer.Y + this.headAddon), 0.5, textRGBA[0], textRGBA[1], textRGBA[2], Math.round(this.headAlpha), 4, 1, false, true, 600);

        this.headAddon -= 0.5;
        this.headAlpha -= 3;
    }
}

/** OnUpdate Event */
API.onUpdate.connect(function () {
    if (headNotification !== null) {
        headNotification.run();
    }

    if (allies.length >= 1) {
        displayCurrentPlayers();
    }

    if (missionPauseState) {
        return;
    }

    if (objectiveTypes.length >= 1) {
        missionObjectives();
    }

    displayObjectiveProgress();
});

// Check all objective types.
function missionObjectives() {
    for (var i = 0; i < objectiveTypes.length; i++) {
        switch (objectiveTypes[i]) {
            case "Finish":
                this.cleanupMission();
                API.showShard("~g~Mission Complete", 5000);
                API.playSoundFrontEnd("WIN", "HUD_AWARDS");
                return;
            case "Location":
                objectiveLocation();
                return;
            case "Teleport":
                objectiveTeleport();
                return;
            case "Capture":
                objectiveCapture();
                return;
            case "FastCapture":
                //objectiveCapture();
                return;
        }
    }
}

//// MISC FUNCTIONS
function displayCurrentPlayers() {
    var team = allies.join("~n~");
    API.drawText(`~b~Current Team ~w~~n~${team}`, screenX - 100, 20, 0.4, textRGBA[0], textRGBA[1], textRGBA[2], textRGBA[3], 4, 1, false, true, 150);
}

function displayObjectiveProgress() {
    if (objectiveProgression.length <= 0) {
        return;
    }

    for (var i = 0; i < objectiveProgression.length; i++) {
        objectiveProgression[i].run();
    }
}

/** Used to add a player to the array stack. **/
function addPlayer(target) {
    let exists = false;
    for (var i = 0; i < allies.length; i++) {
        if (allies[i] === target) {
            exists = true;
            break;
        }
    }

    if (exists) {
        return;
    }

    allies.push(target);
}

/**
 *  Used to remove a player from the array stack.
 * @param target
 */
function removePlayer(target) {
    let index = null;
    for (var i = 0; i < allies.length; i++) {
        if (allies[i] === target) {
            index = i;
            break;
        }
    }

    if (index === null) {
        return;
    }

    allies.splice(index, 1);
}

//// OBJECTIVE TYPES
/** This is a point to point location objective type. */
function objectiveLocation() {
    for (var i = 0; i < objectiveLocations.length; i++) {
        if (API.getEntityPosition(API.getLocalPlayer()).DistanceTo(objectiveLocations[i]) <= 3) {
            API.triggerServerEvent("checkObjective");
        }
    }
}

function objectiveTeleport() {
    API.triggerServerEvent("checkObjective");
}

function objectiveCapture() {
    for (var i = 0; i < objectiveLocations.length; i++) {
        if (API.getEntityPosition(API.getLocalPlayer()).DistanceTo(objectiveLocations[i]) <= 5) {
            API.triggerServerEvent("checkObjective");
        }
    }
}