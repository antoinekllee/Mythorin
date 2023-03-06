using RPG.Attributes;
using UnityEngine;
using RPG.Control;

namespace RPG.Combat
{
    [RequireComponent(typeof(Health))]

    public class CombatTarget : MonoBehaviour, IRaycastable
    {
        public CursorType GetCursorType()
        {
            return CursorType.Combat; 
        }

        public bool HandleRaycast(PlayerController callingController)
        {
            if (!GetComponent<Fighter>().CanAttack(gameObject))
            {
                return false;
            }

            if (Input.GetMouseButton(0))
            {
                callingController.GetComponent<Fighter>().Attack(gameObject);
            }

            return true;
        }
    }
}
