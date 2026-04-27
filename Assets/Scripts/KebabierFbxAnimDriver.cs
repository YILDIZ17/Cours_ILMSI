using System.Collections;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

/// <summary>
/// Lit les clips importés du FBX (non-legacy, ex. "Kebabier_Rig|Run_Loop") via PlayableGraph.
/// </summary>
[DisallowMultipleComponent]
public class KebabierFbxAnimDriver : MonoBehaviour
{
    private Animator _animator;
    private PlayableGraph _graph;
    private AnimationPlayableOutput _output;
    private AnimationClipPlayable _activePlayable;
    private AnimationClip _runClip;
    private AnimationClip _jumpClip;
    private AnimationClip _hitClip;
    private Coroutine _oneShotRoutine;

    public bool IsValid => _runClip != null && _animator != null && _graph.IsValid();

    public void Build(string resourcesFolderPath, Transform visualRoot)
    {
        CleanupGraph();

        Transform rigTransform = FindRigTransform(visualRoot);
        GameObject rigObject = rigTransform != null ? rigTransform.gameObject : visualRoot.gameObject;

        _animator = rigObject.GetComponent<Animator>();
        if (_animator == null)
        {
            _animator = rigObject.AddComponent<Animator>();
        }

        _animator.runtimeAnimatorController = null;
        _animator.applyRootMotion = false;
        _animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

        AnimationClip[] clips = Resources.LoadAll<AnimationClip>(resourcesFolderPath);
        _runClip = FindClipByToken(clips, "Run_Loop");
        _jumpClip = FindClipByToken(clips, "Jump");
        _hitClip = FindClipByToken(clips, "HitReact");

        if (_runClip == null)
        {
            return;
        }

        _graph = PlayableGraph.Create("KebabierFbxAnim");
        _graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
        _output = AnimationPlayableOutput.Create(_graph, "KebabierOut", _animator);

        _activePlayable = AnimationClipPlayable.Create(_graph, _runClip);
        _activePlayable.SetApplyFootIK(false);
        _output.SetSourcePlayable(_activePlayable);
        _graph.Play();
    }

    private void Update()
    {
        if (!IsValid || _oneShotRoutine != null)
        {
            return;
        }

        if (_activePlayable.GetAnimationClip() != _runClip)
        {
            return;
        }

        double len = _runClip.length;
        if (len <= 0.0001)
        {
            return;
        }

        if (_activePlayable.GetTime() >= len)
        {
            _activePlayable.SetTime(_activePlayable.GetTime() % len);
        }
    }

    public void PlayRun()
    {
        if (_runClip == null || !_graph.IsValid())
        {
            return;
        }

        if (_oneShotRoutine != null)
        {
            StopCoroutine(_oneShotRoutine);
            _oneShotRoutine = null;
        }

        SwapActiveClip(_runClip);
    }

    public void PlayJump()
    {
        if (_jumpClip == null || !_graph.IsValid())
        {
            return;
        }

        if (_oneShotRoutine != null)
        {
            StopCoroutine(_oneShotRoutine);
        }

        _oneShotRoutine = StartCoroutine(PlayOneShotThenRun(_jumpClip));
    }

    public void PlayHit()
    {
        if (_hitClip == null || !_graph.IsValid())
        {
            return;
        }

        if (_oneShotRoutine != null)
        {
            StopCoroutine(_oneShotRoutine);
        }

        _oneShotRoutine = StartCoroutine(PlayOneShotThenRun(_hitClip));
    }

    private IEnumerator PlayOneShotThenRun(AnimationClip clip)
    {
        SwapActiveClip(clip);
        yield return new WaitForSeconds(clip.length * 0.98f);
        SwapActiveClip(_runClip);
        _oneShotRoutine = null;
    }

    private void SwapActiveClip(AnimationClip clip)
    {
        if (!_graph.IsValid() || clip == null)
        {
            return;
        }

        if (_activePlayable.IsValid())
        {
            PlayableExtensions.Destroy(_activePlayable);
        }

        _activePlayable = AnimationClipPlayable.Create(_graph, clip);
        _activePlayable.SetApplyFootIK(false);
        _activePlayable.SetTime(0);
        _output.SetSourcePlayable(_activePlayable);
        if (!_graph.IsPlaying())
        {
            _graph.Play();
        }
    }

    private void OnDestroy()
    {
        CleanupGraph();
    }

    private void CleanupGraph()
    {
        if (_oneShotRoutine != null)
        {
            StopCoroutine(_oneShotRoutine);
            _oneShotRoutine = null;
        }

        if (_graph.IsValid())
        {
            _graph.Destroy();
        }
    }

    private static Transform FindRigTransform(Transform root)
    {
        if (root == null)
        {
            return null;
        }

        foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
        {
            string n = t.name;
            if (n.Contains("Rig") || n.Contains("Armature") || n.Contains("Skeleton"))
            {
                return t;
            }
        }

        return root;
    }

    private static AnimationClip FindClipByToken(AnimationClip[] clips, string token)
    {
        if (clips == null)
        {
            return null;
        }

        foreach (AnimationClip clip in clips)
        {
            if (clip == null)
            {
                continue;
            }

            if (token == "Jump")
            {
                if (clip.name.EndsWith("|Jump") || clip.name == "Jump")
                {
                    return clip;
                }

                continue;
            }

            if (clip.name.Contains(token))
            {
                return clip;
            }
        }

        return null;
    }
}
