using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileGenerator : MonoBehaviour
{   
    // 현재 클래스에서 만들어지는 모든 객체들의 부모
    private GameObject m_rootGameObject;

    public GameObject GenerateTile()
    {
        var mapGenerator = GetComponent<MapGenerator>();

        m_rootGameObject = new GameObject(MapGenerator.tileGroupName);
        GenerateFieldTiles();
        GenerateSettingTiles();
        return m_rootGameObject;
    }

    private void GenerateFieldTiles()
    {
        for (int y = 0; y < MapGenerator.instance.mapSize.y; y++)
        {
            for (int x = 0; x < MapGenerator.instance.mapSize.x; x++)
            {
                var newTile = new GameObject(x + "," + y);
                newTile.transform.SetParent(m_rootGameObject.transform);
                newTile.transform.localScale = Vector3.one * MapGenerator.instance.mapCellSize;
                
                var boxCollider = newTile.AddComponent<BoxCollider>();
                boxCollider.center = new Vector3(0, .25f, 0);
                boxCollider.size = new Vector3(.85f, .25f, .85f);
                boxCollider.isTrigger = true;
                
                newTile.AddComponent<Tile>();

                var position = new Vector3(x, 5, y) * MapGenerator.instance.mapCellSize;
                if (Physics.Raycast(position, Vector3.down, out RaycastHit hit, 10, LayerMask.GetMask("Obstacle")))
                {
                    position.y = hit.point.y;
                }
                else
                {
                    position.y = 0;
                }
                newTile.transform.position = position;
            }
        }
    }

    private void GenerateSettingTiles()
    {

    }
}
