using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Image = UnityEngine.UI.Image;

public class GameManager : MonoBehaviour
{
    //盤面生成
    public const int CELL_X         = 8;
    public const int CELL_Y         = 8;

    //Play人数
           const int PLAYER_MAX     = 2;

    //ステイルメイト
           const int LIMIT_TURN     = 50;//50ターンルール
           const int RemainingLives = 2;//残機


    //タイルのプレハブ,カーソルのプレハブ
    public GameObject[] prefabTile;
    public GameObject prefabCursor;

    //チェックメイト、ステイルメイトの画像
    public Image[] judgeMent;


    //内部データ
    GameObject     [,] tiles;
    UnitsController[,] units;

    //シーンの遷移時間
    public const int TRASITION_TIME = 2;


    //ユニットのプレハブ
    public List<GameObject> prefab_WhiteUnits;
    public List<GameObject> prefab_BlackUnits;

    //盤面
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

    //ユニットを削除してからの経過ターン
    int unitsDestroyTurn;

    //前回の盤面
    List<UnitsController[,]> prevUnits;


    private void Start()
    {
        //UIオブジェクト
        textTurnInfo   = GameObject.Find("TextTurnInfo"  );
        textResultInfo = GameObject.Find("TextResultInfo");
        buttonApply    = GameObject.Find("ButtonApply")   ;
        buttonCancel   = GameObject.Find("ButtonCancel")  ;

        judgeMent[0].enabled = false;
        judgeMent[1].enabled = false;
        judgeMent[2].enabled = false;


        //リザルト関連は非表示にする。
        buttonApply .SetActive(false);
        buttonCancel.SetActive(false);


        //内部データの初期化
        tiles     = new GameObject     [CELL_X, CELL_Y];
        units     = new UnitsController[CELL_X, CELL_Y];
        cursors   = new List<GameObject>();
        prevUnits = new List<UnitsController[,]>();


        //盤面を生成
        for(var i = 0; i < CELL_X; i++)
        {
            for (var j = 0; j < CELL_Y; j++)
            {

                var x = i - CELL_X / 2;//横
                var y = j - CELL_Y / 2;//縦


                var createPosition = new Vector3(x , 0 , y);//奥行を指定したいからyをzに格納

                //タイルを生成
                var index = (i + j) % 2;
                GameObject tiles = Instantiate(prefabTile[index] , createPosition , Quaternion.identity);
                this.tiles[i, j] = tiles;


                //ユニットを作成
                var type   = unitsType[i, j] % 10;
                var player = unitsType[i, j] / 10;

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
                units[i, j] = controller;
            }
        }

        currentPlayer = -1;
        currentMode   = MODE.NONE;
        nextMode      = MODE.TURN_CHANGE;
    }


    private void Update()
    {
        if (MODE.CHECK_MATE         == currentMode)
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

        Text info = textResultInfo.GetComponent<Text>();
        info.text = "";

        /*-------------------
        TODO : ステイルメイトの処理（引き分けの処理）
         --------------------*/

        // 1 VS 1になった場合、 GetUnits().Countが 2 になったら引き分けにする。
        if (RemainingLives == GetUnits().Count)
        {
            info.text = "進行不能により試合終了!!";
            nextMode = MODE.RESULT;

            judgeMent[0].enabled = true;
        }

        //50ターンの間、誰も削除されなかった場合
        if(LIMIT_TURN < unitsDestroyTurn)
        {
            info.text = "50ターンルールで試合終了!!";
            nextMode = MODE.RESULT;
            judgeMent[0].enabled = true;
        }

        //3回同じ盤面だった場合

        int prevCount = 0;

        foreach(var n in prevUnits)
        {
            bool check = true;

            for(var i = 0; i < n.GetLength(0); i++)
            {
                for(var j = 0; j < n.GetLength(1); j++)
                {
                    if (n[i, j] != units[i, j]) check = false;
                }
            }

            //チェックが終わった後に
            if (check) prevCount++;
        }

        //同じ盤面が3回続いたかどうか
        if(2 < prevCount)
        {
            info.text = "同じ盤面が３回続いたので試合終了!!";
            nextMode = MODE.RESULT;

            judgeMent[0].enabled = true;
        }

        /*-------------------
         チェックの判定
         --------------------*/
        UnitsController target = GetUnit(currentPlayer ,UnitsController.TYPE.KING);
        //チェックしてるユニット
        List<UnitsController> checkunits = GetCheckUnits(units , currentPlayer);

        bool isCheck = (0 < checkunits.Count) ? true : false;

        //チェック状態をセット
        if(null != target)
        {
            target.SetCheckStatus(isCheck);
        }

        //ゲームが続くならチェックと表示
        if(isCheck && MODE.RESULT != nextMode)
        {
            judgeMent[2].enabled = true;
        }
        else
        {
            judgeMent[2].enabled = false;
        }

        /*-------------------
         移動可能範囲の判定
         --------------------*/
        var tileCount = 0;

        foreach (var n in GetUnits(currentPlayer))
        {
            tileCount += GetMovableTiles(n).Count;
        }

        //動かせない
        if (1 > tileCount)
        {
            judgeMent[0].enabled = true;

            //もしチェックされて動けなかった時は、チェックメイト
            if (isCheck)
            {
                info.text = $"Player{GetNextPlayer() + 1} の勝利！!";

                judgeMent[0].enabled = false;
                judgeMent[1].enabled = true;
                judgeMent[2].enabled = false;
            }

            //チェックメイト後は、ゲーム辞退が終わりだから
            nextMode = MODE.RESULT;
        }

        UnitsController[,] copyunits = GetCopyArracy(units);
        prevUnits.Add(copyunits);


        //リザルトボタンとかを出す（次のモードの準備）
        if(MODE.RESULT == nextMode)
        {
            buttonApply .SetActive(true);
            buttonCancel.SetActive(true);
        }
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

        //コンピューター（ＣＰＵ）の処理
        while (TitleManager.playerCount <= currentPlayer && (null == selectUnit || null == tile))
        {

            if (null == selectUnit)
            {
                //全ユニットを取得
                List<UnitsController> tmpUnits = GetUnits(currentPlayer);
                UnitsController tmp = tmpUnits[UnityEngine.Random.Range(0, tmpUnits.Count)];

                //ユニットが居るタイルを選択
                tile = tiles[tmp.position.x , tmp.position.y];

                break;
            }

            //ここから下はSelectUnitが入った状態で来る
            if( 1 > movableTiles.Count)
            {
                //非選択状態にして、処理を抜ける
                SetSelectCursors();
                break;
            }

            //移動可能範囲があれば、ランダムで動かす
            var random = UnityEngine.Random.Range(0 , movableTiles.Count);
            tile = tiles[movableTiles[random].x , movableTiles[random].y];

        }
            

        //タイル以外の場所は判定しない。
        if (null == tile) return;

        //選んだタイルからユニット取得
        Vector2Int tilepos = new Vector2Int((int)tile.transform.position.x + CELL_X / 2,
                                            (int)tile.transform.position.z + CELL_Y / 2);

        //ユニット
        unit = units[tilepos.x, tilepos.y];


        if (null != unit && selectUnit != unit && currentPlayer == unit.player)
        {
            //移動可能範囲を取得
            List<Vector2Int> tiles = GetMovableTiles(unit);

            //選択不可
            if (1 > tiles.Count) return;


            movableTiles = tiles;
            SetSelectCursors(unit);
        }
        else if (null != selectUnit && movableTiles.Contains(tilepos))//移動
        {
            MoveUnit(selectUnit, tilepos);
            nextMode = MODE.STATUS_UPDATE;
        }
        //自分のターンじゃないときに、敵の駒の移動範囲を見れるように設定
        else if(null != unit && currentPlayer != unit.player)
        {
            SetSelectCursors(unit,false);//選択されずにカーソルだけ表示される
        }
        //カーソルが表示されてる時に、カーソルが表示されてないところを触った場合選択状態を解除
        else
        {
            SetSelectCursors();
        }
    }

    
    //移動後の処理
    private void StatusUpdateMode()
    {
        //キャスリング
        if(selectUnit.status.Contains(UnitsController.STATUS.QSIDE_CASTLINE))
        {
            //左端ルーク
            var unit = units[0, selectUnit.position.y];
            var tile = new Vector2Int(selectUnit.position.x + 1, selectUnit.position.y);

            MoveUnit(unit, tile);
        }
        else if(selectUnit.status.Contains(UnitsController.STATUS.KSIDE_CASTLINE))
        {
            //右端ルーク
            var unit = units[CELL_X - 1, selectUnit.position.y];
            var tile = new Vector2Int(selectUnit.position.x - 1, selectUnit.position.y);

            MoveUnit(unit , tile);
        }

        //アンパッサンとプロモーション
        if (UnitsController.TYPE.PAWN == selectUnit.type)
        {
            //アンパッサン
            //ポーンが2マス進んだ場合、1マス後ろに残像が残るため取られる処理を書く。

            foreach(var n in GetUnits(GetNextPlayer()))
            {
                if (!n.status.Contains(UnitsController.STATUS.EN_PASSANT)) continue;

                //置いた場所がアンパッサン対象か否か
                if(selectUnit.position == n.oldPosition)
                {
                    Destroy(n.gameObject);
                }
            }

            //プロモーション
            var player = CELL_Y - 1;

            if (selectUnit.player == 1) player = 0;

            if(player == selectUnit.position.y)
            {
                //クイーン固定（いろんなユニットに変更できるみたいだけど、今回はクイーンのみにします。）
                GameObject    prefab = GetPrefabUnit(currentPlayer , (int) UnitsController.TYPE.QUEEN);
                UnitsController unit = Instantiate(prefab).GetComponent<UnitsController>();
                GameObject      tile = tiles[selectUnit.position.x, selectUnit.position.y];

                unit.SetUnit(selectUnit.player , UnitsController.TYPE.QUEEN , tile);
                MoveUnit(unit , new Vector2Int(selectUnit.position.x , selectUnit.position.y));
            }
        }

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

        //Player1側に来たら、経過ターン+1する
        if( 0 == currentPlayer)
        {
            unitsDestroyTurn++;
        }

        //Infoの更新
        textTurnInfo.GetComponent<Text>().text = "" + (currentPlayer + 1) + "Pの番です。";

        nextMode = MODE.CHECK_MATE;
    }


    //ターンの加算処理
    public int GetNextPlayer()
    {
        var next = currentPlayer + 1;

        if (PLAYER_MAX <= next) next = 0;

        return next;
    }


    //指定のユニットを取得する。
    UnitsController GetUnit(int player , UnitsController.TYPE type)
    {
        foreach (var n in GetUnits(player))
        {
            if (player != n.player) continue;
            if (type   == n.type)   return n;
        }

        return null;
    }


    //指定されたPlayer番号の複数のユニットを取得する。
    List<UnitsController> GetUnits(int player = -1)
    {
        List<UnitsController> ret = new List<UnitsController>();

        foreach(var n in units)
        {
            if (null == n) continue;

            if (player == n.player) { ret.Add(n);}

            else if (0 > player)    { ret.Add(n);}
        }
        return ret;
    }


    //指定された配列をコピーして返す。
    public static UnitsController[,] GetCopyArracy(UnitsController[,] org)
    {
        UnitsController[,] ret = new UnitsController[org.GetLength(0), org.GetLength(1)];
        Array.Copy(org , ret , org.Length);

        return ret;
    }


    //移動可能範囲の取得
    List<Vector2Int> GetMovableTiles(UnitsController unit)
    {
        // チェックされてしまう場所には置かせない
        UnitsController[,] copyunits1 = GetCopyArracy(units);

        copyunits1[unit.position.x, unit.position.y] = null;

        //チェックされているかどうか
        List<UnitsController> checkunits = GetCheckUnits(copyunits1 , unit.player);


        //チェックを回避できるタイルを探す
        if(0 < checkunits.Count)
        {
            //移動可能範囲
            List<Vector2Int> ret = new List<Vector2Int>();

            List<Vector2Int> moveTiles = unit.GetMovableTiles(units);

            //移動する
            foreach(var n in moveTiles)
            {
                UnitsController[,] copyunits2 = GetCopyArracy(units);

                copyunits2[unit.position.x, unit.position.y] = null;
                copyunits2[n.x, n.y] = unit;
                var checkCount = GetCheckUnits(copyunits2, unit.player, false).Count;

                if (1 > checkCount) ret.Add(n);
            }
            return ret;
        }

        //通常移動可能範囲を返す。
        return unit.GetMovableTiles(units);
    }


    //ユニットの移動
    void MoveUnit(UnitsController unit , Vector2Int tilepos)
    {
        //現在地
        Vector2Int unitPosition = unit.position;

        //消す処理
        //TODO : 効果音を入れる
        if (null != units[tilepos.x, tilepos.y]) 
        {
            Destroy(units[tilepos.x, tilepos.y].gameObject);
        }

        //新しい場所に移動
        unit.MoveUnit(tiles[tilepos.x, tilepos.y]);


        units[unitPosition.x, unitPosition.y] = null;
        units[tilepos.x     , tilepos.y]      = unit;
    }

    
    //選択時の関数
    void SetSelectCursors(UnitsController unit = null , bool setUnit = true)
    {
        //カーソル削除
        foreach (var n in cursors) { Destroy(n); }

        if(null != selectUnit)
        {
            selectUnit.SelectUnit(false);
            selectUnit = null;
        }

        if (null == unit) { return; }

        //カーソル作成
        foreach(var n in GetMovableTiles(unit))
        {
            Vector3 position = tiles[n.x, n.y].transform.position;
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


    //指定された配置でチェックされてるかを判定
    static public List<UnitsController> GetCheckUnits(UnitsController[ , ] units ,
                                                              int player,
                                                             bool checkking = true)
    {
        List<UnitsController> ret = new List<UnitsController>();

        foreach (var n in units)
        {
            if (null == n)          continue;
            if (player == n.player) continue;

            //敵 1体の移動可能範囲
            List<Vector2Int> enemytiles = n.GetMovableTiles(units, checkking);

            foreach(var enemyTile in enemytiles)
            {
                if (null == units[enemyTile.x, enemyTile.y]) continue;

                if (UnitsController.TYPE.KING == units[enemyTile.x, enemyTile.y].type)
                {
                    ret.Add(n);
                }
            }
        }
        return ret;
    }


    public void Retry()
    {
        StartCoroutine(Transition_Retry());
    }


    public void Title()
    {
        StartCoroutine(Transition_Title());
    }


    IEnumerator Transition_Retry()
    {
        yield return new WaitForSeconds(TRASITION_TIME);
        SceneManager.LoadScene("MainScene");
    }


    IEnumerator Transition_Title()
    {
        yield return new WaitForSeconds(TRASITION_TIME);
        SceneManager.LoadScene("TitleScene");
    }
}
