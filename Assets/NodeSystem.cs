using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class NodeSystem : MonoBehaviour {

    [SerializeField] public Transform player;
    [SerializeField] private Transform[] playerNodes;
    [SerializeField] private Node[] cameraNodes;
    [SerializeField]
    private float repeatTime;
    [SerializeField]
    bool draw;
    private Vector3 calculatedCenterPosition, weightedCenterPosition;

    private List<int> currentCameraNodes;

    public Vector3 WeightedPosition { get; set; }

    //public delegate void Calc();
    //public static event Calc OnCalc;

    void Start()
    {
        cameraNodes = new Node[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
        {
            cameraNodes[i] = transform.GetChild(i).GetComponent<Node>();
            if (!draw)
            {
                transform.GetChild(i).GetComponent<Node>().StopDraw();
                cameraNodes[i].StopDraw();
            }
        }

    }

    void OnEnable () {
        InvokeRepeating(nameof(CalculatePositions), 0f, repeatTime);
	}

	void Update () {
        //CalculatePositions();
	}

    void CalculatePositions()
    {
        //print("calculating");
        //OnCalc();
        Vector3 temp = Vector3.zero;
        foreach (Node node in cameraNodes)
            calculatedCenterPosition += node.transform.position;
        calculatedCenterPosition /= cameraNodes.Length;
        calculatedCenterPosition = temp;
        temp = Vector3.zero;
        float tempWeight = 0;
        foreach (Node node in cameraNodes)
        {
            float weight = node.Weight;
          //  print("weight is " + weight);
            tempWeight += weight;
            temp += node.transform.position + ((calculatedCenterPosition - node.transform.position) * node.Weight);
           // Debug.DrawRay(node.transform.position, (calculatedCenterPosition - node.transform.position) * node.Weight, Color.yellow);
            //temp = calculatedCenterPosition - node.position;
        }
        //  print("total weight is " + tempWeight);

        WeightedPosition = Vector3.zero;

        foreach (Node node in cameraNodes)
        {
            float weight = node.GetComponent<Node>().Weight;
            //print("weight for node is " + weight);
            float weightedPercentage = (weight - 0) * 100 / tempWeight;
            //print("weighted percentage for " + node + " is " + weightedPercentage);
            WeightedPosition += node.transform.position * weightedPercentage;
        }
        WeightedPosition /= 100f;


        temp /= cameraNodes.Length;
        weightedCenterPosition = temp;
        //print("calced");
    }

    void OnDisable()
    {
        CancelInvoke();
    }

    void OnDrawGizmos()
    {
        if (draw)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(calculatedCenterPosition, .2f);
            Gizmos.color = Color.magenta;
            Gizmos.DrawCube(WeightedPosition, Vector3.one);
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(weightedCenterPosition, .4f);
        }
    }
}
