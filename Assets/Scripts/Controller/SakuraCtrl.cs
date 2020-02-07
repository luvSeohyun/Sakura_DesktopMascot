using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Animator))]
public class SakuraCtrl : RoleCtrlBase
{
    [SerializeField]
    string _idleStateName = "Avatar_Yae_Sakura_Ani_StandBy";
    [SerializeField]
    string _headAnimationPath = "AnimationClip/Head";
    [SerializeField]
    string _normalAnimationPath = "AnimationClip/Normal";
    [SerializeField]
    string _specialAnimationPath = "AnimationClip/Special";
    [SerializeField]
    string _undefAnimationPath = "AnimationClip/Undefined";
    [SerializeField]
    string _audioClipsPath = "Audio/Sakura_Bridge";
    protected override string idleStateName => _idleStateName;

    protected override void GetAnimationNames(out List<string> animationNames_head, out List<string> animationNames_normal,
     out List<string> animationNames_special, out List<string> animationNames_undef)
    {
        animationNames_head = LoadAniName(_headAnimationPath);
        animationNames_normal = LoadAniName(_normalAnimationPath);
        animationNames_special = LoadAniName(_specialAnimationPath);
        animationNames_undef = LoadAniName(_undefAnimationPath);
    }

    protected override AudioClip[] GetAudioClips()
    {
        return Resources.LoadAll<AudioClip>(_audioClipsPath);
    }

    protected override void StartEvent() { }

    protected override void UpdateEvent() { }

}
