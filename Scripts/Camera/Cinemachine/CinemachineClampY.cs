using UnityEngine;
using Cinemachine;

/// <summary>
/// An add-on module for Cinemachine Virtual Camera that clamps the Y camera movement.
/// </summary>
[AddComponentMenu("")] // Hide in menu
public class CinemachineClampY : CinemachineExtension
{
    public float minY = 19.5f;

    protected override void PostPipelineStageCallback(CinemachineVirtualCameraBase vcam, CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
    {
        if (stage == CinemachineCore.Stage.Body)
        {
            var pos = state.RawPosition;
            if (pos.y < 19.5f)
            {
                pos.y = minY;
            }
            state.RawPosition = pos;
        }
    }
}
