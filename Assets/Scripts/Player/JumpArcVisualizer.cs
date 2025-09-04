//using System.Collections;
//using UnityEngine;
//using UnityEngine.UI;
//using UnityEngine.EventSystems;

//public class PlayedrController : MonoBehaviour
//{
//    [Header("Move Settings")]
//    public float moveSpeed = 5f;

//    [Header("Jump Settings (charge & direction)")]
//    public float minJumpForce = 25f;
//    public float maxJumpForce = 50f;
//    public float maxChargeTime = 1.2f;

//    [Tooltip("���� ���� ������, ������ � ������ �� �����")]
//    public float wallBounceFraction = 0.6f;
//    [Tooltip("����������� ��������� ����� ���� ���������� (0..1)")]
//    public float damping = 0.5f;
//    [Tooltip("������� ������ ����� ������ ��������� �� �����������")]
//    public float dampingExclusionTime = 0.35f;

//    [Header("Wall Bounce Tuning")]
//    [Tooltip("����������� �������������� �������� ����� ��������")]
//    public float minWallKickX = 12f;
//    [Tooltip("�����-�������� �� ����� ����� �������� (��������� ��� �������� �����������)")]
//    public float separationFromWall = 0.05f;         // ���� 0.02
//    [Tooltip("������� ��������, ����� �� ���������� ��������� ��� ������")]
//    public float wallBounceCooldown = 0.05f;

//    [Header("Wall Bounce Extra")]
//    [Tooltip("������� ������ ������������ ����� ����� �������� �� �����")]
//    public float ignoreGroundAfterBounce = 0.12f;
//    [Tooltip("���������������� ��� ����� ��� ��������, ����� X �� �������� (���������)")]
//    public float groundLiftEpsilon = 0.1f;           // ���� 0.05

//    [Header("Mobile Controls")]
//    public Joystick mobileJoystick;
//    public bool useMobileControls = false;

//    [Header("Jump Power Bar UI")]
//    public Image jumpPowerImage;
//    public Image jumpPowerBarBG;
//    public Image jumpBarFix;              // �����������
//    public Canvas uiCanvas;
//    public Vector3 barOffset = new Vector3(0f, -0.6f, 0f);
//    public Camera mainCamera;

//    [Header("Jump Button (Mobile)")]
//    public Button jumpButton;

//    [Header("Ground Check")]
//    public Transform groundCheck;
//    public LayerMask groundLayer;

//    [Header("Face Point")]
//    public Transform facePoint;

//    [Header("Debug")]
//    public bool debugBounceLogs = false;

//    // ���������
//    public bool isGrounded = true;
//    public bool canJump = true;
//    private bool isChargingJump = false;

//    // �������� ������
//    private float jumpChargeTimer = 0f;
//    private float jumpTime = 0f;
//    private float lastJumpTime = 0f;
//    private float jumpStartSpeed = 0f;
//    private bool isFacingRight = true;

//    [HideInInspector] public bool jumpButtonPressedFromMobile = false;

//    // ����������
//    private Rigidbody2D rb;

//    // �������: ������ �� ���������� X ����� �������� � �����
//    private float _suppressInputUntil = 0f;   // �� ����� ������� �� ������� X �� �����
//    private float _lastWallBounceAt = -999f;  // ������� ��������

//    private void Awake()
//    {
//        rb = GetComponent<Rigidbody2D>();
//        if (mainCamera == null) mainCamera = Camera.main;
//        if (uiCanvas == null) uiCanvas = FindObjectOfType<Canvas>();
//    }

//    private void Start()
//    {
//        SetupJumpButtonEvents();
//        UpdateJumpBar(0f);
//    }

//    private void Update()
//    {
//        CheckGrounded();
//        HandleInput();
//    }

//    private void LateUpdate()
//    {
//        UpdateJumpBarPosition();
//    }

//    // ---------- ��������� ----------

//    private void CheckGrounded()
//    {
//        if (groundCheck != null)
//            isGrounded = Physics2D.OverlapCircle(groundCheck.position, 0.1f, groundLayer);
//        else
//            isGrounded = Mathf.Abs(rb.velocity.y) < 0.01f;

//        canJump = isGrounded;
//    }

//    // ---------- ���������� ----------

//    private void HandleInput()
//    {
//        float moveInput = 0f;
//        bool jumpHeld = false;

//        // ���������� �� X ������ ���� �� ��������, ����� �� ����� � ��� ���������� ����� ��������
//        bool allowMoveInputOnGround =
//            !isChargingJump && isGrounded && Time.time >= _suppressInputUntil;

//        if (allowMoveInputOnGround)
//        {
//            if (useMobileControls && mobileJoystick != null)
//                moveInput = mobileJoystick.Horizontal;
//            else if (!useMobileControls && KeyBindingsManager.Instance != null)
//            {
//                if (Input.GetKey(KeyBindingsManager.Instance.GetBind("Left"))) moveInput -= 1f;
//                if (Input.GetKey(KeyBindingsManager.Instance.GetBind("Right"))) moveInput += 1f;
//            }

//            rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);

//            // ���� � ������� facePoint
//            if (moveInput > 0 && !isFacingRight) Flip();
//            else if (moveInput < 0 && isFacingRight) Flip();

//            if (facePoint != null)
//                facePoint.localPosition = isFacingRight ? new Vector3(+1f, 0.5f, 0f)
//                                                        : new Vector3(-1f, 0.5f, 0f);
//        }
//        else
//        {
//            // �� ����� ��� ������� ��������� X = 0. � ������� X �� �������!
//            if (isChargingJump && isGrounded)
//                rb.velocity = new Vector2(0f, rb.velocity.y);
//        }

//        // ������ ������ (�� + �������)
//        if (KeyBindingsManager.Instance != null)
//            jumpHeld = Input.GetKey(KeyBindingsManager.Instance.GetBind("Jump"));
//        jumpHeld = jumpHeld || jumpButtonPressedFromMobile;

//        // ������ �������
//        if (jumpHeld && canJump && !isChargingJump)
//        {
//            isChargingJump = true;
//            jumpChargeTimer = 0f;
//            jumpTime = Time.time;
//            jumpStartSpeed = rb.velocity.x;
//        }

//        // �������
//        if (isChargingJump && jumpHeld)
//        {
//            jumpChargeTimer += Time.deltaTime;
//            jumpChargeTimer = Mathf.Clamp(jumpChargeTimer, 0f, maxChargeTime);
//            UpdateJumpBar(jumpChargeTimer / maxChargeTime);
//        }

//        // ��������� � ������
//        if (isChargingJump && !jumpHeld)
//        {
//            lastJumpTime = Time.time;
//            float norm = Mathf.Clamp01(jumpChargeTimer / maxChargeTime);
//            float power = Mathf.Lerp(minJumpForce, maxJumpForce, norm);

//            DoJump(power);

//            isChargingJump = false;
//            jumpChargeTimer = 0f;
//            UpdateJumpBar(0f);
//            if (jumpBarFix != null) jumpBarFix.fillAmount = norm;
//        }
//    }

//    private void DoJump(float power)
//    {
//        if (!canJump) return;

//        // ��������� ��������� ������ �� X, ���������� ������ Y
//        rb.velocity = new Vector2(rb.velocity.x, 0f);

//        Vector2 direction = facePoint != null
//            ? (facePoint.position - transform.position).normalized
//            : (isFacingRight ? Vector2.right : Vector2.left);

//        rb.AddForce(direction * power, ForceMode2D.Impulse);

//        isGrounded = false;
//        canJump = false;
//    }

//    private void Flip()
//    {
//        isFacingRight = !isFacingRight;
//        var s = transform.localScale;
//        s.x *= -1f;
//        transform.localScale = s;
//    }

//    // ---------- �������� ----------

//    private void OnCollisionEnter2D(Collision2D collision)
//    {
//        if (collision.collider.isTrigger) return;

//        if (debugBounceLogs)
//            Debug.Log($"COLLISION tag={collision.gameObject.tag} contacts={collision.contactCount} vel={rb.velocity}");

//        if (collision.gameObject.CompareTag("Ground"))
//        {
//            isGrounded = true;
//            canJump = true;
//        }

//        if (collision.gameObject.CompareTag("Wall"))
//        {
//            TryBounceFromWall(collision);
//        }

//        if (collision.gameObject.CompareTag("Ceiling"))
//        {
//            BounceOffCeiling();
//        }
//    }

//    // NEW: ��������� �������� ��� ���������� �������� �� ������ ������������ ������
//    private void OnCollisionStay2D(Collision2D collision)
//    {
//        if (collision.collider.isTrigger) return;
//        if (!collision.gameObject.CompareTag("Wall")) return;

//        // ���� ������ ��� �������� � �������
//        if (Mathf.Abs(rb.velocity.x) < 0.01f && Mathf.Abs(rb.velocity.y) < 0.01f) return;

//        TryBounceFromWall(collision);
//    }

//    private void OnCollisionExit2D(Collision2D collision)
//    {
//        if (collision.gameObject.CompareTag("Ground") && !canJump)
//            isGrounded = false;
//    }

//    // --- �������: ����� �� X, � ������� � ������� ����� ---
//    private void TryBounceFromWall(Collision2D collision)
//    {
//        if (Time.time - _lastWallBounceAt < wallBounceCooldown) return;

//        // ���� ������� � ������������ |normal.x| (����� ����������� �������)
//        ContactPoint2D best = collision.GetContact(0);
//        float bestAbsX = Mathf.Abs(best.normal.x);
//        for (int i = 1; i < collision.contactCount; i++)
//        {
//            var c = collision.GetContact(i);
//            float ax = Mathf.Abs(c.normal.x);
//            if (ax > bestAbsX) { best = c; bestAbsX = ax; }
//        }

//        Vector2 n = best.normal;

//        // ������ "������� �����": ���������� �����, ����� �� ������ ��������� ����� ����
//        if (Mathf.Abs(n.x) < 0.9f) return;

//        // ���. ������: ��������� �� � �����?
//        float dot = Vector2.Dot(rb.velocity, -n);
//        bool verticalWall = Mathf.Abs(n.x) > 0.9f;
//        bool slidingDown = rb.velocity.y < -0.1f;

//        // ���� ��� �������� � ����� � ��� �� ������ ������� ����� ������������ ������ � �������
//        if (dot <= 0.01f && !(verticalWall && slidingDown))
//            return;

//        // ���� �� ������� (���������/�������)
//        float normCharge = (lastJumpTime > jumpTime)
//            ? Mathf.Clamp01((lastJumpTime - jumpTime) / maxChargeTime)
//            : Mathf.Clamp01(jumpChargeTimer / maxChargeTime);
//        float jumpForce = Mathf.Lerp(minJumpForce, maxJumpForce, normCharge);

//        // ������� X ����� ��������
//        float baseX = Mathf.Max(
//            minWallKickX,
//            Mathf.Abs(rb.velocity.x),
//            wallBounceFraction * jumpForce
//        );

//        float signAway = -Mathf.Sign(n.x);
//        float damp = (Time.time - lastJumpTime < dampingExclusionTime) ? 1f : Mathf.Clamp01(damping);

//        // ������ ������ X, Y �� �������
//        Vector2 vel = rb.velocity;
//        vel.x = signAway * baseX * damp;
//        rb.velocity = vel;

//        // ���������: �������� ������ X �� ��������� ����. ����,
//        // ����� ������/����� � ������� ����� �� ������ �������
//        ReapplyHorizontalNextFixed(vel.x);

//        // ����������� �� �����
//        rb.position += n * separationFromWall;

//        // ���� � ���� �� ������ ����� �� ����� � ���� �����������, ����� ��� �� ����� X
//        if (groundCheck != null && Physics2D.OverlapCircle(groundCheck.position, 0.1f, groundLayer))
//        {
//            rb.position += Vector2.up * groundLiftEpsilon;
//            isGrounded = false;
//            canJump = false;
//        }

//        // �� ����� ������� �����, ����� X �� ��������� � ��� �� ���. ���
//        StartCoroutine(IgnoreGroundForSeconds(ignoreGroundAfterBounce));

//        // � �� ������� ���� �� X �� ����� �������� ����
//        _suppressInputUntil = Time.time + ignoreGroundAfterBounce;

//        _lastWallBounceAt = Time.time;

//        if (debugBounceLogs)
//            Debug.Log($"WALL BOUNCE X-ONLY: n={n}, velAfter={rb.velocity}, baseX={baseX:F2}, damp={damp:F2}");
//    }

//    private IEnumerator IgnoreGroundForSeconds(float seconds)
//    {
//        int playerLayer = gameObject.layer;
//        int mask = groundLayer.value;

//        for (int i = 0; i < 32; i++)
//            if ((mask & (1 << i)) != 0)
//                Physics2D.IgnoreLayerCollision(playerLayer, i, true);

//        yield return new WaitForSeconds(seconds);

//        for (int i = 0; i < 32; i++)
//            if ((mask & (1 << i)) != 0)
//                Physics2D.IgnoreLayerCollision(playerLayer, i, false);
//    }

//    private void BounceOffCeiling()
//    {
//        rb.velocity = new Vector2(rb.velocity.x, -rb.velocity.y);
//    }

//    // --- ��������� X-�������� �� ��������� ���������� ��� ---
//    private void ReapplyHorizontalNextFixed(float vx)
//    {
//        StartCoroutine(_ReapplyHorizontalNextFixed(vx));
//    }
//    private IEnumerator _ReapplyHorizontalNextFixed(float vx)
//    {
//        yield return new WaitForFixedUpdate();
//        var v = rb.velocity;
//        v.x = vx;
//        rb.velocity = v;
//    }

//    // ---------- ������� ���� ----------

//    private void UpdateJumpBar(float normalized)
//    {
//        bool show = normalized > 0f;
//        if (jumpPowerImage != null)
//        {
//            jumpPowerImage.enabled = show;
//            jumpPowerImage.fillAmount = Mathf.Clamp01(normalized);
//        }
//        if (jumpPowerBarBG != null)
//            jumpPowerBarBG.enabled = show;
//    }

//    private void UpdateJumpBarPosition()
//    {
//        if ((jumpPowerImage == null && jumpPowerBarBG == null) || mainCamera == null || uiCanvas == null)
//            return;

//        Vector3 worldPos = transform.position + barOffset;
//        Vector3 screenPos = mainCamera.WorldToScreenPoint(worldPos);

//        RectTransform canvasRect = uiCanvas.GetComponent<RectTransform>();
//        if (canvasRect == null) return;

//        Vector2 uiPos;
//        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPos, mainCamera, out uiPos))
//        {
//            if (jumpPowerImage != null) jumpPowerImage.rectTransform.anchoredPosition = uiPos;
//            if (jumpPowerBarBG != null) jumpPowerBarBG.rectTransform.anchoredPosition = uiPos;
//            if (jumpBarFix != null && jumpBarFix.enabled) jumpBarFix.rectTransform.anchoredPosition = uiPos;
//        }
//    }

//    // ---------- ���. ������ ----------

//    private void SetupJumpButtonEvents()
//    {
//        if (jumpButton == null) return;

//        EventTrigger trigger = jumpButton.GetComponent<EventTrigger>();
//        if (trigger == null) trigger = jumpButton.gameObject.AddComponent<EventTrigger>();

//        var entryDown = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
//        entryDown.callback.AddListener((_) => OnJumpButtonDown());
//        trigger.triggers.Add(entryDown);

//        var entryUp = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
//        entryUp.callback.AddListener((_) => OnJumpButtonUp());
//        trigger.triggers.Add(entryUp);
//    }

//    public void OnJumpButtonDown() { jumpButtonPressedFromMobile = true; }
//    public void OnJumpButtonUp() { jumpButtonPressedFromMobile = false; }

//    // ---------- ������ ��� ������������� ----------
//    public bool IsChargingJump() => isChargingJump;
//    public float GetJumpCharge() => Mathf.Clamp01(jumpChargeTimer / maxChargeTime);
//    public Vector2 GetJumpDirection()
//    {
//        return facePoint != null
//            ? (facePoint.position - transform.position).normalized
//            : (isFacingRight ? Vector2.right : Vector2.left);
//    }
//}
