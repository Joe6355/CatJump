using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour
{
    // --- Движение и прыжок ---
    public float moveSpeed = 5f;
    public float jumpLength = 5f;
    public float maxJumpForce = 20f;
    public float jumpTimeLimit = 1f;
    public float wallBounceFraction = 0.33f;
    public float damping = 0.5f;
    public float dampingExclusionTime = 0.2f;
    public float jumpCooldown = 0.5f; // Кулдаун на прыжок

    public bool isGrounded = true;
    public bool canJump = true;
    private float jumpTime = 0f;
    [SerializeField] private float lastJumpTime = 0.3f;
    private float lastJumpEndTime = 0f; // Время окончания последнего прыжка
    private bool isFacingRight = true;
    private Rigidbody2D rb;
    private float jumpStartSpeed;
    private Animator animator;

    // --- UI Шкала прыжка ---
    [Header("UI Jump Bar")]
    public Image jumpBarFill; // Заполняемая часть
    public Image jumpBarBG; // Фон
    public float maxJumpBarFill = 1.0f;
    public Camera mainCamera;
    public Canvas uiCanvas;
    public Vector3 barOffset = new Vector3(0, 2f, 0);

    // --- Управление ---
    [Header("Управление")]
    public bool useMobileControls = false; // переключается из Settings.cs
    public Joystick mobileJoystick; // укажи в инспекторе
    public Button mobileJumpButton; // кнопка прыжка
    private bool isMobileJumpButtonHeld = false; // отслеживает удержание кнопки

    // Новая переменная для отслеживания состояния зарядки прыжка
    private bool isChargingJump = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        lastJumpEndTime = -jumpCooldown; // Устанавливаем значение, чтобы прыжок был доступен сразу

        // Настройка мобильного управления
        if (mobileJumpButton != null)
        {
            // Добавляем обработчики для нажатия и отпускания кнопки
            EventTrigger trigger = mobileJumpButton.gameObject.AddComponent<EventTrigger>();

            // Обработчик нажатия
            var pointerDown = new EventTrigger.Entry();
            pointerDown.eventID = EventTriggerType.PointerDown;
            pointerDown.callback.AddListener((e) => OnMobileJumpButtonDown());
            trigger.triggers.Add(pointerDown);

            // Обработчик отпускания
            var pointerUp = new EventTrigger.Entry();
            pointerUp.eventID = EventTriggerType.PointerUp;
            pointerUp.callback.AddListener((e) => OnMobileJumpButtonUp());
            trigger.triggers.Add(pointerUp);
        }
    }

    void Update()
    {
        UpdateJumpBarPosition();
        float horizontalInput = 0f;

        // --- Управление с ПК ---
        if (!useMobileControls)
        {
            horizontalInput = Input.GetAxis("Horizontal");

            // Обновление шкалы прыжка при удержании Space
            if (Input.GetKey(KeyCode.Space) && isGrounded && canJump && (Time.time - lastJumpEndTime >= jumpCooldown))
            {
                isChargingJump = true; // Начинаем зарядку прыжка
                float jumpDuration = Mathf.Clamp(Time.time - jumpTime, 0f, jumpTimeLimit);
                float normalizedJumpDuration = jumpDuration / jumpTimeLimit;
                UpdateJumpBar(normalizedJumpDuration);
            }
            else if (!Input.GetKey(KeyCode.Space) && canJump)
            {
                isChargingJump = false; // Заканчиваем зарядку прыжка
                UpdateJumpBar(0f);
            }

            if (Input.GetKeyDown(KeyCode.Space) && isGrounded && canJump && (Time.time - lastJumpEndTime >= jumpCooldown))
            {
                jumpTime = Time.time;
            }

            if (Input.GetKeyUp(KeyCode.Space) && isGrounded && canJump && (Time.time - lastJumpEndTime >= jumpCooldown))
            {
                isChargingJump = false; // Заканчиваем зарядку прыжка
                PerformJump();
            }
        }
        else // --- Мобильное управление ---
        {
            if (mobileJoystick != null)
                horizontalInput = mobileJoystick.Horizontal;

            // Обновление шкалы прыжка при удержании кнопки
            if (isMobileJumpButtonHeld && isGrounded && canJump && (Time.time - lastJumpEndTime >= jumpCooldown))
            {
                isChargingJump = true; // Начинаем зарядку прыжка
                float jumpDuration = Mathf.Clamp(Time.time - jumpTime, 0f, jumpTimeLimit);
                float normalizedJumpDuration = jumpDuration / jumpTimeLimit;
                UpdateJumpBar(normalizedJumpDuration);
            }
            else if (!isMobileJumpButtonHeld && canJump)
            {
                isChargingJump = false; // Заканчиваем зарядку прыжка
                UpdateJumpBar(0f);
            }
        }

        // Движение - замораживаем, если заряжаем прыжок
        if (canJump && !isChargingJump && !(useMobileControls && isMobileJumpButtonHeld))
        {
            Vector2 movement = new Vector2(horizontalInput * (moveSpeed - 2.5f), rb.velocity.y);
            rb.velocity = movement;

            if (horizontalInput > 0 && !isFacingRight)
                Flip();
            else if (horizontalInput < 0 && isFacingRight)
                Flip();
        }
        else if (isChargingJump)
        {
            // Замораживаем горизонтальное движение при зарядке прыжка
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
    }

    private void PerformJump()
    {
        lastJumpTime = Time.time;
        lastJumpEndTime = Time.time; // Записываем время окончания прыжка
        float jumpDuration = Mathf.Clamp(lastJumpTime - jumpTime, 0f, jumpTimeLimit);
        float jumpForce = CalculateJumpForce(jumpDuration);
        Jump(jumpForce);
        UpdateJumpBar(0f);
    }

    // Обработчики для мобильной кнопки прыжка
    private void OnMobileJumpButtonDown()
    {
        if (!useMobileControls || !isGrounded || !canJump || (Time.time - lastJumpEndTime < jumpCooldown)) return;

        isMobileJumpButtonHeld = true;
        jumpTime = Time.time;
    }

    private void OnMobileJumpButtonUp()
    {
        if (!useMobileControls || !isMobileJumpButtonHeld) return;

        isMobileJumpButtonHeld = false;
        if (isGrounded && canJump && (Time.time - lastJumpEndTime >= jumpCooldown))
        {
            PerformJump();
        }
    }

    float CalculateJumpForce(float duration)
    {
        return Mathf.Clamp01(duration / jumpTimeLimit) * maxJumpForce;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
            canJump = true;
            isChargingJump = false; // Сбрасываем состояние зарядки при приземлении
            animator.SetBool("IsWallBouncing", false);
        }
        else if (collision.gameObject.CompareTag("Wall") && !isGrounded)
        {
            Vector3 contactNormal = collision.contacts[0].normal;
            if (Mathf.Abs(contactNormal.x) > 0.9f)
            {
                float jumpForce = CalculateJumpForce(lastJumpTime - jumpTime);
                BounceOffWall(jumpForce, jumpStartSpeed);
                animator.SetBool("IsWallBouncing", true);
            }
        }
        else if (collision.gameObject.CompareTag("Ceiling"))
        {
            BounceOffCeiling();
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground") && !canJump)
            isGrounded = false;
    }

    void Jump(float force)
    {
        Vector2 jumpVelocity = new Vector2((isFacingRight ? 1 : -1) * moveSpeed, force);
        rb.velocity = jumpVelocity;
        isGrounded = false;
        canJump = false;
    }

    void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    void BounceOffWall(float jumpForce, float jumpStartSpeed)
    {
        float currentDamping = Time.time - lastJumpTime < dampingExclusionTime ? 1f : damping;
        float wallBounceForce = jumpForce * wallBounceFraction * Mathf.Sign(jumpStartSpeed) * currentDamping;
        rb.velocity = new Vector2((isFacingRight ? -1 : 1) * wallBounceForce, rb.velocity.y);
    }

    void BounceOffCeiling()
    {
        rb.velocity = new Vector2(rb.velocity.x, -rb.velocity.y);
    }

    // --- UI ---
    private void UpdateJumpBar(float normalized)
    {
        bool show = normalized > 0f;
        if (jumpBarFill != null)
        {
            jumpBarFill.enabled = show;
            jumpBarFill.fillAmount = Mathf.Clamp01(normalized);
        }
        if (jumpBarBG != null)
            jumpBarBG.enabled = show;
    }

    private void UpdateJumpBarPosition()
    {
        if ((jumpBarFill == null && jumpBarBG == null) || mainCamera == null || uiCanvas == null)
            return;

        Vector3 worldPos = transform.position + barOffset;
        Vector3 screenPos = mainCamera.WorldToScreenPoint(worldPos);
        Vector2 uiPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            uiCanvas.transform as RectTransform,
            screenPos,
            mainCamera,
            out uiPos
        );

        if (jumpBarFill != null)
            jumpBarFill.rectTransform.anchoredPosition = uiPos;
        if (jumpBarBG != null)
            jumpBarBG.rectTransform.anchoredPosition = uiPos;
    }
}