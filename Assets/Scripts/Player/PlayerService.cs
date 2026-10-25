using UnityEngine;

public class PlayerService
{
    private PlayerController playerController;

    public PlayerService(PlayerView view)
    {
        playerController = new PlayerController(view);
    }
}
