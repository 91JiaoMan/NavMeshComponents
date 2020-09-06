using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class nvFlush : MonoBehaviour
{
    private NavMeshData m_NvData_1;
    private NavMeshData m_NvData_2;
    private NavMeshBuildSettings defaultBuildSettings;
    private Bounds sceneBound_1;
    private Bounds sceneBound_2;

    private List<NavMeshBuildSource> sources = new List<NavMeshBuildSource>();

    public Button reflushNav;
    public Button reflushNavAsync;
    public Button reClear;
    public Text navText;

    private float time = 0.0f;

    private Thread th1;

    private void Awake()
    {
        m_NvData_1 = new NavMeshData();
        m_NvData_2 = new NavMeshData();
        defaultBuildSettings = NavMesh.GetSettingsByID(0);
        sceneBound_1 = new Bounds(Vector3.zero, new Vector3(80, 20, 20));
        sceneBound_2 = new Bounds(Vector3.zero, new Vector3(80, 20, 140));
    }

    void Start()
    {
        NavMesh.AddNavMeshData(m_NvData_1);

        AddSceneMeshToList(GameObject.Find("Plane"));
        AddSceneMeshToList(GameObject.Find("cubeGroupPrefab"));

        reflushNav.onClick.AddListener(flushNav);
        reflushNavAsync.onClick.AddListener(flushNavAsync);
        reClear.onClick.AddListener(clearNav);

        th1 = new Thread(addTime);
        th1.Start();

    }
    

    public void flushNav()
    {
        defaultBuildSettings.agentRadius = 0.025f;
        defaultBuildSettings.minRegionArea = 0.25f;

        float tt = time;
        NavMeshBuilder.UpdateNavMeshData(m_NvData_1, defaultBuildSettings, sources, sceneBound_1);     //刷新寻路的核心方法
        navText.text = "时间：" + (time - tt).ToString();
    }

    public void flushNavAsync()
    {
        StartCoroutine("reNavAsync");
    }

    private AsyncOperation async;
    float iett;
    private bool asyncKey = false;
    IEnumerator reNavAsync()
    {
        defaultBuildSettings.agentRadius = 0.025f;
        defaultBuildSettings.minRegionArea = 0.25f;

        iett = time;
        async = NavMeshBuilder.UpdateNavMeshDataAsync(m_NvData_2, defaultBuildSettings, sources, sceneBound_2);     //刷新寻路的核心方法
        asyncKey = true;
        yield return null;
    }

    private void Update()
    {
        if (asyncKey)
        {
            if (async.isDone)
            {
                navText.text = "时间：" + (time - iett).ToString();
                asyncKey = false;
            }

        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            NavMesh.RemoveAllNavMeshData();
            NavMesh.AddNavMeshData(m_NvData_1);

        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            NavMesh.RemoveAllNavMeshData();
            NavMesh.AddNavMeshData(m_NvData_2);
        }
    }

    public void clearNav()
    {
        float tt = time;
        NavMeshBuilder.UpdateNavMeshData(m_NvData_1, defaultBuildSettings, new List<NavMeshBuildSource>(), sceneBound_1);     //刷新寻路的核心方法
        NavMeshBuilder.UpdateNavMeshData(m_NvData_1, defaultBuildSettings, new List<NavMeshBuildSource>(), sceneBound_2);     //刷新寻路的核心方法
        navText.text = "时间：" + (time - tt).ToString();

    }

    /// <summary>
    /// 添加场景物体到List， sceneType：场景编号
    /// </summary>
    public void AddSceneMeshToList(GameObject prefabMesh)
    {
        NavMeshBuildSource s = new NavMeshBuildSource();
        s.shape = NavMeshBuildSourceShape.Mesh;
        MeshFilter mf = prefabMesh.GetComponent<MeshFilter>();
        if (mf == null || mf.sharedMesh == null)        //如果当前加载的预制体上没有Filter组件或者没有sharedMesh就不添加了
        {
            Debug.LogError("错误");
            return;
        }
        s.sourceObject = mf.sharedMesh;
        s.transform = prefabMesh.transform.localToWorldMatrix;
        s.area = 0;

        sources.Add(s);
    }

    private void addTime()
    {
        while (true)
        {
            time += 0.01f;
            Thread.Sleep(10);
        }
    }


    public void OnDestroy()
    {
        th1.Abort();
    }

}
