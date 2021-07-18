using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TetrominoPiece : MonoBehaviour
{
    [SerializeField] GameObject fadeFX;
    MeshRenderer mr;
    private void Start()
    {        
        mr = GetComponent<MeshRenderer>();
        mr.material = new Material(mr.material);
    }
    public void BlinkAndFade()
    {
        StartCoroutine(BlinkAndFadeRoutine());
    }
    IEnumerator BlinkAndFadeRoutine()
    {
        Color startColor = mr.material.color;
        Color endColor = Color.white;
        endColor.a = 0;
        for (float i = 0; i < 1; i += Time.deltaTime / 0.25f)
        {
            mr.material.color = Color.Lerp(startColor, endColor, i);
            yield return new WaitForEndOfFrame();
        }
        mr.material.SetColor("_EmissionColor", Color.black);
        mr.material.color = endColor;
        if (fadeFX != null) fadeFX.SetActive(true);
        yield return new WaitForSeconds(1);
        GetComponentInParent<Tetromino>().CheckChildren();
        Destroy(this.gameObject);        
    }
    public void MovePieceDown(int y)
    {
        Vector3 newPos = transform.position;
        newPos.y = y;
        newPos.y = Mathf.RoundToInt(newPos.y);
        transform.DOKill();
        transform.DOMove(newPos, 0.05f, true);
        Invoke("Snap", 0.1f);
    }
    void Snap()
    {
        Vector3 pos = transform.position;
        Vector3Int newPos = new Vector3Int(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y), Mathf.RoundToInt(pos.z));
        transform.position = newPos;
    }
}
