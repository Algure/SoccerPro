using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Statics 
{
    public static float TimeWaitAfterGoalSaved = 5;//IN seconds
    internal static float freezeTimeAfterShot=0.8f;
    public static float offsetDistance=2;
    public static float GeneralSpeedFactor = 2.8f;
    internal static float GoalShotMinDistance=25;
    internal static float moveToPostMaxDistance4Midfielder=30;
    internal static float moveToPostMaxDistance4Striker=20;
    internal static float maxPlayerPursueRange = 10;
    internal static float TimeB4KickOff=1;
    internal static float TimeToRestoreDefaultPoint=8;

    public static Material[] playerMaterials = {
        Resources.Load("PlayerRedMaterial") as Material,
     Resources.Load("BlueMat") as Material,
    Resources.Load("PlayerPurpleMat") as Material,
    Resources.Load("PlayerRedKitMaterial") as Material};

    enum PlayerTypes { MidFielder, Defender, GoalKeeper, Striker};
}
