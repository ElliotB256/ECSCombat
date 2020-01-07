using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Battle.Effects
{
    /// <summary>
    /// Hacky, boot strapped for now. So I can draw from GL quickly.
    /// </summary>
    public class RenderLaserComponent : MonoBehaviour
    {
        public NativeArray<LaserBeamEffect> Beams;
        public JobHandle LaserSystemJob;

        static Material lineMaterial;
        static void CreateLineMaterial()
        {
            if (!lineMaterial)
            {
                // Unity has a built-in shader that is useful for drawing
                // simple colored things.
                Shader shader = Shader.Find("Hidden/Internal-Colored");
                lineMaterial = new Material(shader);
                lineMaterial.hideFlags = HideFlags.HideAndDontSave;
                lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
                lineMaterial.SetInt("_ZWrite", 0);
            }
        }

        public void OnRenderObject()
        {
            if (Beams == null)
                return;

            var ColorA = Color.red;
            var ColorB = Color.yellow;
            LaserSystemJob.Complete();

            // I'm not going to bother to make this code pretty, because its temporary.
            // It draws colored line segments which move towards the target.
            CreateLineMaterial();
            lineMaterial.SetPass(0);
            GL.PushMatrix();
            GL.MultMatrix(transform.localToWorldMatrix);
            GL.Begin(GL.LINES);
            var segmentLength = 0.1f;
            var wavelength = 1f;
            for (int index = 0; index < Beams.Length; index++)
            {
                var normalisedLifetime = (Beams[index].lifetime) / 0.2f;
                
                var beamStart = Beams[index].start;
                var beamEnd = Beams[index].end;
                var beamDiff = math.normalize(beamEnd - beamStart) * segmentLength;
                var segments = Mathf.Ceil(math.length(beamEnd - beamStart)/segmentLength);
                for (int i = 0; i < segments; i++)
                {
                    var phase = i * segmentLength / wavelength + normalisedLifetime * 1f;
                    // alternating yellow/red laser beam.
                    var color = Color.Lerp(ColorA, ColorB, math.abs(phase % 2f - 1f));
                    //laser beam fades in over time.
                    color = Color.Lerp(color, Color.white, Mathf.Clamp(normalisedLifetime, 0f, 0.5f));
                    GL.Color(color);
                    GL.Vertex(beamStart + beamDiff * (i-1));
                    GL.Vertex(beamStart + beamDiff * (i));
                }
            }
            GL.End();
            GL.PopMatrix();
        }
    }
}
