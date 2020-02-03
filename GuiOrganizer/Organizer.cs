using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum EOrganizerType { VERTICAL, HORIZONTAL, DIAGONAL/*, GRID*/ }
public enum EOrganizerSort { DIRECT, INVERSE }

[ExecuteInEditMode]
public class Organizer : MonoBehaviour
{
    [SerializeField] EOrganizerType _type;
	[SerializeField] EOrganizerSort _sort;
    [SerializeField] bool _countInactive = false;
    [SerializeField] SpriteAlignment _align = SpriteAlignment.Center;
    [SerializeField] float _margin;
    [SerializeField,UnityEngine.Serialization.FormerlySerializedAs("_minimunSpacing")] float _minimumSpacing = float.MinValue;
    [SerializeField, UnityEngine.Serialization.FormerlySerializedAs( "_maximunSpacing" )] float _maximumSpacing = float.MaxValue;
    [SerializeField] List<RectTransform> _ignoreList = new List<RectTransform>();
    float _totalWidth, _totalHeight;

	private readonly EventSlot _beforeOrganize = new EventSlot();
	private readonly EventSlot _afterOrganize = new EventSlot();

	public IEventRegister BeforeOrganize => _beforeOrganize;
	public IEventRegister AfterOrganize => _afterOrganize;

	public float TotalWidth => _totalWidth;
    public float TotalHeight => _totalHeight;

	public EOrganizerSort Sort { get { return _sort; } set { _sort = value; } }

    public void Organize()
    {
		_beforeOrganize.Trigger();

		var rt = transform as RectTransform;
        var rect = rt.rect;
        var scale = rt.lossyScale;

        float width = rect.width;
		float height = rect.height;
		_totalHeight = 0;
		_totalWidth = 0;
		float maxHeight = 0;
		float maxWidth = 0;

        int childCount = transform.childCount;
        for( int i = 0; i < transform.childCount; i++ )
        {
            var child = transform.GetChild( i );
            
            var childTransform = child.transform as RectTransform;
            if(_ignoreList.Contains(childTransform)) continue;

            var childRect = childTransform.rect;
			var childScale = childTransform.localScale;

            if( _countInactive || child.gameObject.activeInHierarchy )
			{
				var w = childRect.width * childScale.x;
				var h = childRect.height * childScale.y;
                _totalWidth += w;
				_totalHeight += h;
				maxWidth = Mathf.Max( maxWidth, w );
				maxHeight = Mathf.Max( maxHeight, h );
            }
            else
            {
                childCount--;
            }
        }

		if( childCount == 0 )
			return;

        float scaledMarginX = _margin /** scale.x*/;
        float scaledMarginY = _margin /** scale.y*/;

        _totalWidth += scaledMarginX * 2;
        _totalHeight += scaledMarginY * 2;

		var restX = ( width - _totalWidth );
		var restY = ( height - _totalHeight );

		float dx = 0;
		float dy = 0;

		if( childCount > 1 )
		{
	        int spacingCount = ( childCount - 1 );

			float ddx = restX / spacingCount;
			float ddy = restY / spacingCount;

	        dx = Mathf.Clamp( ddx, _minimumSpacing, _maximumSpacing );
	        dy = Mathf.Clamp( ddy, _minimumSpacing, _maximumSpacing );

	        restX = ( ddx - dx ) * spacingCount;
	        restY = ( ddy - dy ) * spacingCount;
		}
        
        Vector2 startPos = new Vector2( rect.xMin + scaledMarginX, rect.yMin + scaledMarginY );
        Vector2 pos = startPos;

        switch( _type )
        {
            case EOrganizerType.VERTICAL:
            {
				pos.x += maxWidth / 2;
				restX = width - maxWidth;
				break;
            }

            case EOrganizerType.HORIZONTAL:
            {
				pos.y += maxHeight / 2 - scaledMarginY * 2;
				restY = height - maxHeight;
                break;
            }
        }

        switch( _align )
        {
            case SpriteAlignment.Center:
            case SpriteAlignment.BottomCenter:
            case SpriteAlignment.TopCenter:
                pos.x += restX / 2;
                break;

            case SpriteAlignment.RightCenter:
            case SpriteAlignment.BottomRight:
            case SpriteAlignment.TopRight:
                pos.x += restX;
                break;
        }

        switch( _align )
        {
            case SpriteAlignment.TopCenter:
            case SpriteAlignment.TopLeft:
            case SpriteAlignment.TopRight:
                pos.y += restY;
                break;

            case SpriteAlignment.Center:
            case SpriteAlignment.LeftCenter:
            case SpriteAlignment.RightCenter:
                pos.y += restY / 2;
                break;
        }

        for( int i = 0; i < transform.childCount; i++ )
        {
            var id = GetRealId( i );
            var child = transform.GetChild( id );
        

            if( /*!_countInactive &&*/ !child.gameObject.activeInHierarchy ) 
                continue;

            var childTransform = child.transform as RectTransform;
			var childRect = childTransform.rect;
            if(_ignoreList.Contains(childTransform)) continue;
			var childScale = childTransform.localScale;
            var pivot = childTransform.pivot;

            switch( _type )
            {
                // case EOrganizerType.GRID:
                case EOrganizerType.VERTICAL:
                {
					pos.y += pivot.y * childRect.height * childScale.y;

                    childTransform.localPosition = pos;

					pos.y += ( 1 - pivot.y ) * childRect.height * childScale.y; 
                    pos.y += dy;
                    break;
                }

                case EOrganizerType.HORIZONTAL:
                {
					pos.x += pivot.x * childRect.width * childScale.x;


                    childTransform.localPosition = pos;

					pos.x += ( 1 - pivot.x ) * childRect.width * childScale.x;
                    pos.x += dx;
                    break;
                }

                case EOrganizerType.DIAGONAL:
                {
					pos.x += pivot.x * childRect.width * childScale.x;
                    pos.y += pivot.y * childRect.height * childScale.y;

                    childTransform.localPosition = pos;

					pos.x += ( 1 - pivot.x ) * childRect.width * childScale.x; 
                    pos.x += dx; 
					pos.y += ( 1 - pivot.y ) * childRect.height * childScale.y; 
                    pos.y += dy;

                    break;
                }
            }
		}
		
		_afterOrganize.Trigger();
    }

    int GetRealId( int i )
    {
        if( _sort == EOrganizerSort.INVERSE ) return transform.childCount - ( i + 1 );
        return i;
    }

	void Update()
    {
        Organize();
    }
}
