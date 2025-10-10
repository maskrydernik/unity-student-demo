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
    [Header("AI pacing")]
    [Min(0.02f)] public float thinkInterval = 0.10f;
    public Vector2 attackCooldown = new Vector2(0.35f, 1.00f);
    public Vector2 dashCooldown = new Vector2(1.0f, 2.0f);
    public float jumpChance = 0.20f;
    public float hopCooldown = 0.9f;

    [Header("Spacing")]
    public float engageFar = 4.0f;       // dash to close
    public float keepMid = 1.6f;         // walk in until here
    public float tooClose = 0.55f;       // step back if inside

    readonly List<FProxy> fighters = new List<FProxy>();
    float discoverTimer;

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
        for (int i = 0; i < fighters.Count; i++)
        {
            var f = fighters[i];
            if (!f.Alive()) continue;

            // pick nearest opponent
            FProxy enemy = null;
            float best = float.PositiveInfinity;
            for (int j = 0; j < fighters.Count; j++)
            {
                if (i == j) continue;
                var e = fighters[j];
                if (!e.Alive()) continue;
                float d = Mathf.Abs(e.Pos().x - f.Pos().x);
                if (d < best) { best = d; enemy = e; }
            }
            if (enemy == null) { f.desiredMoveX = 0f; continue; }

            Act(f, enemy, best);
        }
    }

    void Act(FProxy f, FProxy e, float distX)
    {
        float dir = Mathf.Sign(e.Pos().x - f.Pos().x);
        f.SetFacing(dir >= 0f);

        bool grounded = f.IsGrounded();
        float now = Time.time;

        // spacing
        if (distX > engageFar && now >= f.nextDash && grounded && f.StartDash())
        {
            f.nextDash = now + Random.Range(dashCooldown.x, dashCooldown.y);
            f.dashUntil = now + f.dashDuration;
            f.desiredMoveX = 0f;
            return;
        }

        // approach / hold / retreat
        if (distX > keepMid) f.desiredMoveX = dir;           // close in
        else if (distX < tooClose) f.desiredMoveX = -dir;    // give space
        else f.desiredMoveX = Mathf.Sign(Random.value - 0.5f) * 0.3f; // subtle drift

        // opportunistic hop
        if (grounded && now >= f.nextHop && Random.value < jumpChance)
        {
            if (f.StartJump()) f.nextHop = now + hopCooldown;
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
        }

        public bool Valid() => mb != null && tr != null && rb != null && fiMaxHP != null;

        public bool Exists() => mb != null;

        public Vector3 Pos() => tr.position;

        public bool Alive()
        {
            int hp = fiHP != null ? Convert.ToInt32(fiHP.GetValue(mb)) : 1;
            int max = fiMaxHP != null ? Convert.ToInt32(fiMaxHP.GetValue(mb)) : 1;
            return mb.enabled && max > 0 && hp > 0;
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
            if (attackObj == null || afiHitboxOffset == null || afiHitboxSize == null) return 0.9f;
            var off = (Vector2)afiHitboxOffset.GetValue(attackObj);
            var siz = (Vector2)afiHitboxSize.GetValue(attackObj);
            return Mathf.Abs(off.x) + 0.5f * Mathf.Abs(siz.x);
        }
    }
}
