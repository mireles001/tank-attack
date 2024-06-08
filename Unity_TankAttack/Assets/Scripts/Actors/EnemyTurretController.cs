using UnityEngine;

namespace Shibidubi.TankAttack
{
    public class EnemyTurretController : EnemyController
    {
        [SerializeField] private TurretAttackController _attackController;
        [SerializeField] private float _turretPassiveMovementSpeed;

        private readonly Vector3 LOS_BOXCAST_HALF_EXTENDS = new(0.2f, 0.25f, 0.25f);

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

            KillAggroReset();
            _resetAggroCoroutine = StartCoroutine(BeginAggroReset());
        }

        private void MoveTurret(float deltaTime)
        {
            if (_aggroController.AggroTarget == null)
            {
                if (_resetAggroCoroutine == null && _turretPassiveMovementSpeed != 0)
                {
                    // TODO: Use RotateTurret method to move the turrent, not this workaround
                    _attackController.TurretTransform.RotateAround(_attackController.TurretTransform.position, Vector3.up, _turretPassiveMovementSpeed * Time.deltaTime);
                }
            }
            else
            {
                Vector3 lookAtDirection = _aggroController.AggroTargetPosition - _attackController.TurretTransform.position;
                _attackController.RotateTurret(lookAtDirection, deltaTime);
            }
        }
    }
}
