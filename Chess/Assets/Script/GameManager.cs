using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public const int CELL_X     = 8;
    public const int CELL_Y     = 8;
           const int PLAYER_MAX = 2;

    //タイルのプレハブ,カーソルのプレハブ
    public GameObject[] prefabTile;
    public GameObject prefabCursor;


    //内部データ
    GameObject     [,] cells;
    UnitsController[,] units;


    //ユニットのプレハブ
    public List<GameObject> prefab_WhiteUnits;
    public List<GameObject> prefab_BlackUnits;

    
    //わかりずらいと思いますが下記を盤面だと見てください。
    //Unitが何も居ないところは、「0」となっています。

    public int[,] unitsType =
    {
// Player側の最後列                敵側の最後列
//        ⇓                             ⇓
        { 2 , 1 , 0 , 0 , 0 , 0 , 11 , 12 },
        { 3 , 1 , 0 , 0 , 0 , 0 , 11 , 13 },
        { 4 , 1 , 0 , 0 , 0 , 0 , 11 , 14 },
        { 5 , 1 , 0 , 0 , 0 , 0 , 11 , 15 },
        { 6 , 1 , 0 , 0 , 0 , 0 , 11 , 16 },
        { 4 , 1 , 0 , 0 , 0 , 0 , 11 , 14 },
        { 3 , 1 , 0 , 0 , 0 , 0 , 11 , 13 },
        { 2 , 1 , 0 , 0 , 0 , 0 , 11 , 12 },
    };


    private void Start()
    {
        //内部データの初期化
        cells = new GameObject     [CELL_X, CELL_Y];
        units = new UnitsController[CELL_X, CELL_Y];


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


                //ユニットを作成
                var type   = unitsType[X_Axis, Y_Axis] % 10;
                var player = unitsType[X_Axis, Y_Axis] / 10;

                GameObject      prefab     = GetPrefabUnit(player, type);


                //nullチェック
                GameObject      unit       = null;
                UnitsController controller = null;

                if (null == prefab) continue;

                createPosition.y += 1.5f;
                unit              = Instantiate(prefab);


                //初期化状態
                controller = unit.GetComponent<UnitsController>();
                controller.SetUnit(player ,(UnitsController.TYPE)type, tiles);

                //内部のデータセット
                units[X_Axis, Y_Axis] = controller;
            }
        }
    }

    private void Update()
    {
        
    }

    //ユニットのプレハブを取得
    GameObject GetPrefabUnit(int player, int type)
    {
        var index = type - 1;

        if (0 > index){ return null; }

        GameObject prefab = prefab_WhiteUnits[index];

        if (1 == player) { prefab = prefab_BlackUnits[index]; }

        return prefab;
    }
}
