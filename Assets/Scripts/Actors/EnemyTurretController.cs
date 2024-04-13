using UnityEngine;

namespace Shibidubi.TankAttack
{
    public class EnemyTurretController : EnemyController
    {
        [SerializeField] private TurretAttackController _attackController;

        private readonly Vector3 LOS_BOXCAST_HALF_EXTENDS = new(0.25f, 0.25f, 0.25f);

        private void Update()
        {
            if (DoNotUpdate() || _aggroController == null)
            {
                return;
            }

            _isTargetInSight = CheckLineOfSight(_attackController.TurretTransform, _aggroController.AggroTarget, LOS_BOXCAST_HALF_EXTENDS, _attackThrought);
            Attack();
            MoveTurret(Time.deltaTime);
        }

        private void Attack()
        {
            if (!_isTargetInSight || _attackController.OnCoolDown)
            {
                return;
            }

            _attackController.Attack();
        }

        private void MoveTurret(float deltaTime)
        {
            if (_aggroController.AggroTarget == null)
            {
                return;
            }

            Vector3 lookAtDirection = _aggroController.AggroTargetPosition - _attackController.TurretTransform.position;
            _attackController.RotateTurret(lookAtDirection, deltaTime);
        }
    }
}
