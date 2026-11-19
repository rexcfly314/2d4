using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D))]
public sealed class EnemyAnimationController : MonoBehaviour
{
    [Header("視覺元件")]
    [SerializeField] private Animator visualAnimator;
    [SerializeField] private SpriteRenderer visualSpriteRenderer;

    [Header("Run 動畫")]
    [SerializeField] private AnimationClip runClip;

    [Tooltip("Animator Controller 的 Run 狀態原先使用的 Animation Clip 資產名稱")]
    [SerializeField] private string runSourceClipName = "EnemyRun";

    [Tooltip("Animator 狀態的完整路徑，不是 Clip 資產名稱")]
    [SerializeField] private string runStatePath = "Base Layer.EnemyRun";

    [Header("Flip X 設定")]
    [SerializeField] private bool flipXByMovement = true;
    [SerializeField] private bool spriteFacesRightByDefault = true;
    [SerializeField, Min(0f)] private float facingThreshold = 0.05f;

    private Rigidbody2D enemyRigidbody;
    private AnimatorOverrideController runtimeOverrideController;
    private int runStateHash;
    private bool isInitialized;

    private void Reset()
    {
        visualAnimator = GetComponentInChildren<Animator>(true);
        visualSpriteRenderer = GetComponentInChildren<SpriteRenderer>(true);
    }

    private void Awake()
    {
        enemyRigidbody = GetComponent<Rigidbody2D>();

        if (visualAnimator == null)
        {
            visualAnimator = GetComponentInChildren<Animator>(true);
        }

        if (visualSpriteRenderer == null)
        {
            visualSpriteRenderer = GetComponentInChildren<SpriteRenderer>(true);
        }

        if (enemyRigidbody == null || visualAnimator == null ||
            visualSpriteRenderer == null || runClip == null)
        {
            Debug.LogError("敵人動畫初始化失敗，請指定 Animator、SpriteRenderer 與 Run Clip。", this);
            enabled = false;
            return;
        }

        if (string.IsNullOrWhiteSpace(runSourceClipName) ||
            string.IsNullOrWhiteSpace(runStatePath))
        {
            Debug.LogError("敵人動畫初始化失敗，原始 Clip 名稱和 Run State 路徑不可空白。", this);
            enabled = false;
            return;
        }

        runStateHash = Animator.StringToHash(runStatePath);
        if (!visualAnimator.HasState(0, runStateHash))
        {
            Debug.LogError("敵人 Animator 找不到 EnemyRun 狀態，請檢查 Run State Path。", this);
            enabled = false;
            return;
        }

        if (!ApplyRunClip())
        {
            enabled = false;
            return;
        }

        isInitialized = true;
        RestartRun();
    }

    private void OnEnable()
    {
        if (!isInitialized)
        {
            return;
        }

        RestartRun();
    }

    private void RestartRun()
    {
        visualSpriteRenderer.flipX = !spriteFacesRightByDefault;
        visualAnimator.Play(runStateHash, 0, 0f);
    }

    private void LateUpdate()
    {
        if (!isInitialized || !flipXByMovement)
        {
            return;
        }

        float velocityX = enemyRigidbody.linearVelocity.x;
        if (Mathf.Abs(velocityX) <= facingThreshold)
        {
            return;
        }

        bool movingLeft = velocityX < 0f;
        visualSpriteRenderer.flipX = spriteFacesRightByDefault
            ? movingLeft
            : !movingLeft;
    }

    private bool ApplyRunClip()
    {
        RuntimeAnimatorController sourceController = visualAnimator.runtimeAnimatorController;
        if (sourceController == null)
        {
            Debug.LogError("敵人 VisualRoot 尚未指定 Animator Controller。", this);
            return false;
        }

        runtimeOverrideController = new AnimatorOverrideController(sourceController);
        List<KeyValuePair<AnimationClip, AnimationClip>> overrides =
            new List<KeyValuePair<AnimationClip, AnimationClip>>();
        runtimeOverrideController.GetOverrides(overrides);

        bool found = false;
        for (int i = 0; i < overrides.Count; i++)
        {
            AnimationClip sourceClip = overrides[i].Key;
            if (sourceClip == null || sourceClip.name != runSourceClipName)
            {
                continue;
            }

            overrides[i] = new KeyValuePair<AnimationClip, AnimationClip>(sourceClip, runClip);
            found = true;
            break;
        }

        if (!found)
        {
            Debug.LogError("敵人 Animator 找不到指定的原始 Run Clip，請核對 Run Source Clip Name。", this);
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
