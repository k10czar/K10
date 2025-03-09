using UnityEngine;

public class GetInputFieldText : IValueProvider<string>
{
    [SerializeField] TMPro.TMP_InputField _inputField;
    public string Value => _inputField.text;
}
