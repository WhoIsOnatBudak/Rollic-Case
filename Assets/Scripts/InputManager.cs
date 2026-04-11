using UnityEngine;

public class InputManager : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                PassengerContent passenger = hit.collider.GetComponentInParent<PassengerContent>();
                if (passenger == null)
                {
                    Tile tile = hit.collider.GetComponentInParent<Tile>();
                    if (tile != null && tile.GetContent() is PassengerContent)
                    {
                        passenger = (PassengerContent)tile.GetContent();
                    }
                }

                if (passenger != null)
                {
                    if (LevelManager.Instance != null)
                    {
                        LevelManager.Instance.OnPassengerClicked(passenger);
                    }
                }
            }
        }
    }
}
