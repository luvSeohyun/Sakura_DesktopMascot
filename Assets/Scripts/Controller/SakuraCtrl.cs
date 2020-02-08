using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Animator))]
public class SakuraCtrl : RoleCtrlBase
{
    [SerializeField] string _idleStateName = "Avatar_Yae_Sakura_Ani_StandBy";
    [SerializeField] AnimationClip[] _headAnimation;
    [SerializeField] AnimationClip[] _normalAnimation;
    [SerializeField] AnimationClip[] _specialAnimation;
    [SerializeField] AnimationClip[] _undefAnimation;
    [SerializeField] string _audioClipsPath = "Audio/Sakura_Bridge";
    [SerializeField] float _transitionDuration = 0.5f;
    protected override string idleStateName => _idleStateName;

    protected override float transitionDuration => _transitionDuration;

    protected override void GetAnimation(out List<string> animationNames_head, out List<string> animationNames_normal,
     out List<string> animationNames_special, out List<string> animationNames_undef)
    {
        animationNames_head = LoadAniName(_headAnimation);
        animationNames_normal = LoadAniName(_normalAnimation);
        animationNames_special = LoadAniName(_specialAnimation);
        animationNames_undef = LoadAniName(_undefAnimation);
    }

    protected override AudioClip[] GetAudioClips()
    {
        return Resources.LoadAll<AudioClip>(_audioClipsPath);
    }

    protected override void StartEvent() { }

    protected override void UpdateEvent() { }

}
