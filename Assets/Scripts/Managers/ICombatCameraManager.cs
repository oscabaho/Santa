public interface ICombatCameraManager
{
    void SwitchToMainCamera();
    void SwitchToTargetSelectionCamera();
    void SetCombatCameras(Unity.Cinemachine.CinemachineCamera main, Unity.Cinemachine.CinemachineCamera target);
}