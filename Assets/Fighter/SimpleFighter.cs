// Single, self-contained 2D fighter component. Drop on a GameObject with Rigidbody2D + Collider2D.
// Public API is minimal and REQUIRED. No public defaults. Component disables itself if any required field is unset.

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(-10)]
public class SimpleFighter : MonoBehaviour
{
    // ─────────────────────────────────────────────────────────────────────────────
    // REQUIRED PUBLIC CONFIG (no defaults; must be set in Inspector)
    // ─────────────────────────────────────────────────────────────────────────────
    [Header("Identity - REQUIRED")]
    [Tooltip("Name of this fighter")]
    public string fighterName;
    [Tooltip("Team number (fighters on same team won't hit each other)")]
    public int team;
    [Tooltip("UI slot ordering")]
    public int playerIndex;
    [Tooltip("Toggle simple AI input")]
    public bool isAI;
    [Tooltip("Visual root used for facing flip")]
    public Transform visualRoot;

    [Header("Components - REQUIRED")]
    public Rigidbody2D rb;
    public Collider2D bodyCollider;
    public Animator animator;

    [Header("Core Movement - REQUIRED")]
    [Tooltip("Walk speed in units/second")]
    public float walkSpeed;
    [Tooltip("Jump force")]
    public float jumpForce;
    [Tooltip("Layer mask for ground detection")]
    public LayerMask groundMask;

    [Header("Vitals - REQUIRED")]
    [Tooltip("Maximum hit points")]
    public float maxHP;
    [Tooltip("Maximum super meter value")]
    public int maxMeter;

    [Header("Move List - REQUIRED")]
    [Tooltip("Define all normal/special moves here")]
    public List<Attack> attacks;

    [Header("HUD - REQUIRED")]
    public HUDConfig hud;

    [Header("Animator Param Names - REQUIRED")]
    public string p_isGrounded;
    public string p_isWalking;
    public string p_isCrouching;
    public string p_isAttacking;
    public string p_isKO;
    public string p_speedX;
    public string p_speedY;
    [Tooltip("Optional trigger sent when any attack starts; set empty to skip")]
    public string p_triggerAttack;

    [Header("Controls - REQUIRED")]
    public InputBindings controls;

    // ─────────────────────────────────────────────────────────────────────────────
    // INTERNAL TUNABLES (private; sane defaults)
    // ─────────────────────────────────────────────────────────────────────────────
    // Facing/targeting
    bool autoFace = true;
    bool faceRight = true;
    bool flipVisualWithScale = true;
    const bool preferNearestTarget = true;
    const float retargetInterval = 0.2f;

    // Stage bounds
    bool clampToStage = true;
    float stageLeftX = -12f;
    float stageRightX = 12f;
    float floorY = -1000f;

    // Movement extra
    float backWalkSpeed = 4.25f;
    float dashSpeed = 10f;
    float airDrift = 6f;
    float accelGround = 60f;
    float accelAir = 30f;
    float friction = 30f;
    int maxJumps = 1;
    float coyoteTime = 0.08f;
    float jumpBuffer = 0.09f;
    float gravityScale = 3.5f;
    float fallGravityScale = 5.5f;
    float fastFallSpeed = 18f;
    float pushboxWidth = 0.9f;
    float pushboxHeight = 1.8f;
    float pushResolve = 0.1f;
    float groundProbeDistance = 0.12f;
    Vector2 groundProbeOffset = new Vector2(0f, -0.95f);

    // Combat core
    float hp;
    int meter;
    bool enableChip = true;
    float chipRate = 0.08f;
    float gutsMinDamage = 1f;

    // Stun/stop
    float hitstopOnHit = 0.06f;
    float hitstopOnBlock = 0.04f;
    float techWindow = 0.14f;
    float wakeupInvuln = 0.12f;

    // Juggle
    int juggleMax = 6;
    int juggleUsed = 0;
    float juggleResetTime = 1.2f;

    // Throws/Parry
    float throwRange = 1.3f;
    float throwStartup = 0.08f;
    float throwKD = 0.8f;
    int throwDamage = 140;
    float parryWindow = 0.06f;
    float parryFreezeBonus = 0.07f;

    // HUD runtime
    Canvas hudCanvas;
    RectTransform hudPanel;
    Image hpFill;
    Image hpChip;
    Image meterFill;
    Text nameText;
    float chipShown;
    Texture2D whiteTex;
    Sprite whiteSprite;

    // Runtime State
    public enum State { Idle, Walk, Jump, Fall, Crouch, Dash, Backdash, Attack, Hitstun, Blockstun, Knockdown, Tech, Grabbed, Throwing }
    State state = State.Idle;
    bool grounded;
    bool crouching;
    int jumpsUsed;
    float coyoteTimer;
    float jumpBufferTimer;
    float hitstunTimer;
    float blockstunTimer;
    float knockdownTimer;
    float freezeTimer;
    float invulnTimer;
    float techTimer;
    float lastGroundedY;
    float aiThinkTimer;
    float lastRetarget;
    float attackTimer;
    Attack currentAttack;
    float lastHitTime;
    bool holdingBack;
    bool holdingBlock;
    bool holdingParry;
    bool inputLight, inputMedium, inputHeavy, inputSpecial, inputThrow;
    bool inputJump, inputDash, inputDown, inputUp;
    float moveInput;
    Vector2 desiredVelocity;
    Vector2 externalVelocity;
    Vector2 lastVelocity;
    bool landingThisFrame;
    bool queuedReversal;
    HashSet<SimpleFighter> throwImmune = new HashSet<SimpleFighter>();
    static readonly List<SimpleFighter> registry = new List<SimpleFighter>();
    SimpleFighter cachedTarget;

    // ─────────────────────────────────────────────────────────────────────────────
    // Nested types
    // ─────────────────────────────────────────────────────────────────────────────
    public enum Button { Light, Medium, Heavy, Special, Throw, Parry, None }
    public enum Shape { Box, Circle }
    public enum Guard { High, Low, Mid, Unblockable, Throw }
    public enum CancelRule { None, OnHit, OnHitOrBlock, Always }

    [Serializable]
    public class InputBindings
    {
        [Header("Keyboard")]
        public KeyCode left;
        public KeyCode right;
        public KeyCode up;
        public KeyCode down;
        public KeyCode jump;
        public KeyCode dash;
        public KeyCode light;
        public KeyCode medium;
        public KeyCode heavy;
        public KeyCode special;
        public KeyCode throwKey;
        public KeyCode block;
        public KeyCode parry;

        [Header("Axes/Gamepad")]
        public bool useAxes;
        public string axisHorizontal;
        public string axisVertical;
        public string buttonLight;
        public string buttonMedium;
        public string buttonHeavy;
        public string buttonSpecial;
        public string buttonThrow;
        public string buttonBlock;
        public string buttonParry;
        public float axisDeadzone;
    }

    [Serializable]
    public class HitboxWindow
    {
        public Shape shape;
        public Vector2 offset;
        public Vector2 size;
        public float radius;
        public float start;
        public float end;
        public int damage;
        public Guard guard;
        public float hitstun;
        public float blockstun;
        public Vector2 knockback;
        public bool causesLaunch;
        public bool causesHardKD;
        public int meterGainOnHit;
        public int meterGainOnBlock;
        public int meterGainOnWhiff;
        public int priority;
        public float hitstopOverride;
        public bool armorBreak;
        public bool wallBounce;
        public bool groundBounce;
        [NonSerialized] public HashSet<SimpleFighter> hitVictims = new HashSet<SimpleFighter>();
    }

    [Serializable]
    public class Attack
    {
        public string name;
        public Button button;
        public bool allowGround;
        public bool allowAir;
        public float startup;
        public float recovery;
        public float moveDuring;
        public float gravityScaleDuring;
        public CancelRule cancelRule;
        public float cancelFromTime;
        public float cancelToTime;
        public List<HitboxWindow> windows;
        public AnimationClip anim;
        public string animatorTrigger;
        public AudioClip sfx;
        public bool autoTurnOnStart;
        public bool karaCancelIntoMove;
        public string[] cancelIntoNames;
        public int meterCost;
        public bool negativeEdge;
    }

    [Serializable]
    public class HUDConfig
    {
        public bool autoCreateHUD;
        public bool createCanvasIfNone;
        public Canvas existingCanvas;
        public Vector2 panelSize;
        public Vector2 barSize;
        public Vector2 meterSize;
        public Vector2 padding;
        public Color healthColor;
        public Color chipColor;
        public Color meterColor;
        public bool anchorByTeam;
        public bool mirrorRight;
        public Font font;
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // Lifecycle
    // ─────────────────────────────────────────────────────────────────────────────
    void Awake()
    {
        if (!ValidateRequiredFields()) { enabled = false; return; }

        // Internal setup
        rb.gravityScale = gravityScale;

        whiteTex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        whiteTex.SetPixel(0, 0, Color.white);
        whiteTex.Apply();
        whiteSprite = Sprite.Create(whiteTex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);

        hp = maxHP;
        hp = Mathf.Clamp(hp, 0, maxHP);
        meter = Mathf.Clamp(0, 0, maxMeter);

        if (!registry.Contains(this)) registry.Add(this);
        SetupHUD();
    }

    void OnDestroy()
    {
        registry.Remove(this);
    }

    void Update()
    {
        if (freezeTimer > 0f)
        {
            freezeTimer -= Time.unscaledDeltaTime;
            AnimatorUpdate(0f);
            UpdateHUD();
            return;
        }

        ReadInputs();

        bool wasGrounded = grounded;
        grounded = ProbeGround();
        landingThisFrame = (!wasGrounded && grounded);
        if (landingThisFrame) { jumpsUsed = 0; juggleUsed = 0; }

        var tgt = AcquireTarget();
        if (autoFace && tgt) faceRight = (tgt.transform.position.x >= transform.position.x);
        ApplyVisualFlip();

        TickState();
        AnimatorUpdate(Time.deltaTime);
        UpdateHUD();

        if (clampToStage)
        {
            var p = transform.position;
            p.x = Mathf.Clamp(p.x, stageLeftX, stageRightX);
            p.y = Mathf.Max(p.y, floorY);
            transform.position = p;
        }
    }

    void FixedUpdate()
    {
        if (freezeTimer > 0f) return;

        rb.gravityScale = grounded ? gravityScale : (rb.linearVelocity.y < -0.01f ? fallGravityScale : gravityScale);

        Vector2 v = rb.linearVelocity;

        if (state == State.Attack)
        {
            float dir = FacingDir();
            float move = currentAttack != null ? currentAttack.moveDuring : 0f;
            if (Mathf.Abs(move) > 0f) v.x = Mathf.MoveTowards(v.x, move * dir, accelGround * Time.fixedDeltaTime);
        }
        else if (grounded)
        {
            float target = moveInput * (moveInput * FacingDir() >= 0f ? walkSpeed : backWalkSpeed);
            v.x = Mathf.MoveTowards(v.x, target, accelGround * Time.fixedDeltaTime);
            if (Mathf.Abs(moveInput) < 0.01f) v.x = Mathf.MoveTowards(v.x, 0f, friction * Time.fixedDeltaTime);
        }
        else
        {
            float target = moveInput * airDrift;
            v.x = Mathf.MoveTowards(v.x, target, accelAir * Time.fixedDeltaTime);
        }

        if (externalVelocity != Vector2.zero)
        {
            v += externalVelocity;
            externalVelocity = Vector2.zero;
        }

        if (!grounded && inputDown && rb.linearVelocity.y < 0f) v.y = Mathf.Max(v.y, -fastFallSpeed);

        rb.linearVelocity = v;
        lastVelocity = v;

        ResolvePushboxes();
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // Validation
    // ─────────────────────────────────────────────────────────────────────────────
    bool ValidateRequiredFields()
    {
        bool ok = true;

        void Fail(string msg)
        {
            Debug.LogError($"[SimpleFighter] '{gameObject.name}': {msg}", this);
            ok = false;
        }

        // Identity
        if (string.IsNullOrWhiteSpace(fighterName)) Fail("Fighter name is REQUIRED.");
        if (visualRoot == null) Fail("Transform 'visualRoot' is REQUIRED.");
        if (team < 0) Fail("Team must be >= 0.");
        if (playerIndex < 0) Fail("Player index must be >= 0.");

        // Components
        if (rb == null) Fail("Rigidbody2D 'rb' is REQUIRED.");
        if (bodyCollider == null) Fail("Collider2D 'bodyCollider' is REQUIRED.");
        if (animator == null) Fail("Animator is REQUIRED.");

        // Movement
        if (walkSpeed <= 0f) Fail("Walk speed must be > 0.");
        if (jumpForce <= 0f) Fail("Jump force must be > 0.");
        if (groundMask == 0) Fail("Ground mask is REQUIRED.");

        // Vitals
        if (maxHP <= 0f) Fail("Max HP must be > 0.");
        if (maxMeter <= 0) Fail("Max Meter must be > 0.");

        // Animator param names
        if (string.IsNullOrWhiteSpace(p_isGrounded)) Fail("Animator param 'p_isGrounded' is REQUIRED.");
        if (string.IsNullOrWhiteSpace(p_isWalking)) Fail("Animator param 'p_isWalking' is REQUIRED.");
        if (string.IsNullOrWhiteSpace(p_isCrouching)) Fail("Animator param 'p_isCrouching' is REQUIRED.");
        if (string.IsNullOrWhiteSpace(p_isAttacking)) Fail("Animator param 'p_isAttacking' is REQUIRED.");
        if (string.IsNullOrWhiteSpace(p_isKO)) Fail("Animator param 'p_isKO' is REQUIRED.");
        if (string.IsNullOrWhiteSpace(p_speedX)) Fail("Animator param 'p_speedX' is REQUIRED.");
        if (string.IsNullOrWhiteSpace(p_speedY)) Fail("Animator param 'p_speedY' is REQUIRED.");
        // p_triggerAttack may be empty by design

        // Controls
        if (controls == null) Fail("Controls block is REQUIRED.");
        else
        {
            if (controls.useAxes)
            {
                if (string.IsNullOrWhiteSpace(controls.axisHorizontal)) Fail("Axis 'Horizontal' name is REQUIRED.");
                if (string.IsNullOrWhiteSpace(controls.axisVertical)) Fail("Axis 'Vertical' name is REQUIRED.");
                if (string.IsNullOrWhiteSpace(controls.buttonLight)) Fail("Button 'Light' is REQUIRED.");
                if (string.IsNullOrWhiteSpace(controls.buttonMedium)) Fail("Button 'Medium' is REQUIRED.");
                if (string.IsNullOrWhiteSpace(controls.buttonHeavy)) Fail("Button 'Heavy' is REQUIRED.");
                if (string.IsNullOrWhiteSpace(controls.buttonSpecial)) Fail("Button 'Special' is REQUIRED.");
                if (string.IsNullOrWhiteSpace(controls.buttonThrow)) Fail("Button 'Throw' is REQUIRED.");
                if (string.IsNullOrWhiteSpace(controls.buttonBlock)) Fail("Button 'Block' is REQUIRED.");
                if (string.IsNullOrWhiteSpace(controls.buttonParry)) Fail("Button 'Parry' is REQUIRED.");
                if (controls.axisDeadzone <= 0f) Fail("Axis deadzone must be > 0.");
            }
            else
            {
                if (controls.left == KeyCode.None) Fail("Key 'left' is REQUIRED.");
                if (controls.right == KeyCode.None) Fail("Key 'right' is REQUIRED.");
                if (controls.up == KeyCode.None) Fail("Key 'up' is REQUIRED.");
                if (controls.down == KeyCode.None) Fail("Key 'down' is REQUIRED.");
                if (controls.jump == KeyCode.None) Fail("Key 'jump' is REQUIRED.");
                if (controls.dash == KeyCode.None) Fail("Key 'dash' is REQUIRED.");
                if (controls.light == KeyCode.None) Fail("Key 'light' is REQUIRED.");
                if (controls.medium == KeyCode.None) Fail("Key 'medium' is REQUIRED.");
                if (controls.heavy == KeyCode.None) Fail("Key 'heavy' is REQUIRED.");
                if (controls.special == KeyCode.None) Fail("Key 'special' is REQUIRED.");
                if (controls.throwKey == KeyCode.None) Fail("Key 'throw' is REQUIRED.");
                if (controls.block == KeyCode.None) Fail("Key 'block' is REQUIRED.");
                if (controls.parry == KeyCode.None) Fail("Key 'parry' is REQUIRED.");
            }
        }

        // Attacks
        if (attacks == null || attacks.Count == 0) Fail("At least one Attack is REQUIRED.");
        else
        {
            for (int i = 0; i < attacks.Count; i++)
            {
                var a = attacks[i];
                if (a == null) { Fail($"Attack[{i}] is null."); continue; }
                if (string.IsNullOrWhiteSpace(a.name)) Fail($"Attack[{i}] name is REQUIRED.");
                if (!a.allowGround && !a.allowAir) Fail($"Attack[{i}] must allow ground or air.");
                if (a.startup < 0f) Fail($"Attack[{i}] startup must be >= 0.");
                if (a.recovery < 0f) Fail($"Attack[{i}] recovery must be >= 0.");
                if (a.windows == null || a.windows.Count == 0) Fail($"Attack[{i}] needs at least one HitboxWindow.");
                else
                {
                    for (int j = 0; j < a.windows.Count; j++)
                    {
                        var w = a.windows[j];
                        if (w == null) { Fail($"Attack[{i}].Window[{j}] is null."); continue; }
                        if (w.end <= w.start) Fail($"Attack[{i}].Window[{j}] end must be > start.");
                        if (w.shape == Shape.Box && (w.size.x <= 0f || w.size.y <= 0f)) Fail($"Attack[{i}].Window[{j}] box size must be > 0.");
                        if (w.shape == Shape.Circle && w.radius <= 0f) Fail($"Attack[{i}].Window[{j}] radius must be > 0.");
                        if (w.damage <= 0) Fail($"Attack[{i}].Window[{j}] damage must be > 0.");
                        if (w.hitstun < 0f || w.blockstun < 0f) Fail($"Attack[{i}].Window[{j}] stuns must be >= 0.");
                    }
                }
            }
        }

        // HUD
        if (hud == null) Fail("HUD config is REQUIRED.");
        else if (hud.autoCreateHUD)
        {
            if (hud.panelSize.x <= 0f || hud.panelSize.y <= 0f) Fail("HUD panelSize must be set.");
            if (hud.barSize.x <= 0f || hud.barSize.y <= 0f) Fail("HUD barSize must be set.");
            if (hud.meterSize.x <= 0f || hud.meterSize.y <= 0f) Fail("HUD meterSize must be set.");
            // Require either existing canvas or permission to create
            if (hud.existingCanvas == null && !hud.createCanvasIfNone) Fail("Provide HUD canvas or enable createCanvasIfNone.");
            if (hud.font == null) Fail("HUD font is REQUIRED.");
        }

        if (!ok)
        {
            Debug.LogError($"[SimpleFighter] '{gameObject.name}': VALIDATION FAILED - Component will be DISABLED.", this);
        }
        return ok;
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // Input
    // ─────────────────────────────────────────────────────────────────────────────
    void ReadInputs()
    {
        float hz = 0f; float vt = 0f;

        if (controls.useAxes)
        {
            hz = Input.GetAxisRaw(controls.axisHorizontal);
            vt = Input.GetAxisRaw(controls.axisVertical);
            if (Mathf.Abs(hz) < controls.axisDeadzone) hz = 0f;
            if (Mathf.Abs(vt) < controls.axisDeadzone) vt = 0f;
            inputLight = Input.GetButtonDown(controls.buttonLight);
            inputMedium = Input.GetButtonDown(controls.buttonMedium);
            inputHeavy = Input.GetButtonDown(controls.buttonHeavy);
            inputSpecial = Input.GetButtonDown(controls.buttonSpecial);
            inputThrow = Input.GetButtonDown(controls.buttonThrow);
            holdingBlock = Input.GetButton(controls.buttonBlock);
            holdingParry = Input.GetButtonDown(controls.buttonParry);
        }
        else
        {
            hz = (Input.GetKey(controls.left) ? -1f : 0f) + (Input.GetKey(controls.right) ? 1f : 0f);
            vt = (Input.GetKey(controls.down) ? -1f : 0f) + (Input.GetKey(controls.up) ? 1f : 0f);
            inputLight = Input.GetKeyDown(controls.light);
            inputMedium = Input.GetKeyDown(controls.medium);
            inputHeavy = Input.GetKeyDown(controls.heavy);
            inputSpecial = Input.GetKeyDown(controls.special);
            inputThrow = Input.GetKeyDown(controls.throwKey);
            holdingBlock = Input.GetKey(controls.block);
            holdingParry = Input.GetKeyDown(controls.parry);
        }

        inputUp = vt > 0.5f || Input.GetKeyDown(controls.jump);
        inputDown = vt < -0.5f;
        inputDash = Input.GetKeyDown(controls.dash);
        inputJump = inputUp;

        moveInput = Mathf.Clamp(hz, -1f, 1f);
        float backSign = -FacingDir();
        holdingBack = moveInput * backSign > 0.3f;

        if (grounded) coyoteTimer = coyoteTime; else coyoteTimer -= Time.deltaTime;
        if (inputJump) jumpBufferTimer = jumpBuffer; else jumpBufferTimer -= Time.deltaTime;

        if (isAI) AITick();
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // State
    // ─────────────────────────────────────────────────────────────────────────────
    void TickState()
    {
        if (hitstunTimer > 0f) hitstunTimer -= Time.deltaTime;
        if (blockstunTimer > 0f) blockstunTimer -= Time.deltaTime;
        if (knockdownTimer > 0f) knockdownTimer -= Time.deltaTime;
        if (invulnTimer > 0f) invulnTimer -= Time.deltaTime;
        if (techTimer > 0f) techTimer -= Time.deltaTime;
        if (Time.time - lastHitTime > juggleResetTime) juggleUsed = 0;

        if (state == State.Hitstun && hitstunTimer <= 0f) state = grounded ? State.Idle : State.Fall;
        if (state == State.Blockstun && blockstunTimer <= 0f) state = grounded ? State.Idle : State.Fall;
        if (state == State.Knockdown && knockdownTimer <= 0f) state = State.Idle;

        if (state == State.Attack)
        {
            attackTimer += Time.deltaTime;
            if (currentAttack != null)
            {
                foreach (var w in currentAttack.windows)
                    if (attackTimer >= w.start && attackTimer <= w.end)
                        DoHitDetect(w);

                float total = currentAttack.startup;
                foreach (var w in currentAttack.windows) total = Mathf.Max(total, w.end);
                total += currentAttack.recovery;
                if (attackTimer >= total) EndAttack();

                if (CanCancelFromCurrent()) TryStartAttackByInput();
            }
        }

        if (CanAct() && inputThrow) TryThrow();
        if (CanAct() && holdingParry) TryParry();
        if (CanAct()) TryStartAttackByInput();

        if (CanAct())
        {
            crouching = inputDown && grounded && Mathf.Abs(moveInput) < 0.2f;
            if (grounded)
            {
                if (Mathf.Abs(moveInput) > 0.05f) state = State.Walk; else if (!crouching) state = State.Idle;
                if (jumpBufferTimer > 0f && coyoteTimer > 0f && jumpsUsed < maxJumps) DoJump();
                if (inputDash && Mathf.Sign(moveInput) == FacingDir()) DoDash(false);
                else if (inputDash && Mathf.Sign(moveInput) == -FacingDir()) DoDash(true);
            }
            else
            {
                state = rb.linearVelocity.y >= 0 ? State.Jump : State.Fall;
                if (inputJump && jumpsUsed < maxJumps) DoAirJump();
            }
        }
    }

    bool CanAct()
    {
        if (hp <= 0f) return false;
        if (state == State.Attack || state == State.Hitstun || state == State.Blockstun || state == State.Knockdown || state == State.Throwing || state == State.Grabbed) return false;
        if (freezeTimer > 0f) return false;
        return true;
    }

    void DoJump()
    {
        jumpsUsed = Mathf.Max(jumpsUsed, 1);
        jumpBufferTimer = 0f;
        coyoteTimer = 0f;
        var v = rb.linearVelocity; v.y = jumpForce; rb.linearVelocity = v;
        state = State.Jump;
    }

    void DoAirJump()
    {
        jumpsUsed++;
        var v = rb.linearVelocity; v.y = jumpForce; rb.linearVelocity = v;
        state = State.Jump;
    }

    void DoDash(bool back)
    {
        state = back ? State.Backdash : State.Dash;
        float dir = back ? -FacingDir() : FacingDir();
        rb.linearVelocity = new Vector2(dashSpeed * dir, rb.linearVelocity.y);
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // Attacks
    // ─────────────────────────────────────────────────────────────────────────────
    bool TryStartAttackByInput()
    {
        Attack cand = null;
        if (inputSpecial) cand = FindAttack(Button.Special, airborne: !grounded) ?? cand;
        if (inputHeavy)   cand = FindAttack(Button.Heavy, airborne: !grounded) ?? cand;
        if (inputMedium)  cand = FindAttack(Button.Medium, airborne: !grounded) ?? cand;
        if (inputLight)   cand = FindAttack(Button.Light, airborne: !grounded) ?? cand;

        if (cand != null && CanSpendMeter(cand.meterCost))
        {
            StartAttack(cand);
            return true;
        }
        return false;
    }

    Attack FindAttack(Button b, bool airborne)
    {
        for (int i = 0; i < attacks.Count; i++)
        {
            var a = attacks[i];
            if (a.button != b) continue;
            if (airborne && !a.allowAir) continue;
            if (!airborne && !a.allowGround) continue;
            return a;
        }
        return null;
    }

    bool CanCancelFromCurrent()
    {
        if (currentAttack == null) return false;
        if (currentAttack.cancelRule == CancelRule.None) return false;
        if (attackTimer < currentAttack.cancelFromTime || attackTimer > currentAttack.cancelToTime) return false;
        return true; // simplified behavior
    }

    void StartAttack(Attack a)
    {
        if (a.autoTurnOnStart) { var t = AcquireTarget(); if (t) faceRight = (t.transform.position.x >= transform.position.x); }
        currentAttack = a;
        attackTimer = 0f;
        state = State.Attack;
        SpendMeter(a.meterCost);
        foreach (var w in a.windows) w.hitVictims.Clear();

        if (a.gravityScaleDuring >= 0f) rb.gravityScale = a.gravityScaleDuring;

        if (animator)
        {
            if (a.anim != null) animator.Play(a.anim.name, 0, 0f);
            if (!string.IsNullOrEmpty(a.animatorTrigger)) animator.SetTrigger(a.animatorTrigger);
            if (!string.IsNullOrEmpty(p_triggerAttack)) animator.SetTrigger(p_triggerAttack);
        }
    }

    void EndAttack()
    {
        currentAttack = null;
        state = grounded ? State.Idle : State.Fall;
    }

    void DoHitDetect(HitboxWindow w)
    {
        Vector2 center = (Vector2)transform.position + RotateFacing(w.offset);
        int hits = 0;

        if (w.shape == Shape.Box)
        {
            var cols = Physics2D.OverlapBoxAll(center, w.size, 0f);
            hits += ProcessHits(cols, w);
        }
        else
        {
            var cols = Physics2D.OverlapCircleAll(center, w.radius);
            hits += ProcessHits(cols, w);
        }

        if (hits == 0 && w.meterGainOnWhiff != 0) GainMeter(w.meterGainOnWhiff);
    }

    int ProcessHits(Collider2D[] cols, HitboxWindow w)
    {
        int count = 0;
        for (int i = 0; i < cols.Length; i++)
        {
            var f = cols[i].GetComponentInParent<SimpleFighter>();
            if (!f || f == this) continue;
            if (f.team == team) continue;
            if (w.hitVictims.Contains(f)) continue;

            if (!OverlapsPushboxes(this, f)) continue;

            bool front = (transform.position.x - f.transform.position.x) * FacingDir() < 0f;
            bool canBlock = (w.guard == Guard.Mid && f.holdingBack && front) ||
                            (w.guard == Guard.High && f.holdingBack && !f.crouching && front) ||
                            (w.guard == Guard.Low && f.holdingBack && f.crouching && front);

            bool parried = false;
            if (f.techTimer > 0f && w.guard == Guard.Throw) canBlock = true;

            if (f.state != State.Hitstun && f.state != State.Blockstun && f.holdingParry && front && w.guard != Guard.Throw)
            {
                parried = true;
                f.freezeTimer = Mathf.Max(f.freezeTimer, hitstopOnHit + parryFreezeBonus);
                this.freezeTimer = Mathf.Max(this.freezeTimer, hitstopOnHit + parryFreezeBonus);
                f.blockstunTimer = 0.12f;
                f.state = State.Blockstun;
            }

            if (w.guard == Guard.Throw)
            {
                if (f.grounded && f.invulnTimer <= 0f && !f.IsThrowImmuneTo(this))
                {
                    w.hitVictims.Add(f);
                    f.TakeDamage(throwDamage, Guard.Throw, this, w, true);
                    count++;
                }
                continue;
            }

            if (canBlock && !parried)
            {
                w.hitVictims.Add(f);
                f.OnBlocked(this, w);
                count++;
                continue;
            }

            if (f.invulnTimer > 0f) continue;
            w.hitVictims.Add(f);
            f.OnHit(this, w);
            count++;
        }
        return count;
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // Damage / Block / Parry / Throw
    // ─────────────────────────────────────────────────────────────────────────────
    public void OnHit(SimpleFighter attacker, HitboxWindow w)
    {
        float hs = w.hitstopOverride > 0 ? w.hitstopOverride : hitstopOnHit;
        this.freezeTimer = Mathf.Max(this.freezeTimer, hs);
        attacker.freezeTimer = Mathf.Max(attacker.freezeTimer, hs);

        TakeDamage(w.damage, w.guard, attacker, w, false);

        Vector2 kb = new Vector2(w.knockback.x * attacker.FacingDir(), w.knockback.y);
        externalVelocity += kb;
        lastHitTime = Time.time;

        if (grounded && w.causesHardKD)
        {
            knockdownTimer = Mathf.Max(knockdownTimer, 0.6f);
            state = State.Knockdown;
            invulnTimer = Mathf.Max(invulnTimer, wakeupInvuln);
        }
        else
        {
            hitstunTimer = Mathf.Max(hitstunTimer, w.hitstun);
            state = State.Hitstun;
        }

        attacker.GainMeter(w.meterGainOnHit);
    }

    public void OnBlocked(SimpleFighter attacker, HitboxWindow w)
    {
        float hs = w.hitstopOverride > 0 ? w.hitstopOverride : hitstopOnBlock;
        this.freezeTimer = Mathf.Max(this.freezeTimer, hs);
        attacker.freezeTimer = Mathf.Max(attacker.freezeTimer, hs);

        if (enableChip && w.guard != Guard.Unblockable && w.guard != Guard.Throw)
        {
            float chip = Mathf.Max(gutsMinDamage, w.damage * chipRate);
            TakeRawDamage(chip);
            chipShown = Mathf.Max(chipShown, hp / maxHP);
        }

        blockstunTimer = Mathf.Max(blockstunTimer, w.blockstun);
        state = grounded ? State.Blockstun : State.Fall;

        float s = Mathf.Sign(transform.position.x - attacker.transform.position.x);
        externalVelocity += new Vector2(3.5f * s, 0.5f);
        attacker.externalVelocity -= new Vector2(1.2f * s, 0.1f);

        GainMeter(w.meterGainOnBlock);
    }

    public void TakeDamage(float dmg, Guard type, SimpleFighter attacker, HitboxWindow w, bool hardKD)
    {
        float scale = 1f - 0.12f * juggleUsed;
        scale = Mathf.Clamp(scale, 0.25f, 1f);
        float finalDamage = Mathf.Max(gutsMinDamage, dmg * scale);
        TakeRawDamage(finalDamage);

        juggleUsed++;
        if (hardKD) { knockdownTimer = Mathf.Max(knockdownTimer, throwKD); state = State.Knockdown; }

        if (hp <= 0f)
        {
            state = State.Knockdown;
            rb.linearVelocity = Vector2.zero;
        }
    }

    public void TakeRawDamage(float dmg)
    {
        hp = Mathf.Clamp(hp - dmg, 0f, maxHP);
    }

    public void TryThrow()
    {
        var t = AcquireTarget();
        if (!t) return;
        if (Vector2.Distance(transform.position, t.transform.position) > throwRange) return;
        if (!t.grounded) return;

        freezeTimer = Mathf.Max(freezeTimer, throwStartup);
        t.freezeTimer = Mathf.Max(t.freezeTimer, throwStartup);
        t.techTimer = Mathf.Max(t.techTimer, techWindow);
        Invoke(nameof(ResolveThrow), throwStartup * 0.95f);
    }

    void ResolveThrow()
    {
        var t = AcquireTarget();
        if (!t) return;
        if (t.techTimer > 0f) { t.techTimer = 0f; return; }
        if (Vector2.Distance(transform.position, t.transform.position) > throwRange) return;

        var w = new HitboxWindow { guard = Guard.Throw };
        OnHit(t, w);
        t.TakeDamage(throwDamage, Guard.Throw, this, w, true);
        t.throwImmune.Add(this);
        Invoke(nameof(ClearThrowImmune), 0.5f);
    }

    void ClearThrowImmune()
    {
        foreach (var f in registry) f.throwImmune.Remove(this);
    }

    bool IsThrowImmuneTo(SimpleFighter other) => throwImmune.Contains(other);

    public void TryParry()
    {
        invulnTimer = Mathf.Max(invulnTimer, parryWindow);
        freezeTimer = Mathf.Max(freezeTimer, 0.015f);
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // AI
    // ─────────────────────────────────────────────────────────────────────────────
    void AITick()
    {
        aiThinkTimer -= Time.deltaTime;
        if (aiThinkTimer > 0f) return;
        aiThinkTimer = UnityEngine.Random.Range(0.08f, 0.16f);

        var t = AcquireTarget();
        if (!t) return;
        float dx = t.transform.position.x - transform.position.x;
        float dist = Mathf.Abs(dx);
        faceRight = dx >= 0f;

        moveInput = dist > 2.8f ? FacingDir() : (dist < 1.4f ? -FacingDir() : 0f);

        if (grounded && UnityEngine.Random.value < 0.04f) inputJump = true;

        if (dist < 1.8f)
        {
            float r = UnityEngine.Random.value;
            inputLight = r < 0.55f;
            inputMedium = r >= 0.55f && r < 0.85f;
            inputHeavy = r >= 0.85f;
            inputThrow = UnityEngine.Random.value < 0.07f;
            holdingBlock = false;
        }
        else
        {
            inputLight = inputMedium = inputHeavy = inputThrow = false;
            holdingBlock = UnityEngine.Random.value < 0.2f && dist < 3.5f;
        }
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // HUD
    // ─────────────────────────────────────────────────────────────────────────────
    void SetupHUD()
    {
        if (hud == null || !hud.autoCreateHUD) return;

        if (hud.existingCanvas == null && hud.createCanvasIfNone)
        {
            var go = new GameObject("FighterCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            hudCanvas = go.GetComponent<Canvas>();
            hudCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
        }
        else
        {
            hudCanvas = hud.existingCanvas;
        }

        if (!hudCanvas)
        {
            Debug.LogError("[SimpleFighter] HUD canvas missing and creation disabled.", this);
            enabled = false;
            return;
        }

        var panelGO = new GameObject($"{fighterName}_HUD", typeof(RectTransform));
        panelGO.transform.SetParent(hudCanvas.transform, false);
        hudPanel = panelGO.GetComponent<RectTransform>();
        hudPanel.sizeDelta = hud.panelSize;

        bool anchorRight = hud.anchorByTeam ? (team % 2 == 1) : (playerIndex % 2 == 1);
        var aMin = new Vector2(anchorRight ? 1f : 0f, 1f);
        hudPanel.anchorMin = aMin; hudPanel.anchorMax = aMin; hudPanel.pivot = new Vector2(anchorRight ? 1f : 0f, 1f);
        hudPanel.anchoredPosition = new Vector2(anchorRight ? -hud.padding.x : hud.padding.x, -hud.padding.y - (playerIndex * (hud.panelSize.y + 8f)));

        var nameGO = new GameObject("Name", typeof(Text));
        nameGO.transform.SetParent(hudPanel, false);
        nameText = nameGO.GetComponent<Text>();
        nameText.text = fighterName;
        nameText.font = hud.font;
        nameText.alignment = anchorRight ? TextAnchor.UpperRight : TextAnchor.UpperLeft;
        var nameRT = nameGO.GetComponent<RectTransform>();
        nameRT.anchorMin = aMin; nameRT.anchorMax = aMin; nameRT.pivot = new Vector2(anchorRight ? 1f : 0f, 1f);
        nameRT.sizeDelta = new Vector2(hud.panelSize.x, 14f);
        nameRT.anchoredPosition = Vector2.zero;

        var backGO = new GameObject("HP_Back", typeof(Image));
        backGO.transform.SetParent(hudPanel, false);
        var backImg = backGO.GetComponent<Image>(); backImg.sprite = whiteSprite; backImg.color = new Color(0f, 0f, 0f, 0.6f);
        var backRT = backGO.GetComponent<RectTransform>();
        backRT.anchorMin = aMin; backRT.anchorMax = aMin; backRT.pivot = new Vector2(anchorRight ? 1f : 0f, 1f);
        backRT.sizeDelta = hud.barSize;
        backRT.anchoredPosition = new Vector2(0f, -16f);

        var chipGO = new GameObject("HP_Chip", typeof(Image));
        chipGO.transform.SetParent(backRT, false);
        hpChip = chipGO.GetComponent<Image>(); hpChip.sprite = whiteSprite; hpChip.color = hud.chipColor; hpChip.type = Image.Type.Filled; hpChip.fillMethod = Image.FillMethod.Horizontal; hpChip.fillOrigin = anchorRight ? 1 : 0; hpChip.fillAmount = 1f;
        var chipRT = chipGO.GetComponent<RectTransform>(); chipRT.anchorMin = Vector2.zero; chipRT.anchorMax = Vector2.one; chipRT.offsetMin = Vector2.zero; chipRT.offsetMax = Vector2.zero;

        var hpGO = new GameObject("HP_Fill", typeof(Image));
        hpGO.transform.SetParent(backRT, false);
        hpFill = hpGO.GetComponent<Image>(); hpFill.sprite = whiteSprite; hpFill.color = hud.healthColor; hpFill.type = Image.Type.Filled; hpFill.fillMethod = Image.FillMethod.Horizontal; hpFill.fillOrigin = anchorRight ? 1 : 0; hpFill.fillAmount = 1f;
        var hpRT = hpGO.GetComponent<RectTransform>(); hpRT.anchorMin = Vector2.zero; hpRT.anchorMax = Vector2.one; hpRT.offsetMin = Vector2.zero; hpRT.offsetMax = Vector2.zero;

        var meterBackGO = new GameObject("Meter_Back", typeof(Image));
        meterBackGO.transform.SetParent(hudPanel, false);
        var meterBack = meterBackGO.GetComponent<Image>(); meterBack.sprite = whiteSprite; meterBack.color = new Color(0f, 0f, 0f, 0.6f);
        var meterBackRT = meterBackGO.GetComponent<RectTransform>();
        meterBackRT.anchorMin = aMin; meterBackRT.anchorMax = aMin; meterBackRT.pivot = new Vector2(anchorRight ? 1f : 0f, 1f);
        meterBackRT.sizeDelta = hud.meterSize;
        meterBackRT.anchoredPosition = new Vector2(0f, -36f);

        var meterFillGO = new GameObject("Meter_Fill", typeof(Image));
        meterFillGO.transform.SetParent(meterBackRT, false);
        meterFill = meterFillGO.GetComponent<Image>(); meterFill.sprite = whiteSprite; meterFill.color = hud.meterColor; meterFill.type = Image.Type.Filled; meterFill.fillMethod = Image.FillMethod.Horizontal; meterFill.fillOrigin = anchorRight ? 1 : 0; meterFill.fillAmount = 0f;
        var meterFillRT = meterFillGO.GetComponent<RectTransform>(); meterFillRT.anchorMin = Vector2.zero; meterFillRT.anchorMax = Vector2.one; meterFillRT.offsetMin = Vector2.zero; meterFillRT.offsetMax = Vector2.zero;
    }

    void UpdateHUD()
    {
        if (!hpFill) return;
        float t = Mathf.Clamp01(hp / Mathf.Max(1f, maxHP));
        hpFill.fillAmount = t;
        chipShown = Mathf.MoveTowards(chipShown, t, Time.unscaledDeltaTime * 0.6f);
        hpChip.fillAmount = Mathf.Max(chipShown, t);
        if (meterFill) meterFill.fillAmount = Mathf.Clamp01((float)meter / Mathf.Max(1, maxMeter));
        if (nameText) nameText.text = fighterName;
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // Animator
    // ─────────────────────────────────────────────────────────────────────────────
    void AnimatorUpdate(float dt)
    {
        if (!animator) return;
        animator.SetBool(p_isGrounded, grounded);
        animator.SetBool(p_isWalking, grounded && Mathf.Abs(moveInput) > 0.05f);
        animator.SetBool(p_isCrouching, crouching);
        animator.SetBool(p_isAttacking, state == State.Attack);
        animator.SetBool(p_isKO, hp <= 0f);
        animator.SetFloat(p_speedX, rb ? Mathf.Abs(rb.linearVelocity.x) : 0f);
        animator.SetFloat(p_speedY, rb ? rb.linearVelocity.y : 0f);
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // Helpers
    // ─────────────────────────────────────────────────────────────────────────────
    float FacingDir() => (faceRight ? 1f : -1f);

    void ApplyVisualFlip()
    {
        if (!flipVisualWithScale || !visualRoot) return;
        var s = visualRoot.localScale;
        s.x = Mathf.Abs(s.x) * (faceRight ? 1f : -1f);
        visualRoot.localScale = s;
    }

    bool ProbeGround()
    {
        var origin = (Vector2)transform.position + groundProbeOffset;
        var hit = Physics2D.Raycast(origin, Vector2.down, groundProbeDistance, groundMask);
        return hit.collider != null;
    }

    Vector2 RotateFacing(Vector2 v) => new Vector2(v.x * FacingDir(), v.y);

    void ResolvePushboxes()
    {
        var myRect = new Rect(transform.position.x - pushboxWidth * 0.5f, transform.position.y - 0.5f, pushboxWidth, pushboxHeight);
        foreach (var f in registry)
        {
            if (!f || f == this) continue;
            if (Mathf.Abs(f.transform.position.y - transform.position.y) > 3.5f) continue;
            var other = new Rect(f.transform.position.x - f.pushboxWidth * 0.5f, f.transform.position.y - 0.5f, f.pushboxWidth, f.pushboxHeight);
            if (myRect.Overlaps(other))
            {
                float overlap = (myRect.center.x < other.center.x)
                    ? (myRect.xMax - other.xMin)
                    : (other.xMax - myRect.xMin);
                float push = overlap * 0.5f + 0.0001f;
                if (transform.position.x < f.transform.position.x)
                {
                    transform.position += new Vector3(-pushResolve * push, 0f, 0f);
                    f.transform.position += new Vector3(pushResolve * push, 0f, 0f);
                }
                else
                {
                    transform.position += new Vector3(pushResolve * push, 0f, 0f);
                    f.transform.position += new Vector3(-pushResolve * push, 0f, 0f);
                }
            }
        }
    }

    bool OverlapsPushboxes(SimpleFighter a, SimpleFighter b)
    {
        var ar = new Rect(a.transform.position.x - a.pushboxWidth * 0.5f, a.transform.position.y - 0.5f, a.pushboxWidth, a.pushboxHeight);
        var br = new Rect(b.transform.position.x - b.pushboxWidth * 0.5f, b.transform.position.y - 0.5f, b.pushboxWidth, b.pushboxHeight);
        return ar.Overlaps(br);
    }

    SimpleFighter AcquireTarget()
    {
        if (Time.time - lastRetarget < retargetInterval && cachedTarget != null) return cachedTarget;

        SimpleFighter best = null;
        float bestScore = float.MaxValue;
        for (int i = 0; i < registry.Count; i++)
        {
            var f = registry[i];
            if (!f || f == this) continue;
            if (f.team == team) continue;
            float d = preferNearestTarget ? Mathf.Abs(f.transform.position.x - transform.position.x) : i;
            if (d < bestScore) { bestScore = d; best = f; }
        }
        cachedTarget = best;
        lastRetarget = Time.time;
        return best;
    }

    public void GainMeter(int v) => meter = Mathf.Clamp(meter + v, 0, maxMeter);
    public bool CanSpendMeter(int v) => meter >= v;
    public void SpendMeter(int v) => meter = Mathf.Clamp(meter - v, 0, maxMeter);

    // ─────────────────────────────────────────────────────────────────────────────
    // Gizmos (kept private; not essential to expose)
    // ─────────────────────────────────────────────────────────────────────────────
    bool drawHurtbox = true;
    bool drawPushbox = true;
    bool drawHitboxes = true;
    bool drawGroundProbe = true;
    Color gizHurt = new Color(0.2f, 1f, 0.2f, 0.25f);
    Color gizPush = new Color(1f, 1f, 0.2f, 0.25f);
    Color gizHit = new Color(1f, 0.2f, 0.2f, 0.25f);
    Color gizProbe = Color.cyan;

    void OnDrawGizmosSelected()
    {
        if (drawGroundProbe)
        {
            Gizmos.color = gizProbe;
            var origin = (Vector2)transform.position + groundProbeOffset;
            Gizmos.DrawLine(origin, origin + Vector2.down * groundProbeDistance);
        }
        if (drawPushbox)
        {
            Gizmos.color = gizPush;
            var r = new Rect(transform.position.x - pushboxWidth * 0.5f, transform.position.y - 0.5f, pushboxWidth, pushboxHeight);
            Gizmos.DrawWireCube(r.center, r.size);
        }
        if (drawHurtbox)
        {
            Gizmos.color = gizHurt;
            var r = new Rect(transform.position.x - 0.7f, transform.position.y - 0.4f, 1.4f, 1.6f);
            Gizmos.DrawWireCube(r.center, r.size);
        }
        if (drawHitboxes && state == State.Attack && currentAttack != null)
        {
            Gizmos.color = gizHit;
            foreach (var w in currentAttack.windows)
            {
                if (attackTimer >= w.start && attackTimer <= w.end)
                {
                    Vector2 center = (Vector2)transform.position + RotateFacing(w.offset);
                    if (w.shape == Shape.Box) Gizmos.DrawWireCube(center, w.size);
                    else Gizmos.DrawWireSphere(center, w.radius);
                }
            }
        }
    }
}
