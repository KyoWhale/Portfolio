using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class RouteGenerator : MonoBehaviour
{
    enum Direction { Left, Down, Right, Up, DIRECTION_MAX = 3 }
    private const int MAX_OBSTACLE_LENGTH = 4;

    // 현재 클래스에서 만들어지는 모든 객체들의 부모
    private GameObject m_rootGameObject;

    private HashSet<Vector2Int> m_pathCells = new HashSet<Vector2Int>();

    /// <summary>
    /// List(셀들의 모음, 첫 셀으로부터의 나머지 셀들로 가는 방향) <br/>
    /// </summary>
    private List<(List<Vector2Int>, Direction)>[] m_obstaclesByLength;

    /// <summary>
    /// 각 행, 열마다 장애물이 있는지 없는지 레페런스를 통해 확인할 수 있음
    /// null이면 길, GameObject가 있다면 장애물
    /// </summary>
    private GameObject[,] m_obstaclesByCell;

    /// <summary>
    /// SizeXBy1Prefabs를 담는 임시 Nested Array <br/>
    /// 생성된 장애물에 대응대는 프리팹을 생성해야하는데, 이를 중첩 for문에서 처리하기 쉽게하기 위해 사용  <br/>
    /// 행 : 장애물의 길이에 대응됨  <br/>
    /// 열 : 프리팹의 개수만큼 존재함  <br/>
    /// prefabPool[장애물길이][Random] 형식으로 프리팹을 꺼내서 사용
    /// </summary>
    private GameObject[][] m_prefabPool; 

    // Minimap
    private GameObject[,] m_miniMapCubes;

    // Resources
    public Material[] indestructibleMaterials;
    public GameObject[] size1By1Prefabs;
    public GameObject[] size2By1Prefabs;
    public GameObject[] size3By1Prefabs;
    public GameObject[] size4By1Prefabs;

    public GameObject GenerateRoute()
    {
        m_rootGameObject = new GameObject(MapGenerator.routeGroupName);
        GenerateStage();
        return m_rootGameObject;
    }

    public void GenerateStage()
    {
        m_pathCells = new HashSet<Vector2Int>();
        for (int i = 0; i < MapGenerator.instance.pathCount; i++)
        {
            MakePath();
        }
        GetListOfObstaclePosition();
        InstatiateObstacles();
        MakeMinimap();
    }

    public void GenerateNavMesh()
    {
        AttachNavMeshSurfaceToGround();
        AttachNavMeshLinkToObstacle();
    }

    private void MakePath()
    {
        HashSet<Vector2Int> currentPathCells = new HashSet<Vector2Int>();

        int x, y;
        y = MapGenerator.instance.mapSize.y-1;
        x = Random.Range(0, MapGenerator.instance.mapSize.x);

        while(y >= 0)
        {
            currentPathCells.Add(new Vector2Int(x,y));
            
            bool moveValid = false;
            while(!moveValid) 
            {
                Direction direction = (Direction)Random.Range(0,(int)Direction.DIRECTION_MAX);
                switch (direction)
                {
                    case Direction.Left:
                    if (x > 0 && currentPathCells.Contains(new Vector2Int(x-1,y)) == false) {
                        x--;
                        moveValid = true;
                    }
                    break;
                    case Direction.Down:
                    if (y >= 0 && currentPathCells.Contains(new Vector2Int(x,y-1)) == false) {
                        y--;
                        moveValid = true;
                    }
                    break;
                    case Direction.Right:
                    if (x < MapGenerator.instance.mapSize.x-1 && currentPathCells.Contains(new Vector2Int(x+1,y)) == false) {
                        x++;
                        moveValid = true;
                    }
                    break;
                }
            }
        }

        foreach(var cell in currentPathCells)
        {
            m_pathCells.Add(cell);
        }
    }

    private void GetListOfObstaclePosition()
    {
        HashSet<Vector2Int> blockingCells = FindCellsShouldBlock();
        HashSet<Vector2Int> finishedCells = new HashSet<Vector2Int>();

        m_obstaclesByLength = new List<(List<Vector2Int>, Direction)>[MAX_OBSTACLE_LENGTH];
        for (int i = 0; i < MAX_OBSTACLE_LENGTH; i++)
        {
            m_obstaclesByLength[i] = new List<(List<Vector2Int>, Direction)>();
        }

        foreach (var blockingCell in blockingCells)
        {
            if (finishedCells.Contains(blockingCell))
            {
                continue;
            }

            int obstacleLength = GetConnectedCells(blockingCell, out List<Vector2Int> connectedCells, out Direction direction, finishedCells);
            m_obstaclesByLength[obstacleLength - 1].Add((connectedCells, direction));

            foreach (var connectedCell in connectedCells)
            {
                finishedCells.Add(connectedCell);
            }
        }
    }

    private HashSet<Vector2Int> FindCellsShouldBlock()
    {
        HashSet<Vector2Int> cellsShouldBlock = new HashSet<Vector2Int>();

        for (int y = 0; y < MapGenerator.instance.mapSize.y; y++)
        {
            for (int x = 0; x < MapGenerator.instance.mapSize.x; x++)
            {
                if (!m_pathCells.Contains(new Vector2Int(x, y))) {
                    cellsShouldBlock.Add(new Vector2Int(x, y));
                }
            }
        }

        return cellsShouldBlock;
    }

    /// <summary>
    /// 현재 막아야하는 셀로부터 네(동 서 남 북) 방향 중 가장 길게 연결된 셀들과 방향을 반환
    /// </summary>
    private int GetConnectedCells(Vector2Int startCell, out List<Vector2Int> resultCells, out Direction resultDirection, HashSet<Vector2Int> usedCells)
    {
        int[] dx = {1,0,-1,0};
        int[] dy = {0,1,0,-1};

        resultCells = new List<Vector2Int>();

        int maxLength = 1;
        resultDirection = Direction.Left;

        for (int i = 0; i < 4; i++)
        {
            int nextX = startCell.x + dx[i];
            int nextY = startCell.y + dy[i];
            Direction curDirection;
            if (i % 2 == 0) 
                curDirection = Direction.Left;
            else 
                curDirection = Direction.Up;

            List<Vector2Int> temp = new List<Vector2Int>();
            int curLength = 1;
            
            while (curLength < MAX_OBSTACLE_LENGTH
                    && 0 <= nextX && nextX < MapGenerator.instance.mapSize.x 
                    && 0 <= nextY && nextY < MapGenerator.instance.mapSize.y
                    && !usedCells.Contains(new Vector2Int(nextX, nextY))
                    && !m_pathCells.Contains(new Vector2Int(nextX, nextY))) 
            {

                temp.Add(new Vector2Int(nextX, nextY));
                curLength++;

                nextX += dx[i];
                nextY += dy[i];
            }

/* 
    TODO: 현재는 만들 수 있는 가장 큰 크기의 장애물만 만드는데
    만들 수 있는 길이 중에 랜덤으로 길이를 정하고 그걸 놓는 방식이 더 다채롭게 배치할 듯
    어차피 지금은 그냥 배치 기능 자체를 만드는게 우선이니 내버려두겠음
*/
            if (curLength < maxLength)
                continue;
            else if (curLength == maxLength) {
                if (Random.Range(0,2) == 1) 
                    continue;
            }

            resultCells.Clear();
            foreach (var cell in temp) {
                resultCells.Add(cell);
            }

            maxLength = curLength;
            resultDirection = curDirection;
        }

        resultCells.Insert(0, startCell);

        // Debug.Log(maxLength);
        // string logString = "";
        // foreach (var cell in resultCells)
        // {
        //     logString += cell.ToString() + " ";
        // }
        // Debug.Log(logString);

        return maxLength;
    }

    private void InstatiateObstacles()
    {
        m_prefabPool = new GameObject[4][] {size1By1Prefabs, size2By1Prefabs, size3By1Prefabs, size4By1Prefabs};
        m_obstaclesByCell = new GameObject[MapGenerator.instance.mapSize.y, MapGenerator.instance.mapSize.x];

        for (int i = 0; i < MAX_OBSTACLE_LENGTH; i++)
        {
            foreach (var (cells, direction) in m_obstaclesByLength[i])
            {
                Vector2Int obstacleStartPosition = cells[0];
                Vector3 obstaclePosition = new Vector3(obstacleStartPosition.x, 0, obstacleStartPosition.y);
                Quaternion obstacleRotation;
                switch (direction)
                {
                    default:
                    case Direction.Up:
                        obstacleRotation = Quaternion.identity;
                        break;
                    case Direction.Down:
                        obstacleRotation = Quaternion.Euler(0, 180, 0);
                        break;
                    case Direction.Left:
                        obstacleRotation = Quaternion.Euler(0, 90, 0);
                        break;
                    case Direction.Right:
                        obstacleRotation = Quaternion.Euler(0, -90, 0);
                        break;
                }

                int obstacleLength = m_prefabPool[i].Length;
                GameObject obstacle = Instantiate(m_prefabPool[i][Random.Range(0, obstacleLength)], obstaclePosition * MapGenerator.instance.mapCellSize, obstacleRotation);
                obstacle.transform.SetParent(m_rootGameObject.transform);
                obstacle.isStatic = true;

                MeshRenderer obstacleMesh = obstacle.GetComponentInChildren<MeshRenderer>();
                obstacleMesh.sharedMaterial = indestructibleMaterials[Random.Range(0, indestructibleMaterials.Length)];

                foreach (var cell in cells)
                {
                    m_obstaclesByCell[cell.y, cell.x] = obstacle;
                }
            }
        }
    }

    private void AttachNavMeshSurfaceToGround()
    {
        GameObject ground = GameObject.Find(MapGenerator.groundName);
        if (ground == null)
        {
            Debug.LogError("No ground on scene");
            return;
        }

        var navMeshSurface = ground.GetComponent<NavMeshSurface>();
        if (navMeshSurface == null)
        {
            var buildSettings = NavMesh.GetSettingsByIndex(0);
            navMeshSurface = ground.AddComponent<NavMeshSurface>();
            navMeshSurface.agentTypeID = buildSettings.agentTypeID;
            navMeshSurface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;

            buildSettings = NavMesh.GetSettingsByIndex(1);
            navMeshSurface = ground.AddComponent<NavMeshSurface>();
            navMeshSurface.agentTypeID = buildSettings.agentTypeID;
            navMeshSurface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
        }
    }

    private void AttachNavMeshLinkToObstacle()
    {
        int[] dx = {1,0,-1,0};
        int[] dy = {0,1,0,-1};

        var navMeshSurfaces = GameObject.FindObjectsOfType<NavMeshSurface>();
        NavMeshSurface walkClimbNavMesh = navMeshSurfaces[0];
        foreach (var navMeshSurface in navMeshSurfaces)
        {
            navMeshSurface.RemoveData();
            if (navMeshSurface.agentTypeID == 1)
            {
                walkClimbNavMesh = navMeshSurface;
                continue;
            }
            navMeshSurface.BuildNavMesh();
        }

        var buildSettings = NavMesh.GetSettingsByIndex(1);
        var agentRadius = buildSettings.agentRadius;

        for (int y = 0; y < MapGenerator.instance.mapSize.y; y++)
        {
            for (int x = 0; x < MapGenerator.instance.mapSize.x; x++)
            {
                if (!m_pathCells.Contains(new Vector2Int(x, y)))
                {
                    continue;
                }

                for (int d = 0; d < 4; d++)
                {
                    int nx = x + dx[d];
                    int ny = y + dy[d];

                    if (nx < 0 || nx >= MapGenerator.instance.mapSize.x || ny < 0 || ny >= MapGenerator.instance.mapSize.y)
                    {
                        continue;
                    }

                    if (m_pathCells.Contains(new Vector2Int(nx, ny)))
                    {
                        continue;
                    }

                    var navMeshLink = new GameObject("NavMeshLink").AddComponent<NavMeshLink>();
                    navMeshLink.transform.SetParent(m_obstaclesByCell[ny,nx].transform);
                    navMeshLink.transform.position = new Vector3(nx, 0, ny) * MapGenerator.instance.mapCellSize;
                    navMeshLink.agentTypeID = walkClimbNavMesh.agentTypeID;
                    navMeshLink.width = MapGenerator.instance.mapCellSize - agentRadius * 2;
                    navMeshLink.costModifier = 2;

                    var obstacleHeight = new Vector3(0, 1, 0) * MapGenerator.instance.mapCellSize;
                    navMeshLink.startPoint = obstacleHeight + new Vector3(-dx[d], 0, -dy[d]) * agentRadius * 2;
                    navMeshLink.endPoint = new Vector3(-dx[d], 0, -dy[d]) * (1+agentRadius * 2);
                }
            }
        }

        walkClimbNavMesh.BuildNavMesh();
    }

    private void MakeMinimap()
    {
        Material blueMaterial = new Material(Shader.Find("Transparent/Diffuse"));
        Material greenMaterial = new Material(Shader.Find("Transparent/Diffuse"));
        blueMaterial.color = Color.blue;
        greenMaterial.color = Color.green;

        var minimapParent = new GameObject("MiniMap");
        minimapParent.transform.SetParent(m_rootGameObject.transform);
        m_miniMapCubes = new GameObject[MapGenerator.instance.mapSize.y, MapGenerator.instance.mapSize.x];
        for (int y = 0; y < MapGenerator.instance.mapSize.y; y++) 
        {
            for (int x = 0; x < MapGenerator.instance.mapSize.x; x++) 
            {
                m_miniMapCubes[y,x] = GameObject.CreatePrimitive(PrimitiveType.Cube);
                m_miniMapCubes[y,x].transform.SetParent(minimapParent.transform);
                m_miniMapCubes[y,x].name = y + ", " + x;
            }
        }

        for (int y = 0; y < MapGenerator.instance.mapSize.y; y++)
        {
            for (int x = 0; x < MapGenerator.instance.mapSize.x; x++)
            {
                var meshRenderer = m_miniMapCubes[y,x].GetComponent<MeshRenderer>();
                m_miniMapCubes[y,x].transform.position = new Vector3(x, 0, y) + 
                    new Vector3(MapGenerator.instance.minimapPosition.x, 0, MapGenerator.instance.minimapPosition.y) +
                    new Vector3(0, 0, -MapGenerator.instance.mapSize.y);
                
                if (m_pathCells.Contains(new Vector2Int(x,y))) 
                {
                    meshRenderer.sharedMaterial = blueMaterial;
                }
                else 
                {
                    meshRenderer.sharedMaterial = greenMaterial;
                }
            }
        }
    }
}
