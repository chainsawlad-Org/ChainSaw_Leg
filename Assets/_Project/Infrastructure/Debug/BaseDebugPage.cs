using UnityEngine;
using UnityDebugSheet;
using System.Collections;

public sealed class BaseDebugPage : DefaultDebugPageBase
{
    protected override string Title { get; } = "Debug Page";
    private Transform playerTransform;

    public void Initialize(Transform playerTransform)
    {
        this.playerTransform = playerTransform;
    }

    public override IEnumerator Initialize()
    {
        AddSlider(Camera.main.orthographicSize, 15f, 30f, "Camera zoom", valueChanged: x => Camera.main.orthographicSize = x);
        AddSlider(playerTransform.localScale.x, 1f, 3f, "Player size", valueChanged: x => playerTransform.localScale = new Vector3(x, x, 1f));
        AddSlider(9f, 9f, 30f, "Player speed", valueChanged: x => playerTransform.GetComponent<PlayerMovement>().SetSpeed(x));

        yield break;
    }
}
