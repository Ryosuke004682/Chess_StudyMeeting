using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public const int CELL_X     = 8;
    public const int CELL_Y     = 8;
           const int PLAYER_MAX = 2;

    //タイルのプレハブ
    public GameObject[] prefabTile;

    //カーソルのプレハブ
    public GameObject prefabCursor;

    //内部データ
    GameObject     [,] cells;
    UnitsController[,] units;


    //ユニットのプレハブ
    public List<GameObject> prefab_WhiteUnits;
    public List<GameObject> prefab_BlackUnits;


    private void Start()
    {
        //盤面を生成する。
        for(var X_Axis = 0; X_Axis < CELL_X; X_Axis++)
        {
            for (var Y_Axis = 0; Y_Axis < CELL_Y; Y_Axis++)
            {
                //タイルとユニットのポジションを生成する。
                var x = X_Axis - CELL_X / 2;//横
                var y = Y_Axis - CELL_Y / 2;//縦


                var createPosition = new Vector3(x , 0 , y);

                //タイルを生成
                var index = (X_Axis + Y_Axis) % 2;//0と1を交互に指定

                GameObject tiles = Instantiate(prefabTile[index] , createPosition , Quaternion.identity);

                cells[X_Axis , Y_Axis] = tiles;
            }
        }
    }
}
