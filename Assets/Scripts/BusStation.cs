using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BusStation : MonoBehaviour
{
    [SerializeField] private Bus busPrefab;

    [Header("Bus Slots")]
    [SerializeField] private Transform currentBusSlot;
    [SerializeField] private Transform nextBusSlot;

    [Header("Movement")]
    [SerializeField] private float entranceOffsetX = 10f;
    [SerializeField] private float exitOffsetX = 10f;
    [SerializeField] private float filledBusExtraExitDistance = 4f;
    [SerializeField] private float moveSpeed = 8f;

    private Queue<BusData> busQueue = new Queue<BusData>();

    private Bus currentBus;
    private Bus nextBus;

    private bool isTransitioning = false;
    private bool isReady = false;

    public bool IsReady()
    {
        return isReady;
    }

    public bool IsTransitioning()
    {
        return isTransitioning;
    }

    public IEnumerator InitializeRoutine(BusData[] buses)
    {
        isReady = false;
        isTransitioning = false;

        busQueue.Clear();

        if (currentBus != null)
            Destroy(currentBus.gameObject);

        if (nextBus != null)
            Destroy(nextBus.gameObject);

        currentBus = null;
        nextBus = null;

        if (buses == null || buses.Length == 0)
        {
            Debug.LogWarning("Bus listesi boş");
            yield break;
        }

        for (int i = 0; i < buses.Length; i++)
        {
            busQueue.Enqueue(buses[i]);
        }

        yield return StartCoroutine(InitializeBusesRoutine());

        isReady = true;
    }

    private IEnumerator InitializeBusesRoutine()
    {
        currentBus = CreateBusOutside(currentBusSlot);
        float currentBusArrivalTime = 0f;
        if (currentBus != null)
        {
            currentBusArrivalTime = currentBus.GetMoveDuration(currentBusSlot.position, moveSpeed);
            currentBus.MoveTo(currentBusSlot.position, moveSpeed);
        }

        float nextBusArrivalTime = 0f;
        if (busQueue.Count > 0)
        {
            nextBus = CreateBusOutside(nextBusSlot);
            if (nextBus != null)
            {
                nextBusArrivalTime = nextBus.GetMoveDuration(nextBusSlot.position, moveSpeed);
                nextBus.MoveTo(nextBusSlot.position, moveSpeed);
            }
        }

        float waitTime = Mathf.Max(currentBusArrivalTime, nextBusArrivalTime);
        yield return new WaitForSeconds(waitTime + 0.1f);
    }

    private Bus CreateBusOutside(Transform slot)
    {
        if (busQueue.Count == 0)
            return null;

        BusData data = busQueue.Dequeue();

        Vector3 spawnPosition = slot.position - new Vector3(entranceOffsetX, 0f, 0f);

        Bus newBus = Instantiate(busPrefab, spawnPosition, slot.rotation, transform);
        newBus.Initialize(ParseColorType(data.color));

        return newBus;
    }

    private ColorType ParseColorType(string colorString)
    {
        switch (colorString)
        {
            case "Red":
                return ColorType.Red;
            case "Blue":
                return ColorType.Blue;
            case "Green":
                return ColorType.Green;
            case "Yellow":
                return ColorType.Yellow;
            default:
                Debug.LogWarning("Unknown bus color: " + colorString + ", defaulting to Red");
                return ColorType.Red;
        }
    }

    public Bus GetCurrentBus()
    {
        return currentBus;
    }

    public Bus GetNextBus()
    {
        return nextBus;
    }

    public void OnCurrentBusFull()
    {
        if (!isReady)
            return;

        if (isTransitioning)
            return;

        if (currentBus == null)
            return;

        if (IsFinalBusFilled())
            LevelManager.Instance?.OnFinalBusFilled();

        StartCoroutine(HandleBusTransition());
    }

    private bool IsFinalBusFilled()
    {
        return currentBus != null && currentBus.IsFull() && nextBus == null && busQueue.Count == 0;
    }

    private IEnumerator HandleBusTransition()
    {
        isTransitioning = true;
        isReady = false;

        PowerUpManager.Instance?.ClearUndoStack();

        Bus oldCurrent = currentBus;
        Bus oldNext = nextBus;

        float totalExitDistance = exitOffsetX + filledBusExtraExitDistance;
        Vector3 exitPosition = currentBusSlot.position + new Vector3(totalExitDistance, 0f, 0f);
        float exitTime = 0f;
        float currentBusArrivalTime = 0f;

        if (oldCurrent != null)
        {
            exitTime = oldCurrent.GetMoveDuration(exitPosition, moveSpeed);
            oldCurrent.MoveTo(exitPosition, moveSpeed);
        }

        if (oldNext != null)
        {
            currentBusArrivalTime = oldNext.GetMoveDuration(currentBusSlot.position, moveSpeed);
            oldNext.MoveTo(currentBusSlot.position, moveSpeed);
        }

        currentBus = oldNext;

        if (busQueue.Count > 0)
        {
            nextBus = CreateBusOutside(nextBusSlot);

            if (nextBus != null)
                nextBus.MoveTo(nextBusSlot.position, moveSpeed);
        }
        else
        {
            nextBus = null;
        }

        if (currentBus == null)
        {
            yield return new WaitForSeconds(exitTime + 0.1f);

            if (oldCurrent != null)
                Destroy(oldCurrent.gameObject);

            isTransitioning = false;
            LevelManager.Instance?.CheckWinCondition();
            GameOver();
        }
        else
        {
            if (oldCurrent != null)
                StartCoroutine(DestroyBusAfterDelay(oldCurrent, exitTime + 0.1f));

            yield return new WaitForSeconds(currentBusArrivalTime + 0.05f);

            isReady = true;
            isTransitioning = false;
            LevelManager.Instance?.OnImportantActionComplete();
        }
    }

    private IEnumerator DestroyBusAfterDelay(Bus busToDestroy, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (busToDestroy != null)
            Destroy(busToDestroy.gameObject);
    }

    private void GameOver()
    {
        LevelManager.Instance?.GameOver();
    }
}
