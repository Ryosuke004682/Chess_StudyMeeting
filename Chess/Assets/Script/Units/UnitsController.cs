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
        Vector3 position = transform.position;
        position.y      += 1.5f;//選択された時にオブジェクトをどれくらい持ち上げるか。

        GetComponent<Rigidbody>().isKinematic = true;

        //選択を解除するときの条件
        if( !select )
        {
            position.y = 1.35f;
            GetComponent<Rigidbody>().isKinematic = false;
        }
        transform.position = position;
    }

    //移動処理
    public void MoveUnit(GameObject tile)
    {
        SelectUnit(false);

        Vector2Int index = new Vector2Int(
            (int)tile.transform.position.x + GameManager.CELL_X / 2,
            (int)tile.transform.position.z + GameManager.CELL_Y / 2);


        Vector3 pos = tile.transform.position;
        pos.y      += 1.35f;

        transform.position = pos;
        status.Clear();


        //アンパッサンドとかの処理
        if (TYPE.PAWN == type)
        {
            if(1 < Mathf.Abs(index.y - position.y))
            {
                status.Add(STATUS.EN_PASSANT);

                var direction = (1 == player) ? 1 : 0;
                position.y    = index.y + direction;
            }
        }

        //キャスティング
        if(TYPE.KING == type)
        {
            if(1  < index.x - position.x)
            {
                status.Add(STATUS.KSIDE_CASTLINE);
            }

            if(-1 > index.y - position.y)
            {
                status.Add(STATUS.QSIDE_CASTLINE);
            }
        }

        //インデックスの更新
        oldPosition       = position;
        position          = index;
        progressTurnCount = 0;
    }

    public void ProgressTurn()
    {
        if (0 > progressTurnCount) return;
        progressTurnCount++;

        //アンパッサンフラグチェック
        if(TYPE.PAWN == type)
        {
            if (1 < progressTurnCount)
            {
                status.Remove(STATUS.EN_PASSANT);
            }
        }
    }

    UnitsController GetEnPassantUnit(UnitsController[ , ] units , Vector2Int pos)
    {
        foreach(var n in units)
        {
            if (null == n) continue;
            if (!n.status.Contains(STATUS.EN_PASSANT)) continue;

            if (n.oldPosition == pos) return n;
        }
        return null;
    }

    public void SetCheckStatus(bool flag = true)
    {
        status.Remove(STATUS.CHECK);
        if(flag) status.Add(STATUS.CHECK);
    }

}
