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

    //��̎��
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

    //����ȃ��[��
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
        Vector3 pos = transform.position;
        pos.y      += 2;//�I�����ꂽ���ɃI�u�W�F�N�g���ǂꂭ�炢�����グ�邩�B

        GetComponent<Rigidbody>().isKinematic = true;

        //�I������������Ƃ��̏���
        if(!select)
        {
            pos.y = 1.35f;
            GetComponent<Rigidbody>().isKinematic = false;
        }
        transform.position = pos;
    }


    //�ړ��\�͈͂̎擾
    public List<Vector2Int> GetMovableTiles(UnitsController[ , ] units , bool checkking = true)
    {
        List<Vector2Int> ret = new List<Vector2Int>();
       
        //�N�C�[��
        if(TYPE.QUEEN == type)
        {
            //���[�N�ƃr�V���b�v�̓���������
            ret = GetNormalMovableTiles(units, TYPE.ROOK);
            
            foreach(var n in GetNormalMovableTiles(units , TYPE.BISHOP))
            {
                if (!ret.Contains(n)) { ret.Add(n); }
            }
        }
        else if (TYPE.KING == type)
        {
            ret = GetNormalMovableTiles(units, TYPE.KING);

            //����̈ړ��͈͂��l�����Ȃ�
            if (!checkking){ return ret; }

            //�폜�Ώۂ̃^�C��
            List<Vector2Int> removetile = new List<Vector2Int>();


            //�G�̈ړ��\�͈͂܂ōs�����Ȃ�
            foreach (var n in ret)
            {
                UnitsController[,] copyunits2 = GameManager.GetCopyArracy(units);

                Debug.Log(copyunits2);

                copyunits2[position.x, position.y] = null;
                copyunits2[n.x, n.y]               = this;

                var checkCount = GameManager.GetCheckUnits(copyunits2, player, false).Count;

                if (0 < checkCount) removetile.Add(n);
            }

            //�G�̈ړ��\�͈͂Ɣ��^�C��������
            foreach (var n in removetile)
            {
                ret.Remove(n);

                //�L���X�����O�ł��鎞�����^���̃^�C�����S�č폜����
                if (-1 < progressTurnCount || position.y != n.y) { continue; }

                //����
                var direction = 1;
                var control =  units.GetLength(0);

                if (0 > position.x - n.x) direction = 1;
                
                for(var i = 0; i < control; i++)
                {
                    var del = new Vector2Int(n.x + (i * direction), n.y);
                    ret.Remove(del);
                }
            }
        }
        else
        {
            ret = GetNormalMovableTiles(units , type);
        }
        return ret;
    }


    //�ړ��\�͈͂�Ԃ��i�m�[�}���ȏ�ԁj
    List<Vector2Int> GetNormalMovableTiles(UnitsController[,] units, TYPE type)
    {
        List<Vector2Int> ret = new List<Vector2Int>();

        //�|�[���̈ړ�����
        if (TYPE.PAWN == type)
        {
            var direction = 1;
            if (1 == player) { direction = -1; }

            //�O��2�}�X�̏ꍇ(�����̏���)
            List<Vector2Int> vector = new List<Vector2Int>()
            {
                new Vector2Int(0 , 1 * direction),//�ʏ�
                new Vector2Int(0 , 2 * direction),//�����̂�
            };

            //���ڈȍ~�͂P�}�X�����i�߂Ȃ�
            if (-1 < progressTurnCount) { vector.RemoveAt(vector.Count - 1); }

            //�O���̏���
            foreach (var n in vector)
            {
                Vector2Int checkPosition = position + n;

                if (!IsCheckable(units, checkPosition)) continue;
                if (null != units[checkPosition.x, checkPosition.y]) break;

                ret.Add(checkPosition);
            }

            //�G�̎������Ƃ��͎΂߂Ɉړ��ł���B
            vector = new List<Vector2Int>()
            {
                new Vector2Int(-1 , 1 * direction),
                new Vector2Int( 1 , 1 * direction),
            };

            foreach (var n in vector)
            {
                Vector2Int checkPosition = position + n;
                if (!IsCheckable(units, checkPosition)) continue;

                //�A���p�b�T���̏���
                if (null != GetEnPassantUnit(units, checkPosition))
                {
                    ret.Add(checkPosition);
                    continue;
                }

                /*�G�����Ȃ��������͈ړ��ł��Ȃ��A�����͑ΏۊO�ɂ���B*/
                if (null == units[checkPosition.x, checkPosition.y]) continue;
                if (player == units[checkPosition.x, checkPosition.y].player) continue;

                ret.Add(checkPosition);
            }
        }

        //���[�N�̈ړ�����
        if      (TYPE.ROOK   == type)
        {
            //�㉺���j�b�g�ɂԂ���܂łǂ��ɂł��i��
            List<Vector2Int> vector = new List<Vector2Int>()
            {
                new Vector2Int(0  , 1),
                new Vector2Int(0  ,-1),
                new Vector2Int(1  , 0),
                new Vector2Int(-1 , 0),

            };

            foreach (var n in vector)
            {
                var checkPosition = position + n;

                while (IsCheckable(units, checkPosition))
                {
                    //�Ȃ񂩂�����I��(�����������珈���𔲂���)
                    if (null != units[checkPosition.x, checkPosition.y])
                    {
                        //��������Ȃ�������A������
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
        //�i�C�g�̈ړ�����
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

            foreach (var n in vector)
            {
                Vector2Int checkPosition = position + n;

                if (!IsCheckable(units, checkPosition)) { continue; }

                //����Player�̏ꏊ�ɂ͍s���Ȃ��B
                if (null  != units[checkPosition.x, checkPosition.y]
                && player == units[checkPosition.x, checkPosition.y].player)
                {
                    continue; 
                }

                ret.Add(checkPosition);
            }
        }
        //�r�V���b�v�̈ړ�����
        else if (TYPE.BISHOP == type)
        {
            //�΂߂ɂԂ���܂łǂ��ɂł��i��
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

                while (IsCheckable(units, checkPosition))
                {
                    //�Ȃ񂩂�����I��(�����������珈���𔲂���)
                    if (null != units[checkPosition.x, checkPosition.y])
                    {
                        //��������Ȃ�������A������
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
        //�L���O�̈ړ�����
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

                if (!IsCheckable(units, checkPosition)) { continue; }

                //����Player�̏ꏊ�ɂ͍s���Ȃ��B
                if (null  != units[checkPosition.x, checkPosition.y]
                && player == units[checkPosition.x, checkPosition.y].player)
                {
                    continue; 
                }

                ret.Add(checkPosition);
            }

            //�L���X�����O�̏���

            //��������Ȃ�
            if (-1 != progressTurnCount) return ret;

            //�`�F�b�N����Ă���
            if (status.Contains(STATUS.CHECK)) return ret;

            vector = new List<Vector2Int>()
            {
                new Vector2Int(-2 , 0),
                new Vector2Int( 2 , 0),
            };

            foreach (var n in vector)
            {
                //�����̃��[�N
                var positionX =  0;
                var direction = -1;

                //�E���̃��[�N
                if( 0 < n.x)
                {
                    direction = 1;
                    positionX = units.GetLength(0) - 1;
                }

                //�[�ɂ��邩�ǂ���
                if (null == units[positionX, position.y])                 { continue; }

                //���[�N����Ȃ��B
                if (TYPE.ROOK != units[positionX, position.y].type)       { continue; }

                //����
                if (-1 != units[positionX, position.y].progressTurnCount) { continue; }


                //�ړ�����r���ɉ�����������
                bool other_OnLine = true;

                int control = Mathf.Abs(position.x - positionX);

                for(var i = 1; i < control; i++)
                {
                    if (null != units[position.x +(i * direction) , position.y])
                    {
                        other_OnLine = false;//�i�����ɒN�����Ȃ�
                    }
                }

                if (!other_OnLine) { continue; }

                var checkPosition = position + n;
                ret.Add(checkPosition);
                
            }

        }

        return ret;
    }
        

    //�ړ��\�͈͂��z������ǂ���
    bool IsCheckable(UnitsController[ , ] array , Vector2Int index)
    {
        //�z��I�[�o�[����p�^�[��(Not Good Pattern)
        if (index.x < 0 || array.GetLength(0) <= index.x
         || index.y < 0 || array.GetLength(1) <= index.y) { return false; }

        return true;
    }


    //�ړ�����
    public void MoveUnit(GameObject tile)
    {
        //�ړ����͔�I����Ԃɂ���B
        SelectUnit(false);

        //�^�C���̃|�W�V��������z��ԍ���n���B
        Vector2Int index = new Vector2Int(
            (int)tile.transform.position.x + GameManager.CELL_X / 2,
            (int)tile.transform.position.z + GameManager.CELL_Y / 2);


        Vector3 pos = tile.transform.position;
        pos.y = 1.35f;
        transform.position = pos;

        //�ړ���Ԃ����Z�b�g
        status.Clear();


        //�A���p�b�T���h�Ƃ��̏���
        if (TYPE.PAWN == type)
        {
            if (1 < Mathf.Abs(index.y - position.y))//��Βl�����
            {
                status.Add(STATUS.EN_PASSANT);

                //�G���ƁA�������Ői�ޕ������Ⴍ�Ȃ邩�炻�̒����i�G�̓}�C�i�X�����ɐi�ށA�����̓v���X�����ɐi�ށj
                var direction = -1;

                if (1 == player) direction = 1;

                position.y = index.y + direction;
            }
        }

        //�L���X�����O
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

        //�C���f�b�N�X�̍X�V
        oldPosition = position;
        position = index;
        progressTurnCount = 0;
    }


    //�^�[���������Z���鏈��
    public void ProgressTurn()
    {
        if (0 > progressTurnCount) return;
        progressTurnCount++;

        //�A���p�b�T���t���O�`�F�b�N
        if (TYPE.PAWN == type)
        {
            if (1 < progressTurnCount)
            {
                status.Remove(STATUS.EN_PASSANT);
            }
        }
    }

    //����̃A���p�b�T����Ԃ̃��j�b�g��Ԃ��B
    UnitsController GetEnPassantUnit(UnitsController[,] units, Vector2Int pos)
    {
        foreach (var n in units)
        {
            if (null   == n)        continue;
            if (player == n.player) continue;
            if (!n.status.Contains(STATUS.EN_PASSANT)) continue;

            if (n.oldPosition == pos) return n;
        }
        return null;
    }


    public void SetCheckStatus(bool flag = true)
    {
        status.Remove(STATUS.CHECK);
        if (flag) { status.Add(STATUS.CHECK); }
    }

}
