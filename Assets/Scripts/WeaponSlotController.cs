using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
[DisallowMultipleComponent]
public sealed class WeaponSlotController : MonoBehaviour
{
    [Header("Layout")]
    [SerializeField, Min(0f)]
    private float radius = 1.5f;

    [SerializeField]
    private float startAngle = 90f;

    [SerializeField]
    private bool clockwise = true;

    [Header("Rotation")]
    [SerializeField]
    private bool rotateSlotsOutward = true;

    private readonly List<WeaponSlot> weaponSlots =
        new List<WeaponSlot>();

    private void Awake()
    {
        RefreshSlots();
        ApplyLayout();
    }

    private void OnEnable()
    {
        RefreshSlots();
        ApplyLayout();
    }

    private void OnTransformChildrenChanged()
    {
        RefreshSlots();
        ApplyLayout();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        RefreshSlots();
        ApplyLayout();
    }
#endif

    public void RefreshSlots()
    {
        weaponSlots.Clear();

        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);

            if (!child.TryGetComponent(
                    out WeaponSlot weaponSlot))
            {
                continue;
            }

            weaponSlots.Add(weaponSlot);
        }
    }

    public void ApplyLayout()
    {
        int slotCount = weaponSlots.Count;

        if (slotCount == 0)
        {
            return;
        }

        float angleStep = 360f / slotCount;
        float direction = clockwise ? -1f : 1f;

        for (int i = 0; i < slotCount; i++)
        {
            WeaponSlot slot = weaponSlots[i];

            if (slot == null)
            {
                continue;
            }

            float angle =
                startAngle +
                angleStep * i * direction;

            float radians =
                angle * Mathf.Deg2Rad;

            Vector3 localPosition = new Vector3(
                Mathf.Cos(radians) * radius,
                Mathf.Sin(radians) * radius,
                0f);

            slot.transform.localPosition =
                localPosition;

            ApplySlotRotation(
                slot.transform,
                localPosition);
        }
    }

    private void ApplySlotRotation(
        Transform slotTransform,
        Vector3 localPosition)
    {
        if (!rotateSlotsOutward)
        {
            slotTransform.localRotation =
                Quaternion.identity;

            return;
        }

        Vector2 direction = new Vector2(
            localPosition.x,
            localPosition.y);

        if (direction.sqrMagnitude <= Mathf.Epsilon)
        {
            slotTransform.localRotation =
                Quaternion.identity;

            return;
        }

        float angle =
            Mathf.Atan2(direction.y, direction.x) *
            Mathf.Rad2Deg;

        slotTransform.localRotation =
            Quaternion.Euler(
                0f,
                0f,
                angle);
    }
}