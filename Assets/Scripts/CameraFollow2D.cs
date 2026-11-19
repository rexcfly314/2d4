using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Camera))]
public sealed class CameraFollow2D : MonoBehaviour
{
    [Header("追蹤目標")]
    [SerializeField]
    private Transform target;

    [Header("鏡頭設定")]
    [SerializeField]
    private Vector2 offset = Vector2.zero;

    [SerializeField, Min(0.01f)]
    private float smoothTime = 0.15f;

    [SerializeField]
    private bool snapToTargetOnStart = true;

    private Vector3 followVelocity;
    private float cameraDepth;

    private void Awake()
    {
        cameraDepth = transform.position.z;
    }

    private void Start()
    {
        if (target == null && GameManager.Instance != null)
        {
            PlayerUnit player = GameManager.Instance.GetPlayer();
            if (player != null)
            {
                target = player.Transform;
            }
        }

        if (target == null)
        {
            Debug.LogError("鏡頭追蹤初始化失敗，請指定 Player Transform。", this);
            enabled = false;
            return;
        }

        if (snapToTargetOnStart)
        {
            transform.position = GetTargetPosition();
        }
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        transform.position = Vector3.SmoothDamp(
            transform.position,
            GetTargetPosition(),
            ref followVelocity,
            smoothTime);
    }

    private Vector3 GetTargetPosition()
    {
        return new Vector3(
            target.position.x + offset.x,
            target.position.y + offset.y,
            cameraDepth);
    }
}
