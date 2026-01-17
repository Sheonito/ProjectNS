using System;
using UnityEngine;

namespace Percent111.ProjectNS.Player
{
    [Serializable]
    public class PlayerMovementSettings
    {
        [Header("Movement Speed")]
        public float runSpeed = 12f;
        public float airSpeed = 10f;

        [Header("Acceleration")]
        public float groundAcceleration = 80f;
        public float groundDeceleration = 100f;
        public float airAcceleration = 40f;
        public float airDeceleration = 30f;
        public float turnSpeedMultiplier = 2f;

        [Header("Jump")]
        public float jumpForce = 18f;
        public float gravity = -50f;
        public float maxFallSpeed = -30f;
        public float coyoteTime = 0.08f;
        public float jumpCutMultiplier = 0.4f;

        [Header("Ground Check")]
        public float groundCheckDistance = 0.15f;
        public float groundCheckOffset = 0.5f;
        public LayerMask groundLayer = 1;

        [Header("Wall Check")]
        public float wallCheckDistance = 0.3f;
        public float wallCheckHeight = 0.3f;
    }
}
