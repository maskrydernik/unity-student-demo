# BasicFighter2D — Public Tunables Reference

All fields default to zero/null. Units are seconds, world units, or Unity types as noted.

## Identity

* **fighterName : string**
  Used for filenames/logs only. No gameplay effect. Required non-empty.

## Controls

All must be non-`KeyCode.None`. Polled every frame in `Update()`.

* **keyLeft, keyRight : KeyCode**
  Horizontal input. Drives Walk target velocity and facing. Also allows limited drift during Attack.
* **keyJump : KeyCode**
  Triggers `DoJump()` only when grounded and not attacking/hitstunned/KO.
* **keyDash : KeyCode**
  Triggers Dash state. Can be canceled into attacks.
* **keyLight, keyMedium, keyHeavy : KeyCode**
  Start the corresponding `Attack` data block if valid.

## Movement

* **moveSpeed : float (units/s > 0)**
  Horizontal max speed. Also scales acceleration:
  Idle brake = `moveSpeed * 1x`.
  Ground accel = `moveSpeed * 4x`.
  Air accel = `moveSpeed * (2x * airControl)`.
  Attack drift = `0.35 * moveSpeed` with accel `drift * 6x`.

* **jumpHeight : float (world units > 0)**
  Converted to initial vertical velocity: `v0 = sqrt(2*g*jumpHeight)`, where `g = |Physics2D.gravity.y| * gravityScale`.

* **gravityScale : float (> 0)**
  Applied to `rb.gravityScale` while rising or when vertical velocity ≥ 0.

* **fallGravityScale : float (> 0)**
  Applied when `rb.linearVelocity.y < 0` to sharpen fall.

* **dashSpeed : float (units/s > 0)**
  Constant horizontal velocity during Dash. Direction = facing.

* **dashDuration : float (s > 0)**
  Time in Dash state. After expiry, transitions to Idle/Fall.

* **airControl : float (recommended 0..1, must be > 0)**
  Scales air target speed `= moveSpeed * clamp01(airControl)` and air acceleration `= moveSpeed * 2 * airControl`.
  Higher = tighter midair steering.

* **groundMask : LayerMask (required)**
  Layers considered “ground” for circle check. Missing mask => validation fail.

* **groundCheckOffset : Vector2**
  Offset from `transform.position` used for ground probe. Tune to foot position.

* **groundCheckRadius : float (> 0)**
  Radius for `Physics2D.OverlapCircle`. Too small may cause jitter; too large sticks to walls.

## Attacks

Each of the three references must point to a valid `Attack` block. The script uses the last pressed among H/M/L in that frame (H overrides M overrides L if multiple pressed).

* **lightAttack, mediumAttack, heavyAttack : Attack**
  Data-driven attack definitions. Required and validated in `Awake()` and again at use-time.

### Attack (nested class)

* **name : string (required)**
  Debug/log label only.

* **anim : AnimationClip (required)**
  Played via `animator.Play(anim.name, 0, 0)` when the attack starts. No Animator parameters are used for attacks.

* **startup : float (s > 0)**
  Time before the hitbox becomes active. During startup, no collision.

* **active : float (s > 0)**
  Window where `DoHitDetect()` runs each frame. Multiple victims allowed, but each victim only once per swing.

* **recovery : float (s ≥ 0)**
  Lockout after active ends. No input processed for new actions until total duration elapses.

* **damage : int (> 0)**
  Subtracted from defender’s HP on hit. KO at 0.

* **hitstun : float (s > 0)**
  Defender’s forced inaction time. On hit, defender enters Hitstun and can’t act until timer expires. If already in hitstun, duration is extended via `max(existing, new)`.

* **hitboxOffset : Vector2**
  Center offset in local space. Flips on X with facing. Y is unchanged.

* **hitboxSize : Vector2 (> 0, > 0)**
  Box size passed to `Physics2D.OverlapBoxAll`. World-space dimensions.

* **knockback : Vector2**
  Applied on hit. X flips with attacker facing. Y uses `max(currentVy, knockback.y)` so upward knockback won’t be suppressed by current downward velocity.

#### Timing notes

* Total lockout = `startup + active + recovery`. The FSM remains in `State.Attack` for the entire total.
* If the animation clip length is shorter than total, the clip ends visually but state continues through recovery. If longer, the clip keeps playing; you typically set attack clips not to loop.

#### Typical 60 FPS guidelines

* Light: `(startup, active, rec) ≈ (0.08–0.12, 0.06–0.10, 0.10–0.18)`
* Medium: `(0.12–0.16, 0.08–0.12, 0.16–0.24)`
* Heavy: `(0.16–0.22, 0.10–0.16, 0.22–0.32)`

## Vitals

* **maxHP : int (> 0)**
  Initializes `hp` in `Awake()`. KO at 0 sets velocity to zero and enters KO state.

## State Animations

These clips are required. The script sets Animator bools on state change for these seven states. Your controller must contain states with matching clip assignments and transitions based on these bools **or** use a minimalist graph and let direct `Play()` calls handle attacks.

* **animIdle**
  Looped. Played whenever entering Idle.

* **animWalk**
  Looped. Played on Walk.

* **animJump**
  Usually non-loop or short loop.

* **animFall**
  Looped. Played while airborne and falling.

* **animDash**
  Either non-loop with duration ≈ `dashDuration`, or loop.

* **animHitstun**
  Looped or short non-loop. Played while in Hitstun.

* **animKO**
  Non-loop. Played on KO and remains.