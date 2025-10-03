using System;using System;using System;

using System.Collections.Generic;

using UnityEngine;using System.Collections.Generic;using System.Collections.Generic;

using UnityEngine.UI;

using UnityEngine;using UnityEngine;

[DefaultExecutionOrder(-10)]

public class BasicFighter2D : MonoBehaviourusing UnityEngine.UI;

{

    // ─────────────────────────────────────────────────────────────────────────────[DefaultExecutionOrder(-10)]

    // PUBLIC TUNABLES — all default to null/0. Validation required.

    // ─────────────────────────────────────────────────────────────────────────────[DefaultExecutionOrder(-10)]public class BasicFighter2D : MonoBehaviour

    [Header("Identity - REQUIRED")]

    public string fighterName;public class BasicFighter2D : MonoBehaviour{



    [Header("Controls - REQUIRED (keys must not be None)")]{    // ─────────────────────────────────────────────────────────────────────────────

    public KeyCode keyLeft;

    public KeyCode keyRight;    // ─────────────────────────────────────────────────────────────────────────────    // PUBLIC TUNABLES — all default to null/0. Validation required.

    public KeyCode keyJump;

    public KeyCode keyDash;    // PUBLIC TUNABLES — all default to null/0. Validation required.    // ─────────────────────────────────────────────────────────────────────────────

    public KeyCode keyLight;

    public KeyCode keyMedium;    // ─────────────────────────────────────────────────────────────────────────────    [Header("Identity - REQUIRED")]

    public KeyCode keyHeavy;

    [Header("Identity - REQUIRED")]    public string fighterName;

    [Header("Movement - REQUIRED")]

    public float moveSpeed;            // units/s > 0    public string fighterName;

    public float jumpHeight;           // world units > 0 (converted to velocity)

    public float gravityScale;         // > 0    [Header("Controls - REQUIRED (keys must not be None)")]

    public float fallGravityScale;     // > 0

    public float dashSpeed;            // units/s > 0    [Header("Controls - REQUIRED (keys must not be None)")]    public KeyCode keyLeft;

    public float dashDuration;         // seconds > 0

    public float airControl;           // 0..1, but require > 0 to be usable    public KeyCode keyLeft;    public KeyCode keyRight;

    public LayerMask groundMask;       // must be set

    public Vector2 groundCheckOffset;  // set in inspector    public KeyCode keyRight;    public KeyCode keyJump;

    public float groundCheckRadius;    // > 0

    public KeyCode keyJump;    public KeyCode keyDash;

    [Header("Attacks - REQUIRED (3 attacks)")]

    public Attack lightAttack;    public KeyCode keyDash;    public KeyCode keyLight;

    public Attack mediumAttack;

    public Attack heavyAttack;    public KeyCode keyLight;    public KeyCode keyMedium;



    [Header("Vitals - REQUIRED")]    public KeyCode keyMedium;    public KeyCode keyHeavy;

    public int maxHP;                  // > 0

    public KeyCode keyHeavy;

    [Header("UI - REQUIRED")]

    public Image healthBarImage;    [Header("Movement - REQUIRED")]



    [Header("State Animations - REQUIRED")]    [Header("Movement - REQUIRED")]    public float moveSpeed;            // units/s > 0

    [Tooltip("REQUIRED: Idle animation clip")]

    public AnimationClip animIdle;    public float moveSpeed;            // units/s > 0    public float jumpHeight;           // world units > 0 (converted to velocity)

    [Tooltip("REQUIRED: Walk animation clip")]

    public AnimationClip animWalk;    public float jumpHeight;           // world units > 0 (converted to velocity)    public float gravityScale;         // > 0

    [Tooltip("REQUIRED: Jump animation clip")]

    public AnimationClip animJump;    public float gravityScale;         // > 0    public float fallGravityScale;     // > 0

    [Tooltip("REQUIRED: Fall animation clip")]

    public AnimationClip animFall;    public float fallGravityScale;     // > 0    public float dashSpeed;            // units/s > 0

    [Tooltip("REQUIRED: Dash animation clip")]

    public AnimationClip animDash;    public float dashSpeed;            // units/s > 0    public float dashDuration;         // seconds > 0

    [Tooltip("REQUIRED: Hitstun animation clip")]

    public AnimationClip animHitstun;    public float dashDuration;         // seconds > 0    public float airControl;           // 0..1, but require > 0 to be usable

    [Tooltip("REQUIRED: KO/death animation clip")]

    public AnimationClip animKO;    public float airControl;           // 0..1, but require > 0 to be usable    public LayerMask groundMask;       // must be set



    // ─────────────────────────────────────────────────────────────────────────────    public LayerMask groundMask;       // must be set    public Vector2 groundCheckOffset;  // set in inspector

    // PRIVATE COMPONENTS (auto-fetched)

    // ─────────────────────────────────────────────────────────────────────────────    public Vector2 groundCheckOffset;  // set in inspector    public float groundCheckRadius;    // > 0

    Rigidbody2D rb;

    Animator animator;    public float groundCheckRadius;    // > 0

    Collider2D body;

    [Header("Attacks - REQUIRED (3 attacks)")]

    // Animator parameter names (fixed)

    const string paramIdle = "Idle";    [Header("Attacks - REQUIRED (3 attacks)")]    public Attack lightAttack;

    const string paramWalk = "Walk";

    const string paramJump = "Jump";    public Attack lightAttack;    public Attack mediumAttack;

    const string paramFall = "Fall";

    const string paramDash = "Dash";    public Attack mediumAttack;    public Attack heavyAttack;

    const string paramHitstun = "Hitstun";

    const string paramKO = "KO";    public Attack heavyAttack;



    // ─────────────────────────────────────────────────────────────────────────────    [Header("Vitals - REQUIRED")]

    // INTERNAL STATE

    // ─────────────────────────────────────────────────────────────────────────────    [Header("Vitals - REQUIRED")]    public int maxHP;                  // > 0

    enum State { Idle, Walk, Jump, Fall, Dash, Attack, Hitstun, KO }

    State state;    public int maxHP;                  // > 0

    State prevState; // Track state changes for animation updates

    bool grounded;    [Header("State Animations - REQUIRED")]

    float stateTimer;

    bool faceRight = true;    [Header("UI - REQUIRED")]    [Tooltip("REQUIRED: Idle animation clip")]



    int hp;    public Image healthBarImage;    public AnimationClip animIdle;



    Attack currentAttack;    [Tooltip("REQUIRED: Walk animation clip")]

    float attackTimer;

    HashSet<BasicFighter2D> victimsThisSwing = new HashSet<BasicFighter2D>();    [Header("State Animations - REQUIRED")]    public AnimationClip animWalk;

    float hitstunTimer;

    [Tooltip("REQUIRED: Idle animation clip")]    [Tooltip("REQUIRED: Jump animation clip")]

    // Registry for hit-detect

    static readonly List<BasicFighter2D> registry = new List<BasicFighter2D>();    public AnimationClip animIdle;    public AnimationClip animJump;



    // ─────────────────────────────────────────────────────────────────────────────    [Tooltip("REQUIRED: Walk animation clip")]    [Tooltip("REQUIRED: Fall animation clip")]

    // DATA

    // ─────────────────────────────────────────────────────────────────────────────    public AnimationClip animWalk;    public AnimationClip animFall;

    [Serializable]

    public class Attack    [Tooltip("REQUIRED: Jump animation clip")]    [Tooltip("REQUIRED: Dash animation clip")]

    {

        [Tooltip("REQUIRED: Attack name for debugging")]    public AnimationClip animJump;    public AnimationClip animDash;

        public string name;

            [Tooltip("REQUIRED: Fall animation clip")]    [Tooltip("REQUIRED: Hitstun animation clip")]

        [Header("Animation - REQUIRED")]

        [Tooltip("REQUIRED: AnimationClip to play when attack starts")]    public AnimationClip animFall;    public AnimationClip animHitstun;

        public AnimationClip anim;

            [Tooltip("REQUIRED: Dash animation clip")]    [Tooltip("REQUIRED: KO/death animation clip")]

        [Header("Timing (seconds)")]

        [Tooltip("REQUIRED: Startup frames before hitbox becomes active (> 0)")]    public AnimationClip animDash;    public AnimationClip animKO;

        public float startup;

        [Tooltip("REQUIRED: Duration hitbox remains active (> 0)")]    [Tooltip("REQUIRED: Hitstun animation clip")]

        public float active;

        [Tooltip("Recovery time after active frames end (>= 0)")]    public AnimationClip animHitstun;    // ─────────────────────────────────────────────────────────────────────────────

        public float recovery;

            [Tooltip("REQUIRED: KO/death animation clip")]    // PRIVATE COMPONENTS (auto-fetched)

        [Header("Damage & Stun")]

        [Tooltip("REQUIRED: Damage dealt on hit (> 0)")]    public AnimationClip animKO;    // ─────────────────────────────────────────────────────────────────────────────

        public int damage;

        [Tooltip("REQUIRED: Hitstun duration in seconds (> 0)")]    Rigidbody2D rb;

        public float hitstun;

            // ─────────────────────────────────────────────────────────────────────────────    Animator animator;

        [Header("Hitbox")]

        [Tooltip("Hitbox center offset from fighter position (flipped with facing)")]    // PRIVATE COMPONENTS (auto-fetched)    Collider2D body;

        public Vector2 hitboxOffset;

        [Tooltip("REQUIRED: Hitbox width and height (> 0)")]    // ─────────────────────────────────────────────────────────────────────────────

        public Vector2 hitboxSize;

            Rigidbody2D rb;    // Animator parameter names (fixed)

        [Header("Knockback")]

        [Tooltip("Knockback force applied on hit (X flipped with facing)")]    Animator animator;    const string paramIdle = "Idle";

        public Vector2 knockback;

    }    Collider2D body;    const string paramWalk = "Walk";



    // ─────────────────────────────────────────────────────────────────────────────    const string paramJump = "Jump";

    // LIFECYCLE

    // ─────────────────────────────────────────────────────────────────────────────    // Animator parameter names (fixed)    const string paramFall = "Fall";

    void Awake()

    {    const string paramIdle = "Idle";    const string paramDash = "Dash";

        rb = GetComponent<Rigidbody2D>();

        animator = GetComponent<Animator>();    const string paramWalk = "Walk";    const string paramHitstun = "Hitstun";

        body = GetComponent<Collider2D>();

    const string paramJump = "Jump";    const string paramKO = "KO";

        if (!ValidateRequired()) { enabled = false; return; }

    const string paramFall = "Fall";

        rb.gravityScale = gravityScale;

        hp = maxHP;    const string paramDash = "Dash";    // ─────────────────────────────────────────────────────────────────────────────

        if (healthBarImage != null) healthBarImage.fillAmount = 1f;

    const string paramHitstun = "Hitstun";    // INTERNAL STATE

        if (!registry.Contains(this)) registry.Add(this);

    }    const string paramKO = "KO";    // ─────────────────────────────────────────────────────────────────────────────



    void OnDestroy()    enum State { Idle, Walk, Jump, Fall, Dash, Attack, Hitstun, KO }

    {

        registry.Remove(this);    // ─────────────────────────────────────────────────────────────────────────────    State state;

    }

    // INTERNAL STATE    State prevState; // Track state changes for animation updates

    void Update()

    {    // ─────────────────────────────────────────────────────────────────────────────    bool grounded;

        if (!enabled) return;

    enum State { Idle, Walk, Jump, Fall, Dash, Attack, Hitstun, KO }    float stateTimer;

        // Timers

        if (hitstunTimer > 0f) hitstunTimer -= Time.deltaTime;    State state;    bool faceRight = true;



        // State exit conditions    State prevState; // Track state changes for animation updates

        if (state == State.Hitstun && hitstunTimer <= 0f) state = grounded ? State.Idle : State.Fall;

    bool grounded;    int hp;

        // Inputs

        float move = (Input.GetKey(keyLeft) ? -1f : 0f) + (Input.GetKey(keyRight) ? 1f : 0f);    float stateTimer;

        bool pressJump = Input.GetKeyDown(keyJump);

        bool pressDash = Input.GetKeyDown(keyDash);    bool faceRight = true;    Attack currentAttack;

        bool pressL = Input.GetKeyDown(keyLight);

        bool pressM = Input.GetKeyDown(keyMedium);    float attackTimer;

        bool pressH = Input.GetKeyDown(keyHeavy);

    int hp;    HashSet<BasicFighter2D> victimsThisSwing = new HashSet<BasicFighter2D>();

        // Ground probe

        grounded = ProbeGround();    float hitstunTimer;



        // Face by last nonzero move input    Attack currentAttack;

        if (Mathf.Abs(move) > 0.001f) faceRight = move > 0f;

    float attackTimer;    // Registry for hit-detect

        // State machine (Update for transitions and non-physics actions)

        switch (state)    HashSet<BasicFighter2D> victimsThisSwing = new HashSet<BasicFighter2D>();    static readonly List<BasicFighter2D> registry = new List<BasicFighter2D>();

        {

            case State.Idle:    float hitstunTimer;

            case State.Walk:

                if (pressDash) StartDash();    // ─────────────────────────────────────────────────────────────────────────────

                else if (pressJump && grounded) DoJump();

                else if (TryAttack(pressL, pressM, pressH)) { /* handled */ }    // Registry for hit-detect    // DATA

                else state = (Mathf.Abs(move) > 0.05f) ? State.Walk : State.Idle;

                break;    static readonly List<BasicFighter2D> registry = new List<BasicFighter2D>();    // ─────────────────────────────────────────────────────────────────────────────



            case State.Jump:    [Serializable]

            case State.Fall:

                if (TryAttack(pressL, pressM, pressH)) { /* air attacks allowed */ }    // ─────────────────────────────────────────────────────────────────────────────    public class Attack

                if (grounded) state = (Mathf.Abs(move) > 0.05f) ? State.Walk : State.Idle;

                break;    // DATA    {



            case State.Dash:    // ─────────────────────────────────────────────────────────────────────────────        [Tooltip("REQUIRED: Attack name for debugging")]

                stateTimer -= Time.deltaTime;

                if (stateTimer <= 0f) state = grounded ? State.Idle : State.Fall;    [Serializable]        public string name;

                if (TryAttack(pressL, pressM, pressH)) { /* allow dash-cancel into attacks */ }

                break;    public class Attack        



            case State.Attack:    {        [Header("Animation - REQUIRED")]

                TickAttack();

                break;        [Tooltip("REQUIRED: Attack name for debugging")]        [Tooltip("REQUIRED: AnimationClip to play when attack starts")]



            case State.Hitstun:        public string name;        public AnimationClip anim;

                // wait out timer

                break;                



            case State.KO:        [Header("Animation - REQUIRED")]        [Header("Timing (seconds)")]

                // dead stop

                break;        [Tooltip("REQUIRED: AnimationClip to play when attack starts")]        [Tooltip("REQUIRED: Startup frames before hitbox becomes active (> 0)")]

        }

        public AnimationClip anim;        public float startup;

        UpdateStateAnimation();

                [Tooltip("REQUIRED: Duration hitbox remains active (> 0)")]

        // Update healthbar

        if (healthBarImage != null) healthBarImage.fillAmount = (float)hp / maxHP;        [Header("Timing (seconds)")]        public float active;

    }

        [Tooltip("REQUIRED: Startup frames before hitbox becomes active (> 0)")]        [Tooltip("Recovery time after active frames end (>= 0)")]

    void FixedUpdate()

    {        public float startup;        public float recovery;

        if (!enabled) return;

        [Tooltip("REQUIRED: Duration hitbox remains active (> 0)")]        

        // Gravity scale swap for better jump arc

        rb.gravityScale = (rb.linearVelocity.y < 0f) ? fallGravityScale : gravityScale;        public float active;        [Header("Damage & Stun")]



        Vector2 v = rb.linearVelocity;        [Tooltip("Recovery time after active frames end (>= 0)")]        [Tooltip("REQUIRED: Damage dealt on hit (> 0)")]



        switch (state)        public float recovery;        public int damage;

        {

            case State.Idle:                [Tooltip("REQUIRED: Hitstun duration in seconds (> 0)")]

                v.x = Mathf.MoveTowards(v.x, 0f, moveSpeed * Time.fixedDeltaTime);

                break;        [Header("Damage & Stun")]        public float hitstun;



            case State.Walk:        [Tooltip("REQUIRED: Damage dealt on hit (> 0)")]        

                {

                    float input = (Input.GetKey(keyLeft) ? -1f : 0f) + (Input.GetKey(keyRight) ? 1f : 0f);        public int damage;        [Header("Hitbox")]

                    float target = input * moveSpeed;

                    float a = grounded ? moveSpeed * 4f : moveSpeed * (2f * Mathf.Max(airControl, 0f));        [Tooltip("REQUIRED: Hitstun duration in seconds (> 0)")]        [Tooltip("Hitbox center offset from fighter position (flipped with facing)")]

                    v.x = Mathf.MoveTowards(v.x, target, a * Time.fixedDeltaTime);

                }        public float hitstun;        public Vector2 hitboxOffset;

                break;

                [Tooltip("REQUIRED: Hitbox width and height (> 0)")]

            case State.Jump:

            case State.Fall:        [Header("Hitbox")]        public Vector2 hitboxSize;

                {

                    float input = (Input.GetKey(keyLeft) ? -1f : 0f) + (Input.GetKey(keyRight) ? 1f : 0f);        [Tooltip("Hitbox center offset from fighter position (flipped with facing)")]        

                    float target = input * moveSpeed * Mathf.Clamp01(airControl);

                    float a = moveSpeed * 2f * Mathf.Max(airControl, 0f);        public Vector2 hitboxOffset;        [Header("Knockback")]

                    v.x = Mathf.MoveTowards(v.x, target, a * Time.fixedDeltaTime);

                }        [Tooltip("REQUIRED: Hitbox width and height (> 0)")]        [Tooltip("Knockback force applied on hit (X flipped with facing)")]

                break;

        public Vector2 hitboxSize;        public Vector2 knockback;

            case State.Dash:

                v.x = dashSpeed * (faceRight ? 1f : -1f);            }

                break;

        [Header("Knockback")]

            case State.Attack:

                // allow light drift during attack        [Tooltip("Knockback force applied on hit (X flipped with facing)")]    // ─────────────────────────────────────────────────────────────────────────────

                float drift = 0.35f * moveSpeed;

                float inputX = (Input.GetKey(keyLeft) ? -1f : 0f) + (Input.GetKey(keyRight) ? 1f : 0f);        public Vector2 knockback;    // LIFECYCLE

                v.x = Mathf.MoveTowards(v.x, inputX * drift, drift * 6f * Time.fixedDeltaTime);

                break;    }    // ─────────────────────────────────────────────────────────────────────────────



            case State.Hitstun:    void Awake()

                // keep physics

                break;    // ─────────────────────────────────────────────────────────────────────────────    {



            case State.KO:    // LIFECYCLE        rb = GetComponent<Rigidbody2D>();

                v = Vector2.zero;

                break;    // ─────────────────────────────────────────────────────────────────────────────        animator = GetComponent<Animator>();

        }

    void Awake()        body = GetComponent<Collider2D>();

        rb.linearVelocity = v;

    }    {



    // ─────────────────────────────────────────────────────────────────────────────        rb = GetComponent<Rigidbody2D>();        if (!ValidateRequired()) { enabled = false; return; }

    // Movement

    // ─────────────────────────────────────────────────────────────────────────────        animator = GetComponent<Animator>();

    void DoJump()

    {        body = GetComponent<Collider2D>();        rb.gravityScale = gravityScale;

        float g = Mathf.Abs(Physics2D.gravity.y) * gravityScale;

        if (g <= 0f) { Debug.LogError($"[{name}] gravity invalid; set gravityScale > 0"); enabled = false; return; }        hp = maxHP;

        float v0 = Mathf.Sqrt(2f * g * Mathf.Max(jumpHeight, 0.0001f));

        var v = rb.linearVelocity;        if (!ValidateRequired()) { enabled = false; return; }

        v.y = v0;

        rb.linearVelocity = v;        if (!registry.Contains(this)) registry.Add(this);

        state = State.Jump;

    }        rb.gravityScale = gravityScale;    }



    void StartDash()        hp = maxHP;

    {

        state = State.Dash;        if (healthBarImage != null) healthBarImage.fillAmount = 1f;    void OnDestroy()

        stateTimer = dashDuration;

    }    {



    bool ProbeGround()        if (!registry.Contains(this)) registry.Add(this);        registry.Remove(this);

    {

        Vector2 origin = (Vector2)transform.position + groundCheckOffset;    }    }

        var hit = Physics2D.OverlapCircle(origin, groundCheckRadius, groundMask);

        return hit != null;

    }

    void OnDestroy()    void Update()

    // ─────────────────────────────────────────────────────────────────────────────

    // Attacks    {    {

    // ─────────────────────────────────────────────────────────────────────────────

    bool TryAttack(bool pressL, bool pressM, bool pressH)        registry.Remove(this);        if (!enabled) return;

    {

        Attack a = null;    }

        if (pressH) a = heavyAttack ?? a;

        if (pressM) a = mediumAttack ?? a;        // Timers

        if (pressL) a = lightAttack ?? a;

        if (a == null) return false;    void Update()        if (hitstunTimer > 0f) hitstunTimer -= Time.deltaTime;



        // Validate attack at use-time too    {

        if (!ValidateAttack(a, "Use")) { enabled = false; return false; }

        if (!enabled) return;        // State exit conditions

        currentAttack = a;

        attackTimer = 0f;        if (state == State.Hitstun && hitstunTimer <= 0f) state = grounded ? State.Idle : State.Fall;

        victimsThisSwing.Clear();

        state = State.Attack;        // Timers



        // Play animation clip directly (required)        if (hitstunTimer > 0f) hitstunTimer -= Time.deltaTime;        // Inputs

        animator.Play(a.anim.name, 0, 0f);

        return true;        float move = (Input.GetKey(keyLeft) ? -1f : 0f) + (Input.GetKey(keyRight) ? 1f : 0f);

    }

        // State exit conditions        bool pressJump = Input.GetKeyDown(keyJump);

    void TickAttack()

    {        if (state == State.Hitstun && hitstunTimer <= 0f) state = grounded ? State.Idle : State.Fall;        bool pressDash = Input.GetKeyDown(keyDash);

        if (currentAttack == null) { state = grounded ? State.Idle : State.Fall; return; }

        bool pressL = Input.GetKeyDown(keyLight);

        attackTimer += Time.deltaTime;

        // Inputs        bool pressM = Input.GetKeyDown(keyMedium);

        // Active window

        float start = currentAttack.startup;        float move = (Input.GetKey(keyLeft) ? -1f : 0f) + (Input.GetKey(keyRight) ? 1f : 0f);        bool pressH = Input.GetKeyDown(keyHeavy);

        float end = start + currentAttack.active;

        if (attackTimer >= start && attackTimer <= end)        bool pressJump = Input.GetKeyDown(keyJump);

        {

            DoHitDetect(currentAttack);        bool pressDash = Input.GetKeyDown(keyDash);        // Ground probe

        }

        bool pressL = Input.GetKeyDown(keyLight);        grounded = ProbeGround();

        // End on recovery complete

        float total = currentAttack.startup + currentAttack.active + currentAttack.recovery;        bool pressM = Input.GetKeyDown(keyMedium);

        if (attackTimer >= total)

        {        bool pressH = Input.GetKeyDown(keyHeavy);        // Face by last nonzero move input

            currentAttack = null;

            state = grounded ? State.Idle : State.Fall;        if (Mathf.Abs(move) > 0.001f) faceRight = move > 0f;

        }

    }        // Ground probe



    void DoHitDetect(Attack a)        grounded = ProbeGround();        // State machine (Update for transitions and non-physics actions)

    {

        Vector2 center = (Vector2)transform.position + RotateFacing(a.hitboxOffset);        switch (state)

        Vector2 size = a.hitboxSize;

        var cols = Physics2D.OverlapBoxAll(center, size, 0f);        // Face by last nonzero move input        {

        for (int i = 0; i < cols.Length; i++)

        {        if (Mathf.Abs(move) > 0.001f) faceRight = move > 0f;            case State.Idle:

            var f = cols[i].GetComponentInParent<BasicFighter2D>();

            if (!f || f == this) continue;            case State.Walk:

            if (victimsThisSwing.Contains(f)) continue;

        // State machine (Update for transitions and non-physics actions)                if (pressDash) StartDash();

            victimsThisSwing.Add(f);

            f.OnHit(this, a);        switch (state)                else if (pressJump && grounded) DoJump();

        }

    }        {                else if (TryAttack(pressL, pressM, pressH)) { /* handled */ }



    void OnHit(BasicFighter2D attacker, Attack a)            case State.Idle:                else state = (Mathf.Abs(move) > 0.05f) ? State.Walk : State.Idle;

    {

        if (state == State.KO) return;            case State.Walk:                break;



        // Damage                if (pressDash) StartDash();

        hp = Mathf.Max(0, hp - Mathf.Max(0, a.damage));

        if (hp == 0)                else if (pressJump && grounded) DoJump();            case State.Jump:

        {

            state = State.KO;                else if (TryAttack(pressL, pressM, pressH)) { /* handled */ }            case State.Fall:

            rb.linearVelocity = Vector2.zero;

            return;                else state = (Mathf.Abs(move) > 0.05f) ? State.Walk : State.Idle;                if (TryAttack(pressL, pressM, pressH)) { /* air attacks allowed */ }

        }

                break;                if (grounded) state = (Mathf.Abs(move) > 0.05f) ? State.Walk : State.Idle;

        // Hitstun and knockback

        hitstunTimer = Mathf.Max(hitstunTimer, a.hitstun);                break;

        state = State.Hitstun;

            case State.Jump:

        Vector2 kb = new Vector2(a.knockback.x * (attacker.faceRight ? 1f : -1f), a.knockback.y);

        rb.linearVelocity = new Vector2(kb.x, Mathf.Max(rb.linearVelocity.y, kb.y)); // simple overwrite with min Y keep            case State.Fall:            case State.Dash:

    }

                if (TryAttack(pressL, pressM, pressH)) { /* air attacks allowed */ }                stateTimer -= Time.deltaTime;

    // ─────────────────────────────────────────────────────────────────────────────

    // Animation                if (grounded) state = (Mathf.Abs(move) > 0.05f) ? State.Walk : State.Idle;                if (stateTimer <= 0f) state = grounded ? State.Idle : State.Fall;

    // ─────────────────────────────────────────────────────────────────────────────

    void UpdateStateAnimation()                break;                if (TryAttack(pressL, pressM, pressH)) { /* allow dash-cancel into attacks */ }

    {

        // Only update animation if state changed                break;

        if (state == prevState) return;

        prevState = state;            case State.Dash:



        // Reset all state bools                stateTimer -= Time.deltaTime;            case State.Attack:

        SetAnimBool(paramIdle, false);

        SetAnimBool(paramWalk, false);                if (stateTimer <= 0f) state = grounded ? State.Idle : State.Fall;                TickAttack();

        SetAnimBool(paramJump, false);

        SetAnimBool(paramFall, false);                if (TryAttack(pressL, pressM, pressH)) { /* allow dash-cancel into attacks */ }                break;

        SetAnimBool(paramDash, false);

        SetAnimBool(paramHitstun, false);                break;

        SetAnimBool(paramKO, false);

            case State.Hitstun:

        // Set the current state bool to true

        switch (state)            case State.Attack:                // wait out timer

        {

            case State.Idle: SetAnimBool(paramIdle, true); break;                TickAttack();                break;

            case State.Walk: SetAnimBool(paramWalk, true); break;

            case State.Jump: SetAnimBool(paramJump, true); break;                break;

            case State.Fall: SetAnimBool(paramFall, true); break;

            case State.Dash: SetAnimBool(paramDash, true); break;            case State.KO:

            case State.Hitstun: SetAnimBool(paramHitstun, true); break;

            case State.KO: SetAnimBool(paramKO, true); break;            case State.Hitstun:                // dead stop

            case State.Attack: 

                // Attacks still play directly since they're temporary                // wait out timer                break;

                break;

        }                break;        }

    }



    void SetAnimBool(string paramName, bool value)

    {            case State.KO:        UpdateStateAnimation();

        if (string.IsNullOrEmpty(paramName) || animator == null) return;

        animator.SetBool(paramName, value);                // dead stop    }

    }

                break;

    // ─────────────────────────────────────────────────────────────────────────────

    // Helpers        }    void FixedUpdate()

    // ─────────────────────────────────────────────────────────────────────────────

    float FacingDir() => faceRight ? 1f : -1f;    {



    Vector2 RotateFacing(Vector2 v) => new Vector2(v.x * FacingDir(), v.y);        UpdateStateAnimation();        if (!enabled) return;



    // ─────────────────────────────────────────────────────────────────────────────

    // Validation

    // ─────────────────────────────────────────────────────────────────────────────        // Update healthbar        // Gravity scale swap for better jump arc

    bool ValidateRequired()

    {        if (healthBarImage != null) healthBarImage.fillAmount = (float)hp / maxHP;        rb.gravityScale = (rb.linearVelocity.y < 0f) ? fallGravityScale : gravityScale;

        bool ok = true;

        void Fail(string m) { Debug.LogError($"[BasicFighter2D:{gameObject.name}] {m}", this); ok = false; }    }



        if (string.IsNullOrWhiteSpace(fighterName)) Fail("fighterName is REQUIRED.");        Vector2 v = rb.linearVelocity;



        if (keyLeft == KeyCode.None) Fail("keyLeft is REQUIRED.");    void FixedUpdate()

        if (keyRight == KeyCode.None) Fail("keyRight is REQUIRED.");

        if (keyJump == KeyCode.None) Fail("keyJump is REQUIRED.");    {        switch (state)

        if (keyDash == KeyCode.None) Fail("keyDash is REQUIRED.");

        if (keyLight == KeyCode.None) Fail("keyLight is REQUIRED.");        if (!enabled) return;        {

        if (keyMedium == KeyCode.None) Fail("keyMedium is REQUIRED.");

        if (keyHeavy == KeyCode.None) Fail("keyHeavy is REQUIRED.");            case State.Idle:



        if (!rb) Fail("Rigidbody2D is REQUIRED on GameObject.");        // Gravity scale swap for better jump arc                v.x = Mathf.MoveTowards(v.x, 0f, moveSpeed * Time.fixedDeltaTime);

        if (!animator) Fail("Animator is REQUIRED on GameObject.");

        if (!body) Fail("Collider2D is REQUIRED on GameObject.");        rb.gravityScale = (rb.linearVelocity.y < 0f) ? fallGravityScale : gravityScale;                break;



        if (moveSpeed <= 0f) Fail("moveSpeed must be > 0.");

        if (jumpHeight <= 0f) Fail("jumpHeight must be > 0.");

        if (gravityScale <= 0f) Fail("gravityScale must be > 0.");        Vector2 v = rb.linearVelocity;            case State.Walk:

        if (fallGravityScale <= 0f) Fail("fallGravityScale must be > 0.");

        if (dashSpeed <= 0f) Fail("dashSpeed must be > 0.");                {

        if (dashDuration <= 0f) Fail("dashDuration must be > 0.");

        if (airControl <= 0f) Fail("airControl must be > 0.");        switch (state)                    float input = (Input.GetKey(keyLeft) ? -1f : 0f) + (Input.GetKey(keyRight) ? 1f : 0f);

        if (groundMask == 0) Fail("groundMask must be set.");

        if (groundCheckRadius <= 0f) Fail("groundCheckRadius must be > 0.");        {                    float target = input * moveSpeed;



        if (maxHP <= 0) Fail("maxHP must be > 0.");            case State.Idle:                    float a = grounded ? moveSpeed * 4f : moveSpeed * (2f * Mathf.Max(airControl, 0f));



        if (healthBarImage == null) Fail("healthBarImage is REQUIRED.");                v.x = Mathf.MoveTowards(v.x, 0f, moveSpeed * Time.fixedDeltaTime);                    v.x = Mathf.MoveTowards(v.x, target, a * Time.fixedDeltaTime);



        if (animIdle == null) Fail("animIdle is REQUIRED.");                break;                }

        if (animWalk == null) Fail("animWalk is REQUIRED.");

        if (animJump == null) Fail("animJump is REQUIRED.");                break;

        if (animFall == null) Fail("animFall is REQUIRED.");

        if (animDash == null) Fail("animDash is REQUIRED.");            case State.Walk:

        if (animHitstun == null) Fail("animHitstun is REQUIRED.");

        if (animKO == null) Fail("animKO is REQUIRED.");                {            case State.Jump:



        if (lightAttack == null) Fail("lightAttack is REQUIRED.");                    float input = (Input.GetKey(keyLeft) ? -1f : 0f) + (Input.GetKey(keyRight) ? 1f : 0f);            case State.Fall:

        else if (!ValidateAttack(lightAttack, "lightAttack")) ok = false;

        if (mediumAttack == null) Fail("mediumAttack is REQUIRED.");                    float target = input * moveSpeed;                {

        else if (!ValidateAttack(mediumAttack, "mediumAttack")) ok = false;

        if (heavyAttack == null) Fail("heavyAttack is REQUIRED.");                    float a = grounded ? moveSpeed * 4f : moveSpeed * (2f * Mathf.Max(airControl, 0f));                    float input = (Input.GetKey(keyLeft) ? -1f : 0f) + (Input.GetKey(keyRight) ? 1f : 0f);

        else if (!ValidateAttack(heavyAttack, "heavyAttack")) ok = false;

                    v.x = Mathf.MoveTowards(v.x, target, a * Time.fixedDeltaTime);                    float target = input * moveSpeed * Mathf.Clamp01(airControl);

        return ok;

    }                }                    float a = moveSpeed * 2f * Mathf.Max(airControl, 0f);



    bool ValidateAttack(Attack a, string label)                break;                    v.x = Mathf.MoveTowards(v.x, target, a * Time.fixedDeltaTime);

    {

        bool ok = true;                }

        void Fail(string m) { Debug.LogError($"[BasicFighter2D:{gameObject.name}] Attack '{label}': {m}", this); ok = false; }

        if (string.IsNullOrWhiteSpace(a.name)) Fail("name is REQUIRED.");            case State.Jump:                break;

        if (a.anim == null) Fail("AnimationClip is REQUIRED.");

        if (a.startup <= 0f) Fail("startup must be > 0.");            case State.Fall:

        if (a.active <= 0f) Fail("active must be > 0.");

        if (a.recovery < 0f) Fail("recovery must be >= 0.");                {            case State.Dash:

        if (a.damage <= 0) Fail("damage must be > 0.");

        if (a.hitstun <= 0f) Fail("hitstun must be > 0.");                    float input = (Input.GetKey(keyLeft) ? -1f : 0f) + (Input.GetKey(keyRight) ? 1f : 0f);                v.x = dashSpeed * (faceRight ? 1f : -1f);

        if (a.hitboxSize.x <= 0f || a.hitboxSize.y <= 0f) Fail("hitboxSize must be > 0.");

        return ok;                    float target = input * moveSpeed * Mathf.Clamp01(airControl);                break;

    }

                    float a = moveSpeed * 2f * Mathf.Max(airControl, 0f);

    // ─────────────────────────────────────────────────────────────────────────────

    // Gizmos                    v.x = Mathf.MoveTowards(v.x, target, a * Time.fixedDeltaTime);            case State.Attack:

    // ─────────────────────────────────────────────────────────────────────────────

    void OnDrawGizmosSelected()                }                // allow light drift during attack

    {

        // Ground check                break;                float drift = 0.35f * moveSpeed;

        Gizmos.color = Color.cyan;

        Vector2 origin = (Vector2)transform.position + groundCheckOffset;                float inputX = (Input.GetKey(keyLeft) ? -1f : 0f) + (Input.GetKey(keyRight) ? 1f : 0f);

        Gizmos.DrawWireSphere(origin, groundCheckRadius);

            case State.Dash:                v.x = Mathf.MoveTowards(v.x, inputX * drift, drift * 6f * Time.fixedDeltaTime);

        // Active hitbox preview if attacking

        if (state == State.Attack && currentAttack != null)                v.x = dashSpeed * (faceRight ? 1f : -1f);                break;

        {

            Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.35f);                break;

            Vector2 center = (Vector2)transform.position + RotateFacing(currentAttack.hitboxOffset);

            Gizmos.DrawWireCube(center, currentAttack.hitboxSize);            case State.Hitstun:

        }

    }            case State.Attack:                // keep physics

}
                // allow light drift during attack                break;

                float drift = 0.35f * moveSpeed;

                float inputX = (Input.GetKey(keyLeft) ? -1f : 0f) + (Input.GetKey(keyRight) ? 1f : 0f);            case State.KO:

                v.x = Mathf.MoveTowards(v.x, inputX * drift, drift * 6f * Time.fixedDeltaTime);                v = Vector2.zero;

                break;                break;

        }

            case State.Hitstun:

                // keep physics        rb.linearVelocity = v;

                break;    }



            case State.KO:    // ─────────────────────────────────────────────────────────────────────────────

                v = Vector2.zero;    // Movement

                break;    // ─────────────────────────────────────────────────────────────────────────────

        }    void DoJump()

    {

        rb.linearVelocity = v;        float g = Mathf.Abs(Physics2D.gravity.y) * gravityScale;

    }        if (g <= 0f) { Debug.LogError($"[{name}] gravity invalid; set gravityScale > 0"); enabled = false; return; }

        float v0 = Mathf.Sqrt(2f * g * Mathf.Max(jumpHeight, 0.0001f));

    // ─────────────────────────────────────────────────────────────────────────────        var v = rb.linearVelocity;

    // Movement        v.y = v0;

    // ─────────────────────────────────────────────────────────────────────────────        rb.linearVelocity = v;

    void DoJump()        state = State.Jump;

    {    }

        float g = Mathf.Abs(Physics2D.gravity.y) * gravityScale;

        if (g <= 0f) { Debug.LogError($"[{name}] gravity invalid; set gravityScale > 0"); enabled = false; return; }    void StartDash()

        float v0 = Mathf.Sqrt(2f * g * Mathf.Max(jumpHeight, 0.0001f));    {

        var v = rb.linearVelocity;        state = State.Dash;

        v.y = v0;        stateTimer = dashDuration;

        rb.linearVelocity = v;    }

        state = State.Jump;

    }    bool ProbeGround()

    {

    void StartDash()        Vector2 origin = (Vector2)transform.position + groundCheckOffset;

    {        var hit = Physics2D.OverlapCircle(origin, groundCheckRadius, groundMask);

        state = State.Dash;        return hit != null;

        stateTimer = dashDuration;    }

    }

    // ─────────────────────────────────────────────────────────────────────────────

    bool ProbeGround()    // Attacks

    {    // ─────────────────────────────────────────────────────────────────────────────

        Vector2 origin = (Vector2)transform.position + groundCheckOffset;    bool TryAttack(bool pressL, bool pressM, bool pressH)

        var hit = Physics2D.OverlapCircle(origin, groundCheckRadius, groundMask);    {

        return hit != null;        Attack a = null;

    }        if (pressH) a = heavyAttack ?? a;

        if (pressM) a = mediumAttack ?? a;

    // ─────────────────────────────────────────────────────────────────────────────        if (pressL) a = lightAttack ?? a;

    // Attacks        if (a == null) return false;

    // ─────────────────────────────────────────────────────────────────────────────

    bool TryAttack(bool pressL, bool pressM, bool pressH)        // Validate attack at use-time too

    {        if (!ValidateAttack(a, "Use")) { enabled = false; return false; }

        Attack a = null;

        if (pressH) a = heavyAttack ?? a;        currentAttack = a;

        if (pressM) a = mediumAttack ?? a;        attackTimer = 0f;

        if (pressL) a = lightAttack ?? a;        victimsThisSwing.Clear();

        if (a == null) return false;        state = State.Attack;



        // Validate attack at use-time too        // Play animation clip directly (required)

        if (!ValidateAttack(a, "Use")) { enabled = false; return false; }        animator.Play(a.anim.name, 0, 0f);

        return true;

        currentAttack = a;    }

        attackTimer = 0f;

        victimsThisSwing.Clear();    void TickAttack()

        state = State.Attack;    {

        if (currentAttack == null) { state = grounded ? State.Idle : State.Fall; return; }

        // Play animation clip directly (required)

        animator.Play(a.anim.name, 0, 0f);        attackTimer += Time.deltaTime;

        return true;

    }        // Active window

        float start = currentAttack.startup;

    void TickAttack()        float end = start + currentAttack.active;

    {        if (attackTimer >= start && attackTimer <= end)

        if (currentAttack == null) { state = grounded ? State.Idle : State.Fall; return; }        {

            DoHitDetect(currentAttack);

        attackTimer += Time.deltaTime;        }



        // Active window        // End on recovery complete

        float start = currentAttack.startup;        float total = currentAttack.startup + currentAttack.active + currentAttack.recovery;

        float end = start + currentAttack.active;        if (attackTimer >= total)

        if (attackTimer >= start && attackTimer <= end)        {

        {            currentAttack = null;

            DoHitDetect(currentAttack);            state = grounded ? State.Idle : State.Fall;

        }        }

    }

        // End on recovery complete

        float total = currentAttack.startup + currentAttack.active + currentAttack.recovery;    void DoHitDetect(Attack a)

        if (attackTimer >= total)    {

        {        Vector2 center = (Vector2)transform.position + RotateFacing(a.hitboxOffset);

            currentAttack = null;        Vector2 size = a.hitboxSize;

            state = grounded ? State.Idle : State.Fall;        var cols = Physics2D.OverlapBoxAll(center, size, 0f);

        }        for (int i = 0; i < cols.Length; i++)

    }        {

            var f = cols[i].GetComponentInParent<BasicFighter2D>();

    void DoHitDetect(Attack a)            if (!f || f == this) continue;

    {            if (victimsThisSwing.Contains(f)) continue;

        Vector2 center = (Vector2)transform.position + RotateFacing(a.hitboxOffset);

        Vector2 size = a.hitboxSize;            victimsThisSwing.Add(f);

        var cols = Physics2D.OverlapBoxAll(center, size, 0f);            f.OnHit(this, a);

        for (int i = 0; i < cols.Length; i++)        }

        {    }

            var f = cols[i].GetComponentInParent<BasicFighter2D>();

            if (!f || f == this) continue;    void OnHit(BasicFighter2D attacker, Attack a)

            if (victimsThisSwing.Contains(f)) continue;    {

        if (state == State.KO) return;

            victimsThisSwing.Add(f);

            f.OnHit(this, a);        // Damage

        }        hp = Mathf.Max(0, hp - Mathf.Max(0, a.damage));

    }        if (hp == 0)

        {

    void OnHit(BasicFighter2D attacker, Attack a)            state = State.KO;

    {            rb.linearVelocity = Vector2.zero;

        if (state == State.KO) return;            return;

        }

        // Damage

        hp = Mathf.Max(0, hp - Mathf.Max(0, a.damage));        // Hitstun and knockback

        if (hp == 0)        hitstunTimer = Mathf.Max(hitstunTimer, a.hitstun);

        {        state = State.Hitstun;

            state = State.KO;

            rb.linearVelocity = Vector2.zero;        Vector2 kb = new Vector2(a.knockback.x * (attacker.faceRight ? 1f : -1f), a.knockback.y);

            return;        rb.linearVelocity = new Vector2(kb.x, Mathf.Max(rb.linearVelocity.y, kb.y)); // simple overwrite with min Y keep

        }    }



        // Hitstun and knockback    // ─────────────────────────────────────────────────────────────────────────────

        hitstunTimer = Mathf.Max(hitstunTimer, a.hitstun);    // Animation

        state = State.Hitstun;    // ─────────────────────────────────────────────────────────────────────────────

    void UpdateStateAnimation()

        Vector2 kb = new Vector2(a.knockback.x * (attacker.faceRight ? 1f : -1f), a.knockback.y);    {

        rb.linearVelocity = new Vector2(kb.x, Mathf.Max(rb.linearVelocity.y, kb.y)); // simple overwrite with min Y keep        // Only update animation if state changed

    }        if (state == prevState) return;

        prevState = state;

    // ─────────────────────────────────────────────────────────────────────────────

    // Animation        // Reset all state bools

    // ─────────────────────────────────────────────────────────────────────────────        SetAnimBool(paramIdle, false);

    void UpdateStateAnimation()        SetAnimBool(paramWalk, false);

    {        SetAnimBool(paramJump, false);

        // Only update animation if state changed        SetAnimBool(paramFall, false);

        if (state == prevState) return;        SetAnimBool(paramDash, false);

        prevState = state;        SetAnimBool(paramHitstun, false);

        SetAnimBool(paramKO, false);

        // Reset all state bools

        SetAnimBool(paramIdle, false);        // Set the current state bool to true

        SetAnimBool(paramWalk, false);        switch (state)

        SetAnimBool(paramJump, false);        {

        SetAnimBool(paramFall, false);            case State.Idle: SetAnimBool(paramIdle, true); break;

        SetAnimBool(paramDash, false);            case State.Walk: SetAnimBool(paramWalk, true); break;

        SetAnimBool(paramHitstun, false);            case State.Jump: SetAnimBool(paramJump, true); break;

        SetAnimBool(paramKO, false);            case State.Fall: SetAnimBool(paramFall, true); break;

            case State.Dash: SetAnimBool(paramDash, true); break;

        // Set the current state bool to true            case State.Hitstun: SetAnimBool(paramHitstun, true); break;

        switch (state)            case State.KO: SetAnimBool(paramKO, true); break;

        {            case State.Attack: 

            case State.Idle: SetAnimBool(paramIdle, true); break;                // Attacks still play directly since they're temporary

            case State.Walk: SetAnimBool(paramWalk, true); break;                break;

            case State.Jump: SetAnimBool(paramJump, true); break;        }

            case State.Fall: SetAnimBool(paramFall, true); break;    }

            case State.Dash: SetAnimBool(paramDash, true); break;

            case State.Hitstun: SetAnimBool(paramHitstun, true); break;    void SetAnimBool(string paramName, bool value)

            case State.KO: SetAnimBool(paramKO, true); break;    {

            case State.Attack:         if (string.IsNullOrEmpty(paramName) || animator == null) return;

                // Attacks still play directly since they're temporary        animator.SetBool(paramName, value);

                break;    }

        }

    }    // ─────────────────────────────────────────────────────────────────────────────

    // Helpers

    void SetAnimBool(string paramName, bool value)    // ─────────────────────────────────────────────────────────────────────────────

    {    float FacingDir() => faceRight ? 1f : -1f;

        if (string.IsNullOrEmpty(paramName) || animator == null) return;

        animator.SetBool(paramName, value);    Vector2 RotateFacing(Vector2 v) => new Vector2(v.x * FacingDir(), v.y);

    }

    // ─────────────────────────────────────────────────────────────────────────────

    // ─────────────────────────────────────────────────────────────────────────────    // Validation

    // Helpers    // ─────────────────────────────────────────────────────────────────────────────

    // ─────────────────────────────────────────────────────────────────────────────    bool ValidateRequired()

    float FacingDir() => faceRight ? 1f : -1f;    {

        bool ok = true;

    Vector2 RotateFacing(Vector2 v) => new Vector2(v.x * FacingDir(), v.y);        void Fail(string m) { Debug.LogError($"[BasicFighter2D:{gameObject.name}] {m}", this); ok = false; }



    // ─────────────────────────────────────────────────────────────────────────────        if (string.IsNullOrWhiteSpace(fighterName)) Fail("fighterName is REQUIRED.");

    // Validation

    // ─────────────────────────────────────────────────────────────────────────────        if (keyLeft == KeyCode.None) Fail("keyLeft is REQUIRED.");

    bool ValidateRequired()        if (keyRight == KeyCode.None) Fail("keyRight is REQUIRED.");

    {        if (keyJump == KeyCode.None) Fail("keyJump is REQUIRED.");

        bool ok = true;        if (keyDash == KeyCode.None) Fail("keyDash is REQUIRED.");

        void Fail(string m) { Debug.LogError($"[BasicFighter2D:{gameObject.name}] {m}", this); ok = false; }        if (keyLight == KeyCode.None) Fail("keyLight is REQUIRED.");

        if (keyMedium == KeyCode.None) Fail("keyMedium is REQUIRED.");

        if (string.IsNullOrWhiteSpace(fighterName)) Fail("fighterName is REQUIRED.");        if (keyHeavy == KeyCode.None) Fail("keyHeavy is REQUIRED.");



        if (keyLeft == KeyCode.None) Fail("keyLeft is REQUIRED.");        if (!rb) Fail("Rigidbody2D is REQUIRED on GameObject.");

        if (keyRight == KeyCode.None) Fail("keyRight is REQUIRED.");        if (!animator) Fail("Animator is REQUIRED on GameObject.");

        if (keyJump == KeyCode.None) Fail("keyJump is REQUIRED.");        if (!body) Fail("Collider2D is REQUIRED on GameObject.");

        if (keyDash == KeyCode.None) Fail("keyDash is REQUIRED.");

        if (keyLight == KeyCode.None) Fail("keyLight is REQUIRED.");        if (moveSpeed <= 0f) Fail("moveSpeed must be > 0.");

        if (keyMedium == KeyCode.None) Fail("keyMedium is REQUIRED.");        if (jumpHeight <= 0f) Fail("jumpHeight must be > 0.");

        if (keyHeavy == KeyCode.None) Fail("keyHeavy is REQUIRED.");        if (gravityScale <= 0f) Fail("gravityScale must be > 0.");

        if (fallGravityScale <= 0f) Fail("fallGravityScale must be > 0.");

        if (!rb) Fail("Rigidbody2D is REQUIRED on GameObject.");        if (dashSpeed <= 0f) Fail("dashSpeed must be > 0.");

        if (!animator) Fail("Animator is REQUIRED on GameObject.");        if (dashDuration <= 0f) Fail("dashDuration must be > 0.");

        if (!body) Fail("Collider2D is REQUIRED on GameObject.");        if (airControl <= 0f) Fail("airControl must be > 0.");

        if (groundMask == 0) Fail("groundMask must be set.");

        if (moveSpeed <= 0f) Fail("moveSpeed must be > 0.");        if (groundCheckRadius <= 0f) Fail("groundCheckRadius must be > 0.");

        if (jumpHeight <= 0f) Fail("jumpHeight must be > 0.");

        if (gravityScale <= 0f) Fail("gravityScale must be > 0.");        if (maxHP <= 0) Fail("maxHP must be > 0.");

        if (fallGravityScale <= 0f) Fail("fallGravityScale must be > 0.");

        if (dashSpeed <= 0f) Fail("dashSpeed must be > 0.");        if (animIdle == null) Fail("animIdle is REQUIRED.");

        if (dashDuration <= 0f) Fail("dashDuration must be > 0.");        if (animWalk == null) Fail("animWalk is REQUIRED.");

        if (airControl <= 0f) Fail("airControl must be > 0.");        if (animJump == null) Fail("animJump is REQUIRED.");

        if (groundMask == 0) Fail("groundMask must be set.");        if (animFall == null) Fail("animFall is REQUIRED.");

        if (groundCheckRadius <= 0f) Fail("groundCheckRadius must be > 0.");        if (animDash == null) Fail("animDash is REQUIRED.");

        if (animHitstun == null) Fail("animHitstun is REQUIRED.");

        if (maxHP <= 0) Fail("maxHP must be > 0.");        if (animKO == null) Fail("animKO is REQUIRED.");



        if (healthBarImage == null) Fail("healthBarImage is REQUIRED.");        if (lightAttack == null) Fail("lightAttack is REQUIRED.");

        else if (!ValidateAttack(lightAttack, "lightAttack")) ok = false;

        if (animIdle == null) Fail("animIdle is REQUIRED.");        if (mediumAttack == null) Fail("mediumAttack is REQUIRED.");

        if (animWalk == null) Fail("animWalk is REQUIRED.");        else if (!ValidateAttack(mediumAttack, "mediumAttack")) ok = false;

        if (animJump == null) Fail("animJump is REQUIRED.");        if (heavyAttack == null) Fail("heavyAttack is REQUIRED.");

        if (animFall == null) Fail("animFall is REQUIRED.");        else if (!ValidateAttack(heavyAttack, "heavyAttack")) ok = false;

        if (animDash == null) Fail("animDash is REQUIRED.");

        if (animHitstun == null) Fail("animHitstun is REQUIRED.");        return ok;

        if (animKO == null) Fail("animKO is REQUIRED.");    }



        if (lightAttack == null) Fail("lightAttack is REQUIRED.");    bool ValidateAttack(Attack a, string label)

        else if (!ValidateAttack(lightAttack, "lightAttack")) ok = false;    {

        if (mediumAttack == null) Fail("mediumAttack is REQUIRED.");        bool ok = true;

        else if (!ValidateAttack(mediumAttack, "mediumAttack")) ok = false;        void Fail(string m) { Debug.LogError($"[BasicFighter2D:{gameObject.name}] Attack '{label}': {m}", this); ok = false; }

        if (heavyAttack == null) Fail("heavyAttack is REQUIRED.");        if (string.IsNullOrWhiteSpace(a.name)) Fail("name is REQUIRED.");

        else if (!ValidateAttack(heavyAttack, "heavyAttack")) ok = false;        if (a.anim == null) Fail("AnimationClip is REQUIRED.");

        if (a.startup <= 0f) Fail("startup must be > 0.");

        return ok;        if (a.active <= 0f) Fail("active must be > 0.");

    }        if (a.recovery < 0f) Fail("recovery must be >= 0.");

        if (a.damage <= 0) Fail("damage must be > 0.");

    bool ValidateAttack(Attack a, string label)        if (a.hitstun <= 0f) Fail("hitstun must be > 0.");

    {        if (a.hitboxSize.x <= 0f || a.hitboxSize.y <= 0f) Fail("hitboxSize must be > 0.");

        bool ok = true;        return ok;

        void Fail(string m) { Debug.LogError($"[BasicFighter2D:{gameObject.name}] Attack '{label}': {m}", this); ok = false; }    }

        if (string.IsNullOrWhiteSpace(a.name)) Fail("name is REQUIRED.");

        if (a.anim == null) Fail("AnimationClip is REQUIRED.");    // ─────────────────────────────────────────────────────────────────────────────

        if (a.startup <= 0f) Fail("startup must be > 0.");    // Gizmos

        if (a.active <= 0f) Fail("active must be > 0.");    // ─────────────────────────────────────────────────────────────────────────────

        if (a.recovery < 0f) Fail("recovery must be >= 0.");    void OnDrawGizmosSelected()

        if (a.damage <= 0) Fail("damage must be > 0.");    {

        if (a.hitstun <= 0f) Fail("hitstun must be > 0.");        // Ground check

        if (a.hitboxSize.x <= 0f || a.hitboxSize.y <= 0f) Fail("hitboxSize must be > 0.");        Gizmos.color = Color.cyan;

        return ok;        Vector2 origin = (Vector2)transform.position + groundCheckOffset;

    }        Gizmos.DrawWireSphere(origin, groundCheckRadius);



    // ─────────────────────────────────────────────────────────────────────────────        // Active hitbox preview if attacking

    // Gizmos        if (state == State.Attack && currentAttack != null)

    // ─────────────────────────────────────────────────────────────────────────────        {

    void OnDrawGizmosSelected()            Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.35f);

    {            Vector2 center = (Vector2)transform.position + RotateFacing(currentAttack.hitboxOffset);

        // Ground check            Gizmos.DrawWireCube(center, currentAttack.hitboxSize);

        Gizmos.color = Color.cyan;        }

        Vector2 origin = (Vector2)transform.position + groundCheckOffset;    }

        Gizmos.DrawWireSphere(origin, groundCheckRadius);}


        // Active hitbox preview if attacking
        if (state == State.Attack && currentAttack != null)
        {
            Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.35f);
            Vector2 center = (Vector2)transform.position + RotateFacing(currentAttack.hitboxOffset);
            Gizmos.DrawWireCube(center, currentAttack.hitboxSize);
        }
    }
}