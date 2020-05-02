using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;

namespace Battle.Combat
{
    /// <summary>
    /// Ok sure, obviously I would only do this once and not every frame if I was going about things properly.
    /// For example, by using an 'Init' component to flag ships that are freshly created.
    /// 
    /// But right now, it's late, and I want to see my ships colored properly when spawned.
    /// </summary>
    public class SetTeamColorsSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities.ForEach(
                (ref MaterialColor color, in Team team) =>
                {
                    float4 teamColor;
                    switch (team.ID)
                    {
                        default: teamColor = new float4(1.0f, 1.0f, 1.0f, 1.0f); break;
                        case 1: teamColor = new float4(0.5f, 0.7f, 1.0f, 1.0f); break;
                        case 2: teamColor = new float4(1.0f, 0.0f, 0.0f, 1.0f); break;
                    }
                    color.Value = teamColor;
                }
                ).Schedule();
        }
    }
}
