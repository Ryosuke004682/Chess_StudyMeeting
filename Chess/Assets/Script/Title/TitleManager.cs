using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    //ゲーム全体のPlayer数
    static public int playerCount;

    //タイルのプレハブ,カーソルのプレハブ
    public GameObject[] prefabTile;

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


    private void Start()
    {
        //盤面を生成
        for (var i = 0; i < GameManager.CELL_X; i++)
        {
            for (var j = 0; j < GameManager.CELL_Y; j++)
            {

                var x = i - GameManager.CELL_X / 2;//横
                var y = j - GameManager.CELL_Y / 2;//縦


                var createPosition = new Vector3(x, 0, y);//奥行を指定したいからyをzに格納

                //タイルを生成
                var index = (i + j) % 2;
                GameObject tiles = Instantiate(prefabTile[index], createPosition, Quaternion.identity);


                //ユニットを作成
                var type = unitsType[i, j] % 10;
                var player = unitsType[i, j] / 10;

                GameObject prefab = GetPrefabUnit(player, type);


                GameObject unit = null;
                UnitsController controller = null;

                if (null == prefab) continue;

                createPosition.y += 1.5f;
                unit = Instantiate(prefab);


                //初期化状態
                controller = unit.GetComponent<UnitsController>();
                controller.SetUnit(player, (UnitsController.TYPE)type, tiles);
            }
        }
    }

    //ユニットのプレハブを取得
    GameObject GetPrefabUnit(int player, int type)
    {
        var index = type - 1;

        if (0 > index) { return null; }

        GameObject prefab = prefab_WhiteUnits[index];

        if (1 == player) { prefab = prefab_BlackUnits[index]; }

        return prefab;
    }


    public void PvP()
    {
        playerCount = 2;

        SceneManager.LoadScene("MainScene");
    }

    public void PvE()
    {
        playerCount = 1;

        SceneManager.LoadScene("MainScene");
    }
    public void EvE()
    {
        playerCount = 0;

        SceneManager.LoadScene("MainScene");
    }



}
