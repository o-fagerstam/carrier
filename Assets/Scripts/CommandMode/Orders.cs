using PhysicsUtilities;
using UnityEngine;

namespace CommandMode {
    public class Order {
       public IMoveOrder MoveOrder;

       public override string ToString() {
           return MoveOrder.ToString();
       }
    }
    
    /*
     * MOVE ORDER
     */
    public interface IMoveOrder {
       public Vector3 TargetPos { get; }
   }

   public class PointMoveOrder : IMoveOrder {
       public PointMoveOrder(Vector3 getTarget) {
           TargetPos = getTarget;
       }
       public Vector3 TargetPos { get; }
       public override string ToString() {
           return $"Point move order: {TargetPos}";
       }
   }

   public class TrackMoveOrder : IMoveOrder {
       private readonly GameUnit _trackedUnit;
       public TrackMoveOrder(GameUnit trackedUnit) {
           _trackedUnit = trackedUnit;
       }
       public Vector3 TargetPos => VectorTools.HorizontalComponent(_trackedUnit.transform.position);
       public override string ToString() {
           return $"Tracking move order: {_trackedUnit.name}";
       }
   }

   /*
    * ATTACK ORDER
    */
   
   public class AttackOrder {
       
   }

   public interface IAttackTarget {
       public Vector3 CurrentPosition();
       public Vector3 PredictedPosition(float timeOffset);
   }

   public class PointTarget : IAttackTarget {
       private readonly Vector3 _point;

       public PointTarget(Vector3 point) {
           _point = point;
       }

       public Vector3 CurrentPosition() {
           return _point;
       }

       public Vector3 PredictedPosition(float timeOffset) {
           return _point;
       }
   }

   public class UnitTarget : IAttackTarget {
       private readonly GameUnit _target;
       public UnitTarget(GameUnit target) {
           _target = target;
       }

       public Vector3 CurrentPosition() {
           return _target.Rigidbody.position;
       }

       public Vector3 PredictedPosition(float timeOffset) {
           return _target.Rigidbody.position + _target.Rigidbody.velocity * timeOffset;
       }
   }
}