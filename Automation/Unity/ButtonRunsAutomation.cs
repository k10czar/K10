using System.Collections;
using System.Collections.Generic;
using Automation;
using K10;
using UnityEngine;
using UnityEngine.UI;

public class ButtonRunsAutomation : MonoBehaviour
{
    [SerializeField] bool runOnExternal = false;
    [SerializeField] bool log = true;
    [SerializeField] Button button;
    [SerializeReference,ExtendedDrawer(true)] IOperation operation;
    

    void Start()
    {
        if( !button ) button = GetComponent<Button>();
        button.onClick.AddListener( Run );
    }

    void OnDestroy()
    {
        button.onClick.RemoveListener( Run );
    }

    void Run()
    {
        operation.ExecuteOn( runOnExternal ? ExternalCoroutine.Instance : this, log );
    }
}
