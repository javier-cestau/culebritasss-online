using UnityEngine;
using UnityEngine.UIElements;

public class LocalGamesFinder : MonoBehaviour
{
    //We will be pulling in our SourceAsset from TitleScreenUI GameObject so we can reference Visual Elements
    public UIDocument m_TitleUIDocument;

    //When we grab the rootVisualElement of our UIDocument we will be able to query the TitleScreenManager Visual Element
    private VisualElement m_titleScreenManagerVE;

    //We will query for our TitleScreenManager cVE by its name "TitleScreenManager"
    private TitleScreenManager m_titleScreenManagerClass;
    
    //This is our ListItem uxml that we will drag to the public field
    //We need a reference to the uxml so we can build it in makeItem
    public VisualTreeAsset m_localGameListItemAsset;

    //These variables are used in Update() to pace how often we check for GameObjects
    public float perSecond = 1.0f;
    private float nextTime = 0; 

    void OnEnable()
    {
        //Here we grab the SourceAsset rootVisualElement
        //This is a MAJOR KEY, really couldn't find this key step in information online
        //If you want to reference your active UI in a script make a public UIDocument variable and 
        //then call rootVisualElement on it, from there you can query the Visual Element tree by names
        //or element types
        m_titleScreenManagerVE = m_TitleUIDocument.rootVisualElement;
        //Here we grab the TitleScreenManager by querying by name
        m_titleScreenManagerClass = m_titleScreenManagerVE.Q<TitleScreenManager>("TitleScreenManager");
        //From within TitleScreenManager we query local-games-list by name
         m_titleScreenManagerVE.Q<Button>("join-local-game").RegisterCallback<ClickEvent>(ev =>
        { 
            ClickedJoinGame();
        });
    }
    
    void ClickedJoinGame()
    {
        //We then call EnableJoinScreen on our TitleScreenManager cVE (which displays JoinGameScreen)
#if UNITY_EDITOR
        m_titleScreenManagerClass.ClickedHostGame();
#else
        m_titleScreenManagerClass.ClickedJoinGame();
#endif
    }
    
}