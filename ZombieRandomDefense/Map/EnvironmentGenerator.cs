using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentGenerator : MonoBehaviour
{
    // 현재 클래스에서 만들어지는 모든 객체들의 부모
    private GameObject m_rootGameObject;

    public GameObject GenerateEnvirnoment()
    {
        var mapGenerator = GetComponent<MapGenerator>();

        m_rootGameObject = new GameObject(MapGenerator.environmentGroupName);
        GenerateTree();
        return m_rootGameObject;
    }

    void GenerateTree()
    {

    }
}
