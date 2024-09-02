using UnityEngine;

public class ToggleTwoPrefabsVisibility : MonoBehaviour
{
    // References to the two prefabs or GameObjects in the hierarchy
    public GameObject prefab1;
    public GameObject prefab2;

    void Start()
    {
        // Make both prefabs invisible at the start
        if (prefab1 != null || prefab2 != null)
        {
            prefab1.SetActive(false);
            prefab2.SetActive(false);
        }
    }

    // This function toggles the visibility of both prefabs
    public void ToggleVisibility()
    {
        if (prefab1 != null && prefab2 != null)
        {
            bool anyPrefabNotVisible = !prefab1.activeSelf || !prefab2.activeSelf;

            // If either of the prefabs is not visible, make both visible
            if (anyPrefabNotVisible)
            {
                prefab1.SetActive(true);
                prefab2.SetActive(true);
            }
            else
            {
                // Otherwise, toggle both prefabs
                prefab1.SetActive(!prefab1.activeSelf);
                prefab2.SetActive(!prefab2.activeSelf);
            }
        }
        else
        {
            Debug.LogWarning("One or both prefabs are not assigned.");
        }
    }
}
