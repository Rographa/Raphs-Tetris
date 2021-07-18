using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InputManager : MonoBehaviour
{
    [SerializeField] float delayPressedDown = 0.2f;
    [SerializeField] float delayPressedHorizontal = 0.5f;
    public delegate void InputEvents();
    public static event InputEvents MoveRight, MoveLeft, Rotate, MoveDown, EscPressed;    
    float downStarted, rightStarted, leftStarted;
    float downCurrent, rightCurrent, leftCurrent;

    Coroutine multipleCalls;
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
    void CheckInputs()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            EscPressed?.Invoke();
        }
        if (Time.timeScale == 1)
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                leftCurrent = leftStarted = Time.time;
                MoveLeft?.Invoke();
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                rightCurrent = rightStarted = Time.time;
                MoveRight?.Invoke();
            }
            else if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                Rotate?.Invoke();
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                downCurrent = downStarted = Time.time;
                MoveDown?.Invoke();
            }

            if (Input.GetKey(KeyCode.DownArrow))
            {
                downCurrent += Time.deltaTime;
                if (downCurrent - downStarted > 0.5f)
                {
                    StopMultipleCalls();
                    multipleCalls = StartCoroutine(MultipleCalls(MoveDown, delayPressedDown));
                }
            }
            else if (Input.GetKey(KeyCode.RightArrow))
            {
                rightCurrent += Time.deltaTime;
                if (rightCurrent - rightStarted > 0.5f)
                {
                    StopMultipleCalls();
                    multipleCalls = StartCoroutine(MultipleCalls(MoveRight, delayPressedHorizontal));
                }
            }
            else if (Input.GetKey(KeyCode.LeftArrow))
            {
                leftCurrent += Time.deltaTime;
                if (leftCurrent - leftStarted > 0.5f)
                {
                    StopMultipleCalls();
                    multipleCalls = StartCoroutine(MultipleCalls(MoveLeft, delayPressedHorizontal));
                }
            }

            if (Input.GetKeyUp(KeyCode.DownArrow))
            {
                downCurrent = downStarted = 0;
                StopMultipleCalls();
            }
            else if (Input.GetKeyUp(KeyCode.RightArrow))
            {
                rightCurrent = rightStarted = 0;
                StopMultipleCalls();
            }
            else if (Input.GetKeyUp(KeyCode.RightArrow))
            {
                leftCurrent = leftStarted = 0;
                StopMultipleCalls();
            }
        }
    }
    public void StopMultipleCalls()
    {
        if (multipleCalls != null) StopCoroutine(multipleCalls);
    }
    public void StopMultipleCalls(Transform pNull)
    {
        if (multipleCalls != null) StopCoroutine(multipleCalls);
    }
    IEnumerator MultipleCalls(InputEvents _event, float delay)
    {
        while (true)
        {
            _event?.Invoke();
            yield return new WaitForSeconds(delay);
        }
    }
}
