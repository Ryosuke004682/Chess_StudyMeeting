using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    //�^�C�g���V�[���ŁA�J��������]������B

    public bool IsRotate = false;

    //�ǂ��𒆐S�ɉ��̂�
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

        //�Q�[�����ɂ��J��������]��������悤�ɐݒ肷��B
        if(Input.GetMouseButtonDown(0))
        {
            prevPosition = Input.mousePosition;//������Ă�ꏊ���擾
        }
        else if(Input.GetMouseButton(0))//������Ă鎞
        {
            var value = prevPosition.x - Input.mousePosition.x;
            value *= -0.1f; //�񂵂Ă�����Ƌt�����ɉ񂷂̂�-1���|����

            transform.RotateAround(lookAt_Camera , Vector3.up , value);
            prevPosition = Input.mousePosition;
        }
    }
}
