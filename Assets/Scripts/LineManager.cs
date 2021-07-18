using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineManager : MonoBehaviour
{
    public static LineManager instance;
    [SerializeField] LineRenderer[] lines;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }
    public void RenderLine(Transform[] objs)
    {
        HideLines();   
        for (int i = 0; i < objs.Length; i++)
        {
            LineRenderer line = lines[i];
            line.enabled = true;
            Vector3 pos = objs[i].position;
            line.SetPosition(0, pos);
            pos.y -= 25;
            line.SetPosition(1, pos);
        }
    }
    public void HideLines()
    {
        foreach (LineRenderer line in lines)
        {
            line.enabled = false;
        }
    }
}
