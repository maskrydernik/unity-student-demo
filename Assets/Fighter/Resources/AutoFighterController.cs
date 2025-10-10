// AutoFightController.cs
// Drop this on an empty GameObject. Press Play.
// It discovers fighters (BasicFighter2D or SimpleFighter) and makes them spar.

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Random = UnityEngine.Random;

public class AutoFightController : MonoBehaviour
{
    // AI pacing
    private float thinkInterval = 0.10f;
    private Vector2 attackCooldown = new Vector2(0.35f, 1.00f);
    private Vector2 dashCooldown = new Vector2(1.0f, 2.0f);
    private float jumpChance = 0.08f;
    private float hopCooldown = 1.8f;
    private float retargetDelay = 0f;  // 0 = locked until target dies

    // Spacing
    private float engageDistanceMultiplier = 2.5f;  // dash when target is this many body widths away
    private float keepDistanceMultiplier = 1.2f;    // ideal fighting distance (body widths)
    private float tooCloseMultiplier = 0.4f;        // back up if closer than this (body widths)
    
    // Idle wandering
    private float wanderChangeInterval = 1.5f;  // how often to change wander direction
    private float idleJumpChance = 0.02f;        // chance to jump while wandering

    private readonly List<FProxy> fighters = new List<FProxy>();
    private float discoverTimer;
    
    // Victory tracking
    private Dictionary<FProxy, int> previousHP = new Dictionary<FProxy, int>();
    private bool victoryAnnounced = false;
    private Queue<VictoryMessage> messageQueue = new Queue<VictoryMessage>();
    private float messageTimer = 0f;
    private float messageDuration = 3f;
    private bool showingMessage = false;
    
    struct VictoryMessage
    {
        public string text;
        public Color color;
    }

    void Awake()
    {
        Discover();
        InvokeRepeating(nameof(Think), 0.2f, thinkInterval);
    }

    void OnDisable() { CancelInvoke(nameof(Think)); }

    void Update()
    {
        // Light periodic rediscover in case fighters spawn later
        discoverTimer += Time.deltaTime;
        if (discoverTimer >= 1.0f) { discoverTimer = 0f; Discover(); }
        
        // Handle message queue
        if (showingMessage)
        {
            messageTimer += Time.deltaTime;
            if (messageTimer >= messageDuration)
            {
                showingMessage = false;
                messageTimer = 0f;
            }
        }
        else if (messageQueue.Count > 0)
        {
            var msg = messageQueue.Dequeue();
            DisplayVictoryText(msg.text, msg.color);
            showingMessage = true;
            messageTimer = 0f;
        }
    }

    void FixedUpdate()
    {
        // Apply horizontal steering after the fighter script runs (its DefaultExecutionOrder is -10)
        for (int i = 0; i < fighters.Count; i++)
        {
            var f = fighters[i];
            if (!f.Alive()) continue;

            if (Time.time < f.dashUntil) continue; // let native dash own velocity
            var v = f.rb.velocity;
            v.x = Mathf.MoveTowards(v.x, f.desiredMoveX * f.moveSpeed, f.moveSpeed * 6f * Time.fixedDeltaTime);
            f.rb.velocity = v;
        }
    }

    void Think()
    {
        // Pair and plan
        for (int i = fighters.Count - 1; i >= 0; i--) if (fighters[i] == null || !fighters[i].Exists()) fighters.RemoveAt(i);
        
        // Check for deaths and announce victories
        CheckForDefeats();
        
        // Check if only one fighter remains
        int aliveCount = 0;
        FProxy survivor = null;
        foreach (var f in fighters)
        {
            if (f.Alive())
            {
                aliveCount++;
                survivor = f;
            }
        }
        
        if (aliveCount == 1 && !victoryAnnounced)
        {
            QueueVictoryText($"{survivor.GetName()} SURVIVES!", new Color(1f, 0.84f, 0f)); // Gold
            victoryAnnounced = true;
            return;
        }
        
        // First pass: check if targets need updating
        for (int i = 0; i < fighters.Count; i++)
        {
            var f = fighters[i];
            if (!f.Alive()) continue;

            // Update retarget timer
            f.retargetTimer += thinkInterval;

            // Check if current target is still valid
            if (f.currentTarget != null)
            {
                // Target died or became invalid - find new one immediately
                if (!f.currentTarget.Exists() || !f.currentTarget.Alive())
                {
                    f.currentTarget = null;
                    f.retargetTimer = 0f;
                }
                // Periodic retargeting if enabled (retargetDelay > 0)
                else if (retargetDelay > 0f && f.retargetTimer >= retargetDelay)
                {
                    f.currentTarget = null;
                    f.retargetTimer = 0f;
                }
            }

            // Pick a new target if needed (closest opponent)
            if (f.currentTarget == null)
            {
                f.currentTarget = PickClosestTarget(f, i);
                f.retargetTimer = 0f;
                
                // Ensure mutual targeting: if we pick someone, they should pick us back
                if (f.currentTarget != null && f.currentTarget.currentTarget == null)
                {
                    f.currentTarget.currentTarget = f;
                    f.currentTarget.retargetTimer = 0f;
                }
            }
        }

        // Second pass: act on targets
        for (int i = 0; i < fighters.Count; i++)
        {
            var f = fighters[i];
            if (!f.Alive()) continue;

            // No valid target - wander around
            if (f.currentTarget == null) 
            { 
                Wander(f);
                continue; 
            }

            // Calculate distance and act
            float distX = Mathf.Abs(f.currentTarget.Pos().x - f.Pos().x);
            Act(f, f.currentTarget, distX);
        }
    }

    void CheckForDefeats()
    {
        foreach (var f in fighters)
        {
            if (f == null || !f.Exists()) continue;
            
            int currentHP = f.GetHP();
            
            // Initialize HP tracking
            if (!previousHP.ContainsKey(f))
            {
                previousHP[f] = currentHP;
                continue;
            }
            
            // Check if fighter just died
            if (previousHP[f] > 0 && currentHP <= 0)
            {
                // Find who killed them (their current target is the attacker)
                if (f.currentTarget != null && f.currentTarget.Alive())
                {
                    string victimName = f.GetName();
                    string killerName = f.currentTarget.GetName();
                    QueueVictoryText($"{killerName} DEFEATED {victimName}!", Color.red);
                }
            }
            
            previousHP[f] = currentHP;
        }
    }

    void QueueVictoryText(string message, Color color)
    {
        messageQueue.Enqueue(new VictoryMessage { text = message, color = color });
    }

    void DisplayVictoryText(string message, Color color)
    {
        // Create a temporary GameObject with TextMesh for the announcement
        GameObject textObj = new GameObject("VictoryText");
        TextMesh textMesh = textObj.AddComponent<TextMesh>();
        
        textMesh.text = message;
        textMesh.fontSize = 72;
        textMesh.color = color;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.characterSize = 0.1f;
        
        // Position at top-center of screen
        Camera cam = Camera.main;
        if (cam != null)
        {
            Vector3 worldPos = cam.ViewportToWorldPoint(new Vector3(0.5f, 0.85f, 10f));
            textObj.transform.position = worldPos;
        }
        else
        {
            textObj.transform.position = new Vector3(0f, 5f, 0f);
        }
        
        // Add MeshRenderer and set sorting layer
        MeshRenderer renderer = textObj.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            renderer.sortingOrder = 1000; // Render on top
        }
        
        // Destroy after 3 seconds
        Destroy(textObj, 3f);
    }

    FProxy PickClosestTarget(FProxy self, int selfIndex)
    {
        // Find closest valid opponent
        FProxy closest = null;
        float bestDist = float.PositiveInfinity;
        
        for (int j = 0; j < fighters.Count; j++)
        {
            if (j == selfIndex) continue;
            var e = fighters[j];
            if (!e.Alive()) continue;
            
            float dist = Mathf.Abs(e.Pos().x - self.Pos().x);
            if (dist < bestDist)
            {
                bestDist = dist;
                closest = e;
            }
        }

        return closest;
    }

    void Wander(FProxy f)
    {
        // Update wander timer
        f.wanderTimer += thinkInterval;
        
        // Change direction periodically
        if (f.wanderTimer >= wanderChangeInterval)
        {
            f.wanderTimer = 0f;
            
            // Pick a random behavior: walk, stand still, or pace
            float roll = Random.value;
            if (roll < 0.3f)
            {
                // Stand still
                f.wanderDirection = 0f;
            }
            else if (roll < 0.5f)
            {
                // Walk slowly in random direction
                f.wanderDirection = (Random.value < 0.5f ? -1f : 1f) * 0.5f;
            }
            else
            {
                // Walk at full speed in random direction
                f.wanderDirection = Random.value < 0.5f ? -1f : 1f;
            }
        }
        
        f.desiredMoveX = f.wanderDirection;
        
        // Update facing based on movement
        if (Mathf.Abs(f.wanderDirection) > 0.1f)
        {
            f.SetFacing(f.wanderDirection > 0f);
        }
        
        // Occasional random jumps while wandering
        if (f.IsGrounded() && Random.value < idleJumpChance)
        {
            f.StartJump();
        }
    }

    void Act(FProxy f, FProxy e, float distX)
    {
        float dir = Mathf.Sign(e.Pos().x - f.Pos().x);
        f.SetFacing(dir >= 0f);

        bool grounded = f.IsGrounded();
        float now = Time.time;

        // Calculate intelligent spacing based on fighter size
        float bodyWidth = f.GetBodyWidth();
        float engageFar = bodyWidth * engageDistanceMultiplier;
        float keepMid = bodyWidth * keepDistanceMultiplier;
        float tooClose = bodyWidth * tooCloseMultiplier;

        // Dynamic movement: add feints and repositioning
        bool inCombatRange = distX <= keepMid;
        
        // Aggressive dash-in from far range
        if (distX > engageFar && now >= f.nextDash && grounded && f.StartDash())
        {
            f.nextDash = now + Random.Range(dashCooldown.x, dashCooldown.y);
            f.dashUntil = now + f.dashDuration;
            f.desiredMoveX = 0f;
            return;
        }

        // Dynamic spacing behavior
        if (distX > keepMid)
        {
            // Approach with occasional hesitation
            f.desiredMoveX = Random.value < 0.85f ? dir : dir * 0.3f;
        }
        else if (distX < tooClose)
        {
            // Back up with occasional dash away
            if (Random.value < 0.15f && now >= f.nextDash && grounded)
            {
                f.StartDash();
                f.nextDash = now + Random.Range(dashCooldown.x, dashCooldown.y);
                f.dashUntil = now + f.dashDuration;
                f.desiredMoveX = -dir;
            }
            else
            {
                f.desiredMoveX = -dir;
            }
        }
        else
        {
            // In optimal range - use varied footsies
            float footsieRoll = Random.value;
            if (footsieRoll < 0.3f)
            {
                // Hold ground
                f.desiredMoveX = 0f;
            }
            else if (footsieRoll < 0.5f)
            {
                // Micro step forward (pressure)
                f.desiredMoveX = dir * 0.4f;
            }
            else if (footsieRoll < 0.7f)
            {
                // Micro step back (bait)
                f.desiredMoveX = -dir * 0.4f;
            }
            else
            {
                // Quick dash through for mixup
                if (now >= f.nextDash && grounded && Random.value < 0.3f)
                {
                    f.StartDash();
                    f.nextDash = now + Random.Range(dashCooldown.x, dashCooldown.y);
                    f.dashUntil = now + f.dashDuration;
                }
                else
                {
                    f.desiredMoveX = Mathf.Sign(Random.value - 0.5f) * 0.3f;
                }
            }
        }

        // More aggressive jumping in combat
        if (grounded && now >= f.nextHop)
        {
            float jumpRoll = Random.value;
            bool shouldJump = inCombatRange ? jumpRoll < jumpChance * 1.2f : jumpRoll < jumpChance;
            
            if (shouldJump && f.StartJump())
            {
                f.nextHop = now + hopCooldown;
            }
        }

        // pick an attack that can reach
        if (now >= f.nextAttack)
        {
            // reach estimate using hitbox data
            float reachL = f.ReachX(f.light);
            float reachM = f.ReachX(f.medium);
            float reachH = f.ReachX(f.heavy);

            bool canHeavy = distX <= reachH * 1.05f;
            bool canMedium = distX <= reachM * 1.05f;
            bool canLight = distX <= reachL * 1.05f;

            bool didAttack = false;
            // prefer heavier if in range; add variance
            if (!didAttack && canHeavy && Random.value < 0.7f) didAttack = f.TryAttack(false, false, true);
            if (!didAttack && canMedium && Random.value < 0.6f) didAttack = f.TryAttack(false, true, false);
            if (!didAttack && canLight) didAttack = f.TryAttack(true, false, false);

            if (didAttack)
            {
                f.nextAttack = now + Random.Range(attackCooldown.x, attackCooldown.y);
                // tiny micro‑step in on attack to look assertive
                f.desiredMoveX = dir * 0.25f;
            }
        }
    }

    void Discover()
    {
        // Accept classes named "BasicFighter2D" or "SimpleFighter"
        var all = FindObjectsOfType<MonoBehaviour>(true);
        var seen = new HashSet<MonoBehaviour>();
        foreach (var mb in all)
        {
            if (mb == null) continue;
            var t = mb.GetType();
            string n = t.Name;
            if (n != "BasicFighter2D" && n != "SimpleFighter") continue;

            if (seen.Contains(mb)) continue;
            seen.Add(mb);

            // Avoid duplicates
            bool already = false;
            for (int i = 0; i < fighters.Count; i++) if (fighters[i].mb == mb) { already = true; break; }
            if (already) continue;

            var p = new FProxy(mb);
            if (p.Valid()) fighters.Add(p);
        }
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // Fighter proxy via reflection
    // ─────────────────────────────────────────────────────────────────────────────
    class FProxy
    {
        public readonly MonoBehaviour mb;
        public readonly Transform tr;
        public readonly Rigidbody2D rb;

        // public fields
        readonly FieldInfo fiMaxHP, fiMoveSpeed, fiDashDuration;
        readonly FieldInfo fiLight, fiMedium, fiHeavy;

        // private fields/methods
        readonly FieldInfo fiHP, fiFaceRight;
        readonly MethodInfo miStartDash, miStartJump, miTryAttack, miIsGrounded;

        // attack sub‑fields
        readonly FieldInfo afiHitboxOffset, afiHitboxSize;

        public object light, medium, heavy;
        public float moveSpeed, dashDuration;

        // steering
        public float desiredMoveX;
        public float nextAttack, nextDash, nextHop, dashUntil;
        
        // target tracking
        public FProxy currentTarget;
        public float retargetTimer;
        
        // wandering behavior
        public float wanderDirection;
        public float wanderTimer;
        
        // reflection for fighter name
        readonly FieldInfo fiFighterName;
        
        // cached body size
        private float bodyWidth;
        private Collider2D collider;

        public FProxy(MonoBehaviour m)
        {
            mb = m;
            tr = m.transform;
            rb = m.GetComponent<Rigidbody2D>();

            var T = m.GetType();
            fiHP = T.GetField("hp", BindingFlags.Instance | BindingFlags.NonPublic);
            fiFaceRight = T.GetField("faceRight", BindingFlags.Instance | BindingFlags.NonPublic);

            fiMaxHP = T.GetField("maxHP", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            fiMoveSpeed = T.GetField("moveSpeed", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            fiDashDuration = T.GetField("dashDuration", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            fiFighterName = T.GetField("fighterName", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            fiLight = T.GetField("lightAttack", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            fiMedium = T.GetField("mediumAttack", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            fiHeavy = T.GetField("heavyAttack", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            miStartDash = T.GetMethod("StartDash", BindingFlags.Instance | BindingFlags.NonPublic);
            miStartJump = T.GetMethod("StartJump", BindingFlags.Instance | BindingFlags.NonPublic);
            miTryAttack = T.GetMethod("TryAttack", BindingFlags.Instance | BindingFlags.NonPublic);
            miIsGrounded = T.GetMethod("IsGrounded", BindingFlags.Instance | BindingFlags.NonPublic);

            // nested Attack type fields
            var attackType = T.GetNestedType("Attack", BindingFlags.Public | BindingFlags.NonPublic);
            if (attackType != null)
            {
                afiHitboxOffset = attackType.GetField("hitboxOffset", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                afiHitboxSize = attackType.GetField("hitboxSize", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            }

            // cache public values
            moveSpeed = fiMoveSpeed != null ? Convert.ToSingle(fiMoveSpeed.GetValue(mb)) : 5f;
            dashDuration = fiDashDuration != null ? Convert.ToSingle(fiDashDuration.GetValue(mb)) : 0.2f;

            // cache attacks
            light = fiLight != null ? fiLight.GetValue(mb) : null;
            medium = fiMedium != null ? fiMedium.GetValue(mb) : null;
            heavy = fiHeavy != null ? fiHeavy.GetValue(mb) : null;

            desiredMoveX = 0f;
            nextAttack = nextDash = nextHop = dashUntil = 0f;
            currentTarget = null;
            retargetTimer = 0f;
            wanderDirection = 0f;
            wanderTimer = 0f;
            
            // Cache collider and calculate body width
            collider = m.GetComponent<Collider2D>();
            CalculateBodyWidth();
        }
        
        void CalculateBodyWidth()
        {
            if (collider == null)
            {
                bodyWidth = 1.0f; // Default fallback
                return;
            }
            
            // Check for CapsuleCollider2D first
            if (collider is CapsuleCollider2D capsule)
            {
                // Use the width of the capsule
                bodyWidth = capsule.size.x * Mathf.Abs(tr.localScale.x);
            }
            // Fallback to BoxCollider2D
            else if (collider is BoxCollider2D box)
            {
                bodyWidth = box.size.x * Mathf.Abs(tr.localScale.x);
            }
            // Fallback to CircleCollider2D
            else if (collider is CircleCollider2D circle)
            {
                bodyWidth = circle.radius * 2f * Mathf.Abs(tr.localScale.x);
            }
            else
            {
                // Generic bounds
                bodyWidth = collider.bounds.size.x;
            }
            
            // Ensure minimum width
            if (bodyWidth < 0.1f) bodyWidth = 1.0f;
        }
        
        public float GetBodyWidth()
        {
            // Recalculate if scale changed
            if (collider != null && collider is CapsuleCollider2D capsule)
            {
                float currentWidth = capsule.size.x * Mathf.Abs(tr.localScale.x);
                if (Mathf.Abs(currentWidth - bodyWidth) > 0.01f)
                {
                    CalculateBodyWidth();
                }
            }
            return bodyWidth;
        }

        public bool Valid() => mb != null && tr != null && rb != null && fiMaxHP != null;

        public bool Exists() => mb != null;

        public Vector3 Pos() => tr.position;

        public bool Alive()
        {
            if (mb == null || !mb.enabled) return false;
            
            int hp = fiHP != null ? Convert.ToInt32(fiHP.GetValue(mb)) : 1;
            int max = fiMaxHP != null ? Convert.ToInt32(fiMaxHP.GetValue(mb)) : 1;
            return max > 0 && hp > 0;
        }
        
        public int GetHP()
        {
            return fiHP != null ? Convert.ToInt32(fiHP.GetValue(mb)) : 1;
        }
        
        public string GetName()
        {
            if (fiFighterName != null)
            {
                string name = fiFighterName.GetValue(mb) as string;
                if (!string.IsNullOrEmpty(name)) return name;
            }
            return mb.gameObject.name;
        }

        public void SetFacing(bool right)
        {
            if (fiFaceRight != null) fiFaceRight.SetValue(mb, right);
            // immediate visual flip for current frame
            var sr = mb.GetComponentInChildren<SpriteRenderer>();
            if (sr != null) sr.flipX = !right;
        }

        public bool StartDash()
        {
            if (miStartDash == null) return false;
            miStartDash.Invoke(mb, null);
            return true;
        }

        public bool StartJump()
        {
            if (miStartJump == null) return false;
            miStartJump.Invoke(mb, null);
            return true;
        }

        public bool TryAttack(bool lightBtn, bool mediumBtn, bool heavyBtn)
        {
            if (miTryAttack == null) return false;
            object r = miTryAttack.Invoke(mb, new object[] { lightBtn, mediumBtn, heavyBtn });
            return r is bool b && b;
        }

        public bool IsGrounded()
        {
            if (miIsGrounded == null) return true;
            object r = miIsGrounded.Invoke(mb, null);
            return r is bool b && b;
        }

        public float ReachX(object attackObj)
        {
            if (attackObj == null || afiHitboxOffset == null || afiHitboxSize == null) return bodyWidth * 0.8f;
            
            var off = (Vector2)afiHitboxOffset.GetValue(attackObj);
            var siz = (Vector2)afiHitboxSize.GetValue(attackObj);
            
            // Calculate reach: hitbox offset + half hitbox size + half body width
            float reach = Mathf.Abs(off.x) + (siz.x * 0.5f) + (bodyWidth * 0.5f);
            return reach;
        }
    }
}
