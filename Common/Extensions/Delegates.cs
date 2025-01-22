using System.Collections;
using UnityEngine;

namespace K10
{
    public delegate Coroutine StartCoroutineReference(IEnumerator routine);
    public delegate void StopCoroutineReference(Coroutine routine);
}