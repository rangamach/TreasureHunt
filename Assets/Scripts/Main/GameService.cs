using UnityEngine;

public class GameService : GenericMonoSingleton<GameService>
{
    //Services:
    public PlayerService PlayerService { get; private set; }
    //public SoundService SoundService { get; private set; }

    [Header("Services:")]
    //[SerializeField] private UIService uiService;
    //public UIService UIService() => this.uiService;

    //Scriptable Objects:
    [Header("Scriptable Objects:")]
    //[SerializeField] private SoundSO soundSO;

    [SerializeField] private AudioSource bgAudio;
    [SerializeField] private AudioSource sfxAudio;
    [SerializeField] private PlayerView playerView;

    public bool onMobile { get; private set; }


    protected override void Awake()
    {
        base.Awake();
    }
    private void Start()
    {
        CreateServices();
    }

    private void CreateServices()
    {
        this.PlayerService = new PlayerService(playerView);
        //this.SoundService = new SoundService(soundSO, bgAudio, sfxAudio);
    }
}
