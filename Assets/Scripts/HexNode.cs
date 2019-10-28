/**
    Author: Ashutosh Rautela
    Date: 29 October 2019
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public enum NodeType
{
    Green = 0,
    Red = 1,
    Blue = 2,
    Yellow = 3
}

public class HexNode : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerUpHandler
{

    public NodeType nodeType;
    public Vector2 nodeIndex = Vector2.zero;

    private Image image;

    private LinkedListNode<HexNode> owner;

    private bool isSelected = false;
    private bool isVisited = false;
    private Vector2 tileBounds = Vector2.zero;
    public HexColumn ownerColumn;
    public float movementSpeed = 4000;
    private Vector3 targetPosition = Vector3.zero;
    private float rayDistance = 0.7f;


    private void Awake()
    {
        RectTransform rt = this.GetComponent<RectTransform>();
        this.tileBounds.Set(rt.rect.width, rt.rect.height);
    }

    // Start is called before the first frame update
    void Start()
    {
        nodeType = (NodeType)Random.Range(0, 4);
        this.image = this.GetComponent<Image>();
        this.assignColor();
        this.FadeInEntry();
    }

    private void Update()
    {
        if (this.transform.localPosition != this.targetPosition)
        {
            this.transform.localPosition = Vector3.MoveTowards(this.transform.localPosition, this.targetPosition, this.movementSpeed * Time.deltaTime);
        }
    }

    public void FadeInEntry()
    {
        this.StartCoroutine("FadeInRoutine");
    }

    public void Focus() {
        this.StopFadeInRoutine();
        this.GetComponent<Image>().color = Color.grey;
    }

    private void StopFadeInRoutine() {
        this.assignColor();
        this.transform.localScale = new Vector3(1 , 1, 1);
        StopCoroutine("FadeInRoutine");
    }

    private IEnumerator FadeInRoutine()
    {
        float fadeTime = 3f;
        float elapsedTime = 0;
        this.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);

        Color targetColor = this.image.color;
        this.image.color = new Color(this.image.color.r, this.image.color.g, this.image.color.b, 0);

        while (elapsedTime < fadeTime)
        {
            yield return new WaitForEndOfFrame();
            this.transform.localScale = Vector3.Lerp(this.transform.localScale, new Vector3(1, 1, 1), elapsedTime/fadeTime);
            this.image.color = Color.Lerp(this.image.color, targetColor, elapsedTime/fadeTime);
            elapsedTime += Time.deltaTime;
        }
    }

    private void assignColor()
    {
        if (this.nodeType == NodeType.Green)
        {
            this.image.color = Color.green;
        }
        else if (this.nodeType == NodeType.Red)
        {
            this.image.color = Color.red;
        }
        else if (this.nodeType == NodeType.Blue)
        {
            this.image.color = Color.blue;
        }
        else if (this.nodeType == NodeType.Yellow)
        {
            this.image.color = Color.yellow;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        MovementTracker.Instance.StartMovement(this);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        MovementTracker.Instance.PassThrough(this);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        MovementTracker.Instance.EndMovement();
    }

    public bool IsSelected
    {
        set { this.isSelected = value; }
        get { return this.isSelected; }
    }

    public bool IsVisisted
    {
        set { this.isVisited = value; }
        get { return this.isVisited; }
    }

    public void Reset()
    {
        this.assignColor();
        this.isSelected = false;
    }

    public void HighlightAnimate() {
        this.StopFadeInRoutine();
        this.StartCoroutine("PrepareHightlightCoroutine_ScaleIN");
    }

    public void StopHighlightAnimate() {
        this.image.color = new Color(this.image.color.r, this.image.color.g, this.image.color.b , 1);
        this.StopCoroutine("PrepareHightlightCoroutine_ScaleIN");
        this.StopCoroutine("PrepareHightlightCoroutine_ScaleOUT");
    }

    private IEnumerator PrepareHightlightCoroutine_ScaleIN() {
        float scaleTime = 1f;
        float elapsedTime = 0;

        Vector3 targetScale = new Vector3(1.1f, 1.1f , 1.1f);
        this.image.color = new Color(this.image.color.r , this.image.color.g , this.image.color.b , 0.5f);

        while (elapsedTime <  0.5f * scaleTime)
        {
            yield return new WaitForEndOfFrame();
            this.transform.localScale = Vector3.Lerp(this.transform.localScale, targetScale, elapsedTime/scaleTime);
            elapsedTime += Time.deltaTime;
        }
        this.StartCoroutine("PrepareHightlightCoroutine_ScaleOUT");
    }

    private IEnumerator PrepareHightlightCoroutine_ScaleOUT() {
        float scaleTime = 1f;
        float elapsedTime = 0;

        Vector3 targetScale = new Vector3(1, 1 , 1);

        while (elapsedTime < 0.5f * scaleTime)
        {
            yield return new WaitForEndOfFrame();
            this.transform.localScale = Vector3.Lerp(this.transform.localScale, targetScale, elapsedTime/scaleTime);
            elapsedTime += Time.deltaTime;
        }
        this.StartCoroutine("PrepareHightlightCoroutine_ScaleIN");
    }
    public void ResetPosition(int index)

    {
        this.nodeIndex.y = index;
        this.targetPosition = Vector3.up * this.tileBounds.y * index;
    }

    public LinkedListNode<HexNode> Owner
    {
        set { this.owner = value; }
        get { return this.owner; }
    }

    /**
        Find nearby connected neighbours using DFS
     */
    public List<HexNode> GetConnectedChain()
    {
        List<HexNode> connectedChain = new List<HexNode>();
        this.FindChainFrom(connectedChain, this);
        connectedChain.ForEach(delegate (HexNode node)
        {
            node.isVisited = false;
        });
        return connectedChain;
    }

    /**
        Get nearby neighbours recursive
     */
    private void FindChainFrom(List<HexNode> chain, HexNode node)
    {
        chain.Add(node);
        node.isVisited = true;
        List<HexNode> connectedNodes = node.GetConnectedNodes(this.nodeType);
        if (connectedNodes.Count > 0)
        {
            connectedNodes.ForEach(delegate (HexNode cNode)
            {
                cNode.GetComponent<Image>().color = Color.black;
                if (!cNode.isVisited)
                {
                    FindChainFrom(chain, cNode);
                }
            });
        }
    }

    /*
        Get all the nearby filtered neighbours
     */
    public List<HexNode> GetConnectedNodes(NodeType filteredNodeType)
    {
        List<HexNode> nodes = new List<HexNode>();

        int[] hitAngles = new int[6] { 30, 90, 150, 210, 270, 330 };


        for (int i = 0; i < hitAngles.Length; i++)
        {
            Vector3 direction = Quaternion.Euler(0, 0, hitAngles[i]) * Vector3.right;
            Ray ray = new Ray(this.transform.position, direction);
            RaycastHit2D[] rayHit = Physics2D.RaycastAll(ray.origin, ray.direction, this.rayDistance);
            for (int k = 0; k < rayHit.Length; k++)
            {
                if (rayHit[k].collider.gameObject.tag == "HexNode" && rayHit[k].collider.gameObject != this.gameObject)
                {
                    HexNode hxNode = rayHit[k].collider.gameObject.GetComponent<HexNode>();
                    if (hxNode.nodeType == filteredNodeType)
                    {
                        nodes.Add(hxNode);
                    }
                }
            }
        }

        return nodes;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        
        Vector3 direction1 = Quaternion.Euler(0, 0, 30) * Vector3.right;
        Gizmos.DrawLine(this.transform.position, this.transform.position + direction1);

        Vector3 direction2 = Quaternion.Euler(0, 0, 90) * Vector3.right;
        Gizmos.DrawLine(this.transform.position, this.transform.position + direction2);

        Vector3 direction3 = Quaternion.Euler(0, 0, 150) * Vector3.right;
        Gizmos.DrawLine(this.transform.position, this.transform.position + direction3);

        Vector3 direction4 = Quaternion.Euler(0, 0, 210) * Vector3.right;
        Gizmos.DrawLine(this.transform.position, this.transform.position + direction4);

        Vector3 direction5 = Quaternion.Euler(0, 0, 270) * Vector3.right;
        Gizmos.DrawLine(this.transform.position, this.transform.position + direction5);

        Vector3 direction6 = Quaternion.Euler(0, 0, 330) * Vector3.right;
        Gizmos.DrawLine(this.transform.position, this.transform.position + direction6);
    }
}
