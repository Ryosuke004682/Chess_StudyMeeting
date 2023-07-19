using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitsController : MonoBehaviour
{
    //ユニットのプレイヤー番号
    [SerializeField] public int player;
    
    //ユニットの種類
    [SerializeField] public TYPE type;

    //移動状態
    [SerializeField] public List<STATUS> status;

    //経過ターン
    public int progressTurnCount;

    //置いてる場所
    public Vector2Int position, oldPosition;

    public enum TYPE
    {
        NONE = -1,
        PAWN =  1,
        ROOK     ,
        KNIGHT   ,
        BISHOP   ,
        QUEEN    ,
        KING     ,
    }

    public enum STATUS
    {
        NONE           = -1,
        QSIDE_CASTLINE =  1,
        KSIDE_CASTLINE     ,
        EN_PASSANT         ,
        CHECK              ,
    }

    private void Start()
    {
        
    }

    private void Update()
    {
        
    }

    // 初期設定
    public void SetUnit(int player , TYPE type , GameObject tile)
    {
        this.player = player;
        this.type   = type;
        MoveUnit(tile);
        progressTurnCount = -1;
    }

    //選択時の処理
    public void SelectUnit(bool select = true)
    {
        Vector3 pos = transform.position;
        pos.y      += 2;//選択された時にオブジェクトをどれくらい持ち上げるか。

        GetComponent<Rigidbody>().isKinematic = true;

        //選択を解除するときの条件
        if(!select)
        {
            pos.y = 1.35f;
            GetComponent<Rigidbody>().isKinematic = false;
        }
        transform.position = pos;
    }

    //移動可能範囲の取得
    public List<Vector2Int> GetMovableTiles(UnitsController[ , ] units , bool checkking = true)
    {
        List<Vector2Int> ret = new List<Vector2Int>();
        
        if(TYPE.PAWN == type)
        {
            ret = GetNormalMovableTiles (units, type);
        }
        else if (TYPE.ROOK   == type)
        {
            ret = GetNormalMovableTiles (units , type);
        }
        else if (TYPE.KNIGHT == type)
        {
            ret = GetNormalMovableTiles (units , type);
        }
        else if (TYPE.BISHOP == type)
        {
            ret = GetNormalMovableTiles (units , type);
        }
        else if (TYPE.KING   == type)
        {
            ret = GetNormalMovableTiles(units, type);

            //相手の移動範囲を考慮しない。
            if (!checkking) return ret;

            //TODO 相手の移動可能範囲には行けないようにする。

        }
        else if(TYPE.QUEEN == type)
        {
            ret = GetNormalMovableTiles(units, type);
        }

        return ret;
    }


    //移動可能範囲を返す（ノーマルな状態）
    List<Vector2Int> GetNormalMovableTiles(UnitsController[,] units , TYPE type)
    {
        List<Vector2Int> ret = new List<Vector2Int>();

        //ポーンの移動処理
        if (TYPE.PAWN == type)
        {
            var direction = 1;
            if (1 == player){ direction = -1; }

            //前方2マスの場合(初動の処理)
            List<Vector2Int> vector = new List<Vector2Int>()
            {
                new Vector2Int(0 , 1 * direction),//通常
                new Vector2Int(0 , 2 * direction),//初動のみ
            };

            //二回目以降は１マスしか進めない
            if (-1 < progressTurnCount) { vector.RemoveAt(vector.Count - 1); }

            //前方の処理
            foreach(var n in vector)
            {
                Vector2Int checkPosition = position + n;

                if (!isCheckable(units, checkPosition)) continue;
                if (null != units[checkPosition.x, checkPosition.y]) break;

                ret.Add(checkPosition);
            }

            //敵の首を取れるときは斜めに移動できる。
            vector = new List<Vector2Int>()
            {
                new Vector2Int(-1 , 1 * direction),
                new Vector2Int( 1 , 1 * direction),
            };

            foreach(var n in vector)
            {
                Vector2Int checkPosition = position + n;
                if (!isCheckable(units, checkPosition)) continue;

                //アンパッサンの処理
                if(null != GetEnPassantUnit(units , checkPosition))
                {
                    ret.Add(checkPosition);
                    continue;
                }


                /*敵が居なかった時は移動できない、見方は対象外にする。*/
                if (null   == units[checkPosition.x, checkPosition.y])        continue;
                if (player == units[checkPosition.x, checkPosition.y].player) continue;

                ret.Add(checkPosition);
            }
        }
        //ルークの移動処理
        else if (TYPE.ROOK   == type)
        {
            //上下ユニットにぶつかるまでどこにでも進む
            List<Vector2Int> vector = new List<Vector2Int>()
            {
                new Vector2Int(0  , 1),
                new Vector2Int(0  ,-1),
                new Vector2Int(1  , 0),
                new Vector2Int(-1 , 0),

            };

            foreach(var n in vector)
            {
                var checkPosition = position + n;
                
                while (isCheckable(units , checkPosition))
                {
                    //なんかいたら終了(味方だったら処理を抜ける)
                    if(null != units[checkPosition.x , checkPosition.y])
                    {
                        //味方じゃなかったら、首を取れ
                        if(player != units[checkPosition.x , checkPosition.y].player)
                        {
                            ret.Add(checkPosition);
                        }
                        break;
                    }
                    ret.Add(checkPosition);
                    checkPosition += n;
                }
            }
        }
        //ナイトの移動処理
        else if (TYPE.KNIGHT == type)
        {
            List<Vector2Int> vector = new List<Vector2Int>()
            {
                new Vector2Int(-1  ,  2),
                new Vector2Int(-2  ,  1),
                new Vector2Int( 1  ,  2),
                new Vector2Int( 2  ,  1),

                new Vector2Int(-1  , -2),
                new Vector2Int(-2  , -1),
                new Vector2Int( 1  , -2),
                new Vector2Int( 2  , -1),
            };

            foreach(var n in vector)
            {
                Vector2Int checkPosition = position + n;

                if (!isCheckable(units, checkPosition)) continue;

                //同じPlayerの場所には行けない。
                if (null != units[checkPosition.x, checkPosition.y]
                         && player == units[checkPosition.x, checkPosition.y].player)
                { continue; }

                ret.Add(checkPosition);
            }
        }
        //ビショップの移動処理
        else if (TYPE.BISHOP == type)
        {
            //斜めにぶつかるまでどこにでも進む
            List<Vector2Int> vector = new List<Vector2Int>()
            {
                new Vector2Int(1  ,  1),
                new Vector2Int(-1 ,  1),
                new Vector2Int(1  , -1),
                new Vector2Int(-1 , -1),

            };

            foreach (var n in vector)
            {
                var checkPosition = position + n;

                while (isCheckable(units, checkPosition))
                {
                    //なんかいたら終了(味方だったら処理を抜ける)
                    if (null != units[checkPosition.x, checkPosition.y])
                    {
                        //味方じゃなかったら、首を取れ
                        if (player != units[checkPosition.x, checkPosition.y].player)
                        {
                            ret.Add(checkPosition);
                        }
                        break;
                    }
                    ret.Add(checkPosition);
                    checkPosition += n;
                }
            }
        }
        //キングの移動処理
        else if (TYPE.KING   == type)
        {
            List<Vector2Int> vector = new List<Vector2Int>()
            {
                new Vector2Int(-1  ,  1),
                new Vector2Int( 0  ,  1),
                new Vector2Int( 1  ,  1),
                new Vector2Int(-1  ,  0),

                new Vector2Int( 1  ,  0),
                new Vector2Int(-1  , -1),
                new Vector2Int( 0  , -1),
                new Vector2Int( 1  , -1),
            };

            foreach (var n in vector)
            {
                Vector2Int checkPosition = position + n;

                if (!isCheckable(units, checkPosition)) continue;

                //同じPlayerの場所には行けない。
                if (null != units[checkPosition.x, checkPosition.y]
                         && player == units[checkPosition.x, checkPosition.y].player)
                { continue; }

                ret.Add(checkPosition);
            }
            // TODO : ここから下はキャスリングの処理

        }
        //クイーンの移動処理
        else if(TYPE.QUEEN == type)
        {
            //斜めにぶつかるまでどこにでも進む
            List<Vector2Int> vector = new List<Vector2Int>()
            {
                new Vector2Int(0  , 1),
                new Vector2Int(0  ,-1),
                new Vector2Int(1  , 0),
                new Vector2Int(-1 , 0),

                new Vector2Int(1  ,  1),
                new Vector2Int(-1 ,  1),
                new Vector2Int(1  , -1),
                new Vector2Int(-1 , -1),

            };

            foreach (var n in vector)
            {
                var checkPosition = position + n;

                while (isCheckable(units, checkPosition))
                {
                    //なんかいたら終了(味方だったら処理を抜ける)
                    if (null != units[checkPosition.x, checkPosition.y])
                    {
                        //味方じゃなかったら、首を取れ
                        if (player != units[checkPosition.x, checkPosition.y].player)
                        {
                            ret.Add(checkPosition);
                        }
                        break;
                    }
                    ret.Add(checkPosition);
                    checkPosition += n;
                }
            }
        }

        return ret;
    }

    bool isCheckable(UnitsController[ , ] array , Vector2Int index)
    {
        //配列オーバーするパターン(Not Good Pattern)
        if (index.x < 0 || array.GetLength(0) <= index.x
         || index.y < 0 || array.GetLength(1) <= index.y) { return false; }

        return true;
    }



    //移動処理
    public void MoveUnit(GameObject tile)
    {
        SelectUnit(false);

        Vector2Int index = new Vector2Int(
            (int)tile.transform.position.x + GameManager.CELL_X / 2,
            (int)tile.transform.position.z + GameManager.CELL_Y / 2);


        Vector3 pos = tile.transform.position;
        pos.y = 1.35f;
        transform.position = pos;

        //移動状態をリセット
        status.Clear();


        //アンパッサンドとかの処理
        if (TYPE.PAWN == type)
        {
            if (1 < Mathf.Abs(index.y - position.y))
            {
                status.Add(STATUS.EN_PASSANT);

                var direction = -1;

                if (1 == player) direction = 1;

                position.y = index.y + direction;
            }
        }

        //キャスティング
        if (TYPE.KING == type)
        {
            if (1 < index.x - position.x)
            {
                status.Add(STATUS.KSIDE_CASTLINE);
            }

            if (-1 > index.x - position.x)
            {
                status.Add(STATUS.QSIDE_CASTLINE);
            }
        }

        //インデックスの更新
        oldPosition = position;
        position = index;
        progressTurnCount = 0;
    }

    //ターン数を加算する処理
    public void ProgressTurn()
    {
        if (0 > progressTurnCount) return;
        progressTurnCount++;

        //アンパッサンフラグチェック
        if (TYPE.PAWN == type)
        {
            if (1 < progressTurnCount)
            {
                status.Remove(STATUS.EN_PASSANT);
            }
        }
    }

    UnitsController GetEnPassantUnit(UnitsController[,] units, Vector2Int pos)
    {
        foreach (var n in units)
        {
            if (null == n) continue;
            if (player == n.player) continue;
            if (!n.status.Contains(STATUS.EN_PASSANT)) continue;

            if (n.oldPosition == pos) return n;
        }
        return null;
    }

    public void SetCheckStatus(bool flag = true)
    {
        status.Remove(STATUS.CHECK);
        if (flag) status.Add(STATUS.CHECK);
    }

}
