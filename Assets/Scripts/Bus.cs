using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bus : MonoBehaviour
{
    [Header("Bus Visual")]
    [SerializeField] private Renderer busRenderer;
    [SerializeField] private Renderer busRenderer2;

    [SerializeField] private Material redMaterial;
    [SerializeField] private Material blueMaterial;
    [SerializeField] private Material greenMaterial;
    [SerializeField] private Material yellowMaterial;

    [Header("Seats")]
    [SerializeField] private Transform chair1;
    [SerializeField] private Transform chair2;
    [SerializeField] private Transform chair3;

    [Header("Effect")]
    [SerializeField] private BusSeatEffectPlayer effectPlayer;

    [Header("Movement Feel")]
    [SerializeField] private float travelSpeedMultiplier = 1.15f;
    [SerializeField] private float brakeDistanceRatio = 0.4f;
    [SerializeField] private float minBrakeDistance = 1.2f;
    [SerializeField] private float maxBrakeDistance = 2.8f;
    [SerializeField] private float cruiseTimeRatio = 0.38f;

    [Header("Brake Squash")]
    [SerializeField] private float brakeSquashX = 0.08f;
    [SerializeField] private float brakeStretchZ = 0.12f;

    private List<Transform> seats = new List<Transform>();
    private int seatCapacity = 3;
    private int currentPassengerCount = 0;

    private ColorType busColor;
    private bool isMoving = false;
    private Vector3 initialScale;

    private void Awake()
    {
        initialScale = transform.localScale;
    }

    public void Initialize(ColorType color)
    {
        busColor = color;

        seats.Clear();

        if (chair1 != null) seats.Add(chair1);
        if (chair2 != null) seats.Add(chair2);
        if (chair3 != null) seats.Add(chair3);

        currentPassengerCount = 0;
        seatCapacity = seats.Count;

        ApplyBusMaterial();
    }

    private void ApplyBusMaterial()
    {
        if (busRenderer == null || busRenderer2 == null)
        {
            Debug.LogWarning("Bus renderer is missing");
            return;
        }

        switch (busColor)
        {
            case ColorType.Red:
                busRenderer.material = redMaterial;
                busRenderer2.material = redMaterial;
                break;
            case ColorType.Blue:
                busRenderer.material = blueMaterial;
                busRenderer2.material = blueMaterial;
                break;
            case ColorType.Green:
                busRenderer.material = greenMaterial;
                busRenderer2.material = greenMaterial;
                break;
            case ColorType.Yellow:
                busRenderer.material = yellowMaterial;
                busRenderer2.material = yellowMaterial;
                break;
        }
    }

    public ColorType GetBusColor()
    {
        return busColor;
    }

    public int GetSeatCapacity()
    {
        return seatCapacity;
    }

    public int GetCurrentPassengerCount()
    {
        return currentPassengerCount;
    }

    public bool IsFull()
    {
        return currentPassengerCount >= seatCapacity;
    }

    private int reservedCount = 0;

    public void ReserveSeat()
    {
        reservedCount++;
    }

    public bool IsFullyReserved()
    {
        return (currentPassengerCount + reservedCount) >= seatCapacity;
    }

    public Transform GetFirstEmptySeat()
    {
        if (IsFullyReserved())
            return null;

        return seats[currentPassengerCount + reservedCount];
    }

    public bool AddPassenger(GameObject passengerObject)
    {
        if (IsFull())
            return false;

        Transform emptySeat = seats[currentPassengerCount];

        if (emptySeat == null)
            return false;

        if (reservedCount > 0) reservedCount--;

        if (effectPlayer != null)
            effectPlayer.PlayAtSeat(emptySeat);

        passengerObject.transform.SetParent(emptySeat);
        passengerObject.transform.localPosition = Vector3.zero;
        passengerObject.transform.localEulerAngles = new Vector3(0f, 90f, 0f);

        currentPassengerCount++;

        return true;
    }

    public void MoveTo(Vector3 targetPosition, float speed)
    {
        if (!isMoving)
            StartCoroutine(MoveCoroutine(targetPosition, speed));
    }

    private IEnumerator MoveCoroutine(Vector3 targetPosition, float speed)
    {
        Vector3 startPosition = transform.position;
        float totalDistance = Vector3.Distance(startPosition, targetPosition);

        if (totalDistance <= 0.001f)
        {
            transform.position = targetPosition;
            transform.localScale = initialScale;
            yield break;
        }

        isMoving = true;

        float brakeDistance = Mathf.Clamp(totalDistance * brakeDistanceRatio, minBrakeDistance, maxBrakeDistance);
        brakeDistance = Mathf.Min(brakeDistance, totalDistance * 0.75f);

        float brakeStartProgress = 1f - (brakeDistance / totalDistance);
        float totalDuration = totalDistance / Mathf.Max(speed * travelSpeedMultiplier, 0.01f);
        float clampedCruiseRatio = Mathf.Clamp(cruiseTimeRatio, 0.15f, 0.8f);
        float cruiseDuration = totalDuration * clampedCruiseRatio;
        float brakeDuration = Mathf.Max(totalDuration - cruiseDuration, 0.08f);
        float elapsed = 0f;

        while (elapsed < totalDuration)
        {
            elapsed += Time.deltaTime;

            if (elapsed < cruiseDuration)
            {
                float cruiseT = cruiseDuration <= 0f ? 1f : elapsed / cruiseDuration;
                float progress = Mathf.Lerp(0f, brakeStartProgress, cruiseT);
                transform.position = Vector3.LerpUnclamped(startPosition, targetPosition, progress);
                transform.localScale = initialScale;
            }
            else
            {
                float brakeT = Mathf.Clamp01((elapsed - cruiseDuration) / brakeDuration);
                float easedBrakeT = 1f - Mathf.Pow(1f - brakeT, 3f);
                float progress = Mathf.Lerp(brakeStartProgress, 1f, easedBrakeT);
                transform.position = Vector3.LerpUnclamped(startPosition, targetPosition, progress);
                ApplyBrakeSquash(brakeT);
            }

            yield return null;
        }

        transform.position = targetPosition;
        transform.localScale = initialScale;
        isMoving = false;
    }

    private void ApplyBrakeSquash(float normalizedBrakeTime)
    {
        if (initialScale == Vector3.zero)
            return;

        float squashBlend = Mathf.Sin(normalizedBrakeTime * Mathf.PI);

        transform.localScale = new Vector3(
            initialScale.x * (1f - brakeSquashX * squashBlend),
            initialScale.y,
            initialScale.z * (1f + brakeStretchZ * squashBlend)
        );
    }
}
