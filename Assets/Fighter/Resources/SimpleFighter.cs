using System;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-10)]
public class BasicFighter2D : MonoBehaviour
{
    // ─────────────────────────────────────────────────────────────────────────────
    // PUBLIC TUNABLES — all default to null/0. Validation required.
    // ─────────────────────────────────────────────────────────────────────────────
    [Header("Identity - REQUIRED")]
    public string fighterName;

    [Header("Controls - REQUIRED (keys must not be None)")]
    public KeyCode keyLeft;
    public KeyCode keyRight;
    public KeyCode keyJump;
    public KeyCode keyDash;
    public KeyCode keyLight;
    public KeyCode keyMedium;
    public KeyCode keyHeavy;

    [Header("Movement - REQUIRED")]
    public float moveSpeed;            // units/s > 0
    public float jumpHeight;           // world units > 0 (converted to velocity)
    public float gravityScale;         // > 0
    public float fallGravityScale;     // > 0
    public float dashSpeed;            // units/s > 0
    public float dashDuration;         // seconds > 0
    public float airControl;           // 0..1, but require > 0 to be usable
    public LayerMask groundMask;       // must be set
    public Vector2 groundCheckOffset;  // set in inspector
    public float groundCheckRadius;    // > 0

    [Header("Attacks - REQUIRED (3 attacks)")]
    public Attack light;
    public Attack medium;
    public Attack heavy;

    [Header("Vitals - REQUIRED")]
    public int maxHP;                  // > 0

    [Header("Animator Params - OPTIONAL (empty = skip)")]
    public string p_isGrounded;
    public string p_isWalking;
    public string p_isDashing;
    public string p_isJumping;
    public string p_isKO;
    public string p_speedX;
    public string p_speedY;
    public string trig_Light;
    public string trig_Medium;
    public string trig_Heavy;

    [Header("Visual - OPTIONAL")]
    public Transform visualRoot;       // flip target. null = no flip

    // ─────────────────────────────────────────────────────────────────────────────
    // PRIVATE COMPONENTS (auto-fetched)
    // ─────────────────────────────────────────────────────────────────────────────
    Rigidbody2D rb;
    Animator animator;
    Collider2D body;

    // ─────────────────────────────────────────────────────────────────────────────
    // INTERNAL STATE
    // ─────────────────────────────────────────────────────────────────────────────
    enum State { Idle, Walk, Jump, Fall, Dash, Attack, Hitstun, KO }
    State state;
    bool grounded;
    float stateTimer;
    bool faceRight = true;

    int hp;

    Attack currentAttack;
    float attackTimer;
    HashSet<BasicFighter2D> victimsThisSwing = new HashSet<BasicFighter2D>();
    float hitstunTimer;

    // Registry for hit-detect
    static readonly List<BasicFighter2D> registry = new List<BasicFighter2D>();

    // ─────────────────────────────────────────────────────────────────────────────
    // DATA
    // ─────────────────────────────────────────────────────────────────────────────
    [Serializable]
    public class Attack
    {
        public string name;            // REQUIRED
        public float startup;          // > 0
        public float active;           // > 0
        public float recovery;         // >= 0
        public int damage;             // > 0
        public float hitstun;          // > 0
        public Vector2 hitboxOffset;   // in local units, flipped on X
        public Vector2 hitboxSize;     // > 0
        public Vector2 knockback;      // applied with facing sign
        public string animatorTrigger; // OPTIONAL
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // LIFECYCLE
    // ─────────────────────────────────────────────────────────────────────────────
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        body = GetComponent<Collider2D>();

        if (!ValidateRequired()) { enabled = false; return; }

        rb.gravityScale = gravityScale;
        hp = maxHP;

        if (!registry.Contains(this)) registry.Add(this);
    }

    void OnDestroy()
    {
        registry.Remove(this);
    }

    void Update()
    {
        if (!enabled) return;

        // Timers
        if (hitstunTimer > 0f) hitstunTimer -= Time.deltaTime;

        // State exit conditions
        if (state == State.Hitstun && hitstunTimer <= 0f) state = grounded ? State.Idle : State.Fall;

        // Inputs
        float move = (Input.GetKey(keyLeft) ? -1f : 0f) + (Input.GetKey(keyRight) ? 1f : 0f);
        bool pressJump = Input.GetKeyDown(keyJump);
        bool pressDash = Input.GetKeyDown(keyDash);
        bool pressL = Input.GetKeyDown(keyLight);
        bool pressM = Input.GetKeyDown(keyMedium);
        bool pressH = Input.GetKeyDown(keyHeavy);

        // Ground probe
        grounded = ProbeGround();

        // Face by last nonzero move input
        if (Mathf.Abs(move) > 0.001f) faceRight = move > 0f;
        ApplyVisualFlip();

        // State machine (Update for transitions and non-physics actions)
        switch (state)
        {
            case State.Idle:
            case State.Walk:
                if (pressDash) StartDash();
                else if (pressJump && grounded) DoJump();
                else if (TryAttack(pressL, pressM, pressH)) { /* handled */ }
                else state = (Mathf.Abs(move) > 0.05f) ? State.Walk : State.Idle;
                break;

            case State.Jump:
            case State.Fall:
                if (TryAttack(pressL, pressM, pressH)) { /* air attacks allowed */ }
                if (grounded) state = (Mathf.Abs(move) > 0.05f) ? State.Walk : State.Idle;
                break;

            case State.Dash:
                stateTimer -= Time.deltaTime;
                if (stateTimer <= 0f) state = grounded ? State.Idle : State.Fall;
                if (TryAttack(pressL, pressM, pressH)) { /* allow dash-cancel into attacks */ }
                break;

            case State.Attack:
                TickAttack();
                break;

            case State.Hitstun:
                // wait out timer
                break;

            case State.KO:
                // dead stop
                break;
        }

        DriveAnimator(move);
    }

    void FixedUpdate()
    {
        if (!enabled) return;

        // Gravity scale swap for better jump arc
        rb.gravityScale = (rb.velocity.y < 0f) ? fallGravityScale : gravityScale;

        Vector2 v = rb.velocity;

        switch (state)
        {
            case State.Idle:
                v.x = Mathf.MoveTowards(v.x, 0f, moveSpeed * Time.fixedDeltaTime);
                break;

            case State.Walk:
                {
                    float input = (Input.GetKey(keyLeft) ? -1f : 0f) + (Input.GetKey(keyRight) ? 1f : 0f);
                    float target = input * moveSpeed;
                    float a = grounded ? moveSpeed * 4f : moveSpeed * (2f * Mathf.Max(airControl, 0f));
                    v.x = Mathf.MoveTowards(v.x, target, a * Time.fixedDeltaTime);
                }
                break;

            case State.Jump:
            case State.Fall:
                {
                    float input = (Input.GetKey(keyLeft) ? -1f : 0f) + (Input.GetKey(keyRight) ? 1f : 0f);
                    float target = input * moveSpeed * Mathf.Clamp01(airControl);
                    float a = moveSpeed * 2f * Mathf.Max(airControl, 0f);
                    v.x = Mathf.MoveTowards(v.x, target, a * Time.fixedDeltaTime);
                }
                break;

            case State.Dash:
                v.x = dashSpeed * (faceRight ? 1f : -1f);
                break;

            case State.Attack:
                // allow light drift during attack
                float drift = 0.35f * moveSpeed;
                float inputX = (Input.GetKey(keyLeft) ? -1f : 0f) + (Input.GetKey(keyRight) ? 1f : 0f);
                v.x = Mathf.MoveTowards(v.x, inputX * drift, drift * 6f * Time.fixedDeltaTime);
                break;

            case State.Hitstun:
                // keep physics
                break;

            case State.KO:
                v = Vector2.zero;
                break;
        }

        rb.velocity = v;
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // Movement
    // ─────────────────────────────────────────────────────────────────────────────
    void DoJump()
    {
        float g = Mathf.Abs(Physics2D.gravity.y) * gravityScale;
        if (g <= 0f) { Debug.LogError($"[{name}] gravity invalid; set gravityScale > 0"); enabled = false; return; }
        float v0 = Mathf.Sqrt(2f * g * Mathf.Max(jumpHeight, 0.0001f));
        var v = rb.velocity;
        v.y = v0;
        rb.velocity = v;
        state = State.Jump;
    }

    void StartDash()
    {
        state = State.Dash;
        stateTimer = dashDuration;
    }

    bool ProbeGround()
    {
        Vector2 origin = (Vector2)transform.position + groundCheckOffset;
        var hit = Physics2D.OverlapCircle(origin, groundCheckRadius, groundMask);
        return hit != null;
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // Attacks
    // ─────────────────────────────────────────────────────────────────────────────
    bool TryAttack(bool pressL, bool pressM, bool pressH)
    {
        Attack a = null;
        if (pressH) a = heavy ?? a;
        if (pressM) a = medium ?? a;
        if (pressL) a = light ?? a;
        if (a == null) return false;

        // Validate attack at use-time too
        if (!ValidateAttack(a, "Use")) { enabled = false; return false; }

        currentAttack = a;
        attackTimer = 0f;
        victimsThisSwing.Clear();
        state = State.Attack;

        // Anim triggers
        if (animator)
        {
            if (!string.IsNullOrEmpty(a.animatorTrigger)) animator.SetTrigger(a.animatorTrigger);
            if (!string.IsNullOrEmpty(trig_Light) && a == light) animator.SetTrigger(trig_Light);
            if (!string.IsNullOrEmpty(trig_Medium) && a == medium) animator.SetTrigger(trig_Medium);
            if (!string.IsNullOrEmpty(trig_Heavy) && a == heavy) animator.SetTrigger(trig_Heavy);
        }
        return true;
    }

    void TickAttack()
    {
        if (currentAttack == null) { state = grounded ? State.Idle : State.Fall; return; }

        attackTimer += Time.deltaTime;

        // Active window
        float start = currentAttack.startup;
        float end = start + currentAttack.active;
        if (attackTimer >= start && attackTimer <= end)
        {
            DoHitDetect(currentAttack);
        }

        // End on recovery complete
        float total = currentAttack.startup + currentAttack.active + currentAttack.recovery;
        if (attackTimer >= total)
        {
            currentAttack = null;
            state = grounded ? State.Idle : State.Fall;
        }
    }

    void DoHitDetect(Attack a)
    {
        Vector2 center = (Vector2)transform.position + RotateFacing(a.hitboxOffset);
        Vector2 size = a.hitboxSize;
        var cols = Physics2D.OverlapBoxAll(center, size, 0f);
        for (int i = 0; i < cols.Length; i++)
        {
            var f = cols[i].GetComponentInParent<BasicFighter2D>();
            if (!f || f == this) continue;
            if (victimsThisSwing.Contains(f)) continue;

            victimsThisSwing.Add(f);
            f.OnHit(this, a);
        }
    }

    void OnHit(BasicFighter2D attacker, Attack a)
    {
        if (state == State.KO) return;

        // Damage
        hp = Mathf.Max(0, hp - Mathf.Max(0, a.damage));
        if (hp == 0)
        {
            state = State.KO;
            rb.velocity = Vector2.zero;
            if (!string.IsNullOrEmpty(p_isKO) && animator) animator.SetBool(p_isKO, true);
            return;
        }

        // Hitstun and knockback
        hitstunTimer = Mathf.Max(hitstunTimer, a.hitstun);
        state = State.Hitstun;

        Vector2 kb = new Vector2(a.knockback.x * (attacker.faceRight ? 1f : -1f), a.knockback.y);
        rb.velocity = new Vector2(kb.x, Mathf.Max(rb.velocity.y, kb.y)); // simple overwrite with min Y keep
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // Animator
    // ─────────────────────────────────────────────────────────────────────────────
    void DriveAnimator(float moveInput)
    {
        if (!animator) return;
        if (!string.IsNullOrEmpty(p_isGrounded)) animator.SetBool(p_isGrounded, grounded);
        if (!string.IsNullOrEmpty(p_isWalking)) animator.SetBool(p_isWalking, grounded && Mathf.Abs(moveInput) > 0.05f);
        if (!string.IsNullOrEmpty(p_isDashing)) animator.SetBool(p_isDashing, state == State.Dash);
        if (!string.IsNullOrEmpty(p_isJumping)) animator.SetBool(p_isJumping, state == State.Jump || state == State.Fall);
        if (!string.IsNullOrEmpty(p_speedX)) animator.SetFloat(p_speedX, Mathf.Abs(rb.velocity.x));
        if (!string.IsNullOrEmpty(p_speedY)) animator.SetFloat(p_speedY, rb.velocity.y);
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // Helpers
    // ─────────────────────────────────────────────────────────────────────────────
    float FacingDir() => faceRight ? 1f : -1f;

    Vector2 RotateFacing(Vector2 v) => new Vector2(v.x * FacingDir(), v.y);

    void ApplyVisualFlip()
    {
        if (!visualRoot) return;
        var s = visualRoot.localScale;
        s.x = Mathf.Abs(s.x) * (faceRight ? 1f : -1f);
        visualRoot.localScale = s;
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // Validation
    // ─────────────────────────────────────────────────────────────────────────────
    bool ValidateRequired()
    {
        bool ok = true;
        void Fail(string m) { Debug.LogError($"[BasicFighter2D:{gameObject.name}] {m}", this); ok = false; }

        if (string.IsNullOrWhiteSpace(fighterName)) Fail("fighterName is REQUIRED.");

        if (keyLeft == KeyCode.None) Fail("keyLeft is REQUIRED.");
        if (keyRight == KeyCode.None) Fail("keyRight is REQUIRED.");
        if (keyJump == KeyCode.None) Fail("keyJump is REQUIRED.");
        if (keyDash == KeyCode.None) Fail("keyDash is REQUIRED.");
        if (keyLight == KeyCode.None) Fail("keyLight is REQUIRED.");
        if (keyMedium == KeyCode.None) Fail("keyMedium is REQUIRED.");
        if (keyHeavy == KeyCode.None) Fail("keyHeavy is REQUIRED.");

        if (!rb) Fail("Rigidbody2D missing on GameObject.");
        if (!animator) Fail("Animator missing on GameObject.");
        if (!body) Fail("Collider2D missing on GameObject.");

        if (moveSpeed <= 0f) Fail("moveSpeed must be > 0.");
        if (jumpHeight <= 0f) Fail("jumpHeight must be > 0.");
        if (gravityScale <= 0f) Fail("gravityScale must be > 0.");
        if (fallGravityScale <= 0f) Fail("fallGravityScale must be > 0.");
        if (dashSpeed <= 0f) Fail("dashSpeed must be > 0.");
        if (dashDuration <= 0f) Fail("dashDuration must be > 0.");
        if (airControl <= 0f) Fail("airControl must be > 0.");
        if (groundMask == 0) Fail("groundMask must be set.");
        if (groundCheckRadius <= 0f) Fail("groundCheckRadius must be > 0.");

        if (maxHP <= 0) Fail("maxHP must be > 0.");

        if (light == null) Fail("light attack is REQUIRED.");
        else if (!ValidateAttack(light, "light")) ok = false;
        if (medium == null) Fail("medium attack is REQUIRED.");
        else if (!ValidateAttack(medium, "medium")) ok = false;
        if (heavy == null) Fail("heavy attack is REQUIRED.");
        else if (!ValidateAttack(heavy, "heavy")) ok = false;

        return ok;
    }

    bool ValidateAttack(Attack a, string label)
    {
        bool ok = true;
        void Fail(string m) { Debug.LogError($"[BasicFighter2D:{gameObject.name}] Attack '{label}': {m}", this); ok = false; }
        if (string.IsNullOrWhiteSpace(a.name)) Fail("name is REQUIRED.");
        if (a.startup <= 0f) Fail("startup must be > 0.");
        if (a.active <= 0f) Fail("active must be > 0.");
        if (a.recovery < 0f) Fail("recovery must be >= 0.");
        if (a.damage <= 0) Fail("damage must be > 0.");
        if (a.hitstun <= 0f) Fail("hitstun must be > 0.");
        if (a.hitboxSize.x <= 0f || a.hitboxSize.y <= 0f) Fail("hitboxSize must be > 0.");
        return ok;
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // Gizmos
    // ─────────────────────────────────────────────────────────────────────────────
    void OnDrawGizmosSelected()
    {
        // Ground check
        Gizmos.color = Color.cyan;
        Vector2 origin = (Vector2)transform.position + groundCheckOffset;
        Gizmos.DrawWireSphere(origin, groundCheckRadius);

        // Active hitbox preview if attacking
        if (state == State.Attack && currentAttack != null)
        {
            Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.35f);
            Vector2 center = (Vector2)transform.position + RotateFacing(currentAttack.hitboxOffset);
            Gizmos.DrawWireCube(center, currentAttack.hitboxSize);
        }
    }
}
