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
    
    [Header("Ground Check - REQUIRED")]
    [Tooltip("Ground check position offset from player position")]
    public Vector2 groundCheckOffset;
    [Tooltip("Ground check radius for overlap detection")]
    public float groundCheckRadius;

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

    [Header("Debug")]
    [Tooltip("Show attack hitboxes and player hurtboxes in game")]
    private bool debugAttacks = true;

    // ─────────────────────────────────────────────────────────────────────────────
    // PRIVATE COMPONENTS (auto-fetched)
    // ─────────────────────────────────────────────────────────────────────────────
    Rigidbody2D rb;
    Animator animator;
    Collider2D body;
    SpriteRenderer sprite;
    Transform healthBarRoot;
    SpriteRenderer healthBarBg;
    SpriteRenderer healthBarFill;
    Texture2D healthBarTexture;

    // Animation sampling
    AnimationClip currentClip;
    float animationTime;

    // ─────────────────────────────────────────────────────────────────────────────
    // INTERNAL STATE
    // ─────────────────────────────────────────────────────────────────────────────
    enum State { Idle, Walk, Jump, Fall, Dash, Attack, Hitstun, KO }
    State state;
    State prevState; // Track state changes for animation updates
    bool grounded;
    float stateTimer;
    bool faceRight = true;

    private int hp; // Public for debugging

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
        // Cache components
        rb = GetComponent<Rigidbody2D>();
        body = GetComponent<Collider2D>();
        sprite = GetComponentInChildren<SpriteRenderer>();
        animator = GetComponent<Animator>();

        if (!ValidateRequired()) { enabled = false; return; }

        rb.gravityScale = gravityScale;
        hp = maxHP;
        SetupHealthBar();

        // Start in idle state
        state = State.Idle;
        prevState = State.KO; // Set to different state to force first animation update

        // Register for hit detection
        if (!registry.Contains(this)) registry.Add(this);
    }

    void OnDestroy()
    {
        registry.Remove(this);
        if (healthBarTexture != null) Destroy(healthBarTexture);
    }

    void Update()
    {
        if (!enabled) return;

        UpdateTimers();
        ProcessInput();
        UpdateStateAnimation();
        UpdateHealthBar();
        SampleCurrentAnimation();
        
        if (debugAttacks)
            DrawDebugBoxes();
    }

    void FixedUpdate()
    {
        if (!enabled) return;
        
        UpdateGravity();
        UpdateVelocity();
    }

    void DrawDebugBoxes()
    {
        // Draw player hurtbox (green)
        if (body != null && body is BoxCollider2D box)
        {
            Vector2 center = (Vector2)transform.position + box.offset;
            Vector2 size = box.size * transform.localScale;
            DrawBox(center, size, Color.green);
        }

        // Draw active attack hitbox (red)
        if (state == State.Attack && currentAttack != null)
        {
            if (attackTimer >= currentAttack.startup && 
                attackTimer < currentAttack.startup + currentAttack.active)
            {
                Vector2 center = (Vector2)transform.position + FlipVector(currentAttack.hitboxOffset);
                DrawBox(center, currentAttack.hitboxSize, Color.red);
            }
        }
    }

    void DrawBox(Vector2 center, Vector2 size, Color color)
    {
        Vector2 halfSize = size * 0.5f;
        Vector2 topLeft = new Vector2(center.x - halfSize.x, center.y + halfSize.y);
        Vector2 topRight = new Vector2(center.x + halfSize.x, center.y + halfSize.y);
        Vector2 botLeft = new Vector2(center.x - halfSize.x, center.y - halfSize.y);
        Vector2 botRight = new Vector2(center.x + halfSize.x, center.y - halfSize.y);

        Debug.DrawLine(topLeft, topRight, color);
        Debug.DrawLine(topRight, botRight, color);
        Debug.DrawLine(botRight, botLeft, color);
        Debug.DrawLine(botLeft, topLeft, color);
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // UPDATE HELPERS
    // ─────────────────────────────────────────────────────────────────────────────
    void UpdateTimers()
    {
        if (hitstunTimer > 0f) hitstunTimer -= Time.deltaTime;
        if (state == State.Hitstun && hitstunTimer <= 0f)
            state = grounded ? State.Idle : State.Fall;
    }

    void ProcessInput()
    {
        // Input
        float move = (Input.GetKey(keyLeft) ? -1f : 0f) + (Input.GetKey(keyRight) ? 1f : 0f);
        bool jump = Input.GetKeyDown(keyJump);
        bool dash = Input.GetKeyDown(keyDash);
        bool light = Input.GetKeyDown(keyLight);
        bool medium = Input.GetKeyDown(keyMedium);
        bool heavy = Input.GetKeyDown(keyHeavy);

        // Ground check and facing
        grounded = IsGrounded();
        if (Mathf.Abs(move) > 0.001f) faceRight = move > 0f;
        UpdateSpriteFacing();

        // State machine - allow more freedom of movement
        switch (state)
        {
            case State.Idle:
            case State.Walk:
                if (dash) StartDash();
                else if (jump && grounded) StartJump();
                else if (TryAttack(light, medium, heavy)) { }
                else state = (Mathf.Abs(move) > 0.05f) ? State.Walk : State.Idle;
                break;

            case State.Jump:
            case State.Fall:
                // Allow dashing and attacking in air
                if (dash) StartDash();
                else if (TryAttack(light, medium, heavy)) { }
                else if (grounded) state = (Mathf.Abs(move) > 0.05f) ? State.Walk : State.Idle;
                break;

            case State.Dash:
                // Allow jumping and attacking out of dash
                stateTimer -= Time.deltaTime;
                if (jump && grounded) StartJump();
                else if (TryAttack(light, medium, heavy)) { }
                else if (stateTimer <= 0f)
                {
                    // Dash ended - transition based on movement input
                    if (grounded)
                        state = (Mathf.Abs(move) > 0.05f) ? State.Walk : State.Idle;
                    else
                        state = State.Fall;
                }
                break;

            case State.Attack:
                // Allow jumping during attack (for cancel options)
                if (jump && grounded)
                {
                    StartJump();
                    // Don't interrupt attack state, just add jump velocity
                    state = State.Attack; // Stay in attack
                }
                TickAttack();
                break;

            case State.Hitstun:
                // Hitstun timer handled in UpdateTimers
                break;

            case State.KO:
                // Dead, no input allowed
                break;
        }
    }

    void UpdateGravity()
    {
        rb.gravityScale = (rb.linearVelocity.y < 0f) ? fallGravityScale : gravityScale;
    }

    void UpdateVelocity()
    {
        Vector2 v = rb.linearVelocity;

        switch (state)
        {
            case State.Idle:
                v.x = Mathf.MoveTowards(v.x, 0f, moveSpeed * Time.fixedDeltaTime);
                break;

            case State.Walk:
                v.x = MoveTowardsTarget(v.x, GetMoveInput() * moveSpeed, grounded ? moveSpeed * 4f : moveSpeed * 2f * Mathf.Max(airControl, 0f));
                break;

            case State.Jump:
            case State.Fall:
                v.x = MoveTowardsTarget(v.x, GetMoveInput() * moveSpeed * Mathf.Clamp01(airControl), moveSpeed * 2f * Mathf.Max(airControl, 0f));
                break;

            case State.Dash:
                // Allow movement control during dash with some influence
                float dashControl = 0.6f; // 60% control during dash
                v.x = MoveTowardsTarget(v.x, GetMoveInput() * moveSpeed * dashControl, moveSpeed * 3f);
                break;

            case State.Attack:
                float drift = 0.35f * moveSpeed;
                v.x = Mathf.MoveTowards(v.x, GetMoveInput() * drift, drift * 6f * Time.fixedDeltaTime);
                break;

            case State.KO:
                v = Vector2.zero;
                break;
        }

        rb.linearVelocity = v;
    }

    float GetMoveInput()
    {
        return (Input.GetKey(keyLeft) ? -1f : 0f) + (Input.GetKey(keyRight) ? 1f : 0f);
    }

    float MoveTowardsTarget(float current, float target, float acceleration)
    {
        return Mathf.MoveTowards(current, target, acceleration * Time.fixedDeltaTime);
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // MOVEMENT
    // ─────────────────────────────────────────────────────────────────────────────
    void StartJump()
    {
        float g = Mathf.Abs(Physics2D.gravity.y) * gravityScale;
        if (g <= 0f) 
        { 
            Debug.LogError($"[{name}] Invalid gravity; set gravityScale > 0"); 
            enabled = false; 
            return; 
        }
        
        Vector2 v = rb.linearVelocity;
        v.y = Mathf.Sqrt(2f * g * Mathf.Max(jumpHeight, 0.0001f));
        rb.linearVelocity = v;
        state = State.Jump;
    }

    void StartDash()
    {
        state = State.Dash;
        stateTimer = dashDuration;
        
        // Apply dash impulse immediately
        Vector2 v = rb.linearVelocity;
        v.x = dashSpeed * (faceRight ? 1f : -1f);
        rb.linearVelocity = v;
    }

    bool IsGrounded()
    {
        Vector2 origin = (Vector2)transform.position + groundCheckOffset;
        return Physics2D.OverlapCircle(origin, groundCheckRadius, groundMask) != null;
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // COMBAT
    // ─────────────────────────────────────────────────────────────────────────────
    bool TryAttack(bool light, bool medium, bool heavy)
    {
        Attack a = null;
        if (heavy) a = heavyAttack;
        else if (medium) a = mediumAttack;
        else if (light) a = lightAttack;
        if (a == null) return false;

        if (!ValidateAttack(a, "Use")) { enabled = false; return false; }

        currentAttack = a;
        attackTimer = 0f;
        victimsThisSwing.Clear();
        state = State.Attack;
        // Animation will be set by UpdateStateAnimation()
        
        return true;
    }

    void TickAttack()
    {
        if (currentAttack == null) 
        { 
            state = grounded ? State.Idle : State.Fall;
            return; 
        }

        attackTimer += Time.deltaTime;

        // Hit detection during active frames
        float activeStart = currentAttack.startup;
        float activeEnd = activeStart + currentAttack.active;
        if (attackTimer >= activeStart && attackTimer <= activeEnd)
        {
            DetectHits(currentAttack);
        }

        // End attack after full duration (startup + active + recovery)
        float totalTime = currentAttack.startup + currentAttack.active + currentAttack.recovery;
        
        if (attackTimer >= totalTime)
        {
            currentAttack = null;
            state = grounded ? State.Idle : State.Fall;
            // Don't update prevState here - let UpdateStateAnimation handle it
        }
    }

    void DetectHits(Attack attack)
    {
        Vector2 center = (Vector2)transform.position + FlipVector(attack.hitboxOffset);
        Collider2D[] hits = Physics2D.OverlapBoxAll(center, attack.hitboxSize, 0f);
        
        foreach (var col in hits)
        {
            var target = col.GetComponentInParent<BasicFighter2D>();
            if (target == null || target == this || victimsThisSwing.Contains(target)) 
                continue;

            victimsThisSwing.Add(target);
            target.TakeHit(this, attack);
        }
    }

    void TakeHit(BasicFighter2D attacker, Attack attack)
    {
        if (state == State.KO) return;

        // Apply damage
        hp = Mathf.Max(0, hp - attack.damage);
        
        if (hp == 0)
        {
            state = State.KO;
            rb.linearVelocity = Vector2.zero;
            // Animation will be set by UpdateStateAnimation()
            return;
        }

        // Apply hitstun and knockback
        hitstunTimer = Mathf.Max(hitstunTimer, attack.hitstun);
        state = State.Hitstun;
        // Animation will be set by UpdateStateAnimation()

        Vector2 knockback = new Vector2(
            attack.knockback.x * (attacker.faceRight ? 1f : -1f),
            attack.knockback.y
        );
        
        rb.linearVelocity = new Vector2(knockback.x, Mathf.Max(rb.linearVelocity.y, knockback.y));
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // ANIMATION
    // ─────────────────────────────────────────────────────────────────────────────
    void UpdateStateAnimation()
    {
        // Only update animation if state changed
        if (state == prevState) return;
        
        prevState = state;

        // Pick animation based on state
        AnimationClip clip = state switch
        {
            State.Idle => animIdle,
            State.Walk => animWalk,
            State.Jump => animJump,
            State.Fall => animFall,
            State.Dash => animDash,
            State.Attack => currentAttack?.anim,
            State.Hitstun => animHitstun,
            State.KO => animKO,
            _ => null
        };

        if (clip != null)
        {
            currentClip = clip;
            animationTime = 0f;
        }
    }

    void PlayStateClip(AnimationClip clip, bool immediate = false)
    {
        if (clip == null) return;
        
        if (immediate || currentClip != clip)
        {
            currentClip = clip;
            animationTime = 0f;
        }
    }

    void PlayClipByName(string clipName, float transition = 0.1f, bool restart = false)
    {
        // Find clip by name from state animations
        AnimationClip clip = clipName switch
        {
            var name when animIdle != null && name == animIdle.name => animIdle,
            var name when animWalk != null && name == animWalk.name => animWalk,
            var name when animJump != null && name == animJump.name => animJump,
            var name when animFall != null && name == animFall.name => animFall,
            var name when animDash != null && name == animDash.name => animDash,
            var name when animHitstun != null && name == animHitstun.name => animHitstun,
            var name when animKO != null && name == animKO.name => animKO,
            var name when lightAttack?.anim != null && name == lightAttack.anim.name => lightAttack.anim,
            var name when mediumAttack?.anim != null && name == mediumAttack.anim.name => mediumAttack.anim,
            var name when heavyAttack?.anim != null && name == heavyAttack.anim.name => heavyAttack.anim,
            _ => null
        };

        if (clip == null) return;

        if (restart || currentClip != clip)
        {
            currentClip = clip;
            animationTime = 0f;
        }
    }

    void SampleCurrentAnimation()
    {
        if (currentClip == null || sprite == null) return;

        // Advance animation time
        animationTime += Time.deltaTime;

        // Check if we're in attack state and animation has completed
        if (state == State.Attack && animationTime >= currentClip.length)
        {
            // Force transition out of attack state
            currentAttack = null;
            // Do a fresh ground check to determine next state
            bool isGrounded = IsGrounded();
            State newState = isGrounded ? State.Idle : State.Fall;
            state = newState;
            prevState = State.Attack; // Force animation system to detect the change
            
            // Immediately switch to the new animation
            AnimationClip newClip = newState == State.Idle ? animIdle : animFall;
            if (newClip != null)
            {
                currentClip = newClip;
                animationTime = 0f;
            }
            return;
        }

        // Always loop animations (for non-attack states)
        if (animationTime >= currentClip.length)
        {
            animationTime = animationTime % currentClip.length;
        }

        // Store position before sampling to prevent animation-driven movement
        Vector3 posBeforeSample = transform.position;

        // Sample the animation clip
        currentClip.SampleAnimation(gameObject, animationTime);

        // Restore position to prevent animation clips from moving the character
        transform.position = posBeforeSample;
    }

    void UpdateSpriteFacing()
    {
        if (sprite != null) sprite.flipX = !faceRight;
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // HEALTH BAR
    // ─────────────────────────────────────────────────────────────────────────────
    void SetupHealthBar()
    {
        // Create root
        healthBarRoot = new GameObject($"{fighterName}_HealthBar").transform;
        healthBarRoot.SetParent(transform, false);
        healthBarRoot.localPosition = new Vector3(healthBarOffsetX, healthBarOffsetY, -1f); // Ensure it's in front

        // Background - use the provided sprite
        var bgObj = new GameObject("Background");
        bgObj.transform.SetParent(healthBarRoot, false);
        healthBarBg = bgObj.AddComponent<SpriteRenderer>();
        healthBarBg.sprite = healthBarSprite;
        healthBarBg.color = Color.black; // Dark background
        healthBarBg.sortingOrder = healthBarSortingOrder;
        bgObj.transform.localScale = new Vector3(healthBarScaleX + 0.05f, healthBarScaleY + 0.05f, 1f);
        bgObj.transform.localPosition = Vector3.zero;

        // Fill - use the provided sprite with full alpha
        var fillObj = new GameObject("Fill");
        fillObj.transform.SetParent(healthBarRoot, false);
        fillObj.transform.localPosition = Vector3.zero;
        healthBarFill = fillObj.AddComponent<SpriteRenderer>();
        healthBarFill.sprite = healthBarSprite;
        
        // Ensure colors have full alpha
        Color fullColor = healthBarColorFull;
        fullColor.a = 1f;
        healthBarFill.color = fullColor;
        
        healthBarFill.sortingOrder = healthBarSortingOrder + 1;
        fillObj.transform.localScale = new Vector3(healthBarScaleX, healthBarScaleY, 1f);
    }

    Sprite CreateWhiteSprite()
    {
        if (healthBarTexture == null)
        {
            healthBarTexture = new Texture2D(1, 1);
            healthBarTexture.SetPixel(0, 0, Color.white);
            healthBarTexture.Apply();
        }
        return Sprite.Create(healthBarTexture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
    }

    void UpdateHealthBar()
    {
        if (healthBarFill == null)
        {
            Debug.LogWarning($"[{fighterName}] Health bar fill is null!", this);
            return;
        }

        float hpRatio = maxHP > 0 ? (float)hp / maxHP : 0f;
        Vector3 newScale = new Vector3(healthBarScaleX * hpRatio, healthBarScaleY, 1f);
        healthBarFill.transform.localScale = newScale;
        
        // Lerp color and ensure alpha is always 1
        Color newColor = Color.Lerp(healthBarColorEmpty, healthBarColorFull, hpRatio);
        newColor.a = 1f;
        healthBarFill.color = newColor;
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // HELPERS
    // ─────────────────────────────────────────────────────────────────────────────
    Vector2 FlipVector(Vector2 v) => new Vector2(v.x * (faceRight ? 1f : -1f), v.y);

    // ─────────────────────────────────────────────────────────────────────────────
    // VALIDATION
    // ─────────────────────────────────────────────────────────────────────────────
    bool ValidateRequired()
    {
        bool ok = true;
        void Fail(string msg) => Debug.LogError($"[{gameObject.name}] {msg}", this);

        // Identity
        if (string.IsNullOrWhiteSpace(fighterName)) { Fail("fighterName is REQUIRED"); ok = false; }

        // Controls
        if (keyLeft == KeyCode.None) { Fail("keyLeft is REQUIRED"); ok = false; }
        if (keyRight == KeyCode.None) { Fail("keyRight is REQUIRED"); ok = false; }
        if (keyJump == KeyCode.None) { Fail("keyJump is REQUIRED"); ok = false; }
        if (keyDash == KeyCode.None) { Fail("keyDash is REQUIRED"); ok = false; }
        if (keyLight == KeyCode.None) { Fail("keyLight is REQUIRED"); ok = false; }
        if (keyMedium == KeyCode.None) { Fail("keyMedium is REQUIRED"); ok = false; }
        if (keyHeavy == KeyCode.None) { Fail("keyHeavy is REQUIRED"); ok = false; }

        // Components
        if (!rb) { Fail("Rigidbody2D is REQUIRED"); ok = false; }
        if (!body) { Fail("Collider2D is REQUIRED"); ok = false; }
        if (!sprite) { Fail("SpriteRenderer is REQUIRED (on GameObject or child)"); ok = false; }

        // Movement
        if (moveSpeed <= 0f) { Fail("moveSpeed must be > 0"); ok = false; }
        if (jumpHeight <= 0f) { Fail("jumpHeight must be > 0"); ok = false; }
        if (gravityScale <= 0f) { Fail("gravityScale must be > 0"); ok = false; }
        if (fallGravityScale <= 0f) { Fail("fallGravityScale must be > 0"); ok = false; }
        if (dashSpeed <= 0f) { Fail("dashSpeed must be > 0"); ok = false; }
        if (dashDuration <= 0f) { Fail("dashDuration must be > 0"); ok = false; }
        if (airControl <= 0f) { Fail("airControl must be > 0"); ok = false; }
        if (groundMask == 0) { Fail("groundMask must be set"); ok = false; }
        if (groundCheckRadius <= 0f) { Fail("groundCheckRadius must be > 0"); ok = false; }

        // Vitals
        if (maxHP <= 0) { Fail("maxHP must be > 0"); ok = false; }

        // Health Bar
        if (healthBarSprite == null) { Fail("healthBarSprite is REQUIRED"); ok = false; }
        if (healthBarScaleX <= 0f) { Fail("healthBarScaleX must be > 0"); ok = false; }
        if (healthBarScaleY <= 0f) { Fail("healthBarScaleY must be > 0"); ok = false; }

        // Animations - just check that clips exist
        if (animIdle == null) { Fail("animIdle is REQUIRED"); ok = false; }
        if (animWalk == null) { Fail("animWalk is REQUIRED"); ok = false; }
        if (animJump == null) { Fail("animJump is REQUIRED"); ok = false; }
        if (animFall == null) { Fail("animFall is REQUIRED"); ok = false; }
        if (animDash == null) { Fail("animDash is REQUIRED"); ok = false; }
        if (animHitstun == null) { Fail("animHitstun is REQUIRED"); ok = false; }
        if (animKO == null) { Fail("animKO is REQUIRED"); ok = false; }

        // Attacks
        if (lightAttack == null) { Fail("lightAttack is REQUIRED"); ok = false; }
        else ok &= ValidateAttack(lightAttack, "lightAttack");
        
        if (mediumAttack == null) { Fail("mediumAttack is REQUIRED"); ok = false; }
        else ok &= ValidateAttack(mediumAttack, "mediumAttack");
        
        if (heavyAttack == null) { Fail("heavyAttack is REQUIRED"); ok = false; }
        else ok &= ValidateAttack(heavyAttack, "heavyAttack");

        return ok;
    }

    bool ValidateAttack(Attack attack, string label)
    {
        bool ok = true;
        void Fail(string msg) => Debug.LogError($"[{gameObject.name}] Attack '{label}': {msg}", this);

        if (string.IsNullOrWhiteSpace(attack.name)) { Fail("name is REQUIRED"); ok = false; }
        if (attack.anim == null) { Fail("AnimationClip is REQUIRED"); ok = false; }
        if (attack.startup <= 0f) { Fail("startup must be > 0"); ok = false; }
        if (attack.active <= 0f) { Fail("active must be > 0"); ok = false; }
        if (attack.recovery < 0f) { Fail("recovery must be >= 0"); ok = false; }
        if (attack.damage <= 0) { Fail("damage must be > 0"); ok = false; }
        if (attack.hitstun <= 0f) { Fail("hitstun must be > 0"); ok = false; }
        if (attack.hitboxSize.x <= 0f || attack.hitboxSize.y <= 0f) { Fail("hitboxSize must be > 0"); ok = false; }

        return ok;
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // GIZMOS
    // ─────────────────────────────────────────────────────────────────────────────
    void OnDrawGizmosSelected()
    {
        // Ground check visualization
        Gizmos.color = Color.cyan;
        Vector2 groundOrigin = (Vector2)transform.position + groundCheckOffset;
        Gizmos.DrawWireSphere(groundOrigin, groundCheckRadius);

        // Active attack hitbox visualization
        if (state == State.Attack && currentAttack != null)
        {
            Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.5f);
            Vector2 hitboxCenter = (Vector2)transform.position + FlipVector(currentAttack.hitboxOffset);
            Gizmos.DrawWireCube(hitboxCenter, currentAttack.hitboxSize);
        }
    }
}
