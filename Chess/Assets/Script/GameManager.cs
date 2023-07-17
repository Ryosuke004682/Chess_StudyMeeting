using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public const int CELL_X     = 8;
    public const int CELL_Y     = 8;
           const int PLAYER_MAX = 2;

    //�^�C���̃v���n�u
    public GameObject[] prefabTile;

    //�J�[�\���̃v���n�u
    public GameObject prefabCursor;

    //�����f�[�^
    GameObject     [,] cells;
    UnitsController[,] units;


    //���j�b�g�̃v���n�u
    public List<GameObject> prefab_WhiteUnits;
    public List<GameObject> prefab_BlackUnits;


    private void Start()
    {
        //�Ֆʂ𐶐�����B
        for(var X_Axis = 0; X_Axis < CELL_X; X_Axis++)
        {
            for (var Y_Axis = 0; Y_Axis < CELL_Y; Y_Axis++)
            {
                //�^�C���ƃ��j�b�g�̃|�W�V�����𐶐�����B
                var x = X_Axis - CELL_X / 2;//��
                var y = Y_Axis - CELL_Y / 2;//�c


                var createPosition = new Vector3(x , 0 , y);

                //�^�C���𐶐�
                var index = (X_Axis + Y_Axis) % 2;//0��1�����݂Ɏw��

                GameObject tiles = Instantiate(prefabTile[index] , createPosition , Quaternion.identity);

                cells[X_Axis , Y_Axis] = tiles;
            }
        }
    }
}
