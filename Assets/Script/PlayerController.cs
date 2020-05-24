using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(NavigationWalker))]
public class PlayerController : MonoBehaviour
{
    private NavigationWalker walker;

    [SerializeField]
    private GameObject mousePositionIndicator;

    private void Awake() {
        walker = GetComponent<NavigationWalker>();
    }

    private void Update() {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int pointing;

        if (NavigationManager.ins.FindBlockUnderPosition(mousePos, out pointing)) {
            if (!mousePositionIndicator.activeSelf)
                mousePositionIndicator.SetActive(true);

            mousePositionIndicator.transform.position =  NavigationManager.ins.EnvMap.CellToWorld(pointing) + new Vector3(0.5f, 0.5f);

            if (Input.GetMouseButtonDown(0)) {
                walker.TryWalkTo(pointing);
            }
        }
        else {
            if (mousePositionIndicator.activeSelf)
                mousePositionIndicator.SetActive(false);
        }
    }
}
