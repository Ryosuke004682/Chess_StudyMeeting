using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    //タイトルシーンで、カメラを回転させる。

    public bool IsRotate = false;

    //どこを中心に回るのか
    Vector3 lookAt_Camera = Vector3.zero;
    Vector2 prevPosition;


    private void Start()
    {
        
    }


    private void Update()
    {
        if(IsRotate)
        {
            transform.RotateAround(lookAt_Camera , Vector3.up , 0.1f);
            return;
        }

        //ゲーム中にもカメラを回転させられるように設定する。
        if(Input.GetMouseButtonDown(0))
        {
            prevPosition = Input.mousePosition;//押されてる場所を取得
        }
        else if(Input.GetMouseButton(0))//押されてる時
        {
            var value = prevPosition.x - Input.mousePosition.x;
            value *= -0.1f; //回してる方向と逆向きに回すので-1を掛ける

            transform.RotateAround(lookAt_Camera , Vector3.up , value);
            prevPosition = Input.mousePosition;
        }
    }
}
