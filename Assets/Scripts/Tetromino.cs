using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public class Tetromino : MonoBehaviour
{   
    [SerializeField] private float moveDownCooldown = 0.75f;
    [SerializeField] private float rotDuration = 0.2f;
    [SerializeField] private Transform rotPos;
    [SerializeField] private MeshRenderer[] mrs;
    [SerializeField] private Material[] materials;
    private bool _rotating;
    private bool _ableToMove;
    private bool _finished;
    private readonly List<Transform> _pieces = new List<Transform>();
    private readonly List<Transform> _xPieces = new List<Transform>();
    public delegate void TetrominoEvents(Transform obj);
    public static event TetrominoEvents OnFinished, OnPopup;

    private Coroutine _moveDownRoutine;
    private void Awake()
    {
        SetupRandomColor();
    }   
    private void Start()
    {
        Setup();
    }
    private void OnEnable() => SubscribeInputEvents();
    
    private void OnDisable() => UnsubscribeInputEvents();

    private void Setup()
    {
        GetPieces();
        CheckXPieces();
        transform.localScale = Vector3.zero;
        transform.DOScale(1, 0.5f).SetEase(Ease.OutBounce).OnComplete(Popup);
        AudioManager.Instance.PlayClip(AudioManager.Clips.TETRO_POPUP, true);
        //Invoke("Popup", 0.5f);
        _moveDownRoutine = StartCoroutine(MoveDownRoutine());
    }
    public void CheckChildren()
    {
        StartCoroutine(CheckChildrenRoutine());
    }

    private IEnumerator CheckChildrenRoutine()
    {
        yield return new WaitForEndOfFrame();
        if (transform.childCount <= 1)
        {
            Destroy(this.gameObject);
        }
    }

    private void Popup()
    {
        RenderLines();        
        OnPopup?.Invoke(transform);
    }

    private void GetPieces()
    {
        foreach (Transform children in transform)
        {
            if (children == rotPos) continue;
            _pieces.Add(children);
        }
    }

    private void SetupRandomColor()
    {
        var mat = materials[Random.Range(0, materials.Length)];
        foreach (var mr in mrs)
        {
            mr.material = mat;
        }
    }

    private void Finish()
    {
        if (AbleToMove(Vector3.down))
        {
            if (_moveDownRoutine is null)
            {
                _moveDownRoutine = StartCoroutine(MoveDownRoutine(false));
                return;
            }
        }
        if (_finished) return;
        
        _finished = true;            
        UnsubscribeInputEvents();
        OnFinished?.Invoke(transform);
        StartCoroutine(BlobEffect());
        LineManager.Instance.HideLines();
    }

    private IEnumerator BlobEffect()
    {
        transform.DOShakePosition(0.2f, 0.1f);
        transform.DOScale(0.8f, 0.05f);
        yield return new WaitForSeconds(0.05f);
        transform.DOScale(1.2f, 0.1f);
        yield return new WaitForSeconds(0.1f);
        transform.DOScale(1, 0.05f);
    }

    private void Move(Vector3 dir)
    {
        if (_finished || !_ableToMove) return;
        
        transform.Translate(dir, Space.World);
        CheckXPieces();
        RenderLines();
    }

    private void MoveRight()
    {
        if (_rotating) return;
        if (AbleToMove(Vector3.right)) Move(Vector3.right);
    }

    private void MoveLeft()
    {
        if (_rotating) return;
        if (AbleToMove(Vector3.left)) Move(Vector3.left);
    }

    private void MoveDown()
    {
        if (_rotating) return;
        if (AbleToMove(Vector3.down)) Move(Vector3.down);
        else
        {
            Finish();
        }
    }

    private void Rotate()
    {
        if (!_rotating && AbleToMove(Vector3.zero))
            StartCoroutine(RotateRoutine());
    }

    private IEnumerator RotateRoutine()
    {
        _rotating = true;
        var previousPos = transform.position;
        var previousRot = transform.rotation;
        transform.RotateAround(rotPos.position, Vector3.forward, 90);
        if (!CheckChildrenSpaceOccupied())
        {
            var targetRot = transform.rotation;
            transform.rotation = previousRot;
            transform.DORotateQuaternion(targetRot, rotDuration).SetEase(Ease.OutElastic);
            for (float i = 0; i < 1; i += Time.deltaTime / rotDuration)
            {
                Reposition();
                CheckXPieces();
                RenderLines();
                yield return new WaitForEndOfFrame();
            }
            Reposition();
            CheckXPieces();
            RenderLines();
        }
        else
        {
            transform.position = previousPos;
            transform.rotation = previousRot;
        }
        _rotating = false;
    }

    private IEnumerator MoveDownRoutine(bool delay = true)
    {
        if (delay)
            yield return new WaitForSeconds(moveDownCooldown);
        _ableToMove = true;
        while (true)
        {
            yield return new WaitForSeconds(moveDownCooldown);
            if (!AbleToMove(Vector3.down)) break;
            MoveDown();
        } 
        yield return new WaitForSeconds(moveDownCooldown);
        _moveDownRoutine = null;
        Finish();
    }

    private void Reposition()
    {        
        if (!int.TryParse(transform.position.x.ToString(), out var i))
        {
            var newX = transform.position.x > GameManager.ScreenLimitX / 2 ? Mathf.FloorToInt(transform.position.x) : Mathf.CeilToInt(transform.position.x);
            var newY = transform.position.y > GameManager.ScreenLimitY / 2 ? Mathf.FloorToInt(transform.position.y) : Mathf.CeilToInt(transform.position.y);
            transform.position = new Vector3(newX, newY);
        }        
        foreach (var children in _pieces)
        {            
            var childPos = children.position;
            var childPosX = Mathf.RoundToInt(childPos.x);
            var childPosY = Mathf.RoundToInt(childPos.y);
            switch (childPosX)
            {
                case < 0:
                    transform.Translate(Vector3.right, Space.World);
                    break;
                case >= GameManager.ScreenLimitX:
                    transform.Translate(Vector3.left, Space.World);
                    break;
            }            
        }        
    }

    private bool AbleToMove(Vector3 newPos)
    {
        if (_rotating) return true;
        var x = Mathf.RoundToInt(newPos.x);
        var y = Mathf.RoundToInt(newPos.y);
        
        foreach (var children in _pieces)
        {            
            var childPos = children.position;
            var childPosX = Mathf.RoundToInt(childPos.x);
            var childPosY = Mathf.RoundToInt(childPos.y);            
            if (childPosX + x < 0 || childPosX + x >= GameManager.ScreenLimitX || childPosY + y < 0)
                return false;
            if (GameManager.SpaceOccupied(childPosX + x, childPosY + y))
                return false;
        }        
        return true;
    }

    private void CheckXPieces()
    {
        _xPieces.Clear();
        var xList = new List<int>();
        foreach (var piece in _pieces)
        {
            var x = Mathf.RoundToInt(piece.position.x);
            if (xList.Contains(x)) continue;
            
            xList.Add(x);
            _xPieces.Add(piece);
        }
    }

    private bool CheckChildrenSpaceOccupied()
    {
        foreach (var children in _pieces)
        {
            var childPos = children.position;
            var childPosX = Mathf.RoundToInt(childPos.x);
            var childPosY = Mathf.RoundToInt(childPos.y);
            if (GameManager.SpaceOccupied(childPosX, childPosY)) return true;
        }
        return false;
    }

    private void RenderLines() 
    {
        LineManager.Instance.RenderLine(_xPieces.ToArray());
    }

    private void SubscribeInputEvents()
    {
        InputManager.MoveDown += MoveDown;
        InputManager.MoveLeft += MoveLeft;
        InputManager.MoveRight += MoveRight;
        InputManager.Rotate += Rotate;        
    }

    private void UnsubscribeInputEvents()
    {
        InputManager.MoveDown -= MoveDown;
        InputManager.MoveLeft -= MoveLeft;
        InputManager.MoveRight -= MoveRight;
        InputManager.Rotate -= Rotate;        
    }
}
