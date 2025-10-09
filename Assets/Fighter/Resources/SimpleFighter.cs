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
    public Attack lightAttack;
    public Attack mediumAttack;
    public Attack heavyAttack;

    [Header("Vitals - REQUIRED")]
    public int maxHP;                  // > 0

    [Header("Health Bar - REQUIRED")]
    [Tooltip("REQUIRED: Sprite for healthbar (white square recommended)")]
    public Sprite healthBarSprite;
    [Tooltip("REQUIRED: X offset from player")]
    public float healthBarOffsetX;
    [Tooltip("REQUIRED: Y offset from player")]
    public float healthBarOffsetY;
    [Tooltip("REQUIRED: Width scale (> 0)")]
    public float healthBarScaleX;
    [Tooltip("REQUIRED: Height scale (> 0)")]
    public float healthBarScaleY;
    [Tooltip("REQUIRED: Color when at full HP")]
    public Color healthBarColorFull;
    [Tooltip("REQUIRED: Color when at zero HP")]
    public Color healthBarColorEmpty;
    
    private int healthBarSortingOrder = 100;

    [Header("State Animations - REQUIRED")]
    [Tooltip("REQUIRED: Idle animation clip")]
    public AnimationClip animIdle;
    [Tooltip("REQUIRED: Walk animation clip")]
    public AnimationClip animWalk;
    [Tooltip("REQUIRED: Jump animation clip")]
    public AnimationClip animJump;
    [Tooltip("REQUIRED: Fall animation clip")]
    public AnimationClip animFall;
    [Tooltip("REQUIRED: Dash animation clip")]
    public AnimationClip animDash;
    [Tooltip("REQUIRED: Hitstun animation clip")]
    public AnimationClip animHitstun;
    [Tooltip("REQUIRED: KO/death animation clip")]
    public AnimationClip animKO;

    // ─────────────────────────────────────────────────────────────────────────────
    // PRIVATE COMPONENTS (auto-fetched)
    // ─────────────────────────────────────────────────────────────────────────────
    Rigidbody2D rb;
    Animation animationComponent; // Legacy Animation component for direct clip playback
    Collider2D body;
    SpriteRenderer spriteRenderer;
    Transform healthBarRoot;
    SpriteRenderer healthBarBackground;
    SpriteRenderer healthBarFill;
    Texture2D healthBarTexture; // Track texture for cleanup

    // ─────────────────────────────────────────────────────────────────────────────
    // INTERNAL STATE
    // ─────────────────────────────────────────────────────────────────────────────
    enum State { Idle, Walk, Jump, Fall, Dash, Attack, Hitstun, KO }
    State state;
    State prevState; // Track state changes for animation updates
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
        [Tooltip("REQUIRED: Attack name for debugging")]
        public string name;

        [Header("Animation - REQUIRED")]
        [Tooltip("REQUIRED: AnimationClip to play when attack starts")]
        public AnimationClip anim;

        [Header("Timing (seconds)")]
        [Tooltip("REQUIRED: Startup frames before hitbox becomes active (> 0)")]
        public float startup;
        [Tooltip("REQUIRED: Duration hitbox remains active (> 0)")]
        public float active;
        [Tooltip("Recovery time after active frames end (>= 0)")]
        public float recovery;

        [Header("Damage & Stun")]
        [Tooltip("REQUIRED: Damage dealt on hit (> 0)")]
        public int damage;
        [Tooltip("REQUIRED: Hitstun duration in seconds (> 0)")]
        public float hitstun;

        [Header("Hitbox")]
        [Tooltip("Hitbox center offset from fighter position (flipped with facing)")]
        public Vector2 hitboxOffset;
        [Tooltip("REQUIRED: Hitbox width and height (> 0)")]
        public Vector2 hitboxSize;

        [Header("Knockback")]
        [Tooltip("Knockback force applied on hit (X flipped with facing)")]
        public Vector2 knockback;
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // LIFECYCLE
    // ─────────────────────────────────────────────────────────────────────────────
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        body = GetComponent<Collider2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        // Get or add Animation component for direct playback
        animationComponent = GetComponent<Animation>();
        if (animationComponent == null)
        {
            animationComponent = gameObject.AddComponent<Animation>();
        }
        
        // Setup animation clips
        SetupAnimations();

        if (!ValidateRequired()) { enabled = false; return; }

        rb.gravityScale = gravityScale;
        hp = maxHP;

        SetupHealthBar();

        if (!registry.Contains(this)) registry.Add(this);
    }

    void OnDestroy()
    {
        registry.Remove(this);
        // Cleanup texture to prevent memory leak
        if (healthBarTexture != null) Destroy(healthBarTexture);
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

        // Update sprite facing
        UpdateSpriteFacing();

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

        UpdateStateAnimation();

        // Update healthbar
        UpdateHealthBar();
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
        if (pressH) a = heavyAttack ?? a;
        if (pressM) a = mediumAttack ?? a;
        if (pressL) a = lightAttack ?? a;
        if (a == null) return false;

        // Validate attack at use-time too
        if (!ValidateAttack(a, "Use")) { enabled = false; return false; }

        currentAttack = a;
        attackTimer = 0f;
        victimsThisSwing.Clear();
        state = State.Attack;

        // Play animation clip directly using our registered name
        if (animationComponent != null && a.anim != null)
        {
            string attackName = null;
            if (a == lightAttack) attackName = "LightAttack";
            else if (a == mediumAttack) attackName = "MediumAttack";
            else if (a == heavyAttack) attackName = "HeavyAttack";

            if (attackName != null)
            {
                AnimationState animState = animationComponent[attackName];
                if (animState != null)
                {
                    animState.wrapMode = WrapMode.Once;
                    animationComponent.CrossFade(attackName, 0.05f);
                }
            }
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
            return;
        }

        // Hitstun and knockback
        hitstunTimer = Mathf.Max(hitstunTimer, a.hitstun);
        state = State.Hitstun;

        Vector2 kb = new Vector2(a.knockback.x * (attacker.faceRight ? 1f : -1f), a.knockback.y);
        rb.velocity = new Vector2(kb.x, Mathf.Max(rb.velocity.y, kb.y)); // simple overwrite with min Y keep
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // Animation
    // ─────────────────────────────────────────────────────────────────────────────
    void SetupAnimations()
    {
        // Helper to safely add clip with Legacy check
        bool AddClipSafe(AnimationClip clip, string name)
        {
            if (clip == null) return false;
            
            if (!clip.legacy)
            {
                Debug.LogError($"[{gameObject.name}] AnimationClip '{clip.name}' must be marked as LEGACY!\n" +
                              $"Fix: Select the clip → Inspector → Debug mode → Check 'Legacy' checkbox\n" +
                              $"Or: Select clip → Import Settings → Animation Type: Legacy", this);
                return false;
            }
            
            try
            {
                animationComponent.AddClip(clip, name);
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[{gameObject.name}] Failed to add clip '{name}': {e.Message}", this);
                return false;
            }
        }

        // Register all animation clips with the Animation component
        bool allOk = true;
        
        if (animIdle != null) 
        {
            if (AddClipSafe(animIdle, "Idle"))
            {
                animationComponent.clip = animIdle; // Set as default
            }
            else allOk = false;
        }
        
        if (animWalk != null && !AddClipSafe(animWalk, "Walk")) allOk = false;
        if (animJump != null && !AddClipSafe(animJump, "Jump")) allOk = false;
        if (animFall != null && !AddClipSafe(animFall, "Fall")) allOk = false;
        if (animDash != null && !AddClipSafe(animDash, "Dash")) allOk = false;
        if (animHitstun != null && !AddClipSafe(animHitstun, "Hitstun")) allOk = false;
        if (animKO != null && !AddClipSafe(animKO, "KO")) allOk = false;

        // Add attack clips with simple names
        if (lightAttack?.anim != null && !AddClipSafe(lightAttack.anim, "LightAttack")) allOk = false;
        if (mediumAttack?.anim != null && !AddClipSafe(mediumAttack.anim, "MediumAttack")) allOk = false;
        if (heavyAttack?.anim != null && !AddClipSafe(heavyAttack.anim, "HeavyAttack")) allOk = false;

        if (!allOk)
        {
            Debug.LogError($"[{gameObject.name}] Animation setup FAILED! See errors above. Fighter disabled.", this);
            enabled = false;
            return;
        }

        // Play idle by default
        if (animIdle != null)
        {
            animationComponent.wrapMode = WrapMode.Loop;
            animationComponent.Play("Idle");
        }
    }

    void UpdateStateAnimation()
    {
        // Only update animation if state changed
        if (state == prevState) return;
        prevState = state;

        // Play the appropriate animation clip directly
        string clipName = null;
        WrapMode wrapMode = WrapMode.Loop;

        switch (state)
        {
            case State.Idle: 
                clipName = "Idle";
                wrapMode = WrapMode.Loop;
                break;
            case State.Walk: 
                clipName = "Walk";
                wrapMode = WrapMode.Loop;
                break;
            case State.Jump: 
                clipName = "Jump";
                wrapMode = WrapMode.Once;
                break;
            case State.Fall: 
                clipName = "Fall";
                wrapMode = WrapMode.Loop;
                break;
            case State.Dash: 
                clipName = "Dash";
                wrapMode = WrapMode.Once;
                break;
            case State.Hitstun: 
                clipName = "Hitstun";
                wrapMode = WrapMode.Once;
                break;
            case State.KO: 
                clipName = "KO";
                wrapMode = WrapMode.Once;
                break;
            case State.Attack:
                // Attacks are handled when they start
                return;
        }

        if (clipName != null && animationComponent != null)
        {
            // Set wrap mode for the specific clip
            AnimationState animState = animationComponent[clipName];
            if (animState != null)
            {
                animState.wrapMode = wrapMode;
                animationComponent.CrossFade(clipName, 0.1f);
            }
        }
    }

    void UpdateSpriteFacing()
    {
        if (spriteRenderer != null)
        {
            // Flip sprite based on facing direction
            spriteRenderer.flipX = !faceRight;
        }
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // UI
    // ─────────────────────────────────────────────────────────────────────────────
    void SetupHealthBar()
    {
        // Create health bar root
        healthBarRoot = new GameObject("HealthBar").transform;
        healthBarRoot.SetParent(transform, false);
        healthBarRoot.localPosition = new Vector3(healthBarOffsetX, healthBarOffsetY, 0f);

        // Create background (black)
        var bgObj = new GameObject("Background");
        bgObj.transform.SetParent(healthBarRoot, false);
        healthBarBackground = bgObj.AddComponent<SpriteRenderer>();
        healthBarBackground.sprite = CreateSimpleSprite();
        healthBarBackground.color = Color.black;
        healthBarBackground.sortingOrder = healthBarSortingOrder;
        bgObj.transform.localScale = new Vector3(healthBarScaleX + 0.05f, healthBarScaleY + 0.05f, 1f);

        // Create fill (green to red) - using simple Transform.localScale for visibility
        var fillObj = new GameObject("Fill");
        fillObj.transform.SetParent(healthBarRoot, false);
        fillObj.transform.localPosition = new Vector3(0f, 0f, -0.01f);
        healthBarFill = fillObj.AddComponent<SpriteRenderer>();
        healthBarFill.sprite = healthBarSprite ?? CreateSimpleSprite();
        healthBarFill.color = healthBarColorFull;
        healthBarFill.sortingOrder = healthBarSortingOrder + 1;
        // Use simple scale instead of DrawMode.Sliced for better visibility
        fillObj.transform.localScale = new Vector3(healthBarScaleX, healthBarScaleY, 1f);
    }

    Sprite CreateSimpleSprite()
    {
        // Create a simple white 1x1 texture
        var tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
    }

    void UpdateHealthBar()
    {
        if (healthBarFill == null) return;

        // Update fill size based on HP using Transform.localScale
        float hpRatio = maxHP > 0 ? (float)hp / maxHP : 0f;
        healthBarFill.transform.localScale = new Vector3(healthBarScaleX * hpRatio, healthBarScaleY, 1f);

        // Lerp color from full to empty
        healthBarFill.color = Color.Lerp(healthBarColorEmpty, healthBarColorFull, hpRatio);
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // Helpers
    // ─────────────────────────────────────────────────────────────────────────────
    float FacingDir() => faceRight ? 1f : -1f;

    Vector2 RotateFacing(Vector2 v) => new Vector2(v.x * FacingDir(), v.y);

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

        if (!rb) Fail("Rigidbody2D is REQUIRED on GameObject.");
        if (!animationComponent) Fail("Animation component is REQUIRED (auto-added if missing).");
        if (!body) Fail("Collider2D is REQUIRED on GameObject.");
        if (!spriteRenderer) Fail("SpriteRenderer is REQUIRED (on GameObject or child).");

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

        if (healthBarSprite == null) Fail("healthBarSprite is REQUIRED.");
        if (healthBarScaleX <= 0f) Fail("healthBarScaleX must be > 0.");
        if (healthBarScaleY <= 0f) Fail("healthBarScaleY must be > 0.");

        if (animIdle == null) Fail("animIdle is REQUIRED.");
        if (animWalk == null) Fail("animWalk is REQUIRED.");
        if (animJump == null) Fail("animJump is REQUIRED.");
        if (animFall == null) Fail("animFall is REQUIRED.");
        if (animDash == null) Fail("animDash is REQUIRED.");
        if (animHitstun == null) Fail("animHitstun is REQUIRED.");
        if (animKO == null) Fail("animKO is REQUIRED.");

        if (lightAttack == null) Fail("lightAttack is REQUIRED.");
        else if (!ValidateAttack(lightAttack, "lightAttack")) ok = false;
        if (mediumAttack == null) Fail("mediumAttack is REQUIRED.");
        else if (!ValidateAttack(mediumAttack, "mediumAttack")) ok = false;
        if (heavyAttack == null) Fail("heavyAttack is REQUIRED.");
        else if (!ValidateAttack(heavyAttack, "heavyAttack")) ok = false;

        return ok;
    }

    bool ValidateAttack(Attack a, string label)
    {
        bool ok = true;
        void Fail(string m) { Debug.LogError($"[BasicFighter2D:{gameObject.name}] Attack '{label}': {m}", this); ok = false; }
        if (string.IsNullOrWhiteSpace(a.name)) Fail("name is REQUIRED.");
        if (a.anim == null) Fail("AnimationClip is REQUIRED.");
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
