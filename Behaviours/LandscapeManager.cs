using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

public class LandscapeManager : MonoBehaviour
{
    [SerializeField]
    private Mesh mesh;
    [SerializeField]
    private Material meshMaterial;

    public int3 worldDimensions;
    private int3 lastWorldDimensions;

    public float3 blockDimensions;
    private float3 lastBlockDimensions;

    private EntityManager entityManager;
    private EntityArchetype entityArchetype;
    private Entity gameObjectEntity;
    private Parent parent;

    public void Start()
    {
        entityManager = World.Active.EntityManager;
        entityArchetype = entityManager.CreateArchetype(
            typeof(Translation),
            typeof(RenderMesh),
            typeof(NonUniformScale),
            typeof(LocalToWorld)
        );
        gameObjectEntity = entityManager.CreateEntity(typeof(Translation), typeof(LocalToWorld));
        entityManager.SetComponentData(gameObjectEntity, new Translation { Value = transform.position });
        parent = new Parent { Value = gameObjectEntity };
    }

    private void Update()
    {
        entityManager.SetComponentData(gameObjectEntity, new Translation { Value = transform.position });
        var worldEquality = worldDimensions == lastWorldDimensions;
        var blockEquality = blockDimensions == lastBlockDimensions;
        if (
            !worldEquality.x || !worldEquality.y || !worldEquality.z ||
            !blockEquality.x || !blockEquality.y || !blockEquality.z
            )
            GenerateWorld();
    }

    private void GenerateWorld()
    {
        if (int.MaxValue < ((long)worldDimensions.x * (long)worldDimensions.y * (long)worldDimensions.z))
        {
            Debug.LogError("Oversized area, reduce dimensions");
            return;
        }

        var query = entityManager.CreateEntityQuery(typeof(RenderMesh), typeof(LocalToWorld), typeof(Translation));
        entityManager.DestroyEntity(query);

        var entities = new NativeArray<Entity>(worldDimensions.x * worldDimensions.y * worldDimensions.z, Allocator.Temp);
        entityManager.CreateEntity(entityArchetype, entities);

        var renderMeshData = new RenderMesh { mesh = mesh, material = meshMaterial };
        var scale = new NonUniformScale { Value = blockDimensions };

        int i = 0;
        for (int h = 0; h < worldDimensions.y; h++)
            for (int w = 0; w < worldDimensions.x; w++)
                for (int l = 0; l < worldDimensions.z; l++)
                {
                    var entity = entities[i];

                    entityManager.SetComponentData(entity, scale);

                    var pos = new float3(
                        (float)w * scale.Value.x,
                        (float)h * scale.Value.y,
                        (float)l * scale.Value.z);

                    entityManager.SetComponentData(entity, new Translation { Value = pos });


                    entityManager.SetSharedComponentData(entity, renderMeshData);

                    i++;
                }

        lastWorldDimensions = worldDimensions;
        lastBlockDimensions = blockDimensions;

        entities.Dispose();
    }
}
