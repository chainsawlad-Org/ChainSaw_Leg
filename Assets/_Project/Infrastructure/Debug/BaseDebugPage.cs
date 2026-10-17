using UnityEngine;
using UnityDebugSheet;
using System.Collections;
using ChainSawLeg.Features.Exploration;

public sealed class BaseDebugPage : DefaultDebugPageBase
{
    protected override string Title { get; } = "Debug Page";
    private Transform playerTransform;
    private PlayerKick playerKick;

    public void Initialize(Transform playerTransform, PlayerKick playerKick)
    {
        this.playerTransform = playerTransform;
        this.playerKick = playerKick;
    }

    public override IEnumerator Initialize()
    {
        AddSlider(Camera.main.orthographicSize, 15f, 30f, "Camera zoom", valueChanged: x => Camera.main.orthographicSize = x);
        AddSlider(playerTransform.localScale.x, 1f, 3f, "Player size", valueChanged: x => playerTransform.localScale = new Vector3(x, x, 1f));
        AddSlider(9f, 9f, 30f, "Player speed", valueChanged: x => playerTransform.GetComponent<PlayerMovement>().SetSpeed(x));
        AddSlider(playerKick.kickForcePower, 0f, 100f, "Kick power", valueChanged: x => playerKick.SetKickPowerForce(x));
        yield break;
    }
}
