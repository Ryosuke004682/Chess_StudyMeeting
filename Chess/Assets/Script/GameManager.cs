using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

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

    public int[,] unitsType =
    {
        { 2 , 1 , 0 , 0 , 0 , 0 , 11 , 12 },
        { 3 , 1 , 0 , 0 , 0 , 0 , 11 , 13 },
        { 4 , 1 , 0 , 0 , 0 , 0 , 11 , 14 },
        { 5 , 1 , 0 , 0 , 0 , 0 , 11 , 15 },
        { 6 , 1 , 0 , 0 , 0 , 0 , 11 , 16 },
        { 4 , 1 , 0 , 0 , 0 , 0 , 11 , 14 },
        { 3 , 1 , 0 , 0 , 0 , 0 , 11 , 13 },
        { 2 , 1 , 0 , 0 , 0 , 0 , 11 , 12 },
    };

    //UI関連
    GameObject   textTurnInfo;
    GameObject textResultInfo;
    GameObject    buttonApply;
    GameObject   buttonCancel;

    //選択ユニット
    UnitsController selectUnit = null;

    
    //移動関連
    List<Vector2Int> movableTiles;
    List<GameObject> cursors;


    //モード
    enum MODE
    {
        NONE,
        CHECK_MATE,
        NORMAL,
        STATUS_UPDATE,
        TURN_CHANGE,
        RESULT
    }

    MODE currentMode, nextMode;
    int  currentPlayer;


    private void Start()
    {
        //UIオブジェクト
        textTurnInfo   = GameObject.Find("TextTurnInfo"  );
        textResultInfo = GameObject.Find("TextResultInfo");
        buttonApply    = GameObject.Find("ButtonApply")   ;
        buttonCancel   = GameObject.Find("ButtonCancel")  ;

        //リザルト関連は非表示にする。
        buttonApply .SetActive(false);
        buttonCancel.SetActive(false);


        //内部データの初期化
        cells   = new GameObject     [CELL_X, CELL_Y];
        units   = new UnitsController[CELL_X, CELL_Y];
        cursors = new List<GameObject>();


        //盤面を生成
        for(var X_Axis = 0; X_Axis < CELL_X; X_Axis++)
        {
            for (var Y_Axis = 0; Y_Axis < CELL_Y; Y_Axis++)
            {

                var x = X_Axis - CELL_X / 2;//横
                var y = Y_Axis - CELL_Y / 2;//縦


                var createPosition = new Vector3(x , 0 , y);

                //タイルを生成
                var index = (X_Axis + Y_Axis) % 2;
                GameObject tiles = Instantiate(prefabTile[index] , createPosition , Quaternion.identity);
                cells[X_Axis , Y_Axis] = tiles;


                //ユニットを作成
                var type   = unitsType[X_Axis, Y_Axis] % 10;
                var player = unitsType[X_Axis, Y_Axis] / 10;

                GameObject prefab = GetPrefabUnit(player, type);


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

        currentPlayer = -1;
        currentMode   = MODE.NONE;
        nextMode      = MODE.TURN_CHANGE;
    }

    private void Update()
    {
        if(MODE.CHECK_MATE         == currentMode)
        {
            CheckMateMode();
        }
        else if(MODE.NORMAL        == currentMode)
        {
            NormalMode();
        }
        else if(MODE.STATUS_UPDATE == currentMode)
        {
            StatusUpdateMode();
        }
        else if(MODE.TURN_CHANGE   == currentMode)
        {
            TurnChangeMode();
        }

        //モード変更
        if(MODE.NONE != nextMode)
        {
            currentMode = nextMode;
            nextMode    = MODE.NONE;
        }
    }

    //チェックメイトモード
    void CheckMateMode()
    {
        nextMode = MODE.NORMAL;
    }

    //ノーマルモード
    private void NormalMode()
    {

        GameObject      tile = null;
        UnitsController unit = null;


        //Playerの処理
        if (Input.GetMouseButtonUp(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            //ヒットしたすべてのオブジェクトの情報を取得
            foreach (RaycastHit hit in Physics.RaycastAll(ray))
            {
                if (hit.transform.name.Contains("Board_Black")
                 || hit.transform.name.Contains("Board_White"))
                {
                    tile = hit.transform.gameObject;
                    break;
                }
            }
        }

        if (null == tile) return;

        //選んだタイルからユニット取得
        Vector2Int tilepos = new Vector2Int((int)tile.transform.position.x + CELL_X / 2,
                                            (int)tile.transform.position.z + CELL_Y / 2);

        //ユニット
        unit = units[tilepos.x, tilepos.y];


        if (null != unit && selectUnit != unit && currentPlayer == unit.player)
        {
            List<Vector2Int> tiles = GetMovableTiles(unit);

            if (1 > tiles.Count) return;


            movableTiles = tiles;
            SetSelectCursors(unit);
        }
        else if (null != selectUnit && movableTiles.Contains(tilepos))
        {
            MoveUnit(selectUnit, tilepos);
            nextMode = MODE.STATUS_UPDATE;
        }
    }

    
    //移動後の処理
    private void StatusUpdateMode()
    {
        //キャスティング

        //アンパッサン

        //プロモーション

        //ターン経過
        foreach(var n in GetUnits(currentPlayer))
        {
            n.ProgressTurn();//ターンを経過させる。
        }

        //カーソルを消す。
        SetSelectCursors();
        nextMode = MODE.TURN_CHANGE;
    }

    //ターン変更
    private void TurnChangeMode()
    {
        //ターンの処理
        currentPlayer = GetNextPlayer();

        //Infoの更新
        textTurnInfo.GetComponent<Text>().text = "" + (currentPlayer + 1) + "Pの番です。";

        nextMode = MODE.CHECK_MATE;
    }

    //ターンの加算処理
    int GetNextPlayer()
    {
        var next = currentPlayer + 1;

        if (PLAYER_MAX <= next) next = 0;

        return next;
    }


    //指定されたPlayer番号のユニットを取得する。
    List<UnitsController> GetUnits(int player = -1)
    {
        List<UnitsController> ret = new List<UnitsController>();

        foreach(var n in units)
        {
            if (null == n) continue;

            if (player == n.player) { ret.Add(n); }

            else if (0 > player)    { ret.Add(n); }
        }
        return ret;
    }


    //移動可能範囲の取得
    List<Vector2Int> GetMovableTiles(UnitsController unit)
    {
        return unit.GetMovableTiles(units);
    }


    //ユニットの移動
    void MoveUnit(UnitsController unit , Vector2Int tilepos)
    {
        Vector2Int unitPosition = unit.position;


        if (null != units[tilepos.x, tilepos.y]) 
        {
            Destroy(units[tilepos.x, tilepos.y].gameObject);
        }

        unit.MoveUnit(cells[tilepos.x, tilepos.y]);


        units[unitPosition.x, unitPosition.y] = null;
        units[tilepos.x     , tilepos.y]      = unit;
    }

    //選択時の関数
    void SetSelectCursors(UnitsController unit = null , bool setUnit = true)
    {
        foreach (var n in cursors) { Destroy(n); }

        if(null != selectUnit)
        {
            selectUnit.SelectUnit(false);
            selectUnit = null;
        }

        if (null == unit) { return; }

        foreach(var n in GetMovableTiles(unit))
        {
            Vector3 position = cells[n.x, n.y].transform.position;
            position.y += 0.07f;

            GameObject  obj = Instantiate(prefabCursor , position , Quaternion.identity);
            cursors.Add(obj);
        }

        if(setUnit)
        {
            selectUnit = unit;
            selectUnit.SelectUnit();
        }
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
