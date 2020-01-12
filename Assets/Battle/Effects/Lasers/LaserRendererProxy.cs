using Unity.Entities;
using Unity.Rendering;
using UnityEngine;

namespace Battle.Effects
{
    [RequiresEntityConversion]
    public class LaserRendererProxy : MonoBehaviour, IConvertGameObjectToEntity
    {
        public Material Material;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, new LaserRenderer());
            dstManager.AddSharedComponentData(entity, new RenderMesh() {
                mesh = new Mesh(), castShadows = UnityEngine.Rendering.ShadowCastingMode.Off, material = Material, receiveShadows = false, subMesh = 0 });
        }
    }
}