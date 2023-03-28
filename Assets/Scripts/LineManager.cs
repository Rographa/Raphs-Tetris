using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineManager : MonoBehaviour
{
    public static LineManager Instance;
    [SerializeField] private LineRenderer[] lines;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }
    public void RenderLine(Transform[] objs)
    {
        HideLines();   
        for (var i = 0; i < objs.Length; i++)
        {
            var line = lines[i];
            line.enabled = true;
            var pos = objs[i].position;
            line.SetPosition(0, pos);
            pos.y -= 25;
            line.SetPosition(1, pos);
        }
    }
    public void HideLines()
    {
        foreach (var line in lines)
        {
            line.enabled = false;
        }
    }
}
