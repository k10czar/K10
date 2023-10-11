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
    // [SerializeField] List<RectTransform> _ignoreList = new List<RectTransform>();
    float _totalWidth, _totalHeight;

	private EventSlot _beforeOrganize;
	private EventSlot _afterOrganize;

	public IEventRegister BeforeOrganize => _beforeOrganize ?? ( _beforeOrganize = new EventSlot() );
	public IEventRegister AfterOrganize => _afterOrganize ?? ( _afterOrganize = new EventSlot() );

	public float TotalWidth => _totalWidth;
    public float TotalHeight => _totalHeight;

	public EOrganizerSort Sort { get { return _sort; } set { _sort = value; } }

    public void Organize()
    {
		_beforeOrganize?.Trigger();

		var rt = transform as RectTransform;
        var rect = rt.rect;
        var scale = rt.lossyScale;

        float width = rect.width;
		float height = rect.height;
		_totalHeight = 0;
		_totalWidth = 0;
		float maxHeight = 0;
		float maxWidth = 0;

        int childCount = rt.childCount;
        int activeChildCount = childCount;
        for( int i = 0; i < childCount; i++ )
        {
            var child = rt.GetChild( i );
            
            var childRectTransform = child.transform as RectTransform;
            // if(_ignoreList.Contains(childRectTransform)) continue;

            var childRect = childRectTransform.rect;
			var childScale = childRectTransform.localScale;

            if( _countInactive || child.gameObject.activeInHierarchy )
			{
				var w = childRect.width * childScale.x;
				var h = childRect.height * childScale.y;
                _totalWidth += w;
				_totalHeight += h;
				if( w > maxWidth ) maxWidth = w;
				if( h > maxHeight ) maxHeight = h;
            }
            else
            {
                activeChildCount--;
            }
        }

		if( activeChildCount == 0 )
			return;

        float scaledMarginX = _margin /** scale.x*/;
        float scaledMarginY = _margin /** scale.y*/;

        _totalWidth += scaledMarginX * 2;
        _totalHeight += scaledMarginY * 2;

		var restX = ( width - _totalWidth );
		var restY = ( height - _totalHeight );

		float dx = 0;
		float dy = 0;

		if( activeChildCount > 1 )
		{
	        int spacingCount = ( activeChildCount - 1 );

			float ddx = restX / spacingCount;
			float ddy = restY / spacingCount;

            if( ddx < _minimumSpacing ) dx = _minimumSpacing;
            else if( ddx > _maximumSpacing )  dx = _maximumSpacing;
            else dx = ddx;

            if( ddy < _minimumSpacing ) dy = _minimumSpacing;
            else if( ddy > _maximumSpacing )  dy = _maximumSpacing;
            else dy = ddy;

	        restX = ( ddx - dx ) * spacingCount;
	        restY = ( ddy - dy ) * spacingCount;
		}
        
        Vector2 pos = new Vector2( rect.xMin + scaledMarginX, rect.yMin + scaledMarginY );

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

        for( int i = 0; i < childCount; i++ )
        {
            var id = GetRealId( i );
            var child = rt.GetChild( id );
        
            if( /*!_countInactive &&*/ !child.gameObject.activeInHierarchy ) 
                continue;

            var childTransform = child.transform as RectTransform;
			var childRect = childTransform.rect;
            // if(_ignoreList.Contains(childTransform)) continue;
			var childScale = childTransform.localScale;
            var pivot = childTransform.pivot;

            switch( _type )
            {
                // case EOrganizerType.GRID:
                case EOrganizerType.VERTICAL:
                {
                    var h = childRect.height;
                    var sh = h * childScale.y;
					pos.y += pivot.y * sh;

                    childTransform.localPosition = pos;

					pos.y += ( 1 - pivot.y ) * sh + dy;
                    break;
                }

                case EOrganizerType.HORIZONTAL:
                {
                    var w = childRect.width;

                    var sw = w * childScale.x;
					pos.x += pivot.x * sw;

                    childTransform.localPosition = pos;

					pos.x += ( 1 - pivot.x ) * sw + dx;
                    break;
                }

                case EOrganizerType.DIAGONAL:
                {
                    var w = childRect.width;
                    var h = childRect.height;
                    
                    var sw = w * childScale.x;
                    var sh = h * childScale.y;

					pos.x += pivot.x * sw;
                    pos.y += pivot.y * sh;

                    childTransform.localPosition = pos;

					pos.x += ( 1 - pivot.x ) * sw + dx;
					pos.y += ( 1 - pivot.y ) * sh + dy;

                    break;
                }
            }
		}
		
		_afterOrganize?.Trigger();
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
