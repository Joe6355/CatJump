using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    //Статы для настройки
    [SerializeField] public float speed;//cкорость ходьбы 
    [SerializeField] public float jump;//прыжок
    [SerializeField] public float preparationJump;//подготовка к прыжку

    //Рабочие компоненты
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
