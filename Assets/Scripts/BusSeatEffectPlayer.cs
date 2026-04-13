using UnityEngine;

public class BusSeatEffectPlayer : MonoBehaviour
{
    [SerializeField] private ParticleSystem teleportEffectPrefab;
    [SerializeField] private Vector3 positionOffset = new Vector3(0f, 0.3f, 0f);

    public void PlayAtSeat(Transform seat)
    {
        if (teleportEffectPrefab == null || seat == null)
            return;

        ParticleSystem effect = Instantiate(
            teleportEffectPrefab,
            seat.position + positionOffset,
            Quaternion.identity
        );

        effect.Play();
        Destroy(effect.gameObject, 2f);
    }
}