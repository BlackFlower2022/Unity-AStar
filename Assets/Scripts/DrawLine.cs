using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawLine : MonoBehaviour
{
    #region 常量
    private const int MaxNum_X = 30; //X坐标轴横向（第一象限）
    public const int MaxNum_Y = 16;  //Y坐标轴纵向（第一象限）
    #endregion

    #region 字段
    float MapWidth;//地图宽
    float MapHeight;//地图高

    float TileWidth;//格子宽
    float TileHeight;//格子高

    private int[,] m_Map = new int[MaxNum_X, MaxNum_Y];//地图信息
    List<Tile> UpPath = new List<Tile>(); //上路径
    List<Tile> DownPath = new List<Tile>(); //下路径
    List<Tile> LeftPath = new List<Tile>(); //左路径
    List<Tile> RightPath = new List<Tile>(); //右路径
    #endregion

    #region 属性
    public static int MaxIndex_X
    {
        get { return MaxNum_X - 1; }
    }
    public static int MaxIndex_Y
    {
        get { return MaxNum_Y - 1; }
    }
    #endregion

    #region 方法
    void OnDrawGizmos()
    {
        DrawMap();
        DrawLoads();
    }

    //绘制底板地图
    private void DrawMap()
    {
        //计算地图和格子大小
        CalculateSize();

        //格子颜色
        Gizmos.color = Color.green;

        //绘制行
        for (int row = 0; row <= MaxNum_Y; row++)
        {
            Vector2 from = new Vector2(-MapWidth / 2, -MapHeight / 2 + row * TileHeight);
            Vector2 to = new Vector2(-MapWidth / 2 + MapWidth, -MapHeight / 2 + row * TileHeight);
            Gizmos.DrawLine(from, to);
        }

        //绘制列
        for (int col = 0; col <= MaxNum_X; col++)
        {
            Vector2 from = new Vector2(-MapWidth / 2 + col * TileWidth, MapHeight / 2);
            Vector2 to = new Vector2(-MapWidth / 2 + col * TileWidth, -MapHeight / 2);
            Gizmos.DrawLine(from, to);
        }
    }

    //绘制所有-寻路路线
    private void DrawLoads()
    {
        DrawLoad(UpPath);
        DrawLoad(DownPath);
        DrawLoad(LeftPath);
        DrawLoad(RightPath);      
    }

    //绘制单个-寻路路线
    private void DrawLoad(List<Tile> load)
    {
        if (load == null)
            return;

        Gizmos.color = Color.red;
        for (int i = 0; i < load.Count; i++)
        {
            if (load.Count > 1 && i != 0)
            {
                Vector3 from = GetPosition(load[i - 1]);
                Vector3 to = GetPosition(load[i]);
                Gizmos.DrawLine(from, to);
            }
        }
    }

    //初始化所有路径
    public void Init_Paths()
    {
        Init_Map();

        Tile centerPoint = Range_CenterPoint(7,4);
        Range_Points(centerPoint);
    }

    //初始化地图信息
    private void Init_Map()
    {
        Debug.Log("map-x：" + MaxNum_X);
        Debug.Log("map-y：" + MaxNum_Y);
        for (int i = 0; i < MaxNum_X; i++)
        {
            for (int j = 0; j < MaxNum_Y; j++)
            {
                m_Map[i, j] = 0;
            }
        }
        Remove_FourCorners();
    }

    //移除地图四角
    private void Remove_FourCorners()
    {
        m_Map[0, 0] = -1;
        m_Map[MaxIndex_X, 0] = -1;
        m_Map[0, MaxIndex_Y] = -1;
        m_Map[MaxIndex_X, MaxIndex_Y] = -1;
    }

    //随机中心区域的点
    private Tile Range_CenterPoint(int disX,int disY)
    {
        int center_x = Random.Range(disX, MaxIndex_X - disX);
        int center_y = Random.Range(disY, MaxIndex_Y - disY); Debug.Log("中心点：" + center_x + "," + center_y);
        Tile centerPoint = new Tile(center_x, center_y);

        return centerPoint;
    }

    //随机四边上的点
    private void Range_Points(Tile centerPoint)
    {
        Tile upPoint = Range_Point(MaxIndex_Y, true); Debug.Log("上起点：" + upPoint.X + "," + upPoint.Y);
        UpPath = Init_Path(upPoint, centerPoint);

        Tile downPoint = Range_Point(0, true); Debug.Log("下起点：" + downPoint.X + "," + downPoint.Y);
        DownPath = Init_Path(downPoint, centerPoint);

        Tile leftPoint = Range_Point(0, false); Debug.Log("左起点：" + leftPoint.X + "," + leftPoint.Y);
        LeftPath = Init_Path(leftPoint, centerPoint);

        Tile rightPoint = Range_Point(MaxIndex_X, false); Debug.Log("右起点：" + rightPoint.X + "," + rightPoint.Y);
        RightPath = Init_Path(rightPoint, centerPoint);
    }

    //随机固定行或固定列上的点
    private Tile Range_Point(int fix, bool isSelectX)//isSelectX为true,fix为固定Y  isSelectX为false,fix为固定X
    {
        int x = -1, y = -1;
        bool isMeet = false;

        while (!isMeet)
        {
            if (isSelectX)
            {
                x = Random.Range(0, MaxNum_X);
                y = fix;
            }
            else
            {
                y = Random.Range(0, MaxNum_Y);
                x = fix;
            }
            isMeet = m_Map[x, y] == 0;
        }

        return new Tile(x, y);
    }

    //初始化路径
    private List<Tile> Init_Path(Tile startPoint, Tile endPoint)
    {
        m_Map[startPoint.X, startPoint.Y] = -1;
        List<Tile> path = AStar.Instance.Execute(m_Map, startPoint.X, startPoint.Y, endPoint.X, endPoint.Y);
        DebugPathString(path);

        return path;
    }

    //显示路径信息
    private void DebugPathString(List<Tile> path)
    {
        string pathString = null;
        foreach (Tile tile in path)
        {
            pathString = pathString + "(" + tile.X + "," + tile.Y + ") ";
        }
        Debug.Log(pathString);
    }
    #endregion

    #region Unity回调
    private void Start()
    {
        Init_Paths();
    }
    #endregion

    #region 帮助方法
    //计算地图大小，格子大小
    void CalculateSize()
    {
        Vector3 leftDown = new Vector3(0, 0);
        Vector3 rightUp = new Vector3(1, 1);

        Vector3 p1 = Camera.main.ViewportToWorldPoint(leftDown);
        Vector3 p2 = Camera.main.ViewportToWorldPoint(rightUp);

        MapWidth = Mathf.Abs(p2.x - p1.x);
        MapHeight = Mathf.Abs(p2.y - p1.y);

        TileWidth = MapWidth / MaxNum_X;
        TileHeight = MapHeight / MaxNum_Y;
    }

    //获取格子中心点所在的世界坐标
    public Vector3 GetPosition(Tile t)
    {
        return new Vector3(
                -MapWidth / 2 + (t.X + 0.5f) * TileWidth,
                -MapHeight / 2 + (t.Y + 0.5f) * TileHeight,
                0
            );
    }
    #endregion
}
