using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public class Tetromino : MonoBehaviour
{   
    [SerializeField] float moveDownCooldown = 0.75f;
    [SerializeField] float rotDuration = 0.2f;
    [SerializeField] Transform rotPos;
    [SerializeField] MeshRenderer[] mrs;
    [SerializeField] Material[] materials;
    bool rotating;
    bool ableToMove;
    bool finished;
    List<Transform> pieces = new List<Transform>();
    List<Transform> xPieces = new List<Transform>();
    public delegate void TetrominoEvents(Transform obj);
    public static event TetrominoEvents OnFinished, OnPopup;

    Coroutine moveDownRoutine;
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
    void Setup()
    {
        GetPieces();
        CheckXPieces();
        transform.localScale = Vector3.zero;
        transform.DOScale(1, 0.5f).SetEase(Ease.OutBounce);
        AudioManager.instance.PlayClip(AudioManager.Clips.TETRO_POPUP, true);
        Invoke("Popup", 0.5f);
        moveDownRoutine = StartCoroutine(MoveDownRoutine());
    }
    public void CheckChildren()
    {
        StartCoroutine(CheckChildrenRoutine());
    }
    IEnumerator CheckChildrenRoutine()
    {
        yield return new WaitForEndOfFrame();
        Debug.Log(transform.childCount);
        if (transform.childCount <= 1)
        {
            Destroy(this.gameObject);
        }
    }
    void Popup()
    {
        RenderLines();        
        OnPopup?.Invoke(transform);
    }
    void GetPieces()
    {
        foreach (Transform children in transform)
        {
            if (children == rotPos) continue;
            pieces.Add(children);
        }
    }
    void SetupRandomColor()
    {
        Material mat = materials[Random.Range(0, materials.Length)];
        foreach (MeshRenderer mr in mrs)
        {
            mr.material = mat;
        }
    }
    void Finish()
    {
        if (AbleToMove(Vector3.down))
        {
            if (moveDownRoutine is null)
            {
                moveDownRoutine = StartCoroutine(MoveDownRoutine(false));
                return;
            }
        }
        if (!finished)
        {
            finished = true;            
            UnsubscribeInputEvents();
            OnFinished?.Invoke(transform);
            StartCoroutine(BlobEffect());
            LineManager.instance.HideLines();
        }
    }
    IEnumerator BlobEffect()
    {
        transform.DOShakePosition(0.2f, 0.1f);
        transform.DOScale(0.8f, 0.05f);
        yield return new WaitForSeconds(0.05f);
        transform.DOScale(1.2f, 0.1f);
        yield return new WaitForSeconds(0.1f);
        transform.DOScale(1, 0.05f);
    }
    void Move(Vector3 dir)
    {
        if (!finished && ableToMove)
        {
            transform.Translate(dir, Space.World);
            CheckXPieces();
            RenderLines();
        }
    }
    void MoveRight()
    {
        if (!rotating)
            if (AbleToMove(Vector3.right)) Move(Vector3.right);
    }
    void MoveLeft()
    {
        if (!rotating)
            if (AbleToMove(Vector3.left)) Move(Vector3.left);
    }
    void MoveDown()
    {
        if (!rotating)
        {
            if (AbleToMove(Vector3.down)) Move(Vector3.down);
            else
            {
                Finish();
            }
        }
    }
    void Rotate()
    {
        if (!rotating && AbleToMove(Vector3.zero))
            StartCoroutine(RotateRoutine());
    }
    IEnumerator RotateRoutine()
    {
        rotating = true;
        Vector3 previousPos = transform.position;
        Quaternion previousRot = transform.rotation;
        transform.RotateAround(rotPos.position, Vector3.forward, 90);
        if (!CheckChildrenSpaceOccupied())
        {
            Quaternion targetRot = transform.rotation;
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
        rotating = false;
    }
    IEnumerator MoveDownRoutine(bool delay = true)
    {
        if (delay)
            yield return new WaitForSeconds(moveDownCooldown);
        ableToMove = true;
        while (true)
        {
            yield return new WaitForSeconds(moveDownCooldown);
            if (!AbleToMove(Vector3.down)) break;
            MoveDown();
        } 
        yield return new WaitForSeconds(moveDownCooldown);
        moveDownRoutine = null;
        Finish();
    }
    void Reposition()
    {        
        if (!int.TryParse(transform.position.x.ToString(), out int i))
        {
            int newX = transform.position.x > GameManager.SCREEN_LIMIT_X / 2 ? Mathf.FloorToInt(transform.position.x) : Mathf.CeilToInt(transform.position.x);
            int newY = transform.position.y > GameManager.SCREEN_LIMIT_Y / 2 ? Mathf.FloorToInt(transform.position.y) : Mathf.CeilToInt(transform.position.y);
            transform.position = new Vector3(newX, newY);
        }        
        foreach (Transform children in pieces)
        {            
            Vector3 childPos = children.position;
            int childPosX = Mathf.RoundToInt(childPos.x);
            int childPosY = Mathf.RoundToInt(childPos.y);
            if (childPosX < 0)
                transform.Translate(Vector3.right, Space.World);
            else if (childPosX >= GameManager.SCREEN_LIMIT_X)
                transform.Translate(Vector3.left, Space.World);            
        }        
    }
    bool AbleToMove(Vector3 newPos)
    {
        if (rotating) return true;
        int _x = Mathf.RoundToInt(newPos.x);
        int _y = Mathf.RoundToInt(newPos.y);
        
        foreach (Transform children in pieces)
        {            
            Vector3 childPos = children.position;
            int childPosX = Mathf.RoundToInt(childPos.x);
            int childPosY = Mathf.RoundToInt(childPos.y);            
            if (childPosX + _x < 0 || childPosX + _x >= GameManager.SCREEN_LIMIT_X || childPosY + _y < 0)
                return false;
            if (GameManager.SpaceOccupied(childPosX + _x, childPosY + _y))
                return false;
        }        
        return true;
    }
    void CheckXPieces()
    {
        xPieces.Clear();
        List<int> xList = new List<int>();
        foreach (Transform piece in pieces)
        {
            int x = Mathf.RoundToInt(piece.position.x);
            if (!xList.Contains(x))
            {
                xList.Add(x);
                xPieces.Add(piece);
            }
        }
    }
    bool CheckChildrenSpaceOccupied()
    {
        foreach (Transform children in pieces)
        {
            Vector3 childPos = children.position;
            int childPosX = Mathf.RoundToInt(childPos.x);
            int childPosY = Mathf.RoundToInt(childPos.y);
            if (GameManager.SpaceOccupied(childPosX, childPosY)) return true;
        }
        return false;
    }
    void RenderLines() 
    {
        LineManager.instance.RenderLine(xPieces.ToArray());
    }
    void SubscribeInputEvents()
    {
        InputManager.MoveDown += MoveDown;
        InputManager.MoveLeft += MoveLeft;
        InputManager.MoveRight += MoveRight;
        InputManager.Rotate += Rotate;        
    }
    void UnsubscribeInputEvents()
    {
        InputManager.MoveDown -= MoveDown;
        InputManager.MoveLeft -= MoveLeft;
        InputManager.MoveRight -= MoveRight;
        InputManager.Rotate -= Rotate;        
    }
}
