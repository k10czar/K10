using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public sealed class AnimatedOrganizer : MonoBehaviour
{
    enum EOrganizerDirection {horizontal, vertical}

    [SerializeField] float _offset;
    [SerializeField]  EOrganizerDirection _direction;

    [SerializeField] List<RectTransform> _siblings;

    void Update()
    {
        CheckSiblings();



    }

    public void CheckSiblings(){
       _siblings = new List<RectTransform>(transform.GetComponentsInChildren<RectTransform>());
    }
}
