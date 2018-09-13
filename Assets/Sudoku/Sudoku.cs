using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/*
 * 以行列的形式随机不对， 首个 3*3 会没有校验
 * 
 * 
 * 
 * 
 * 
 */


public class Sudoku : MonoBehaviour
{
    public Element Element;
    public GridLayoutGroup grid;
    public Sprite[] SourceImages;
	// Use this for initialization
	void Start ()
	{
	    int size = 9;
        List<Element> sourceNumberList = new List<Element>();
	    bool result = true;
        for(int i = 0;i < size;i++)
	    {
	        for (int j = 0; j < size; j++)
	        {
	            Element tempElement =GameObject.Instantiate(Element);
                tempElement.pos = new Vector2Int(i,j);
                List<int> numberPool = new List<int>(){1,2,3,4,5,6,7,8,9};
                int index = Random.Range(0,numberPool.Count);
	            int randomNum = numberPool[index];
                tempElement.num = randomNum;
	            numberPool.RemoveAt(index);
                while(!CheckNum(tempElement,sourceNumberList))
	            {
                    if(numberPool.Count<=0)
	                {
                        Debug.LogError("未取到有效数字！！");
	                    result = false;
	                    break;
	                }
                    index = Random.Range(0,numberPool.Count);
                    randomNum = numberPool[index];
                    tempElement.num = randomNum;
                    numberPool.RemoveAt(index);
	            }
                if(!result)
                    break;//只会跳出一层  可用 goto
                sourceNumberList.Add(tempElement);
                tempElement.GetComponent<Image>().sprite = SourceImages[tempElement.num];
                tempElement.transform.SetParent(grid.transform);
	        }
            if(!result)
                break;//只会跳出一层
	    }

	}


    private bool CheckNum(Element randomNum,List<Element> elements)
    {
        foreach (var element in elements)
        {
            if(element.pos.x == randomNum.pos.x || element.pos.y == randomNum.pos.y)//同行 同列
            {
                if (element.num == randomNum.num)
                {
                    return false;
                }
            }
        }
        return true;
    }
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