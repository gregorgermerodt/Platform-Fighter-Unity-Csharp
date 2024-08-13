using UnityEngine;

[CreateAssetMenu(menuName = "PlatformFighter/FighterStats")]
public class FighterStats : ScriptableObject
{
    public string NAME;

    public float WALKING_SPEED;

    public float HORIZONTAL_GROUND_DECELERATION;

    public float FALL_SPEED;
    public float FALL_GRAVITY_ACCELERATION;

    public float AIR_DRIFT_SPEED;
    public float AIR_DRIFT_ACCELERATION;

    public float SHORTJUMP_JUMP_SPEED;
    public float SHORTJUMP_AIR_DECELERATION;
    public float FULLJUMP_JUMP_SPEED;
    public float FULLJUMP_AIR_DECELERATION;
    public float AIRJUMP_JUMP_SPEED;
    public float AIRJUMP_AIR_DECELERATION;

    public float MAX_AIR_JUMP_COUNT;
}

