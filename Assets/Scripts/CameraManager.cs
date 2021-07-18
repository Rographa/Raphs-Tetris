using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
public class CameraManager : MonoBehaviour
{
    [SerializeField] CinemachineImpulseSource impulseSource;

    private void OnEnable()
    {
        Tetromino.OnFinished += CameraShake;
        GameManager.OnLineCompleted += CameraShake;
    }
    private void OnDisable()
    {
        Tetromino.OnFinished -= CameraShake;
        GameManager.OnLineCompleted -= CameraShake;
    }
    public void CameraShake()
    {
        impulseSource.GenerateImpulse();
    }
    public void CameraShake(Transform pNull)
    {
        impulseSource.GenerateImpulse();
    }
}
