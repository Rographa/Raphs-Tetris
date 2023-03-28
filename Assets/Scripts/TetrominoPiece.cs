using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TetrominoPiece : MonoBehaviour
{
    [SerializeField] private GameObject fadeFX;
    private MeshRenderer _mr;
    private Tetromino _tetromino;
    private void Start()
    {        
        _mr = GetComponent<MeshRenderer>();
        _mr.material = new Material(_mr.material);
        _tetromino = GetComponentInParent<Tetromino>();
    }
    public void BlinkAndFade()
    {
        StartCoroutine(BlinkAndFadeRoutine());
    }
    private IEnumerator BlinkAndFadeRoutine()
    {
        var material = _mr.material;
        var startColor = material.color;
        var endColor = Color.white;
        
        endColor.a = 0;
        for (var i = 0f; i < 1; i += Time.deltaTime / 0.25f)
        {
            material.color = Color.Lerp(startColor, endColor, i);
            yield return new WaitForEndOfFrame();
        }
        material.SetColor("_EmissionColor", Color.black);
        material.color = endColor;
        
        if (fadeFX != null) fadeFX.SetActive(true);
        yield return new WaitForSeconds(1);
        _tetromino ??= GetComponentInParent<Tetromino>(); 
        _tetromino.CheckChildren();
        Destroy(this.gameObject);        
    }
    public void MovePieceDown(int y)
    {
        var newPos = transform.position;
        newPos.y = y;
        newPos.y = Mathf.RoundToInt(newPos.y);
        transform.DOKill();
        transform.DOMove(newPos, 0.05f, true).OnComplete(Snap);
        //Invoke("Snap", 0.1f);
    }
    private void Snap()
    {
        var pos = transform.position;
        var newPos = new Vector3Int(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y), Mathf.RoundToInt(pos.z));
        transform.position = newPos;
    }
}
