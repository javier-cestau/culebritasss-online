using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class TitleScreenManager : VisualElement
{
    VisualElement m_TitleScreen;
    
    public new class UxmlFactory : UxmlFactory<TitleScreenManager, UxmlTraits> { }

    public TitleScreenManager()
    {
        this.RegisterCallback<GeometryChangedEvent>(OnGeometryChange);
    }

    void OnGeometryChange(GeometryChangedEvent evt)
    {
        m_TitleScreen = this.Q("TitleScreen");

        this.UnregisterCallback<GeometryChangedEvent>(OnGeometryChange);
    }
    

  public void ClickedHostGame()
    {
        //When we click "Host Game" that means we want to be both a server and a client
        //So we will trigger both functions for the server and client
        ServerLauncher();
        ClientLauncher();

        //This function will trigger the MainScene
        StartGameScene();
    }

    public void ClickedJoinGame()
    {
        //When we click 'Join Game" that means we want to only be a client
        //So we do not trigger ServerLauncher
        ClientLauncher();

        //This function triggers the MainScene
        StartGameScene();
    }



    public void ServerLauncher()
    {
        //CreateServerWorld is a method provided by ClientServerBootstrap for precisely this reason
        //Manual creation of worlds

        //We must grab the DefaultGameObjectInjectionWorld first as it is needed to create our ServerWorld
        var world = World.DefaultGameObjectInjectionWorld;
#if !UNITY_CLIENT || UNITY_SERVER || UNITY_EDITOR
        ClientServerBootstrap.CreateServerWorld(world, "ServerWorld");

#endif
    }

    public void ClientLauncher()
    {
        //First we grab the DefaultGameObjectInjectionWorld because it is needed to create ClientWorld
        var world = World.DefaultGameObjectInjectionWorld;

        //We have to account for the fact that we may be in the Editor and using ThinClients
        //We initially start with 1 client world which will not change if not in the editor
        int numClientWorlds = 1;
        int totalNumClients = numClientWorlds;

        //If in the editor we grab the amount of ThinClients from ClientServerBootstrap class (it is a static variable)
        //We add that to the total amount of worlds we must create
#if UNITY_EDITOR
        int numThinClients = ClientServerBootstrap.RequestedNumThinClients;
        totalNumClients += numThinClients;
#endif
        //We create the necessary number of worlds and append the number to the end
        for (int i = 0; i < numClientWorlds; ++i)
        {
            ClientServerBootstrap.CreateClientWorld(world, "ClientWorld" + i);
        }
#if UNITY_EDITOR
        for (int i = numClientWorlds; i < totalNumClients; ++i)
        {
            var clientWorld = ClientServerBootstrap.CreateClientWorld(world, "ClientWorld" + i);
            clientWorld.EntityManager.CreateEntity(typeof(ThinClientComponent));
        }
#endif
    }

    void StartGameScene()
    {
        //Here we trigger MainScene
#if UNITY_EDITOR
        if(Application.isPlaying)
#endif
            SceneManager.LoadSceneAsync("MainScene");
#if UNITY_EDITOR
        else
            Debug.Log("Loading: " + "MainScene");
#endif
    }
}
