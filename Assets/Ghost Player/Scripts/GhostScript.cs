using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Sample
{
    public class GhostScript : MonoBehaviour
    {
        private Animator Anim;
        private CharacterController Ctrl;
        private Vector3 MoveDirection = Vector3.zero;

        private static readonly int IdleState = Animator.StringToHash("Base Layer.idle");
        private static readonly int MoveState = Animator.StringToHash("Base Layer.move");
        private static readonly int SurprisedState = Animator.StringToHash("Base Layer.surprised");
        private static readonly int AttackState = Animator.StringToHash("Base Layer.attack");
        private static readonly int DissolveState = Animator.StringToHash("Base Layer.dissolve");
        private static readonly int AttackTag = Animator.StringToHash("Attack");

        [SerializeField] private SkinnedMeshRenderer[] MeshR;
        private float Dissolve_value = 1;
        private bool DissolveFlg = false;
        private const int maxHP = 3;
        private int HP = maxHP;
        private Text HP_text;

        private bool isJumping = false;
        private bool isPickingUp = false;
        private PlayerMovement playerMovement;
        private CharacterController playerCtrl;
        private int currentState = -1;

        private Coroutine jumpCoroutine;
        private Coroutine pickupCoroutine;

        public void OnJump()
        {
            if (jumpCoroutine != null) StopCoroutine(jumpCoroutine);
            isJumping = true;
            isPickingUp = false;
            currentState = AttackState;
            Anim.CrossFade(AttackState, 0.1f, 0, 0);
            jumpCoroutine = StartCoroutine(WaitForJumpAnim());
        }

        public void OnPickupFruit()
        {
            if (pickupCoroutine != null) StopCoroutine(pickupCoroutine);
            isPickingUp = true;
            currentState = SurprisedState;
            Anim.CrossFade(SurprisedState, 0.1f, 0, 0);
            pickupCoroutine = StartCoroutine(WaitForPickupAnim());
        }

        private IEnumerator WaitForJumpAnim()
        {
            float timeout = 0f;
            while (Anim.GetCurrentAnimatorStateInfo(0).fullPathHash != AttackState && timeout < 0.5f)
            {
                timeout += Time.deltaTime;
                yield return null;
            }

            float length = Anim.GetCurrentAnimatorStateInfo(0).length;
            yield return new WaitForSeconds(length * 0.85f);

            isJumping = false;
            currentState = IdleState;           // set this BEFORE CrossFade
            Anim.CrossFade(IdleState, 0.15f, 0, 0); // force back to idle immediately
            jumpCoroutine = null;
        }

        private IEnumerator WaitForPickupAnim()
        {
            // Wait until we're actually IN the surprised state
            float timeout = 0f;
            while (Anim.GetCurrentAnimatorStateInfo(0).fullPathHash != SurprisedState && timeout < 0.5f)
            {
                timeout += Time.deltaTime;
                yield return null;
            }

            float length = Anim.GetCurrentAnimatorStateInfo(0).length;
            yield return new WaitForSeconds(length);

            isPickingUp = false;
            currentState = -1;
            pickupCoroutine = null;
        }

        void Start()
        {
            Anim = this.GetComponent<Animator>();
            Ctrl = this.GetComponent<CharacterController>();
            playerMovement = this.GetComponent<PlayerMovement>();
            playerCtrl = this.GetComponent<CharacterController>();
            HP_text = GameObject.Find("Canvas/HP").GetComponent<Text>();
            HP_text.text = "HP " + HP.ToString();
        }

        void Update()
        {
            STATUS();
            ANIMATION_KEYS();

            if (PlayerStatus.ContainsValue(true))
            {
                int status_name = 0;
                foreach (var i in PlayerStatus)
                {
                    if (i.Value == true) { status_name = i.Key; break; }
                }
                if (status_name == Dissolve) PlayerDissolve();
            }

            if (HP <= 0 && !DissolveFlg)
            {
                Anim.CrossFade(DissolveState, 0.1f, 0, 0);
                DissolveFlg = true;
            }
            else if (HP == maxHP && DissolveFlg)
            {
                DissolveFlg = false;
            }
        }

        private void ANIMATION_KEYS()
        {
            if (isJumping || isPickingUp) return;

            Vector3 horizontalVelocity = playerCtrl.velocity;
            horizontalVelocity.y = 0f;
            bool isMoving = horizontalVelocity.magnitude > 0.15f;

            int targetState = isMoving ? MoveState : IdleState;

            if (targetState != currentState)
            {
                currentState = targetState;
                Anim.CrossFade(targetState, 0.1f, 0, 0);
            }
        }

        private const int Dissolve = 1;
        private const int Attack = 2;
        private const int Surprised = 3;
        private Dictionary<int, bool> PlayerStatus = new Dictionary<int, bool>
        {
            { Dissolve,  false },
            { Attack,    false },
            { Surprised, false },
        };

        private void STATUS()
        {
            if (DissolveFlg && HP <= 0) PlayerStatus[Dissolve] = true;
            else if (!DissolveFlg) PlayerStatus[Dissolve] = false;

            if (Anim.GetCurrentAnimatorStateInfo(0).tagHash == AttackTag)
                PlayerStatus[Attack] = true;
            else PlayerStatus[Attack] = false;

            if (Anim.GetCurrentAnimatorStateInfo(0).fullPathHash == SurprisedState)
                PlayerStatus[Surprised] = true;
            else PlayerStatus[Surprised] = false;
        }

        private void PlayerDissolve()
        {
            Dissolve_value -= Time.deltaTime;
            for (int i = 0; i < MeshR.Length; i++)
                MeshR[i].material.SetFloat("_Dissolve", Dissolve_value);
            if (Dissolve_value <= 0)
                Ctrl.enabled = false;
        }

        private bool CheckGrounded()
        {
            if (Ctrl.isGrounded && Ctrl.enabled) return true;
            Ray ray = new Ray(this.transform.position + Vector3.up * 0.1f, Vector3.down);
            return Physics.Raycast(ray, 0.2f);
        }
    }
}