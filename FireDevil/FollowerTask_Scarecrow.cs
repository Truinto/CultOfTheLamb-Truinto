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
        private FollowerLocation _location;
        private int _scareCrowID;
        private Structures_Scarecrow _scareCrow;
        //private Follower _follower;
        private float _progress;
        private float _gameTimeSinceLastProgress;

        public FollowerTask_Scarecrow(int scareCrowID)
        {
            this._scareCrowID = scareCrowID;
            this._scareCrow = StructureManager.GetStructureByID<Structures_Scarecrow>(scareCrowID);
            this._location = this._scareCrow.Data.Location;
        }

        public override int GetSubTaskCode() => 0;
        public override float Priorty => 23f;
        public override FollowerTaskType Type => FollowerTaskType.Farm;
        public override FollowerLocation Location => _location;

        public override PriorityCategory GetPriorityCategory(FollowerRole FollowerRole, WorkerPriority WorkerPriority, FollowerBrain brain)
        {
            switch (FollowerRole)
            {
                case FollowerRole.Worker:
                case FollowerRole.Farmer:
                    return PriorityCategory.Medium;
                default:
                    return PriorityCategory.Low;
            }
        }

        /// <summary>
        /// Called when follower decides to start this task.
        /// </summary>
        public override void ClaimReservations()
        {
            this._scareCrow = StructureManager.GetStructureByID<Structures_Scarecrow>(this._scareCrowID);
            if (this._scareCrow != null)
                this._scareCrow.ReservedForTask = true;
        }

        /// <summary>
        /// Called when follower changes task.
        /// </summary>
        public override void ReleaseReservations()
        {
            this._scareCrow = StructureManager.GetStructureByID<Structures_Scarecrow>(this._scareCrowID);
            if (this._scareCrow != null)
                this._scareCrow.ReservedForTask = false;
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
            this._scareCrow = StructureManager.GetStructureByID<Structures_Scarecrow>(this._scareCrowID);
            if (this._scareCrow == null)
            {
                End();
                return;
            }

            this._progress += this._gameTimeSinceLastProgress;
            this._gameTimeSinceLastProgress = 0f;
            if (this._progress >= 4f)
            {
                this._progress = 0f;
                if (this._scareCrow.HasBird)
                {
                    Scarecrow.Scarecrows.FirstOrDefault(f => f.Brain == this._scareCrow)?.OpenTrap();
                    this._scareCrow.EmptyTrap();
                    InventoryItem.Spawn(InventoryItem.ITEM_TYPE.MEAT, 1, this._scareCrow.Data.Position, 4f);
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
            this._scareCrow = StructureManager.GetStructureByID<Structures_Scarecrow>(this._scareCrowID);
            if (this._scareCrow == null)
            {
                End();
                return default;
            }
            return this._scareCrow.Data.Position + new Vector3(-0.2f, 0f, 0f);
        }
        
        public override void OnDoingBegin(Follower follower)
        {
            this._scareCrow = StructureManager.GetStructureByID<Structures_Scarecrow>(this._scareCrowID);
            if (this._scareCrow == null)
            {
                End();
                return;
            }

            //this._follower = follower;
            follower.FacePosition(_scareCrow.Data.Position);
            follower.TimedAnimation("action", 3.5f, () => ProgressTask()); // TODO: don't use timed
        }

        public override void TaskTick(float deltaGameTime)
        {
            if (this.State == FollowerTaskState.Doing)
            {
                this._gameTimeSinceLastProgress += deltaGameTime;
                //ProgressTask();
            }
        }

        //OnArrive?
    }
}
