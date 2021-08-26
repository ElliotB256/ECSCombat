using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Battle.Combat
{
    public class UpdateGameTimeDeltaSystem : SystemBase
    {

        protected override void OnCreate()
        {
            if( !HasSingleton<GameTimeDelta>() )
            {
                Entity singleton = EntityManager.CreateEntity( typeof(GameTimeDelta) );
                EntityManager.SetComponentData( singleton , new GameTimeDelta{
                    dT          = 0 ,
                    RateFactor  = 1 ,
                    Paused      = false,
                } );
            }
        }

        protected override void OnUpdate()
        {
            bool faster = Input.GetKeyDown(KeyCode.Equals) || Input.GetKeyDown(KeyCode.Plus) || Input.GetKeyDown(KeyCode.KeypadPlus);
            bool slower = Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.KeypadMinus);
            bool pause = Input.GetKeyDown(KeyCode.Space);

            var singleton = GetSingleton<GameTimeDelta>();
            {
                if (faster)
                    singleton.RateFactor = math.min(2f, singleton.RateFactor + 0.5f);
                if (slower)
                    singleton.RateFactor = math.max(0.5f, singleton.RateFactor - 0.5f);
                if (pause)
                    singleton.Paused = !singleton.Paused;

                singleton.dT = Time.DeltaTime * singleton.RateFactor * (singleton.Paused ? 0f : 1f);
            }
            SetSingleton( singleton );

            Entities
                .WithName($"update_{nameof(GameTimeMaterialProperty)}_job")
                .ForEach( ( ref GameTimeMaterialProperty material ) => material.Value += singleton.dT )
                .ScheduleParallel();
        }

    }
}
