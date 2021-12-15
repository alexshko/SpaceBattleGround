using UnityEngine;

namespace SpaceBattle.Shooting
{
    public class HomingLaserShot : LaserShot
    {
        protected sealed override Vector3 calcTargetDirection()
        {
            base.calcTargetDirection();

            Vector3 dirToTarget = transform.forward;
            if (target)
            {
                Vector3 distToTarget = target.position - transform.position;
                //if too close to the target, then keep going straight:
                if (distToTarget.magnitude < 0.5f)
                {
                    dirToTarget = rb.velocity.normalized;
                }
                else {
                    dirToTarget = distToTarget.normalized;
                }
            }
            return dirToTarget;
        }
    }
}
