// Single, self-contained 2D fighter component. Drop on a GameObject with Rigidbody2D + Collider2D.
// Supports: player/AI, per-fighter keybinds, auto UI (health/meter), attacks with startup/active/recovery,
// block/parry/throw, hitstop, hitstun/blockstun, juggle limit, cancels/links, pushbox separation,
// stage bounds, autoselect target, auto facing, animator params, debug gizmos.
// No other scripts required.

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(-10)]
public class SimpleFighter : MonoBehaviour
{
    // ???????????????????????????????????????????????????????????????????????????????
    // Identity / Team / Targeting
    // ???????????????????????????????????????????????????????????????????????????????
    [Header("Identity")]
    public string fighterName = "Fighter";
    public int team = 0;                                 // Fighters with same team value don't hit each other
    public int playerIndex = 0;                          // For UI anchoring
    public bool isAI = false;
    public bool autoFindOpponent = true;
    public bool preferNearestTarget = true;
    public SimpleFighter manualTarget;                   // Optional hard target
    public float retargetInterval = 0.2f;

    [Header("Facing")]
    public bool autoFace = true;
    public bool faceRight = true;                        // Manual override when autoFace=false
    public bool flipVisualWithScale = true;
    public Transform visualRoot;

    // ???????????????????????????????????????????????????????????????????????????????
    // Components / Scene Integration
    // ???????????????????????????????????????????????????????????????????????????????
    [Header("Components")]
    public Rigidbody2D rb;
    public Collider2D bodyCollider;
    public Animator animator;

    [Header("Stage Bounds")]
    public bool clampToStage = true;
    public float stageLeftX = -12f;
    public float stageRightX = 12f;
    public float floorY = -1000f;                        // Safety floor clamp if you fall through

    // ???????????????????????????????????????????????????????????????????????????????
    // Movement / Physics
    // ???????????????????????????????????????????????????????????????????????????????
    [Header("Movement")]
    public float walkSpeed = 6f;
    public float backWalkSpeed = 4.25f;
    public float dashSpeed = 10f;
    public float airDrift = 6f;
    public float accelGround = 60f;
    public float accelAir = 30f;
    public float friction = 30f;
    public float jumpForce = 14f;
    public int maxJumps = 1;
    public float coyoteTime = 0.08f;
    public float jumpBuffer = 0.09f;
    public float gravityScale = 3.5f;
    public float fallGravityScale = 5.5f;
    public float fastFallSpeed = 18f;
    public float pushboxWidth = 0.9f;
    public float pushboxHeight = 1.8f;
    public float pushResolve = 0.1f;                     // Separation strength vs other fighters
    public LayerMask groundMask = -1;
    public float groundProbeDistance = 0.12f;
    public Vector2 groundProbeOffset = new Vector2(0f, -0.95f);

    // ???????????????????????????????????????????????????????????????????????????????
    // Combat Core
    // ???????????????????????????????????????????????????????????????????????????????
    [Header("Vitals")]
    public float maxHP = 1000f;
    public float hp = 1000f;
    public int maxMeter = 3000;
    public int meter = 0;
    public bool enableChip = true;
    public float chipRate = 0.08f;                       // fraction of damage as chip on block
    public float gutsMinDamage = 1f;                     // minimum damage after reductions

    [Header("Hitstun/Blockstun/Hitstop")]
    public float hitstopOnHit = 0.06f;
    public float hitstopOnBlock = 0.04f;
    public float techWindow = 0.14f;                     // throw tech window
    public float wakeupInvuln = 0.12f;

    [Header("Juggle")]
    public int juggleMax = 6;
    public int juggleUsed = 0;
    public float juggleResetTime = 1.2f;

    // ???????????????????????????????????????????????????????????????????????????????
    // Attacks
    // ???????????????????????????????????????????????????????????????????????????????
    public enum Button { Light, Medium, Heavy, Special, Throw, Parry, None }
    public enum Shape { Box, Circle }
    public enum Guard { High, Low, Mid, Unblockable, Throw }
    public enum CancelRule { None, OnHit, OnHitOrBlock, Always }
    public enum State { Idle, Walk, Jump, Fall, Crouch, Dash, Backdash, Attack, Hitstun, Blockstun, Knockdown, Tech, Grabbed, Throwing }

    [Serializable]
    public class InputBindings
    {
        [Header("Keyboard")]
        public KeyCode left = KeyCode.A;
        public KeyCode right = KeyCode.D;
        public KeyCode up = KeyCode.W;
        public KeyCode down = KeyCode.S;
        public KeyCode jump = KeyCode.K;
        public KeyCode dash = KeyCode.L;
        public KeyCode light = KeyCode.J;
        public KeyCode medium = KeyCode.U;
        public KeyCode heavy = KeyCode.I;
        public KeyCode special = KeyCode.O;
        public KeyCode throwKey = KeyCode.H;
        public KeyCode block = KeyCode.LeftShift;
        public KeyCode parry = KeyCode.P;

        [Header("Axes/Gamepad")]
        public bool useAxes = false;
        public string axisHorizontal = "Horizontal";
        public string axisVertical = "Vertical";
        public string buttonLight = "Fire1";
        public string buttonMedium = "Fire2";
        public string buttonHeavy = "Fire3";
        public string buttonSpecial = "Jump";
        public string buttonThrow = "Submit";
        public string buttonBlock = "Cancel";
        public string buttonParry = "Fire4";
        public float axisDeadzone = 0.25f;
    }
    [Header("Controls")]
    public InputBindings controls = new InputBindings();

    [Serializable]
    public class HitboxWindow
    {
        public Shape shape = Shape.Box;
        public Vector2 offset = new Vector2(1.2f, 0.4f);
        public Vector2 size = new Vector2(1.2f, 0.7f);
        public float radius = 0.6f;                      // used when shape==Circle
        public float start = 0.083f;                     // seconds from attack start
        public float end = 0.16f;                        // seconds from attack start
        public int damage = 90;
        public Guard guard = Guard.Mid;
        public float hitstun = 0.33f;
        public float blockstun = 0.20f;
        public Vector2 knockback = new Vector2(7f, 3.5f);
        public bool causesLaunch = false;
        public bool causesHardKD = false;
        public int meterGainOnHit = 120;
        public int meterGainOnBlock = 40;
        public int meterGainOnWhiff = 15;
        public int priority = 0;
        public float hitstopOverride = -1f;              // <0 uses global
        public bool armorBreak = false;
        public bool wallBounce = false;
        public bool groundBounce = false;
        [NonSerialized] public HashSet<SimpleFighter> hitVictims = new HashSet<SimpleFighter>();
    }

    [Serializable]
    public class Attack
    {
        public string name = "5L";
        public Button button = Button.Light;
        public bool allowGround = true;
        public bool allowAir = false;
        public float startup = 0.10f;
        public float recovery = 0.25f;
        public float moveDuring = 0f;                    // forward drift while attacking (facing dir)
        public float gravityScaleDuring = -1f;           // -1 = unchanged
        public CancelRule cancelRule = CancelRule.OnHitOrBlock;
        public float cancelFromTime = 0.10f;
        public float cancelToTime = 0.20f;
        public List<HitboxWindow> windows = new List<HitboxWindow>()
        {
            new HitboxWindow()
        };
        public AnimationClip anim;                       // optional
        public string animatorTrigger = "";              // or trigger name
        public AudioClip sfx;                             // optional
        public bool autoTurnOnStart = true;
        public bool karaCancelIntoMove = false;
        public string[] cancelIntoNames = new string[0];
        public int meterCost = 0;
        public bool negativeEdge = false;                // release to fire
    }

    [Header("Move List")]
    public List<Attack> attacks = new List<Attack>();
    public List<string> chainRoutes = new List<string>() { "5L>5M>5H>Special" };

    [Header("Throws/Parry")]
    public float throwRange = 1.3f;
    public float throwStartup = 0.08f;
    public float throwKD = 0.8f;
    public int throwDamage = 140;
    public float parryWindow = 0.06f;
    public float parryFreezeBonus = 0.07f;

    // ???????????????????????????????????????????????????????????????????????????????
    // UI / HUD
    // ???????????????????????????????????????????????????????????????????????????????
    [Serializable]
    public class HUDConfig
    {
        public bool autoCreateHUD = true;
        public bool createCanvasIfNone = true;
        public Canvas existingCanvas;
        public Vector2 panelSize = new Vector2(420, 36);
        public Vector2 barSize = new Vector2(360, 16);
        public Vector2 meterSize = new Vector2(360, 8);
        public Vector2 padding = new Vector2(16, 12);
        public Color healthColor = new Color(0.9f, 0.1f, 0.1f);
        public Color chipColor = new Color(1f, 0.6f, 0.2f);
        public Color meterColor = new Color(0.2f, 0.6f, 1f);
        public bool anchorByTeam = true;                 // left for lower playerIndex on team 0, right for team 1
        public bool mirrorRight = true;
        public Font font;
    }
    [Header("HUD")]
    public HUDConfig hud = new HUDConfig();

    // runtime HUD refs
    [NonSerialized] public Canvas hudCanvas;
    [NonSerialized] public RectTransform hudPanel;
    [NonSerialized] public Image hpFill;
    [NonSerialized] public Image hpChip;
    [NonSerialized] public Image meterFill;
    [NonSerialized] public Text nameText;
    [NonSerialized] public float chipShown;              // lerp for chip delay

    // ???????????????????????????????????????????????????????????????????????????????
    // Animator Parameters
    // ???????????????????????????????????????????????????????????????????????????????
    [Header("Animator Params")]
    public string p_isGrounded = "isGrounded";
    public string p_isWalking = "isWalking";
    public string p_isCrouching = "isCrouching";
    public string p_isAttacking = "isAttacking";
    public string p_isKO = "isKO";
    public string p_speedX = "speedX";
    public string p_speedY = "speedY";
    public string p_triggerAttack = "";                 // optional dynamic trigger set from Attack.animatorTrigger

    // ???????????????????????????????????????????????????????????????????????????????
    // Debug / Dev
    // ???????????????????????????????????????????????????????????????????????????????
    [Header("Debug")]
    public bool drawHurtbox = true;
    public bool drawPushbox = true;
    public bool drawHitboxes = true;
    public bool drawGroundProbe = true;
    public Color gizHurt = new Color(0.2f, 1f, 0.2f, 0.25f);
    public Color gizPush = new Color(1f, 1f, 0.2f, 0.25f);
    public Color gizHit = new Color(1f, 0.2f, 0.2f, 0.25f);
    public Color gizProbe = Color.cyan;

    // ???????????????????????????????????????????????????????????????????????????????
    // Internal State (public for full inspector control)
    // ???????????????????????????????????????????????????????????????????????????????
    [Header("Runtime State")]
    public State state = State.Idle;
    public bool grounded;
    public bool crouching;
    public int jumpsUsed;
    public float coyoteTimer;
    public float jumpBufferTimer;
    public float hitstunTimer;
    public float blockstunTimer;
    public float knockdownTimer;
    public float freezeTimer;
    public float invulnTimer;
    public float techTimer;
    public bool canAct = true;
    public float lastGroundedY;
    public float aiThinkTimer;
    public float lastRetarget;
    public float attackTimer;          // time since current attack start
    public Attack currentAttack;
    public float lastHitTime;          // for juggle reset
    public bool holdingBack;           // updated per-frame
    public bool holdingBlock;
    public bool holdingParry;
    public bool inputLight, inputMedium, inputHeavy, inputSpecial, inputThrow;
    public bool inputJump, inputDash, inputDown, inputUp;
    public float moveInput;            // -1..1
    public Vector2 desiredVelocity;
    public Vector2 externalVelocity;   // for knockback etc.
    public Vector2 lastVelocity;
    public bool landingThisFrame;
    public bool queuedReversal;        // example flag for wakeup
    public HashSet<SimpleFighter> throwImmune = new HashSet<SimpleFighter>(); // throw tech
    public Texture2D whiteTex;         // for UI sprite
    public Sprite whiteSprite;

    // Registry of fighters in scene
    static readonly List<SimpleFighter> registry = new List<SimpleFighter>();

    // ???????????????????????????????????????????????????????????????????????????????
    // Unity lifecycle
    // ???????????????????????????????????????????????????????????????????????????????
    void Reset()
    {
        rb = GetComponent<Rigidbody2D>();
        bodyCollider = GetComponent<Collider2D>();
        if (!rb) rb = gameObject.AddComponent<Rigidbody2D>();
        rb.gravityScale = gravityScale;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        if (!bodyCollider) bodyCollider = gameObject.AddComponent<CapsuleCollider2D>();
        // Default move list if empty
        if (attacks.Count == 0)
        {
            var light = new Attack { name = "5L", button = Button.Light, startup = 0.08f, recovery = 0.18f };
            light.windows[0].damage = 60; light.windows[0].hitstun = 0.23f; light.windows[0].blockstun = 0.16f;
            var medium = new Attack { name = "5M", button = Button.Medium, startup = 0.12f, recovery = 0.25f };
            medium.windows[0].damage = 100; medium.windows[0].hitstun = 0.33f; medium.windows[0].blockstun = 0.22f;
            var heavy = new Attack { name = "5H", button = Button.Heavy, startup = 0.16f, recovery = 0.33f };
            heavy.windows[0].damage = 140; heavy.windows[0].hitstun = 0.42f; heavy.windows[0].blockstun = 0.26f;
            attacks.Add(light); attacks.Add(medium); attacks.Add(heavy);
        }
    }

    void Awake()
    {
        if (!rb) rb = GetComponent<Rigidbody2D>();
        if (!bodyCollider) bodyCollider = GetComponent<Collider2D>();
        if (visualRoot == null) visualRoot = transform;
        rb.gravityScale = gravityScale;
        whiteTex = new Texture2D(1, 1, TextureFormat.RGBA32, false); whiteTex.SetPixel(0, 0, Color.white); whiteTex.Apply();
        whiteSprite = Sprite.Create(whiteTex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);

        hp = Mathf.Clamp(hp <= 0 ? maxHP : hp, 0, maxHP);
        meter = Mathf.Clamp(meter, 0, maxMeter);

        if (!registry.Contains(this)) registry.Add(this);
        SetupHUD();
    }

    void OnDestroy()
    {
        registry.Remove(this);
    }

    void Update()
    {
        // Local freeze (hitstop). Does not change Unity Time.timeScale.
        if (freezeTimer > 0f)
        {
            freezeTimer -= Time.unscaledDeltaTime;
            AnimatorUpdate(0f);
            UpdateHUD(); // UI still updates
            return;
        }

        // Inputs
        ReadInputs();

        // Ground probe
        bool wasGrounded = grounded;
        grounded = ProbeGround();
        landingThisFrame = (!wasGrounded && grounded);

        if (landingThisFrame) { jumpsUsed = 0; juggleUsed = 0; }

        // Facing
        var tgt = AcquireTarget();
        if (autoFace && tgt)
        {
            faceRight = (tgt.transform.position.x >= transform.position.x);
        }
        ApplyVisualFlip();

        // State update
        TickState();

        // Animator
        AnimatorUpdate(Time.deltaTime);

        // HUD
        UpdateHUD();

        // Stage clamp
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

        // Gravity
        rb.gravityScale = grounded ? gravityScale : (rb.linearVelocity.y < -0.01f ? fallGravityScale : gravityScale);

        // Movement and friction
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
            // friction when no input
            if (Mathf.Abs(moveInput) < 0.01f) v.x = Mathf.MoveTowards(v.x, 0f, friction * Time.fixedDeltaTime);
        }
        else
        {
            float target = moveInput * airDrift;
            v.x = Mathf.MoveTowards(v.x, target, accelAir * Time.fixedDeltaTime);
        }

        // External velocity (knockback etc.)
        if (externalVelocity != Vector2.zero)
        {
            v += externalVelocity;
            externalVelocity = Vector2.zero;
        }

        // Fast fall
        if (!grounded && inputDown && rb.linearVelocity.y < 0f) v.y = Mathf.Max(v.y, -fastFallSpeed);

        rb.linearVelocity = v;
        lastVelocity = v;

        // Pushbox separation vs other fighters
        ResolvePushboxes();
    }

    // ???????????????????????????????????????????????????????????????????????????????
    // Input handling
    // ???????????????????????????????????????????????????????????????????????????????
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

        // Move input is from player perspective; walking back is negative relative to facing
        moveInput = Mathf.Clamp(hz, -1f, 1f);

        // "Back" = opposite of facing; used for blocking
        float backSign = -FacingDir();
        holdingBack = moveInput * backSign > 0.3f;

        // Jump buffers & coyote
        if (grounded) coyoteTimer = coyoteTime; else coyoteTimer -= Time.deltaTime;
        if (inputJump) jumpBufferTimer = jumpBuffer; else jumpBufferTimer -= Time.deltaTime;

        // AI override
        if (isAI) AITick();
    }

    // ???????????????????????????????????????????????????????????????????????????????
    // State machine
    // ???????????????????????????????????????????????????????????????????????????????
    void TickState()
    {
        // Common timers
        if (hitstunTimer > 0f) hitstunTimer -= Time.deltaTime;
        if (blockstunTimer > 0f) blockstunTimer -= Time.deltaTime;
        if (knockdownTimer > 0f) knockdownTimer -= Time.deltaTime;
        if (invulnTimer > 0f) invulnTimer -= Time.deltaTime;
        if (techTimer > 0f) techTimer -= Time.deltaTime;
        if (Time.time - lastHitTime > juggleResetTime) juggleUsed = 0;

        // Remove stuns -> idle
        if (state == State.Hitstun && hitstunTimer <= 0f) state = grounded ? State.Idle : State.Fall;
        if (state == State.Blockstun && blockstunTimer <= 0f) state = grounded ? State.Idle : State.Fall;
        if (state == State.Knockdown && knockdownTimer <= 0f) state = State.Idle;

        // Attacking
        if (state == State.Attack)
        {
            attackTimer += Time.deltaTime;
            // Active hit windows
            if (currentAttack != null)
            {
                foreach (var w in currentAttack.windows)
                {
                    if (attackTimer >= w.start && attackTimer <= w.end)
                        DoHitDetect(w);
                }
                // End attack on recovery complete
                float total = currentAttack.startup;
                foreach (var w in currentAttack.windows) total = Mathf.Max(total, w.end);
                total += currentAttack.recovery;
                if (attackTimer >= total) EndAttack();
                // Cancel window
                if (CanCancelFromCurrent())
                {
                    TryStartAttackByInput(); // chain/cancel if input pressed
                }
            }
        }

        // Throw
        if (CanAct() && inputThrow) TryThrow();

        // Parry
        if (CanAct() && holdingParry) TryParry();

        // Start attacks from inputs
        if (CanAct()) TryStartAttackByInput();

        // Movement transitions
        if (CanAct())
        {
            crouching = inputDown && grounded && Mathf.Abs(moveInput) < 0.2f;
            if (grounded)
            {
                if (Mathf.Abs(moveInput) > 0.05f) state = State.Walk; else if (!crouching) state = State.Idle;
                if (jumpBufferTimer > 0f && coyoteTimer > 0f && jumpsUsed < maxJumps)
                {
                    DoJump();
                }
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

    // ???????????????????????????????????????????????????????????????????????????????
    // Attacks
    // ???????????????????????????????????????????????????????????????????????????????
    bool TryStartAttackByInput()
    {
        // Highest strength takes priority if multiple pressed
        Attack cand = null;
        if (inputSpecial) cand = FindAttack(Button.Special, airborne: !grounded) ?? cand;
        if (inputHeavy) cand = FindAttack(Button.Heavy, airborne: !grounded) ?? cand;
        if (inputMedium) cand = FindAttack(Button.Medium, airborne: !grounded) ?? cand;
        if (inputLight) cand = FindAttack(Button.Light, airborne: !grounded) ?? cand;

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
        if (currentAttack.cancelRule == CancelRule.Always) return true;
        if (currentAttack.cancelRule == CancelRule.OnHitOrBlock) return true; // simplified: we allow in window
        if (currentAttack.cancelRule == CancelRule.OnHit) return true;        // simplified
        return false;
    }

    void StartAttack(Attack a)
    {
        if (a.autoTurnOnStart) { var t = AcquireTarget(); if (t) faceRight = (t.transform.position.x >= transform.position.x); }
        currentAttack = a;
        attackTimer = 0f;
        state = State.Attack;
        SpendMeter(a.meterCost);
        foreach (var w in a.windows) w.hitVictims.Clear();

        // Gravity override
        if (a.gravityScaleDuring >= 0f) rb.gravityScale = a.gravityScaleDuring;

        // Animator drive
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

        // Meter on whiff during active
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
            if (w.hitVictims.Contains(f)) continue; // already hit by this window

            // Range gate using pushboxes to reduce false positives
            if (!OverlapsPushboxes(this, f)) continue;

            // Determine block possibility
            bool front = (transform.position.x - f.transform.position.x) * FacingDir() < 0f; // attacked from front
            bool canBlock = (w.guard == Guard.Mid && f.holdingBack && front) ||
                            (w.guard == Guard.High && f.holdingBack && !f.crouching && front) ||
                            (w.guard == Guard.Low && f.holdingBack && f.crouching && front);

            bool parried = false;
            if (f.techTimer > 0f && w.guard == Guard.Throw) canBlock = true; // throw tech window

            // Parry check
            if (f.state != State.Hitstun && f.state != State.Blockstun && f.holdingParry && front && w.guard != Guard.Throw)
            {
                parried = true;
                f.freezeTimer = Mathf.Max(f.freezeTimer, hitstopOnHit + parryFreezeBonus);
                this.freezeTimer = Mathf.Max(this.freezeTimer, hitstopOnHit + parryFreezeBonus);
                f.blockstunTimer = 0.12f;
                f.state = State.Blockstun;
            }

            // Apply result
            if (w.guard == Guard.Throw)
            {
                // Throw connects only if target is not airborne and not invuln
                if (f.grounded && f.invulnTimer <= 0f && !f.IsThrowImmuneTo(this))
                {
                    w.hitVictims.Add(f);
                    // Throw knockdown
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

            // Hit
            if (f.invulnTimer > 0f) continue;
            w.hitVictims.Add(f);
            f.OnHit(this, w);
            count++;
        }
        return count;
    }

    // ???????????????????????????????????????????????????????????????????????????????
    // Damage / Block / Parry / Throw
    // ???????????????????????????????????????????????????????????????????????????????
    public void OnHit(SimpleFighter attacker, HitboxWindow w)
    {
        float hs = w.hitstopOverride > 0 ? w.hitstopOverride : hitstopOnHit;
        this.freezeTimer = Mathf.Max(this.freezeTimer, hs);
        attacker.freezeTimer = Mathf.Max(attacker.freezeTimer, hs);

        TakeDamage(w.damage, w.guard, attacker, w, false);

        // Knockback and juggle
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

        // Attacker meter gain
        attacker.GainMeter(w.meterGainOnHit);
    }

    public void OnBlocked(SimpleFighter attacker, HitboxWindow w)
    {
        float hs = w.hitstopOverride > 0 ? w.hitstopOverride : hitstopOnBlock;
        this.freezeTimer = Mathf.Max(this.freezeTimer, hs);
        attacker.freezeTimer = Mathf.Max(attacker.freezeTimer, hs);

        // Chip and blockstun
        if (enableChip && w.guard != Guard.Unblockable && w.guard != Guard.Throw)
        {
            float chip = Mathf.Max(gutsMinDamage, w.damage * chipRate);
            TakeRawDamage(chip);
            chipShown = Mathf.Max(chipShown, hp / maxHP);
        }

        blockstunTimer = Mathf.Max(blockstunTimer, w.blockstun);
        state = grounded ? State.Blockstun : State.Fall;

        // Pushback on block for both
        float s = Mathf.Sign(transform.position.x - attacker.transform.position.x);
        externalVelocity += new Vector2(3.5f * s, 0.5f);
        attacker.externalVelocity -= new Vector2(1.2f * s, 0.1f);

        // Meter gain
        GainMeter(w.meterGainOnBlock);
    }

    public void TakeDamage(float dmg, Guard type, SimpleFighter attacker, HitboxWindow w, bool hardKD)
    {
        // Simple scaling for juggle
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
        // Startup freeze gives tech window
        freezeTimer = Mathf.Max(freezeTimer, throwStartup);
        t.freezeTimer = Mathf.Max(t.freezeTimer, throwStartup);
        t.techTimer = Mathf.Max(t.techTimer, techWindow);
        // Resolve after small delay
        Invoke(nameof(ResolveThrow), throwStartup * 0.95f);
    }

    void ResolveThrow()
    {
        var t = AcquireTarget();
        if (!t) return;
        if (t.techTimer > 0f) { /* teched */ t.techTimer = 0f; return; }
        if (Vector2.Distance(transform.position, t.transform.position) > throwRange) return;
        // Connect
        var w = new HitboxWindow { guard = Guard.Throw };
        OnHit(t, w); // symmetrical call
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
        // Parry sets a brief invuln and extended block window handled on block path
        invulnTimer = Mathf.Max(invulnTimer, parryWindow);
        freezeTimer = Mathf.Max(freezeTimer, 0.015f);
    }

    // ???????????????????????????????????????????????????????????????????????????????
    // AI
    // ???????????????????????????????????????????????????????????????????????????????
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

        // Simple footsies
        moveInput = dist > 2.8f ? FacingDir() : (dist < 1.4f ? -FacingDir() : 0f);

        // Jump over fireballs (not implemented), random jump-in
        if (grounded && UnityEngine.Random.value < 0.04f) inputJump = true;

        // Attack if in range
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

    // ???????????????????????????????????????????????????????????????????????????????
    // HUD
    // ???????????????????????????????????????????????????????????????????????????????
    void SetupHUD()
    {
        if (!hud.autoCreateHUD) return;

        if (!hud.font) hud.font = Resources.GetBuiltinResource<Font>("Arial.ttf");

        if (!hud.existingCanvas)
        {
            hudCanvas = FindFirstObjectByType<Canvas>();
            if (!hudCanvas && hud.createCanvasIfNone)
            {
                var go = new GameObject("FighterCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                hudCanvas = go.GetComponent<Canvas>();
                hudCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                var scaler = go.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
            }
        }
        if (!hudCanvas) hudCanvas = hud.existingCanvas;

        // Panel
        var panelGO = new GameObject($"{fighterName}_HUD", typeof(RectTransform));
        panelGO.transform.SetParent(hudCanvas.transform, false);
        hudPanel = panelGO.GetComponent<RectTransform>();
        hudPanel.sizeDelta = hud.panelSize;

        bool anchorRight = hud.anchorByTeam ? (team % 2 == 1) : (playerIndex % 2 == 1);
        var aMin = new Vector2(anchorRight ? 1f : 0f, 1f);
        var aMax = aMin;
        hudPanel.anchorMin = aMin; hudPanel.anchorMax = aMax; hudPanel.pivot = new Vector2(anchorRight ? 1f : 0f, 1f);
        hudPanel.anchoredPosition = new Vector2(anchorRight ? -hud.padding.x : hud.padding.x, -hud.padding.y - (playerIndex * (hud.panelSize.y + 8f)));

        // Name
        var nameGO = new GameObject("Name", typeof(Text));
        nameGO.transform.SetParent(hudPanel, false);
        nameText = nameGO.GetComponent<Text>();
        nameText.text = fighterName;
        nameText.font = hud.font;
        nameText.alignment = anchorRight ? TextAnchor.UpperRight : TextAnchor.UpperLeft;
        var nameRT = nameGO.GetComponent<RectTransform>();
        nameRT.anchorMin = new Vector2(anchorRight ? 1f : 0f, 1f);
        nameRT.anchorMax = nameRT.anchorMin;
        nameRT.pivot = new Vector2(anchorRight ? 1f : 0f, 1f);
        nameRT.sizeDelta = new Vector2(hud.panelSize.x, 14f);
        nameRT.anchoredPosition = Vector2.zero;

        // HP bar back
        var backGO = new GameObject("HP_Back", typeof(Image));
        backGO.transform.SetParent(hudPanel, false);
        var backImg = backGO.GetComponent<Image>(); backImg.sprite = whiteSprite; backImg.color = new Color(0f, 0f, 0f, 0.6f);
        var backRT = backGO.GetComponent<RectTransform>();
        backRT.anchorMin = new Vector2(anchorRight ? 1f : 0f, 1f);
        backRT.anchorMax = backRT.anchorMin;
        backRT.pivot = new Vector2(anchorRight ? 1f : 0f, 1f);
        backRT.sizeDelta = hud.barSize;
        backRT.anchoredPosition = new Vector2(0f, -16f);

        // HP chip
        var chipGO = new GameObject("HP_Chip", typeof(Image));
        chipGO.transform.SetParent(backRT, false);
        hpChip = chipGO.GetComponent<Image>(); hpChip.sprite = whiteSprite; hpChip.color = hud.chipColor; hpChip.type = Image.Type.Filled; hpChip.fillMethod = Image.FillMethod.Horizontal; hpChip.fillOrigin = anchorRight ? 1 : 0; hpChip.fillAmount = 1f;
        var chipRT = chipGO.GetComponent<RectTransform>(); chipRT.anchorMin = Vector2.zero; chipRT.anchorMax = Vector2.one; chipRT.offsetMin = Vector2.zero; chipRT.offsetMax = Vector2.zero;

        // HP fill
        var hpGO = new GameObject("HP_Fill", typeof(Image));
        hpGO.transform.SetParent(backRT, false);
        hpFill = hpGO.GetComponent<Image>(); hpFill.sprite = whiteSprite; hpFill.color = hud.healthColor; hpFill.type = Image.Type.Filled; hpFill.fillMethod = Image.FillMethod.Horizontal; hpFill.fillOrigin = anchorRight ? 1 : 0; hpFill.fillAmount = 1f;
        var hpRT = hpGO.GetComponent<RectTransform>(); hpRT.anchorMin = Vector2.zero; hpRT.anchorMax = Vector2.one; hpRT.offsetMin = Vector2.zero; hpRT.offsetMax = Vector2.zero;

        // Meter
        var meterBackGO = new GameObject("Meter_Back", typeof(Image));
        meterBackGO.transform.SetParent(hudPanel, false);
        var meterBack = meterBackGO.GetComponent<Image>(); meterBack.sprite = whiteSprite; meterBack.color = new Color(0f, 0f, 0f, 0.6f);
        var meterBackRT = meterBackGO.GetComponent<RectTransform>();
        meterBackRT.anchorMin = new Vector2(anchorRight ? 1f : 0f, 1f);
        meterBackRT.anchorMax = meterBackRT.anchorMin;
        meterBackRT.pivot = new Vector2(anchorRight ? 1f : 0f, 1f);
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

    // ???????????????????????????????????????????????????????????????????????????????
    // Animator driving
    // ???????????????????????????????????????????????????????????????????????????????
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

    // ???????????????????????????????????????????????????????????????????????????????
    // Helpers
    // ???????????????????????????????????????????????????????????????????????????????
    public float FacingDir() => (faceRight ? 1f : -1f) * (flipVisualWithScale ? 1f : 1f);
    public void ApplyVisualFlip()
    {
        if (!flipVisualWithScale || !visualRoot) return;
        var s = visualRoot.localScale;
        s.x = Mathf.Abs(s.x) * (faceRight ? 1f : -1f);
        visualRoot.localScale = s;
    }

    public bool ProbeGround()
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
            if (Mathf.Abs(f.transform.position.y - transform.position.y) > 3.5f) continue; // vertical pass
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
        if (manualTarget) return manualTarget;
        if (!autoFindOpponent) return null;
        if (Time.time - lastRetarget < retargetInterval && manualTarget == null && cachedTarget != null) return cachedTarget;

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
    SimpleFighter cachedTarget;

    public void GainMeter(int v) => meter = Mathf.Clamp(meter + v, 0, maxMeter);
    public bool CanSpendMeter(int v) => meter >= v;
    public void SpendMeter(int v) => meter = Mathf.Clamp(meter - v, 0, maxMeter);

    // ???????????????????????????????????????????????????????????????????????????????
    // Gizmos
    // ???????????????????????????????????????????????????????????????????????????????
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
                    if (w.shape == Shape.Box)
                    {
                        Vector2 center = (Vector2)transform.position + RotateFacing(w.offset);
                        Gizmos.DrawWireCube(center, w.size);
                    }
                    else
                    {
                        Vector2 center = (Vector2)transform.position + RotateFacing(w.offset);
                        Gizmos.DrawWireSphere(center, w.radius);
                    }
                }
            }
        }
    }
}
