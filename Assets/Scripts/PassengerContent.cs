using UnityEngine;

public class PassengerContent : TileContent
{
    [SerializeField] private Renderer passengerRenderer;

    [SerializeField] private Material redMaterial;
    [SerializeField] private Material blueMaterial;
    [SerializeField] private Material greenMaterial;
    [SerializeField] private Material yellowMaterial;

    private ColorType passengerColor;

    private Animator animator;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
    }


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
        SetRunningAnimation(true);

        Vector3 lookDir = targetPosition - transform.position;
        lookDir.y = 0;
        if (lookDir != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(lookDir);

        while (Vector3.Distance(transform.position, targetPosition) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
            yield return null;
        }
        transform.position = targetPosition;

        SetRunningAnimation(false);
        onComplete?.Invoke();
    }



    private void SetRunningAnimation(bool isRunning)
    {
        if (animator != null)
        {
            // Kullanıcı `int` tanımladım dediği için SetInteger kullanıyoruz. (Idle: 0, Run: 1 kabulüyle)
            animator.SetInteger("isRunning", isRunning ? 1 : 0);
        }

    }

    public void MoveAlongPath(System.Collections.Generic.List<Vector3> path, Vector3 finalTarget, float speed, System.Action onComplete = null)
    {
        StartCoroutine(MoveAlongPathRoutine(path, finalTarget, speed, onComplete));
    }

    private System.Collections.IEnumerator MoveAlongPathRoutine(System.Collections.Generic.List<Vector3> path, Vector3 finalTarget, float speed, System.Action onComplete)
    {
        SetRunningAnimation(true);

        if (path != null)
        {
            foreach (Vector3 waypoint in path)
            {
                // Yüzünü hedefe dön
                Vector3 lookDir = waypoint - transform.position;
                lookDir.y = 0;
                if (lookDir != Vector3.zero)
                    transform.rotation = Quaternion.LookRotation(lookDir);

                while (Vector3.Distance(transform.position, waypoint) > 0.05f)
                {
                    transform.position = Vector3.MoveTowards(transform.position, waypoint, speed * Time.deltaTime);
                    yield return null;
                }
                transform.position = waypoint;
            }
        }

        // Final hedefe dönsün ve yürüsün
        Vector3 finalLookDir = finalTarget - transform.position;
        finalLookDir.y = 0;
        if (finalLookDir != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(finalLookDir);

        while (Vector3.Distance(transform.position, finalTarget) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(transform.position, finalTarget, speed * Time.deltaTime);
            yield return null;
        }
        transform.position = finalTarget;

        SetRunningAnimation(false);
        onComplete?.Invoke();
    }
}