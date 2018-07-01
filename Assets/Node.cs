using UnityEngine;
using System.Collections;

public class Node : MonoBehaviour {

    [SerializeField] NodeSystem system;
    [SerializeField] Transform child;

    [SerializeField]
    [Range(0f,1f)] float weight;

    [SerializeField] int index;
    [SerializeField] public bool draw;

    void OnEnable() {
        child = transform.GetChild(0);
    }

    public float Weight
    {
        get{return weight;}
        set{weight = value;}
    }

    void OnDrawGizmos()
    {
        if (draw)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, system.player.position);
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, child.position);
        }
    }


    public void StopDraw()
    {
        draw = false;
    }
}
