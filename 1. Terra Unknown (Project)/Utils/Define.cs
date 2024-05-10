using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Define
{
    public enum Scene
    {
        Unknown,
        Lobby,
        Management,
        Recruit,
        Outpost,
        Squad,
        Campaign,
        Loading,
        Trait,
        BossNodeMap,
        NormalNodeMap,
        Event,
        Shop,
        RestSite,
        Main
    }

    public enum Sound
    {
        Bgm,
        Effect,
        MaxCount,
    }
    
    public enum UIEvent
    {
        Click,
        Drag,
    }

    public enum MouseEvent
    {
        Press,
        Click,
    }

    public enum CameraMode
    {
        QuarterView,
    }
}