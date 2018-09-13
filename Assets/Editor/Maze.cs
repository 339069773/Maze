using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

public class Maze : Editor
{
    //长宽多少个cell
    private const int cellNum =8;

    /// <summary>
    /// 每个cell 尺寸 像素
    /// </summary>
    public const int CELLSIZE = 10;

    private const string FILENAME = "Maze/maze.png";

    private static List<Cell> cells = new List<Cell>();

    [MenuItem("CreatMaze/CreatNumberMap")]
    private static void CreatNumberMap()
    {
        Init();
        DPS(cells[0]);
        DrawMaze(cells, FILENAME);
    }

    static void Init()
    {
        cells.Clear();
        for(int i = 0;i < cellNum;i++)
        {
            for(int j = 0;j < cellNum;j++)
            {
                Cell tempCell = new Cell();
                tempCell.pos = new Vector2Int(i,j);
                if (i==0 && j==0)
                {
                    tempCell.AddRoad(DirEnum.Left);//入口
                }
                else if(i == cellNum - 1 && j == cellNum-1)
                {
                    tempCell.AddRoad(DirEnum.Right);//出口
                }
                cells.Add(tempCell);
            }
        }
        InitCells(cells);
        mVisitCell.Clear();
        mCellsStack.Clear();
        num = 1;//调试用
    }

public static void InitCells(List<Cell> cells)
    {
        for(int i = 0;i < cells.Count;i++)
        {
            int x = cells[i].pos.x;
            int y = cells[i].pos.y;
            cells[i].LeftCell = GetCell(x - 1,y,cells);
            cells[i].RightCell = GetCell(x + 1,y,cells);
            cells[i].UpCell = GetCell(x,y + 1,cells);
            cells[i].DownCell = GetCell(x,y - 1,cells);
        }
    }
    static void DrawMaze(List<Cell> cells,string fileName)
    {
        if (cells==null || cells.Count<=0)
            return;
        Texture2D outTexture2D = new Texture2D(cellNum * CELLSIZE,cellNum*CELLSIZE);
        for (int i = 0; i < cells.Count; i++)
        {
            int x = cells[i].pos.x;
            int y = cells[i].pos.y;
            int startX = x*CELLSIZE;//起始x
            int startY = y*CELLSIZE;//起始y
            
            for (int pixelX = startX ;pixelX < startX+CELLSIZE; pixelX++)
            {
                for(int pixelY = startY;pixelY < startY + CELLSIZE;pixelY++)
                    outTexture2D.SetPixel(pixelX,pixelY,cells[i].GetPixel(pixelX - startX,pixelY - startY));
            }
        }
        SaveTextureToFile(outTexture2D, fileName);
    }
    private static void SaveTextureToFile(Texture2D texture2,string fileName)
    {
        var bytes = texture2.EncodeToPNG();
        FileStream fileStream = File.Open(Application.dataPath + "/" + fileName,FileMode.Create);
        var binary = new BinaryWriter(fileStream);
        binary.Write(bytes);
        fileStream.Close();
    }


    /// <summary>
    /// 当前访问进度列表
    /// </summary>
    static Stack<Cell> mCellsStack = new Stack<Cell>();
    /// <summary>
    /// 所有访问
    /// </summary>
    static List<Cell> mVisitCell = new List<Cell>();
    /// <summary>
    /// 所有未访问
    /// </summary>
    static List<Cell> mUnVisitCell = new List<Cell>();
    static int num = 1;
    private static void DPS(Cell cell)
    {
        if(mVisitCell.Count >= cellNum * cellNum)
        {
            Debug.LogError("mVisitCell.Count==" + mVisitCell.Count);
            return;
        }
        if(cell==null)
        {
            Debug.LogError("cell==null");
            return;
        }
        List<int> range = CheckCellValidDir(cell);
        //死胡同
        if (range.Count <= 0)
        {
            if (!mVisitCell.Contains(cell))
            {
                mVisitCell.Add(cell);
            }
//            DrawMaze(cells, num + ".png");
//            Debug.LogError("//死胡同" + cell.pos.x + " " + cell.pos.y + " " + mVisitCell.Count);
            num++;
            if(mCellsStack.Count>0)
            {
                Cell lastCell = mCellsStack.Pop();
                DPS(lastCell);
            }
        }
        else
        {
            if(!mVisitCell.Contains(cell))
                mVisitCell.Add(cell);

            if(!mCellsStack.Contains(cell))
            mCellsStack.Push(cell);
            int dirIdx = Random.Range(0,range.Count);
            int dir = range[dirIdx];
            switch((DirEnum)dir)
            {
                case DirEnum.Up:
                    cell.AddRoad(DirEnum.Up);
                    cell.UpCell.AddRoad(DirEnum.Down);
                    DPS(cell.UpCell);
                    break;
                case DirEnum.Down:
                    cell.AddRoad(DirEnum.Down);
                    cell.DownCell.AddRoad(DirEnum.Up);
                    DPS(cell.DownCell);
                    break;
                case DirEnum.Left:
                    cell.AddRoad(DirEnum.Left);
                    cell.LeftCell.AddRoad(DirEnum.Right);
                    DPS(cell.LeftCell);
                    break;
                case DirEnum.Right:
                    cell.AddRoad(DirEnum.Right);
                    cell.RightCell.AddRoad(DirEnum.Left);
                    DPS(cell.RightCell);
                    break;
            }
        }
    }


    public static Cell GetCell(int x,int y,List<Cell> cells)
    {
        foreach(var cell in cells)
        {
            if(cell.pos.x == x && cell.pos.y == y)
            {
                return cell;
            }
        }
        return null;
    }

    static List<int> CheckCellValidDir(Cell cell)
    {
        List<int> ret = new List<int>();
        if(cell.UpCell != null && !mVisitCell.Contains(cell.UpCell))
            ret.Add((int)DirEnum.Up);
        if(cell.DownCell != null && !mVisitCell.Contains(cell.DownCell))
            ret.Add((int)DirEnum.Down);
        if(cell.LeftCell != null && !mVisitCell.Contains(cell.LeftCell))
            ret.Add((int)DirEnum.Left);
        if(cell.RightCell != null && !mVisitCell.Contains(cell.RightCell))
            ret.Add((int)DirEnum.Right);
        return ret;
    }


    public class Cell
    {
        public int id = 0;
        public Vector2Int pos;
        public List<Cell> neighbor = new List<Cell>();
        public int value;
        public Cell LeftCell;
        public Cell RightCell;
        public Cell UpCell;
        public Cell DownCell;
        //count代表通路个数，具体每个类型代表方向
        List<DirEnum> roads = new List<DirEnum>();
        public List<DirEnum> Roads {
            get { return roads; }
        }
        //使用图片素材
        TextrueType texType = TextrueType.FourSide;
        public bool AddRoad(DirEnum dir)
        {
            if (!roads.Contains(dir))
            {
                roads.Add(dir);
                return true;
            }
            return false;
        }


        public Color GetPixel(int x,int y)
        {
            RotationType rotationType = RotationType.None;
            string[] dirs = Enum.GetNames(typeof(DirEnum));
            switch(roads.Count)
            {
                #region 一条路
                case 1:
                    texType = TextrueType.ThreeSide;
                    for(int i = 0;i < dirs.Length;i++)
                    {
                        DirEnum temp = (DirEnum)Enum.Parse(typeof(DirEnum),dirs[i]);
                        if(roads.Contains(temp))
                        {
                            switch(temp)
                            {
                                case DirEnum.Up:
                                    rotationType = RotationType.OppositesUpDown;
                                    break;
                                case DirEnum.Down:
                                    rotationType = RotationType.None;
                                    break;
                                case DirEnum.Left:
                                    rotationType = RotationType.ClockWise;
                                    break;
                                case DirEnum.Right:
                                    rotationType = RotationType.AntiClockWise;
                                    break;
                                default:
                                    Debug.LogError("=");
                                    break;
                            }
                            break;
                        }
                    }
                    break;

                #endregion

                #region 2条路

                case 2:
                    if(roads.Contains(DirEnum.Up) && roads.Contains(DirEnum.Down))
                    {
                        texType = TextrueType.TwoSideFaceToFace;
                        rotationType = RotationType.None;
                    }
                    else if(roads.Contains(DirEnum.Left) && roads.Contains(DirEnum.Right))
                    {
                        texType = TextrueType.TwoSideFaceToFace;
                        rotationType = RotationType.ClockWise;
                    }
                    else if(roads.Contains(DirEnum.Left) && roads.Contains(DirEnum.Up))
                    {
                        texType = TextrueType.TwoSideAdjacent;
                        rotationType = RotationType.ClockWise;
                    }
                    else if(roads.Contains(DirEnum.Left) && roads.Contains(DirEnum.Down))
                    {
                        texType = TextrueType.TwoSideAdjacent;
                        rotationType = RotationType.None;

                    }
                    else if(roads.Contains(DirEnum.Right) && roads.Contains(DirEnum.Up))
                    {
                        texType = TextrueType.TwoSideAdjacent;
                        rotationType = RotationType.DuiJiaoRightTop;
                    }
                    else if(roads.Contains(DirEnum.Right) && roads.Contains(DirEnum.Down))
                    {
                        texType = TextrueType.TwoSideAdjacent;
                        rotationType = RotationType.OppositesLeftRight;
                    }
                    break;

                #endregion

                #region 3条路
                case 3:
                    texType = TextrueType.OneSide;
                    for(int i = 0;i < dirs.Length;i++)
                    {
                        DirEnum temp = (DirEnum)Enum.Parse(typeof(DirEnum),dirs[i]);
                        if(!roads.Contains(temp))
                        {
                            switch(temp)
                            {
                                case DirEnum.Up:
                                    rotationType = RotationType.AntiClockWise;
                                    break;
                                case DirEnum.Down:
                                    rotationType = RotationType.ClockWise;
                                    break;
                                case DirEnum.Left:
                                    rotationType = RotationType.OppositesLeftRight;
                                    break;
                                case DirEnum.Right:
                                    rotationType = RotationType.None;
                                    break;
                                default:
                                    Debug.LogError("=");
                                    break;
                            }
                            break;
                        }
                    }
                    break;
                #endregion

                #region 4条路
                case 4:
                    texType = TextrueType.NoSide;
                    rotationType = RotationType.None;
                    break;
                #endregion
                default:
                    texType = TextrueType.FourSide;
                    rotationType = RotationType.None;
                    break;
            }
            Texture2D tex = GetTexture(texType);
            if(tex == null)
                return Color.red;
            return GetColorWithRotation(tex,x,y,rotationType);
        }

        Texture2D GetTexture(TextrueType texType)
        {
            Texture2D sprites = (Texture2D)Resources.Load("0" + (int)texType,typeof(Texture2D));
            return sprites;
        }

        private Color GetColorWithRotation(Texture2D tex, int x, int y, RotationType rotation)
        {
            int tempX = x;
            int tempY = y;
            switch (rotation)
            {
                case RotationType.None:
                    break;
                case RotationType.ClockWise:
                    tempX = CELLSIZE - tempY - 1;
                    tempY = x;
                    break;
                case RotationType.AntiClockWise:
                    tempX = tempY;
                    tempY = CELLSIZE - x - 1;
                    break;
                case RotationType.OppositesUpDown:
                    tempY = CELLSIZE - tempY - 1;
                    break;
                case RotationType.OppositesLeftRight:
                    tempX = CELLSIZE - tempX - 1;
                    break;
                case RotationType.DuiJiaoRightTop:
                    if ((tempX + tempY) != CELLSIZE - 1)
                    {
                        tempX = CELLSIZE - tempY;
                        tempY = CELLSIZE - x;
                    }
                    break;
            }
            return tex.GetPixel(tempX, tempY);
        }
    }
}

public enum DirEnum
{
    Up = 1,
    Down =2,
    Left = 4,
    Right=8,
}
public enum RotationType
{
    None =0,
    ClockWise = 1,//顺时针
    AntiClockWise = 2,//逆时针
    OppositesUpDown = 3,//对立
    OppositesLeftRight = 4,//对立
    DuiJiaoRightTop = 5,//对角
}

public enum TextrueType
{
    OneSide =1,
    TwoSideFaceToFace=2,
    ThreeSide =3,
    TwoSideAdjacent = 4,
    FourSide = 5,
    NoSide =6,
}
public struct Vector2Int
{
    public int x;
    public int y;

    public Vector2Int(int x,int y)
    {
        this.x = x;
        this.y = y;
    }
}
