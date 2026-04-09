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

    private List<Transform> seats = new List<Transform>();
    private int seatCapacity = 3;
    private int currentPassengerCount = 0;

    private ColorType busColor;
    private bool isMoving = false;

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

    public Transform GetFirstEmptySeat()
    {
        if (IsFull())
            return null;

        return seats[currentPassengerCount];
    }

    public bool AddPassenger(GameObject passengerObject)
    {
        if (IsFull())
            return false;

        Transform emptySeat = GetFirstEmptySeat();

        if (emptySeat == null)
            return false;

        passengerObject.transform.position = emptySeat.position;
        passengerObject.transform.rotation = emptySeat.rotation;
        passengerObject.transform.SetParent(emptySeat);

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
        isMoving = true;

        while (Vector3.Distance(transform.position, targetPosition) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPosition,
                speed * Time.deltaTime
            );

            yield return null;
        }

        transform.position = targetPosition;
        isMoving = false;
    }
}