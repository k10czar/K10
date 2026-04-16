using UnityEngine;

namespace K10.Common
{
	public static class TransformExtensions
	{
		public static bool Contains(this Transform parent, Transform candidate)
		{
			while (candidate != null)
			{
				if (parent == candidate) return true;
				candidate = candidate.parent;
			}

			return false;
		}

		public static bool ResizesWithParent(this RectTransform rectTransform, RectTransform.Axis axis)
		{
			if (axis is RectTransform.Axis.Horizontal)
				return !Mathf.Approximately(rectTransform.anchorMin.x, rectTransform.anchorMax.x);

			return !Mathf.Approximately(rectTransform.anchorMin.y, rectTransform.anchorMax.y);
		}

		public static void ReParent(this Transform transform, Transform parent)
		{
			transform.SetParent(parent);
			transform.Reset();
		}

		public static void Reset(this Transform transform)
		{
			transform.localPosition = Vector3.zero;
			transform.localRotation = Quaternion.identity;
			transform.localScale = Vector3.one;
		}

		public static void DestroyChildren(this Transform transform)
		{
			for (var i = transform.childCount - 1; i >= 0; i--)
				Object.Destroy(transform.GetChild(i).gameObject);
		}

		public static string GetFullPath(this Transform current)
		{
			var builder = StringBuilderPool.RequestEmpty();

			while (current != null)
			{
				if (builder.Length > 0)
					builder.Insert(0, '/');

				builder.Insert(0, current.name);
				current = current.parent;
			}

			var result = builder.ToString();
			builder.ReturnToPool();

			return result;
		}
	}
}