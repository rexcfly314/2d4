using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D))]
public sealed class PlayerAnimationController2D : MonoBehaviour
{
    [Header("視覺元件")]
    [SerializeField] private Animator visualAnimator;
    [SerializeField] private SpriteRenderer visualSpriteRenderer;

    [Header("動畫 Clip")]
    [SerializeField] private AnimationClip idleClip;
    [SerializeField] private AnimationClip runClip;

    [Header("原始 Clip 名稱")]
    [Tooltip("Animator Controller 的 Idle 狀態原先使用的 Animation Clip 資產名稱")]
    [SerializeField] private string idleSourceClipName = "PlayerIdle";

    [Tooltip("Animator Controller 的 Run 狀態原先使用的 Animation Clip 資產名稱")]
    [SerializeField] private string runSourceClipName = "PlayerRun";

    [Header("Animator 狀態路徑")]
    [Tooltip("Animator 狀態的完整路徑，不是 Clip 資產名稱")]
    [SerializeField] private string idleStatePath = "Base Layer.PlayerIdle";

    [Tooltip("Animator 狀態的完整路徑，不是 Clip 資產名稱")]
    [SerializeField] private string runStatePath = "Base Layer.PlayerRun";

    [Header("速度與面向")]
    [SerializeField, Min(0f)] private float movementThreshold = 0.05f;
    [SerializeField, Min(0f)] private float facingThreshold = 0.05f;
    [SerializeField] private bool spriteFacesRightByDefault = true;

    private Rigidbody2D playerRigidbody;
    private AnimatorOverrideController runtimeOverrideController;
    private int idleStateHash;
    private int runStateHash;
    private int currentStateHash = -1;
    private bool isInitialized;

    private void Reset()
    {
        visualAnimator = GetComponentInChildren<Animator>(true);
        visualSpriteRenderer = GetComponentInChildren<SpriteRenderer>(true);
    }

    private void Awake()
    {
        playerRigidbody = GetComponent<Rigidbody2D>();

        if (visualAnimator == null)
        {
            visualAnimator = GetComponentInChildren<Animator>(true);
        }

        if (visualSpriteRenderer == null)
        {
            visualSpriteRenderer = GetComponentInChildren<SpriteRenderer>(true);
        }

        if (playerRigidbody == null || visualAnimator == null ||
            visualSpriteRenderer == null || idleClip == null || runClip == null)
        {
            Debug.LogError("玩家動畫初始化失敗，請指定 Animator、SpriteRenderer、Idle Clip 與 Run Clip。", this);
            enabled = false;
            return;
        }

        if (string.IsNullOrWhiteSpace(idleStatePath) ||
            string.IsNullOrWhiteSpace(runStatePath) ||
            string.IsNullOrWhiteSpace(idleSourceClipName) ||
            string.IsNullOrWhiteSpace(runSourceClipName))
        {
            Debug.LogError("玩家動畫初始化失敗，動畫狀態路徑與原始 Clip 名稱不可空白。", this);
            enabled = false;
            return;
        }

        idleStateHash = Animator.StringToHash(idleStatePath);
        runStateHash = Animator.StringToHash(runStatePath);

        if (!visualAnimator.HasState(0, idleStateHash) ||
            !visualAnimator.HasState(0, runStateHash))
        {
            Debug.LogError("玩家 Animator 找不到 PlayerIdle 或 PlayerRun 狀態，請檢查完整狀態路徑。", this);
            enabled = false;
            return;
        }

        if (!ApplyClipOverrides())
        {
            enabled = false;
            return;
        }

        isInitialized = true;
        visualSpriteRenderer.flipX = !spriteFacesRightByDefault;
        PlayState(idleStateHash);
    }

    private void OnEnable()
    {
        if (!isInitialized)
        {
            return;
        }

        currentStateHash = -1;
        PlayState(idleStateHash);
    }

    private void LateUpdate()
    {
        if (!isInitialized)
        {
            return;
        }

        Vector2 velocity = playerRigidbody.linearVelocity;
        bool moving = velocity.sqrMagnitude > movementThreshold * movementThreshold;
        PlayState(moving ? runStateHash : idleStateHash);

        if (Mathf.Abs(velocity.x) <= facingThreshold)
        {
            return;
        }

        bool movingLeft = velocity.x < 0f;
        visualSpriteRenderer.flipX = spriteFacesRightByDefault
            ? movingLeft
            : !movingLeft;
    }

    private void PlayState(int stateHash)
    {
        if (currentStateHash == stateHash)
        {
            return;
        }

        visualAnimator.Play(stateHash, 0, 0f);
        currentStateHash = stateHash;
    }

    private bool ApplyClipOverrides()
    {
        RuntimeAnimatorController sourceController = visualAnimator.runtimeAnimatorController;
        if (sourceController == null)
        {
            Debug.LogError("玩家 VisualRoot 尚未指定 Animator Controller。", this);
            return false;
        }

        runtimeOverrideController = new AnimatorOverrideController(sourceController);
        List<KeyValuePair<AnimationClip, AnimationClip>> overrides =
            new List<KeyValuePair<AnimationClip, AnimationClip>>();
        runtimeOverrideController.GetOverrides(overrides);

        bool foundIdle = false;
        bool foundRun = false;

        for (int i = 0; i < overrides.Count; i++)
        {
            AnimationClip sourceClip = overrides[i].Key;
            if (sourceClip == null)
            {
                continue;
            }

            if (sourceClip.name == idleSourceClipName)
            {
                overrides[i] = new KeyValuePair<AnimationClip, AnimationClip>(sourceClip, idleClip);
                foundIdle = true;
            }
            else if (sourceClip.name == runSourceClipName)
            {
                overrides[i] = new KeyValuePair<AnimationClip, AnimationClip>(sourceClip, runClip);
                foundRun = true;
            }
        }

        if (!foundIdle || !foundRun)
        {
            Debug.LogError(
                "玩家 Animator 找不到指定的原始 Clip，請確認 Idle Source Clip Name 和 Run Source Clip Name 是 Motion 使用的 Clip 資產名稱。",
                this);
            Destroy(runtimeOverrideController);
            runtimeOverrideController = null;
            return false;
        }

        runtimeOverrideController.ApplyOverrides(overrides);
        visualAnimator.runtimeAnimatorController = runtimeOverrideController;
        return true;
    }

    private void OnDestroy()
    {
        if (runtimeOverrideController != null)
        {
            Destroy(runtimeOverrideController);
        }
    }
}
