using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
public class CameraManager : MonoBehaviour
{
    [SerializeField] private CinemachineImpulseSource impulseSource;

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

    private void CameraShake()
    {
        impulseSource.GenerateImpulse();
    }

    private void CameraShake(Transform pNull)
    {
        impulseSource.GenerateImpulse();
    }
}
