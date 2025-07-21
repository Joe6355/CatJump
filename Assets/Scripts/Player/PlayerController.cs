using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    //����� ��� ���������
    [SerializeField] public float speed;//c������� ������ 
    [SerializeField] public float jump;//������
    [SerializeField] public float preparationJump;//���������� � ������

    //������� ����������
    [SerializeField] protected Rigidbody2D rb;
    [SerializeField] protected Animator anim;

    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    protected virtual void FixedUpdate()
    {    
        
    }
}
