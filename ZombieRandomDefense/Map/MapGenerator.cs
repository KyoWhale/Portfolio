using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RouteGenerator))]
[RequireComponent(typeof(EnvironmentGenerator))]
[RequireComponent(typeof(FieldCreatureGenerator))]
[RequireComponent(typeof(TileGenerator))]
public class MapGenerator : Singleton<MapGenerator>
{
    public bool autoUpdate = false;

    [Header("Map Setting")]
    [SerializeField] Vector2Int m_mapSize = new Vector2Int(5, 5);
    [Tooltip("만들어져야하는 길의 개수")]
    [SerializeField] [Range(1, 50)] int m_pathCount = 2;
    [Tooltip("장애물의 너비")]
    [SerializeField][Range(1, 3)] int m_mapCellSize = 2;

    [Header("Ground Settings")]
    [SerializeField] int totalMapSizeOffset = 5;
    [SerializeField] Material m_groundMaterial;

    [Header("MiniMap")]
    [SerializeField] Vector2Int m_minimapPosition = new Vector2Int(0, -20);

    // Generator Components
    private RouteGenerator m_route;
    private EnvironmentGenerator m_environment;
    private FieldCreatureGenerator m_fieldCreature;
    private TileGenerator m_tile;

    // Generated GameObjects
    private GameObject m_ground;
    private GameObject m_routeGroup;
    private GameObject m_environmentGroup;
    private GameObject m_fieldCreatureGroup;
    private GameObject m_tileGroup;

    public Vector2Int mapSize { get => m_mapSize; }
    public int pathCount { get => m_pathCount; }
    public int mapCellSize { get => m_mapCellSize; }
    public Vector2Int minimapPosition { get => m_minimapPosition; }

    // const string
    public const string groundName = "GeneratedGround";
    public const string routeGroupName = "GeneratedRouteGroup";
    public const string environmentGroupName = "GeneratedEnvironmentGroup";
    public const string fieldCreatureGroupName = "GeneratedFieldCreatureGroup";
    public const string tileGroupName = "GeneratedTileGroup";

    private void Initialize()
    {
        GetReferenceOfGenerators();
        if (CheckPreviousGeneration())
        {
            DestoryPreviousGeneration();
        }
    }

    public void GenerateAll()
    {
        GenerateAllWithoutNavMesh();
        GenerateNavMesh();
        InjectWorldCamera();
    }

    public void GenerateAll(Vector2Int mapSize, int mapCellSize)
    {
        m_mapSize = mapSize;
        m_mapCellSize = mapCellSize;
        GenerateAll();
    }

    /// <summary>
    /// NavMesh 빌드를 제외함으로 인해 처리 시간이 대폭 감소함 <br/>
    /// 특히 AutoUpdate로 지형 재생성이 실시간으로 되면 멈추기 때문에 따로 처리함 <br/>
    /// </summary>
    public void GenerateAllWithoutNavMesh()
    {
        Initialize();
        CreateGround();
        m_routeGroup = m_route.GenerateRoute();
        m_environmentGroup = m_environment.GenerateEnvirnoment();
        m_fieldCreatureGroup = m_fieldCreature.GenerateCreatures();
        m_tileGroup = m_tile.GenerateTile();
        InjectWorldCamera();
    }

    public void GenerateNavMesh()
    {
        if (m_routeGroup == null)
        {
            return;
        }

        m_route.GenerateNavMesh();
    }

    private void GetReferenceOfGenerators()
    {
        if (m_route == null)
        {
            m_route = GetComponent<RouteGenerator>();
        }
        if (m_environment == null)
        {
            m_environment = GetComponent<EnvironmentGenerator>();
        }
        if (m_fieldCreature == null)
        {
            m_fieldCreature = GetComponent<FieldCreatureGenerator>();
        }
        if (m_tile == null)
        {
            m_tile = GetComponent<TileGenerator>();
        }
    }

    private bool CheckPreviousGeneration()
    {
        bool hasAny = false;

        if (m_ground || (m_ground = GameObject.Find(groundName)))
        {
            hasAny = true;
        }
        if (m_routeGroup || (m_routeGroup = GameObject.Find(routeGroupName)))
        {
            hasAny = true;
        }
        if (m_environmentGroup || (m_environmentGroup = GameObject.Find(environmentGroupName)))
        {
            hasAny = true;
        }
        if (m_fieldCreatureGroup || (m_fieldCreatureGroup = GameObject.Find(fieldCreatureGroupName)))
        {
            hasAny = true;
        }
        if (m_tileGroup || (m_tileGroup = GameObject.Find(tileGroupName)))
        {
            hasAny = true;
        }

        return hasAny;
    }

    private void DestoryPreviousGeneration()
    {
        if (m_ground)
        {
            DestroyImmediate(m_ground);
        }
        if (m_routeGroup)
        {
            DestroyImmediate(m_routeGroup);
        }
        if (m_environmentGroup)
        {
            DestroyImmediate(m_environmentGroup);
        }
        if (m_fieldCreatureGroup)
        {
            DestroyImmediate(m_fieldCreatureGroup);
        }
        if (m_tileGroup)
        {
            DestroyImmediate(m_tileGroup);
        }
    }

    private void CreateGround()
    {
        m_ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        m_ground.layer = LayerMask.NameToLayer("Ground");
        m_ground.name = groundName;
        m_ground.isStatic = true;

        Vector3 newPosition = Vector3.zero;
        newPosition.x = m_mapSize.x * totalMapSizeOffset * 3;
        newPosition.z = m_mapSize.y * totalMapSizeOffset * 3;
        m_ground.transform.position = newPosition;

        Vector3 newScale = Vector3.one;
        newScale.x = m_mapSize.x * totalMapSizeOffset;
        newScale.z = m_mapSize.y * totalMapSizeOffset;
        m_ground.transform.localScale = newScale;

        var meshRenderer = m_ground.GetComponent<MeshRenderer>();
        if (meshRenderer == null)
        {
            meshRenderer = m_ground.AddComponent<MeshRenderer>();
        }
        if (m_groundMaterial)
        {
            meshRenderer.sharedMaterial = m_groundMaterial;
        }
        else
        {
            meshRenderer.sharedMaterial.color = new Color(0, 0, 0, 0);
        }
    }

    private void InjectWorldCamera()
    {
        var worldCamera = GameObject.FindObjectOfType<WorldCamera>();
        if (worldCamera == null)
        {
            var newGameObject = new GameObject("World Camera");
            worldCamera = newGameObject.AddComponent<WorldCamera>();
        }
    }
}
