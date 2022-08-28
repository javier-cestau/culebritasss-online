#if !UNITY_SERVER
using Client.Data_components;
#endif
using Server.Data_components;
using UnityEngine;
using Unity.Entities;
using Unity.NetCode;

public class ClientServerConnectionHandler : MonoBehaviour
{

    void Awake()
    { 
        //sets the component server data in server world(dots)
        //ClientServerConnectionControl (server) will run in server world
        //it will pick up this component and use it to listen on the port
        foreach (var world in World.All)
        {
            //we cycle through all the worlds, and if the world has ServerSimulationSystemGroup
            //we move forward (because that is the server world)
            if (world.GetExistingSystem<ServerSimulationSystemGroup>() != null)
            { 
                world.EntityManager.CreateEntity(typeof(InitializeServerComponent));
            }
#if !UNITY_SERVER
            if (world.GetExistingSystem<ClientSimulationSystemGroup>() != null)
            { 
                var entity = world.EntityManager.CreateEntity();
                world.EntityManager.AddComponentData(entity, new InitializeClientComponent() { playerName = "hola" });
            }
#endif
        } 
    }
}