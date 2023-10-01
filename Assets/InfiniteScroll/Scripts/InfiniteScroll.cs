using System;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


[RequireComponent(typeof(ScrollRect), typeof(RectTransform))]
public class InfiniteScroll : MonoBehaviour, IBeginDragHandler, IDragHandler, IScrollHandler
{
    [SerializeField] private ScrollViewData scrollViewData;
    [SerializeField] private CellData cellData;
    [SerializeField] private bool reverse;

    private RectTransform _rectTransform;
    private RectTransform _content;
    private ScrollRect _scrollRect;

    private Vector2 _lastDragPosition;

    private bool _positiveDrag;
    private bool _isVertical;

    //For Populate
    private int _headIndex;
    private int _tailIndex;

    //Instead Child Index
    private int _lastCellIndex;
    private int _firstCellIndex;

    //Cell
    private BaseCell[] _cells;
    private int _cellAmount;
    private float _cellWidth;
    private float _cellHeight;

    //Content
    private float _contentHeight;
    private float _contentWidth;

    //World Positions
    private Bounds _bounds;
    private Bounds _cellBounds;

    [SerializeField] private bool debug;

    [DrawIf(nameof(debug), true)] [SerializeField]
    private Transform debugCellTop;

    [DrawIf(nameof(debug), true)] [SerializeField]
    private Transform debugCellBottom;

    [DrawIf(nameof(debug), true)] [SerializeField]
    private Transform debugBoundBottom;

    [DrawIf(nameof(debug), true)] [SerializeField]
    private Transform debugBoundTop;

    private void Awake()
    {
        _scrollRect = GetComponent<ScrollRect>();
        _rectTransform = GetComponent<RectTransform>();

        _isVertical = _scrollRect.vertical;
        if (cellData.infinite) _scrollRect.movementType = ScrollRect.MovementType.Unrestricted;
        if (_scrollRect.vertical == _scrollRect.horizontal)
        {
            Debug.LogError("You must choose one! Vertical or Horizontal Scroll");
            throw new Exception();
        }


        _cells = GetComponentsInChildren<BaseCell>();
        _cellAmount = _cells.Length;
        _cellWidth = _cells[0].Width;
        _cellHeight = _cells[0].Height;

        _content = _scrollRect.content;

        var pivot = reverse ? 0 : 1f;
        var verticalPivot = new Vector2(.5f, pivot);
        var horizontalPivot = new Vector2(pivot, .5f);
        _content.pivot = _isVertical ? verticalPivot : horizontalPivot;

        _contentWidth = !_isVertical
            ? CalculateContentWidth()
            : _rectTransform.rect.width - (scrollViewData.margin.left + scrollViewData.margin.right);
        _contentHeight = _isVertical
            ? CalculateContentHeight()
            : _rectTransform.rect.height - (scrollViewData.margin.top + scrollViewData.margin.bottom);
        _content.sizeDelta = new Vector2(_contentWidth, _contentHeight);

        InitializeContent();

        _headIndex = _cellAmount - 1;
        _lastCellIndex = _cellAmount - 1;
        _firstCellIndex = 0;

        PopulateDisplay();
    }

    private void Start()
    {
        _cellBounds = GetWorldPositions(_cells[0].RectTransform);
        _bounds = GetWorldPositions(_rectTransform);
    }

    private void InitializeContent()
    {
        var contentDistance = _isVertical ? _contentHeight : _contentWidth;
        var contentExtends = contentDistance * 0.5f;

        var cellDistance = _isVertical ? _cellHeight : _cellWidth;
        var cellExtends = cellDistance * 0.5f;

        var verticalMargin = reverse ? scrollViewData.margin.bottom : scrollViewData.margin.top;
        var horizontalMargin = reverse ? scrollViewData.margin.left : scrollViewData.margin.right;
        var margin = _isVertical ? verticalMargin : horizontalMargin;

        margin *= reverse ? 1 : -1f;
        contentExtends *= reverse ? -1f : 1f;
        var origin = contentExtends + margin;

        for (var i = 0; i < _cells.Length; i++)
        {
            Vector2 childPos = _cells[i].RectTransform.anchoredPosition;
            var distance = cellExtends + i * (cellDistance + scrollViewData.itemSpacing);
            distance *= reverse ? 1 : -1;
            if (_isVertical)
                childPos.y = origin + distance;
            else
                childPos.x = origin + distance;
            _cells[i].RectTransform.anchoredPosition = childPos;
        }
    }

    private float CalculateContentWidth()
    {
        if (cellData.infinite)
        {
            return _rectTransform.rect.width;
        }

        var itemAmount = cellData.CellAmount;
        return (itemAmount * (scrollViewData.itemSpacing + _cellWidth)) +
            (scrollViewData.margin.left + scrollViewData.margin.right) - scrollViewData.itemSpacing;
    }

    private float CalculateContentHeight()
    {
        if (cellData.infinite)
        {
            return _rectTransform.rect.height;
        }

        var itemAmount = cellData.CellAmount;
        return (itemAmount * (scrollViewData.itemSpacing + _cellHeight)) +
            (scrollViewData.margin.top + scrollViewData.margin.bottom) - scrollViewData.itemSpacing;
    }

    private void PopulateDisplay()
    {
        var cells = GetComponentsInChildren<BaseCell>().ToList();
        for (var i = 0; i < cells.Count; i++)
        {
            var cell = cells[i];
            cell.UpdateDisplay(i);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        _lastDragPosition = eventData.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_isVertical)
        {
            _positiveDrag = eventData.position.y > _lastDragPosition.y;
        }
        else
        {
            _positiveDrag = eventData.position.x > _lastDragPosition.x;
        }

        _lastDragPosition = eventData.position;
    }

    public void OnScroll(PointerEventData eventData)
    {
        if (_isVertical)
        {
            _positiveDrag = eventData.scrollDelta.y > 0;
        }
        else
        {
            _positiveDrag = eventData.scrollDelta.y < 0;
        }
    }


    public void OnViewScroll()
    {
        HandleScroll();
    }

    private bool IsReachLimit()
    {
        var drag = reverse ? _positiveDrag : !_positiveDrag;
        switch (drag)
        {
            case true when _tailIndex == cellData.limitMin:
            case false when _headIndex + 1 == cellData.limitMax:
                return true;
        }

        return false;
    }

    private void HandleScroll()
    {
        var first = _firstCellIndex;
        var last = _lastCellIndex;
        var drag = _positiveDrag;
        if (!reverse) drag = !drag;
        var currItemIndex = drag ? last : first;
        var currItem = _cells[currItemIndex];

        if (!ReachedThreshold(currItem.RectTransform))
            return;

        if (!cellData.infinite)
            if (IsReachLimit())
                return;

        var endItemIndex = drag ? first : last;
        var endItem = _cells[endItemIndex].RectTransform;
        var newPos = endItem.anchoredPosition;

        var spacing = scrollViewData.itemSpacing;
        var cellDistance = _isVertical ? _cellHeight : _cellWidth;
        var distance = cellDistance + spacing;
        distance *= _positiveDrag ? -1 : 1;

        if (_isVertical)
            newPos.y = endItem.anchoredPosition.y + distance;
        else
            newPos.x = endItem.anchoredPosition.x + distance;

        currItem.RectTransform.anchoredPosition = newPos;

        _firstCellIndex += !drag ? 1 : -1;
        _firstCellIndex = SetLoopMode(_firstCellIndex);

        _lastCellIndex += !drag ? 1 : -1;
        _lastCellIndex = SetLoopMode(_lastCellIndex);

        //OnChange
        var adding = drag ? -1 : 1;
        _headIndex += adding;
        _tailIndex += adding;

        var increase = !drag;
        var currentIndex = increase ? _headIndex : _tailIndex;
        currItem.UpdateDisplay(currentIndex);
    }

    private int SetLoopMode(int index)
    {
        if (index < 0)
            index = _cells.Length - 1;
        if (index == _cells.Length)
            index = 0;

        return index;
    }


    private bool ReachedThreshold(Transform item)
    {
        var outOfBoundsThreshold = scrollViewData.outOfBoundsThreshold;

        if (_isVertical)
        {
            var topYThreshold = _bounds.top + outOfBoundsThreshold;
            var bottomYThreshold = _bounds.bottom - outOfBoundsThreshold;

            var positionY = item.position.y;
            var extendY = _cellBounds.Height * 0.5f;
            var cellTopPoint = positionY + extendY;
            var cellBottomPoint = positionY - extendY;

            if (debug)
                DebugVertical(cellBottomPoint, cellTopPoint, topYThreshold, bottomYThreshold);

            return _positiveDrag
                ? cellBottomPoint > topYThreshold
                : cellTopPoint < bottomYThreshold;
        }
        else
        {
            var rightXThreshold = _bounds.right + outOfBoundsThreshold;
            var leftXThreshold = _bounds.left - outOfBoundsThreshold;

            var positionX = item.position.x;
            var extendX = _cellBounds.Width * 0.5f;
            var cellRightPoint = positionX + extendX;
            var cellLeftPoint = positionX - extendX;

            if (debug)
                DebugHorizontal(cellLeftPoint, cellRightPoint, rightXThreshold, leftXThreshold);

            return _positiveDrag
                ? cellLeftPoint > rightXThreshold
                : cellRightPoint < leftXThreshold;
        }
    }

    private void DebugHorizontal(float cellLeftPoint, float cellRightPoint, float rightXThreshold, float leftXThreshold)
    {
        var position = debugCellBottom.position;
        position =
            new Vector3(cellLeftPoint, position.y, position.z);
        debugCellBottom.position = position;
        var positionCellTop = debugCellTop.position;
        positionCellTop = new Vector3(cellRightPoint, positionCellTop.y, positionCellTop.z);
        debugCellTop.position = positionCellTop;

        var positionBoundTop = debugBoundTop.position;
        positionBoundTop = new Vector3(rightXThreshold, positionBoundTop.y, positionBoundTop.z);
        debugBoundTop.position = positionBoundTop;
        var positionBoundBottom = debugBoundBottom.position;
        positionBoundBottom =
            new Vector3(leftXThreshold, positionBoundBottom.y, positionBoundBottom.z);
        debugBoundBottom.position = positionBoundBottom;
    }

    private void DebugVertical(float cellBottomPoint, float cellTopPoint, float topYThreshold, float bottomYThreshold)
    {
        var position = debugCellBottom.position;
        position =
            new Vector3(position.x, cellBottomPoint, position.z);
        debugCellBottom.position = position;
        var positionCellTop = debugCellTop.position;
        positionCellTop = new Vector3(positionCellTop.x, cellTopPoint, positionCellTop.z);
        debugCellTop.position = positionCellTop;

        var positionBoundTop = debugBoundTop.position;
        positionBoundTop = new Vector3(positionBoundTop.x, topYThreshold, positionBoundTop.z);
        debugBoundTop.position = positionBoundTop;
        var positionBoundBottom = debugBoundBottom.position;
        positionBoundBottom =
            new Vector3(positionBoundBottom.x, bottomYThreshold, positionBoundBottom.z);
        debugBoundBottom.position = positionBoundBottom;
    }

    private Bounds GetWorldPositions(RectTransform current)
    {
        var v = new Vector3[4];
        current.GetWorldCorners(v);
        var data = new Bounds
        {
            bottom = v[0].y,
            top = v[1].y,
            left = v[0].x,
            right = v[3].x
        };
        return data;
    }
}


[Serializable]
public struct CellData
{
    public bool infinite;
    [DrawIf(nameof(infinite), false)] public int limitMax;
    [DrawIf(nameof(infinite), false)] public int limitMin;
    public int CellAmount => limitMax - limitMin;
}

[Serializable]
public struct Bounds
{
    public float top;
    public float left;
    public float right;
    public float bottom;
    public float Height => top - bottom;
    public float Width => right - left;
}

[Serializable]
public struct ScrollViewData
{
    public float outOfBoundsThreshold;
    public float itemSpacing;
    public Bounds margin;
}