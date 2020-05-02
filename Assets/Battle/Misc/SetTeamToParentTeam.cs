using Battle.Combat;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;

namespace Battle.Equipment
{
    /// <summary>
    /// Essentially a workaround until I implement a more permanent solution using initialisation tag components.
    /// </summary>
    [UpdateBefore(typeof(AI.AISystemGroup))]
    public class SetTeamToParentTeam : SystemBase
    {
        protected override void OnCreate()
        {
            //Enabled = false;
        }

        public EntityQuery Query;

        protected override void OnUpdate()
        {
            Entities
                .WithAll<Team>()
                .ForEach(
                (Entity e, in Parent parent) =>
                {
                    if (HasComponent<Team>(parent.Value))
                        SetComponent(e, GetComponent<Team>(parent.Value));
                }
                )
                .Schedule();
        }
    }
}
