//::///////////////////////////////////////////////
//:: Default On Heartbeat
//:: NW_C2_DEFAULT1
//:: Copyright (c) 2001 Bioware Corp.
//:://////////////////////////////////////////////
/*
    This script will have people perform default
    animations.
*/
//:://////////////////////////////////////////////
//:: Created By: Preston Watamaniuk
//:: Created On: Nov 23, 2001
//:://////////////////////////////////////////////
#include "dmfi_db_inc"

void main()
{
    object oFollow = GetLocalObject(OBJECT_SELF, "dmfi_follow");
    int iLoiter = GetLocalInt(OBJECT_SELF, "dmfi_Loiter");

    // Will fire ONE time only - makes the thing hard to see
    if (!GetLocalInt(OBJECT_SELF, "hls_invis"))
    {
        SetListenPattern(OBJECT_SELF, "**", LISTEN_PATTERN); //listen to all text
        SetLocalInt(OBJECT_SELF, "hls_Listening", 1); //listen to all text
        SetListening(OBJECT_SELF, true);      //be sure NPC is listening

        //leave it here rather than add the one time loop to EVERY creature through a OS script change
        ApplyEffectToObject(DurationType.Permanent, EffectVisualEffect(VFX_DUR_CUTSCENE_INVISIBILITY), OBJECT_SELF);
        ApplyEffectToObject(DurationType.Permanent, EffectCutsceneGhost(), OBJECT_SELF);
        SetLocalInt(OBJECT_SELF, "hls_invis",1);
    }

    if (GetIsObjectValid(oFollow))
        {
        if (GetArea(oFollow)==GetArea(OBJECT_SELF))
            {
            AssignCommand(OBJECT_SELF, ClearAllActions(true));
            AssignCommand(OBJECT_SELF, ActionForceFollowObject(oFollow));
            }
            else
            {
            AssignCommand(OBJECT_SELF, ClearAllActions(true));
            AssignCommand(OBJECT_SELF, ActionJumpToObject(oFollow));
            AssignCommand(OBJECT_SELF, ActionForceFollowObject(oFollow));
            }
        }
    // If just following and listening, then return.
    if (!iLoiter)
        return;

    // If in loiter mode, look for a PC and make the announcement when appropraite
    object oPC = GetFirstObjectInShape(Shape.Sphere, 10.0f, GetLocation(OBJECT_SELF), true);
    while(GetIsObjectValid(oPC))
    {
        if (GetIsPC(oPC) &&
            !GetIsDM(oPC) &&
            iLoiter)
            {
                SpeakString(GetLocalString(OBJECT_SELF, "dmfi_LoiterSay"));
                DestroyObject(OBJECT_SELF);
            }
        oPC = GetNextObjectInShape(Shape.Sphere, 10.0f, GetLocation(OBJECT_SELF), true);
    }
}
