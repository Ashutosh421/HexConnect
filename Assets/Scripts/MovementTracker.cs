using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class MovementTracker : MonoBehaviour
{
    public LineRenderer movementLineRenderer;
    private static MovementTracker instance;
    private bool movementStarted = false;

    public List<HexNode> hexNodes;

    private NodeType trackNodeType;
    private float minHexDistanceCheck = 0.7f;

    private Dictionary<int, List<HexNode>> deletionItems = new Dictionary<int, List<HexNode>>();


    private void Start()
    {
        instance = this;
        this.hexNodes = new List<HexNode>();
        this.movementLineRenderer.positionCount = 0;
    }

    public void StartMovement(HexNode node)
    {
        this.movementLineRenderer.positionCount += 1;
        this.hexNodes.Clear();
        this.movementStarted = true;
        this.hexNodes.Add(node);
        this.trackNodeType = node.nodeType;

        node.GetComponent<Image>().color = Color.gray;
        node.IsSelected = true;
        this.movementLineRenderer.SetPosition(this.hexNodes.Count - 1, node.transform.position);
    }

    public void PassThrough(HexNode node)
    {
        if (this.movementStarted && node.nodeType == this.trackNodeType && !node.IsSelected)
        {
            Debug.Log("Distance "+Vector2.Distance(node.transform.position, this.hexNodes.Last().transform.position));
            Debug.Log("TileBounds "+GridManager.Instance.TileBounds.magnitude);
            if (Vector2.Distance(node.transform.position, this.hexNodes.Last().transform.position) <= this.minHexDistanceCheck) {
                this.movementLineRenderer.positionCount += 1;
                this.hexNodes.Add(node);
                node.IsSelected = true;

                node.GetComponent<Image>().color = Color.gray;
                this.movementLineRenderer.SetPosition(this.hexNodes.Count - 1, node.transform.position);
            }
        }
    }

    public void EndMovement()
    {
        if (this.movementStarted)
        {
            this.movementLineRenderer.positionCount = 0;
            this.movementStarted = false;

            if (this.hexNodes.Count >= 3)
            {
                this.hexNodes.ForEach(delegate (HexNode node)
                {
                    int cIndex = (int)node.nodeIndex.x;
                    if (this.deletionItems.ContainsKey(cIndex))
                    {
                        this.deletionItems[cIndex].Add(node);
                    }
                    else
                    {
                        List<HexNode> newList = new List<HexNode>();
                        newList.Add(node);
                        this.deletionItems.Add(cIndex, newList);
                    }
                    node.Reset();
                });

                foreach (KeyValuePair<int, List<HexNode>> item in this.deletionItems)
                {
                    GridManager.Instance.hexColumns[item.Key].WipeOut(item.Value);
                }
            } else {
                this.hexNodes.ForEach(delegate (HexNode node) {
                    node.Reset();
                });
            }

            this.deletionItems.Clear();
        }
    }

    public static MovementTracker Instance
    {
        get { return MovementTracker.instance; }
    }
}
