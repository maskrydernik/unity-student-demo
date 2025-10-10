# BasicFighter2D — Public Tunables

Short guide to the inspector fields and what they control. Only public variables are listed.

## Identity

* **fighterName (string)**: Display/debug name used for the health bar object and logs.

## Controls

* **keyLeft, keyRight (KeyCode)**: Horizontal movement input.
* **keyJump (KeyCode)**: Triggers jump when grounded; can add upward velocity during some states.
* **keyDash (KeyCode)**: Starts a ground or air dash.
* **keyLight, keyMedium, keyHeavy (KeyCode)**: Triggers the corresponding Attack block.

## Movement

* **moveSpeed (float, units/s > 0)**: Horizontal target speed.
* **jumpHeight (float, world units > 0)**: Desired jump apex height; converted to initial vertical velocity.
* **gravityScale (float > 0)**: Gravity while rising.
* **fallGravityScale (float > 0)**: Gravity while falling.
* **dashSpeed (float, units/s > 0)**: Horizontal speed applied at dash start.
* **dashDuration (float, s > 0)**: Dash state time before control returns.
* **airControl (float > 0, usually 0..1)**: Horizontal acceleration factor in air.
* **groundMask (LayerMask)**: Layers considered “ground” for grounding checks.

## Ground Check

* **groundCheckOffset (Vector2)**: Local offset for the ground probe center.
* **groundCheckRadius (float > 0)**: Radius of the ground overlap circle.

## Attacks

Each of these is an **Attack** object used by the state machine.

* **lightAttack (Attack)**
* **mediumAttack (Attack)**
* **heavyAttack (Attack)**

### Attack fields

* **name (string)**: For debugging and logs.
* **anim (AnimationClip)**: Clip played while the attack runs.
* **startup (float, s > 0)**: Time before the hitbox becomes active.
* **active (float, s > 0)**: Time the hitbox can connect.
* **recovery (float, s >= 0)**: Post-active recovery time before you can act freely.
* **damage (int > 0)**: HP removed on hit.
* **hitstun (float, s > 0)**: Time the target is locked in hitstun.
* **hitboxOffset (Vector2)**: Local center of the hitbox; flips with facing.
* **hitboxSize (Vector2 > 0,0)**: Width and height of the hitbox.
* **knockback (Vector2)**: Velocity applied to the target on hit; X flips with attacker facing.

## Vitals

* **maxHP (int > 0)**: Maximum health. Current HP is internal and initialized to this.

## Health Bar

* **healthBarSprite (Sprite)**: Source sprite for both background and fill (white square recommended).
* **healthBarOffsetX, healthBarOffsetY (float)**: Local position of the bar root relative to the fighter.
* **healthBarScaleX, healthBarScaleY (float > 0)**: Base width and height of the bar. X scales by HP ratio at runtime.
* **healthBarColorFull (Color)**: Fill color at full HP.
* **healthBarColorEmpty (Color)**: Fill color at 0 HP. Runtime color lerps between Empty and Full.

## State Animations

Clips sampled directly each frame.

* **animIdle (AnimationClip)**: Idle state.
* **animWalk (AnimationClip)**: Walking state.
* **animJump (AnimationClip)**: Rising after jump.
* **animFall (AnimationClip)**: Falling.
* **animDash (AnimationClip)**: Dashing.
* **animHitstun (AnimationClip)**: While stunned by a hit.
* **animKO (AnimationClip)**: On death/zero HP.
