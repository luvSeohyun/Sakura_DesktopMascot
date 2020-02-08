using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public abstract class RoleCtrlBase : MonoBehaviour
{
    List<string> _aniHead, _aniNormal, _aniSpecial, _aniUndef;
    Dictionary<string, AudioClip> _audioClip = new Dictionary<string, AudioClip>();
    Animator _animator;
    AudioSource _audioPlayer;
    public GameObject rotationCenter;

    public bool isIdleState => _animator.GetCurrentAnimatorStateInfo(0).IsName(idleStateName) &&
    _animator.GetNextAnimatorClipInfo(0).Length == 0;

    protected abstract string idleStateName { get; }
    protected abstract float transitionDuration { get; }
    protected abstract void GetAnimation(out List<string> animationNames_head, out List<string> animationNames_normal,
    out List<string> animationNames_special, out List<string> animationNames_undef);
    protected abstract AudioClip[] GetAudioClips();

    protected abstract void StartEvent();
    protected abstract void UpdateEvent();

    void Start()
    {
        // 读取动作
        GetAnimation(out _aniHead, out _aniNormal, out _aniSpecial, out _aniUndef);
        Debug.Assert(_aniHead.Count + _aniNormal.Count + _aniSpecial.Count + _aniUndef.Count > 0, "未读取到动作");
        // 读取音频
        var audio = GetAudioClips();
        Debug.Assert(audio.Length > 0, "未读取到音频");
        foreach (var audioClip in audio)
        {
            _audioClip.Add(audioClip.name, audioClip);
        }
        // 初始化
        _animator = GetComponent<Animator>();
        Debug.Assert(_animator != null && _animator.hasBoundPlayables, "丢失Animator或AnimatorController");
        _audioPlayer = GetComponent<AudioSource>();
        if (_audioPlayer == null)
            _audioPlayer = gameObject.AddComponent<AudioSource>();
        StartEvent();
    }

    void Update()
    {
        if ((Input.GetMouseButtonUp(0)) && isIdleState)
        {
            RaycastHit hitInfo;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo, 1000f, LayerMask.GetMask("Gal")))
            {
                if (hitInfo.transform.IsChildOf(transform))
                    Touch(hitInfo.collider.tag);
            }
        }
        UpdateEvent();
    }

    void Touch(string tag)
    {
        string aniName = null;
        switch (tag)
        {
            case "Gal_Normal":
                aniName = RandomIndex(_aniNormal);
                break;
            case "Gal_Head":
                aniName = RandomIndex(_aniHead);
                break;
            case "Gal_Special":
                aniName = RandomIndex(_aniSpecial);
                break;
        }
        if (aniName == null)
        {
            aniName = RandomIndex(_aniUndef);
        }
        if (aniName != null)
            _animator.CrossFade(aniName, transitionDuration, 0);
    }

    // 触发音频
    void TriggerAudioPattern(string audioName)
    {
        if (_audioClip.ContainsKey(audioName))
        {
            _audioPlayer.clip = _audioClip[audioName];
            _audioPlayer.Play();
        }
        else
        {
            Debug.LogWarning("当前Animation Clip中的Audio Clip名称不存在");
        }
    }

    // 触发触摸特效
    void GalTouchEffect(string effectName)
    {
        Debug.Log("触发特效：" + effectName);
    }

    // 触发UI特效
    void PlayUIEffect(string effectName)
    {
        Debug.Log("触发UI特效：" + effectName);
    }

    // 触发黑屏
    void FadeBlack(float time)
    {
        Debug.Log("触发黑屏：" + time);
    }

    // 触发重启
    void RestartGame()
    {
        Debug.Log("触发重启");
    }

    void specified()
    {
        Debug.Log("触发specified");
    }

    protected List<string> LoadAniName(string path)
    {
        List<string> names = new List<string>();
        foreach (var item in Resources.LoadAll<AnimationClip>(path))
        {
            names.Add(item.name);
        }
        return names;
    }

    protected List<string> LoadAniName(AnimationClip[] clips)
    {
        List<string> names = new List<string>();
        foreach (var item in clips)
        {
            names.Add(item.name);
        }
        return names;
    }

    protected string RandomIndex(List<string> array)
    {
        if (array.Count > 0)
            return array[Random.Range(0, array.Count)];
        else
            return null;
    }
}
