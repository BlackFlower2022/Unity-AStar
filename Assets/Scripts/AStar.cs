using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AStar
{
    private AStar() { }
    public static AStar Instance { get; } = new AStar();

    #region 结构 
    // 二维坐标点  
    public struct Point
    {
        public int X, Y;
        public Point(int _x, int _y)
        {
            X = _x;
            Y = _y;
        }
    }
 
    // A*的每个节点 
    public class ANode
    {
        public Point point;
        public ANode parent;
        public int fn, gn, hn;
    }
    #endregion

    #region 字段
    private int[,] Map = null;
    private Dictionary<Point, ANode> OpenList = null;
    private HashSet<Point> ClosedList = null;
    private Point EndPoint;
    private int ReachableVal;
    #endregion

    #region 方法
    // 执行算法
    public List<Tile> Execute(int[,] map, int startX, int startY, int endX, int endY, int reachableVal = 0, bool allowDiagonal = false)
    {
        InitData(map, endX, endY, reachableVal);
        AddNodeToOpen(startX, startY);
        List<Tile>path = FindPath(allowDiagonal);

        return path;
    }

    //初始化所需数据
    private void InitData(int[,] map, int endX, int endY, int reachableVal)
    {
        this.OpenList = new Dictionary<Point, ANode>();
        this.ClosedList = new HashSet<Point>();
        this.Map = map;
        this.EndPoint = new Point(endX, endY);
        this.ReachableVal = reachableVal;
    }

    //查找由起点到终点的路径
    private List<Tile> FindPath(bool allowDiagonal)
    {
        while (OpenList.Count > 0)
        {
            //从open列表中找到f(n)最小的结点
            ANode minFn = FindMinFn(OpenList);
            Point point = minFn.point;
            //判断是否到达终点
            if (point.X == EndPoint.X && point.Y == EndPoint.Y)
            {
                return DisplayPath(minFn);
            }
            else
            {
                UpdateCloseAndOpen(minFn, point, allowDiagonal);
            }
        }

        Debug.Log("没找到路径");
        return null;
    }
   
    // 从open列表中获取最小f(n)的节点  
    private ANode FindMinFn(Dictionary<Point, ANode> aNodes)
    {
        ANode minANode = null;
        foreach (var e in aNodes)
        {
            if (minANode == null || e.Value.fn < minANode.fn)
            {
                minANode = e.Value;
            }
        }
        return minANode;
    }

    // 输出相对最短路径
    private List<Tile> DisplayPath(ANode aNode)
    {      
        List<Tile> tiles = new List<Tile>();

        while (aNode != null)
        {          
            Tile tile = new Tile(aNode.point.X, aNode.point.Y);         
            tiles.Insert(0, tile);
            aNode = aNode.parent;
        }

        return tiles;
    }

    //更新列表
    private void UpdateCloseAndOpen(ANode nowNode, Point nowpoint,bool allowDiagonal)
    {
        //去除nowNode，加入到closed列表中
        OpenList.Remove(nowNode.point);
        ClosedList.Add(nowNode.point);

        //将nowNode周围的节点加入到open列表中
        AddPointToOpen(new Point(nowpoint.X - 1, nowpoint.Y), nowNode); //左
        AddPointToOpen(new Point(nowpoint.X + 1, nowpoint.Y), nowNode); //右
        AddPointToOpen(new Point(nowpoint.X, nowpoint.Y - 1), nowNode); //上
        AddPointToOpen(new Point(nowpoint.X, nowpoint.Y + 1), nowNode); //下
        if (allowDiagonal)
        {
            AddPointToOpen(new Point(nowpoint.X - 1, nowpoint.Y - 1), nowNode); //左上
            AddPointToOpen(new Point(nowpoint.X + 1, nowpoint.Y - 1), nowNode); //右上
            AddPointToOpen(new Point(nowpoint.X - 1, nowpoint.Y + 1), nowNode); //左下
            AddPointToOpen(new Point(nowpoint.X + 1, nowpoint.Y + 1), nowNode); //右下
        }
    }

    // 判断节点是否可达，可达则将节点加入到open列表中 
    private void AddPointToOpen(Point point, ANode parent)
    {
        if (point.X > (Map.GetLength(0) - 1) || point.X < 0)//X越界
        {
            return;
        }

        if (point.Y > (Map.GetLength(1) - 1) || point.Y < 0)//Y越界
        {
            return;
        }

        if (IsReachable(point) && !ClosedList.Contains(point))//该点可到达，且没有被使用过
        {
            AddNodeToOpen(point.X, point.Y, parent);
        }
    }
  
    // 判定该点是否可达
    private bool IsReachable(Point point)
    {
        return Map[point.X, point.Y] == this.ReachableVal;
    }

    //初始化节点并判断是否加入到open列表中
    private void AddNodeToOpen(int x, int y, ANode parent = null)
    {
        ANode aNode = new ANode();
        aNode.point = new Point(x, y);
        aNode.parent = parent;
        aNode.gn = (parent == null) ? 0 : (parent.gn + 1);
        aNode.hn = ManHattan(aNode.point, EndPoint);
        aNode.fn = aNode.gn + aNode.hn;

        if (OpenList.ContainsKey(aNode.point))//判断 是否已包含
        {
            if (aNode.fn < OpenList[aNode.point].fn)//更新为fn更小的节点
            {
                OpenList[aNode.point] = aNode;
            }
        }
        else//新增节点
        {
            OpenList.Add(aNode.point, aNode);
        }
    }
  
    // 计算两个点之间的曼哈顿距离
    private int ManHattan(Point a, Point b)
    {
        return Mathf.Abs(a.X - b.X) + Mathf.Abs(a.Y - b.Y);
    } 
    #endregion
}
