using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InputManager : MonoBehaviour
{
    [SerializeField] private float delayPressedDown = 0.2f;
    [SerializeField] private float delayPressedHorizontal = 0.5f;
    public delegate void InputEvents();
    public static event InputEvents MoveRight, MoveLeft, Rotate, MoveDown, EscPressed;
    private float _downStarted, _rightStarted, _leftStarted;
    private float _downCurrent, _rightCurrent, _leftCurrent;

    private  Coroutine _multipleCalls;
    private void OnEnable()
    {
        Tetromino.OnFinished += StopMultipleCalls;
    }
    private void OnDisable()
    {
        Tetromino.OnFinished -= StopMultipleCalls;
    }
    private void Update()
    {
        CheckInputs();
    }
    private void CheckInputs()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            EscPressed?.Invoke();
        }
        
        if (Time.timeScale != 1) return;
        
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            _leftCurrent = _leftStarted = Time.time;
            MoveLeft?.Invoke();
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            _rightCurrent = _rightStarted = Time.time;
            MoveRight?.Invoke();
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            Rotate?.Invoke();
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            _downCurrent = _downStarted = Time.time;
            MoveDown?.Invoke();
        }

        if (Input.GetKey(KeyCode.DownArrow))
        {
            _downCurrent += Time.deltaTime;
            if (_downCurrent - _downStarted > 0.5f)
            {
                StopMultipleCalls();
                _multipleCalls = StartCoroutine(MultipleCalls(MoveDown, delayPressedDown));
            }
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            _rightCurrent += Time.deltaTime;
            if (_rightCurrent - _rightStarted > 0.5f)
            {
                StopMultipleCalls();
                _multipleCalls = StartCoroutine(MultipleCalls(MoveRight, delayPressedHorizontal));
            }
        }
        else if (Input.GetKey(KeyCode.LeftArrow))
        {
            _leftCurrent += Time.deltaTime;
            if (_leftCurrent - _leftStarted > 0.5f)
            {
                StopMultipleCalls();
                _multipleCalls = StartCoroutine(MultipleCalls(MoveLeft, delayPressedHorizontal));
            }
        }

        if (Input.GetKeyUp(KeyCode.DownArrow))
        {
            _downCurrent = _downStarted = 0;
            StopMultipleCalls();
        }
        else if (Input.GetKeyUp(KeyCode.RightArrow))
        {
            _rightCurrent = _rightStarted = 0;
            StopMultipleCalls();
        }
        else if (Input.GetKeyUp(KeyCode.RightArrow))
        {
            _leftCurrent = _leftStarted = 0;
            StopMultipleCalls();
        }
    }

    private void StopMultipleCalls()
    {
        if (_multipleCalls != null) StopCoroutine(_multipleCalls);
    }

    private void StopMultipleCalls(Transform pNull)
    {
        if (_multipleCalls != null) StopCoroutine(_multipleCalls);
    }
    private IEnumerator MultipleCalls(InputEvents @event, float delay)
    {
        while (true)
        {
            @event?.Invoke();
            yield return new WaitForSeconds(delay);
        }
    }
}
