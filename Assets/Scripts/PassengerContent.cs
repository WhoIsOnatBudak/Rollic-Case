using UnityEngine;

public class PassengerContent : TileContent
{
    [SerializeField] private Renderer passengerRenderer;

    [SerializeField] private Material redMaterial;
    [SerializeField] private Material blueMaterial;
    [SerializeField] private Material greenMaterial;
    [SerializeField] private Material yellowMaterial;

    private ColorType passengerColor;


    public void SetColor(ColorType color)
    {
        passengerColor = color;
        ApplyMaterial();
    }

    public ColorType GetColor()
    {
        return passengerColor;
    }

    private void ApplyMaterial()
    {
        if (passengerRenderer == null)
            return;

        switch (passengerColor)
        {
            case ColorType.Red:
                passengerRenderer.material = redMaterial;
                break;
            case ColorType.Blue:
                passengerRenderer.material = blueMaterial;
                break;
            case ColorType.Green:
                passengerRenderer.material = greenMaterial;
                break;
            case ColorType.Yellow:
                passengerRenderer.material = yellowMaterial;
                break;
        }
    }

    public void MoveTo(Vector3 targetPosition, float speed, System.Action onComplete = null)
    {
        StartCoroutine(MoveRoutine(targetPosition, speed, onComplete));
    }

    private System.Collections.IEnumerator MoveRoutine(Vector3 targetPosition, float speed, System.Action onComplete)
    {
        while (Vector3.Distance(transform.position, targetPosition) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
            yield return null;
        }
        transform.position = targetPosition;
        onComplete?.Invoke();
    }

    public void MoveAlongPath(System.Collections.Generic.List<Vector3> path, Vector3 finalTarget, float speed, System.Action onComplete = null)
    {
        StartCoroutine(MoveAlongPathRoutine(path, finalTarget, speed, onComplete));
    }

    private System.Collections.IEnumerator MoveAlongPathRoutine(System.Collections.Generic.List<Vector3> path, Vector3 finalTarget, float speed, System.Action onComplete)
    {
        if (path != null)
        {
            foreach (Vector3 waypoint in path)
            {
                while (Vector3.Distance(transform.position, waypoint) > 0.05f)
                {
                    transform.position = Vector3.MoveTowards(transform.position, waypoint, speed * Time.deltaTime);
                    yield return null;
                }
                transform.position = waypoint;
            }
        }

        while (Vector3.Distance(transform.position, finalTarget) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(transform.position, finalTarget, speed * Time.deltaTime);
            yield return null;
        }
        transform.position = finalTarget;

        onComplete?.Invoke();
    }
}