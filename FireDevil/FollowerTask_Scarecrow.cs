using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FireDevil
{
    // TODO: scarecrow
    // - every follower will do this task once
    // - follower will go there
    // - follower will do animation
    // - does not wait for follower to arrive
    // - does not wait for bird catch animation to finish
    //
    // need to check: Scarecrow.InBirdRoutine
    // 
    // Lesezeichen1		void Structures_FarmerStation.GetAvailableTasks(ScheduledActivity activity, SortedList<float, FollowerTask> tasks)
    // Lesezeichen2		void FollowerTask_Farm.ProgressTask()
    // Lesezeichen3		void Structures_Scarecrow.EmptyTrap()
    // Lesezeichen3		List<FollowerTask> FollowerBrain.GetDesiredTask_Work(FollowerLocation location)
    // Lesezeichen4		void Follower.OnTaskChanged(FollowerTask newTask, FollowerTask oldTask)
    // 

    public class FollowerTask_Scarecrow : FollowerTask
    {
        private FollowerLocation _Location;
        private int _ScareCrowID;
        private Structures_Scarecrow _ScareCrow;
        //private Follower _follower;
        private float _Progress;
        private float _GameTimeSinceLastProgress;

        public FollowerTask_Scarecrow(int scareCrowID)
        {
            this._ScareCrowID = scareCrowID;
            this._ScareCrow = StructureManager.GetStructureByID<Structures_Scarecrow>(scareCrowID);
            this._Location = this._ScareCrow.Data.Location;
        }

        public override int GetSubTaskCode() => 0;
        public override float Priorty => 23f;
        public override FollowerTaskType Type => FollowerTaskType.Farm;
        public override FollowerLocation Location => _Location;

        public override PriorityCategory GetPriorityCategory(FollowerRole FollowerRole, WorkerPriority WorkerPriority, FollowerBrain brain)
        {
            return FollowerRole switch
            {
                FollowerRole.Worker or FollowerRole.Farmer => PriorityCategory.Medium,
                _ => PriorityCategory.Low,
            };
        }

        /// <summary>
        /// Called when follower decides to start this task.
        /// </summary>
        public override void ClaimReservations()
        {
            this._ScareCrow = StructureManager.GetStructureByID<Structures_Scarecrow>(this._ScareCrowID);
            if (this._ScareCrow != null)
                this._ScareCrow.ReservedForTask = true;
        }

        /// <summary>
        /// Called when follower changes task.
        /// </summary>
        public override void ReleaseReservations()
        {
            this._ScareCrow = StructureManager.GetStructureByID<Structures_Scarecrow>(this._ScareCrowID);
            if (this._ScareCrow != null)
                this._ScareCrow.ReservedForTask = false;
        }

        /// <summary>
        /// 
        /// </summary>
        public override void OnStart()
        {
            SetState(FollowerTaskState.GoingTo);
        }

        public override void ProgressTask()
        {
            this._ScareCrow = StructureManager.GetStructureByID<Structures_Scarecrow>(this._ScareCrowID);
            if (this._ScareCrow == null)
            {
                End();
                return;
            }

            this._Progress += this._GameTimeSinceLastProgress;
            this._GameTimeSinceLastProgress = 0f;
            if (this._Progress >= 4f)
            {
                this._Progress = 0f;
                if (this._ScareCrow.HasBird)
                {
                    Scarecrow.Scarecrows.FirstOrDefault(f => f.Brain == this._ScareCrow)?.OpenTrap();
                    this._ScareCrow.EmptyTrap();
                    InventoryItem.Spawn(InventoryItem.ITEM_TYPE.MEAT, 1, this._ScareCrow.Data.Position, 4f);
                }
                Loop();
            }
        }

        private void Loop()
        {
            End();
            //Structures_Waste nextWaste = this.GetNextWaste();
            //if (nextWaste == null)
            //{
            //    base.End();
            //    return;
            //}
            //base.ClearDestination();
            //this._wasteID = nextWaste.Data.ID;
            //nextWaste.ReservedForTask = true;
            //base.SetState(FollowerTaskState.GoingTo);
        }

        public override Vector3 UpdateDestination(Follower follower)
        {
            this._ScareCrow = StructureManager.GetStructureByID<Structures_Scarecrow>(this._ScareCrowID);
            if (this._ScareCrow == null)
            {
                End();
                return default;
            }
            return this._ScareCrow.Data.Position + new Vector3(-0.2f, 0f, 0f);
        }
        
        public override void OnDoingBegin(Follower follower)
        {
            this._ScareCrow = StructureManager.GetStructureByID<Structures_Scarecrow>(this._ScareCrowID);
            if (this._ScareCrow == null)
            {
                End();
                return;
            }

            //this._follower = follower;
            follower.FacePosition(_ScareCrow.Data.Position);
            follower.TimedAnimation("action", 3.5f, ProgressTask); // TODO: don't use timed
        }

        public override void TaskTick(float deltaGameTime)
        {
            if (this.State == FollowerTaskState.Doing)
            {
                this._GameTimeSinceLastProgress += deltaGameTime;
                //ProgressTask();
            }
        }

        //OnArrive?
    }
}
