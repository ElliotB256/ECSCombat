using Unity.Entities;

namespace Battle.Combat
{
    /// <summary>
    /// Buffer system used to delete dead (less than zero health) entities from the world.
    /// </summary>
    public class DeleteDeadEntitiesBuffer : EntityCommandBufferSystem
    {
    }
}
