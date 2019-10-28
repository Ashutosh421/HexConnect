/**
    Author: Ashutosh Rautela
    Date: 29 October 2019
 */

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
    private float minHexDistanceCheck = 0.99f;

    private Dictionary<int, List<HexNode>> deletionItems = new Dictionary<int, List<HexNode>>();
    private float timer = 5;

    private List<HexNode> highlightingNodes;

    private void Start()
    {
        instance = this;
        this.hexNodes = new List<HexNode>();
        this.movementLineRenderer.positionCount = 0;
    }

    private void Update()
    {
        this.timer -= Time.deltaTime;
        if (this.timer <= 0)
        {
            this.SuggestNextMove();
            this.ResetTimer();
        }
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2)) {
            if (this.highlightingNodes != null && this.highlightingNodes.Count > 0) {
                this.highlightingNodes.ForEach(delegate(HexNode node) {
                    node.StopHighlightAnimate();
                });
                this.highlightingNodes.Clear();
            }
        }
    }

    private void ResetTimer()
    {
        this.timer = 5;
    }

    private void SuggestNextMove()
    {
        Debug.Log("Suggesting Next Move");
        List<HexNode> newNodes;   
        for (int i = 0 ; i < GridManager.Instance.hexColumns.Count; i++) {
            IEnumerator it = GridManager.Instance.hexColumns[i].hexNodesLinkedList.GetEnumerator();
            while(it.MoveNext()) {
                newNodes = (it.Current as HexNode).GetConnectedChain();
                if (newNodes.Count >= 3) {
                    this.HighlightNodes(newNodes);
                    return;
                }
            }
        }
    }

    public void HighlightNodes(List<HexNode> nodes) {
        this.highlightingNodes = nodes;
        nodes.ForEach(delegate(HexNode node) {
            node.HighlightAnimate();
        });
    }

    public void StartMovement(HexNode node)
    {
        this.ResetTimer();
        this.movementLineRenderer.positionCount += 1;
        this.hexNodes.Clear();
        this.movementStarted = true;
        this.hexNodes.Add(node);
        this.trackNodeType = node.nodeType;

        // node.GetComponent<Image>().color = Color.gray;
        node.Focus();

        node.IsSelected = true;
        this.movementLineRenderer.SetPosition(this.hexNodes.Count - 1, node.transform.position);
    }

    public void PassThrough(HexNode node)
    {
        this.ResetTimer();
        if (this.movementStarted && node.nodeType == this.trackNodeType)
        {
            if (node.IsSelected)
            {
                if (this.hexNodes.Count > 1)
                {
                    if (node == this.hexNodes[this.hexNodes.Count - 2])
                    {
                        this.hexNodes[this.hexNodes.Count - 1].IsSelected = false;
                        this.hexNodes[this.hexNodes.Count - 1].Reset();
                        this.hexNodes.RemoveAt(this.hexNodes.Count - 1);
                        this.movementLineRenderer.positionCount -= 1;
                        return;
                    }
                }
            }
            else if (!node.IsSelected)
            {
                if (Vector2.Distance(node.transform.position, this.hexNodes.Last().transform.position) <= this.minHexDistanceCheck)
                {
                    if (!node.IsSelected && !this.hexNodes.Contains(node))
                    {
                        this.movementLineRenderer.positionCount += 1;
                        this.hexNodes.Add(node);
                        node.IsSelected = true;

                        // node.GetComponent<Image>().color = Color.gray;
                        node.Focus();
                        this.movementLineRenderer.SetPosition(this.hexNodes.Count - 1, node.transform.position);
                    }
                }
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
            }
            else
            {
                this.hexNodes.ForEach(delegate (HexNode node)
                {
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
