using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Manager : MonoBehaviour
{
    public static Manager ins;

    [SerializeField]
    private TileBase markerTile;

    [SerializeField]
    private Tilemap markerMap;

    private void Awake() {
        ins = this;
    }

    public void MarkTiles(Vector3Int[] tiles) {
        for (var i = 0; i < tiles.Length; i++) {
            markerMap.SetTile(tiles[i], markerTile);
        }
    }
}
