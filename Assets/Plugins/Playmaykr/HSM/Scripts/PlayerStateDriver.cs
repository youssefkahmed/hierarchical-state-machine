using System;
using System.Linq;
using Playmaykr.HSM.ConcreteStates;
using Playmaykr.HSM.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Playmaykr.HSM
{
    #region Data

    [Serializable]
    public class PlayerContext
    {
        public Vector3 move;
        public Vector3 velocity;
        public bool grounded;
        public float moveSpeed = 6f;
        public float accel = 40f;
        public float jumpSpeed = 7f;
        public bool jumpPressed;
        public Animator anim;
        public Rigidbody rb;
        public Renderer renderer;
    }

    #endregion
    
    public class PlayerStateDriver : MonoBehaviour
    {
        [Header("Ground Settings:")]
        [SerializeField] private Transform groundCheck;
        [SerializeField] private float groundRadius = 0.2f;
        [SerializeField] private LayerMask groundMask;
        
        [Header("Debug:")]
        [SerializeField] private bool drawGizmos = true;

        private Rigidbody _rb;
        
        private readonly PlayerContext _ctx = new();
        private StateMachine _stateMachine;
        private State _rootState;
        
        private string _lastPath;

        private void OnDrawGizmosSelected()
        {
            if (!drawGizmos || !groundCheck)
            {
                return;
            }

            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(groundCheck.position, groundRadius);
        }

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

            _ctx.rb = _rb;
            _ctx.anim = GetComponentInChildren<Animator>();
            _ctx.renderer = GetComponent<Renderer>();

            _rootState = new PlayerRoot(null, _ctx);
            _stateMachine = new StateMachineBuilder(_rootState).Build();

            // Fallback: create a groundCheck just below the collider's bounds
            if (groundCheck == null)
            {
                var col = GetComponent<Collider>();
                Transform t = new GameObject("GroundCheck").transform;
                t.SetParent(transform, false);
                float y = col ? -col.bounds.extents.y + 0.01f : -0.5f;
                t.localPosition = new Vector3(0, y, 0);
                groundCheck = t;
            }
        }

        private void Update()
        {
            var x = 0f;
            
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
            {
                x -= 1f;
            }
            
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
            {
                x += 1f;
            }

            _ctx.jumpPressed = Keyboard.current.spaceKey.wasPressedThisFrame;
            _ctx.move.x = Mathf.Clamp(x, -1f, 1f);
            _ctx.grounded = Physics.CheckSphere(groundCheck.position, groundRadius, groundMask);

            _stateMachine.Tick(Time.deltaTime);

            string path = GetStatePath(_stateMachine.Root.Leaf());
            if (path != _lastPath)
            {
                Debug.Log($"State: {path}", gameObject);
                _lastPath = path;
            }
        }

        private void FixedUpdate()
        {
            Vector3 currentVelocity = _rb.linearVelocity;
            currentVelocity.x = _ctx.velocity.x;
            _rb.linearVelocity = currentVelocity;

            _ctx.velocity.x = _rb.linearVelocity.x;
        }

        private static string GetStatePath(State s)
        {
            return string.Join(" > ", s.PathToRoot().Reverse().Select(n => n.GetType().Name));
        }
    }
}