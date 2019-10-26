using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class HexColumn : MonoBehaviour
{

    public GameObject tilePrefab;

    public List<HexNode> hexNodes;
    
    [SerializeField]
    public LinkedList<HexNode> hexNodesLinkedList;

    private Vector2 tileBounds;
    private int totalElements = 1;
    private float spawnTime = 0.02f;
    private int columnIndex = 0;

    // Start is called before the first frame update
    void Start()
    {
        RectTransform tI = tilePrefab.GetComponent<RectTransform>();
        this.tileBounds.Set(tI.rect.width, tI.rect.height);

        this.hexNodesLinkedList = new LinkedList<HexNode>();
    }

    public HexNode AddNode() {
        GameObject tile = Instantiate(tilePrefab, Vector2.zero, Quaternion.identity);
        tile.transform.SetParent(this.transform, false);
        tile.transform.localPosition = new Vector3(0 , 1500 , 0);

        HexNode node = tile.GetComponent<HexNode>();
        LinkedListNode<HexNode> lastNode = this.hexNodesLinkedList.Last;
        LinkedListNode<HexNode> llNode =this.hexNodesLinkedList.AddLast(node);
        node.Owner = llNode;
        node.ownerColumn = this;

        node.nodeIndex.Set(this.ColumnIndex, this.hexNodesLinkedList.Count - 1);
        return node;
    }

    public void WipeOut(List<HexNode> nodes){
        int tobeAdded = nodes.Count;
        nodes.ForEach(delegate(HexNode node){
            this.hexNodesLinkedList.Remove(node.Owner);
            Destroy(node.gameObject);
        });
        IEnumerator it = this.hexNodesLinkedList.GetEnumerator();
        int index = 0;
        while(it.MoveNext()) {
            (it.Current as HexNode).ResetPosition(index++);
        }
        this.Fill(this.totalElements);
    }

    public void Fill(int nodes) {
        this.totalElements = nodes;
        StartCoroutine("PrepareFilling");
    }

    public int ColumnIndex {
        set {this.columnIndex = value;}
        get {return this.columnIndex;}
    }

    private IEnumerator PrepareFilling() {
        yield return new WaitForSeconds(this.spawnTime);
        HexNode node = this.AddNode();
        node.ResetPosition(this.hexNodesLinkedList.Count - 1);
        if (this.hexNodesLinkedList.Count < this.totalElements) {
            StartCoroutine("PrepareFilling");
        }
    }
}
