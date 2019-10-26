using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public enum NodeType {
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
    private Vector2 tileBounds = Vector2.zero;
    public HexColumn ownerColumn;
    public float movementSpeed = 5000;
    private Vector3 targetPosition = Vector3.zero;


    private void Awake() {
        RectTransform rt = this.GetComponent<RectTransform>();
        this.tileBounds.Set(rt.rect.width, rt.rect.height);
    }

    // Start is called before the first frame update
    void Start()
    {
        nodeType = (NodeType)Random.Range(0 , 4);
        this.image = this.GetComponent<Image>();
        this.assignColor();
        this.FadeInEntry();
    }

    public void FadeInEntry() {
        this.StartCoroutine("FadeInRoutine");
    }

    private IEnumerator FadeInRoutine() {
        float fadeTime = 0.5f;
        float elapsedTime = 0;
        this.transform.localScale = new Vector3(1.5f, 1.5f , 1.5f);

        Color targetColor = this.image.color;
        this.image.color = new Color(this.image.color.r , this.image.color.g , this.image.color.b, 0);

        while(elapsedTime < fadeTime) {
            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
            this.transform.localScale = Vector3.Lerp(this.transform.localScale, new Vector3(1, 1 , 1), elapsedTime);
            this.image.color = Color.Lerp(this.image.color, targetColor, elapsedTime);
        }
    }

    private void assignColor() {
        if (this.nodeType == NodeType.Green) {
            this.image.color = Color.green;
        } else if (this.nodeType == NodeType.Red) {
            this.image.color = Color.red;
        } else if (this.nodeType == NodeType.Blue) {
            this.image.color = Color.blue;
        } else if (this.nodeType == NodeType.Yellow) {
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

    public bool IsSelected {
        set {this.isSelected = value;}
        get {return this.isSelected;}
    }

    public void Reset() {
        this.assignColor();
        this.isSelected = false;
    }

    private void Update() {
        if (this.transform.localPosition != this.targetPosition) {
            this.transform.localPosition = Vector3.MoveTowards(this.transform.localPosition, this.targetPosition, this.movementSpeed * Time.deltaTime);
        }
    }

    public void ResetPosition(int index) {
        this.nodeIndex.y = index;
        this.targetPosition = Vector3.up * this.tileBounds.y * index;
    }

    public LinkedListNode<HexNode> Owner {
        set { this.owner = value; }
         get { return this.owner; }
    }
}
