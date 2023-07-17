using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitsController : MonoBehaviour
{
    //���j�b�g�̃v���C���[�ԍ�
    [SerializeField] public int player;
    
    //���j�b�g�̎��
    [SerializeField] public TYPE type;

    //�ړ����
    [SerializeField] public List<STATUS> status;

    //�o�߃^�[��
    public int progressTurnCount;

    //�u���Ă�ꏊ
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

    // �����ݒ�
    public void SetUnit(int player , TYPE type , GameObject tile)
    {
        this.player = player;
        this.type   = type;
        MoveUnit(tile);
        progressTurnCount = -1;
    }

    //�I�����̏���
    public void SelectUnit(bool select = true)
    {
        Vector3 position = transform.position;
        position.y      += 1.5f;//�I�����ꂽ���ɃI�u�W�F�N�g���ǂꂭ�炢�����グ�邩�B

        GetComponent<Rigidbody>().isKinematic = true;

        //�I������������Ƃ��̏���
        if( !select )
        {
            position.y = 1.35f;
            GetComponent<Rigidbody>().isKinematic = false;
        }
        transform.position = position;
    }

    //�ړ�����
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


        //�A���p�b�T���h�Ƃ��̏���
        if (TYPE.PAWN == type)
        {
            if(1 < Mathf.Abs(index.y - position.y))
            {
                status.Add(STATUS.EN_PASSANT);

                var direction = (1 == player) ? 1 : 0;
                position.y    = index.y + direction;
            }
        }

        //�L���X�e�B���O
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

        //�C���f�b�N�X�̍X�V
        oldPosition       = position;
        position          = index;
        progressTurnCount = 0;
    }

    public void ProgressTurn()
    {
        if (0 > progressTurnCount) return;
        progressTurnCount++;

        //�A���p�b�T���t���O�`�F�b�N
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
