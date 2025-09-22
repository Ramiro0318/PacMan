using UnityEngine;
using UnityEngine.Tilemaps;

public class PelletGenerator : MonoBehaviour
{
    public Tilemap pelletTilemap;
    public Sprite pelletSprite;

    void Start()
    {
        
        Invoke("GeneratePelletsFromTilemap", 0.1f);
    }

    void GeneratePelletsFromTilemap()
    {
        if (pelletTilemap == null)
        {
            Debug.LogError("Pellet Tilemap no está asignado!");
            return;
        }

       
        GameObject pelletsContainer = new GameObject("Pellets Container");
        pelletsContainer.transform.SetParent(null); // ¡NO como hijo de Elements!
        pelletsContainer.transform.position = Vector3.zero;

        foreach (var position in pelletTilemap.cellBounds.allPositionsWithin)
        {
            if (pelletTilemap.HasTile(position))
            {
                CreatePelletAtPosition(position, pelletsContainer.transform);
            }
        }

        if (pelletTilemap.TryGetComponent<TilemapRenderer>(out var renderer))
            renderer.enabled = false;

        if (pelletTilemap.TryGetComponent<TilemapCollider2D>(out var collider))
            collider.enabled = false;
    }

    void CreatePelletAtPosition(Vector3Int cellPosition, Transform parent)
    {
        Vector3 worldPosition = pelletTilemap.GetCellCenterWorld(cellPosition);

        GameObject pellet = new GameObject("Pellet");
        pellet.transform.position = worldPosition;
        pellet.transform.SetParent(parent);

        
        SpriteRenderer sr = pellet.AddComponent<SpriteRenderer>();
        sr.sprite = pelletSprite;
        sr.sortingOrder = 1;

        
        CircleCollider2D col = pellet.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.1f;  
        col.offset = Vector2.zero;

        
        Pellet p = pellet.AddComponent<Pellet>();
        p.points = 10;
    }
}