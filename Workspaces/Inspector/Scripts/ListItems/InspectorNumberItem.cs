﻿using UnityEditor;
using UnityEngine;
using UnityEngine.VR.UI;

public class InspectorNumberItem : InspectorPropertyItem
{
	[SerializeField]
	private NumericInputField m_InputField;

	public override void Setup(InspectorData data)
	{
		base.Setup(data);

		var val = string.Empty;
		switch (m_SerializedProperty.propertyType)
		{
			case SerializedPropertyType.ArraySize:
			case SerializedPropertyType.Integer:
				val = m_SerializedProperty.intValue.ToString();
				m_InputField.numberType = NumericInputField.NumberType.Int;
				break;
			case SerializedPropertyType.Float:
				val = m_SerializedProperty.floatValue.ToString();
				m_InputField.numberType = NumericInputField.NumberType.Float;
				break;
		}

		m_InputField.text = val;
		m_InputField.ForceUpdateLabel();
	}

	public void SetValue(string input)
	{
		switch (m_SerializedProperty.propertyType)
		{
			case SerializedPropertyType.ArraySize:
				int size;
				if (int.TryParse(input, out size) && m_SerializedProperty.intValue != size)
				{
					m_SerializedProperty.arraySize = size;

					m_InputField.text = size.ToString();
					m_InputField.ForceUpdateLabel();
					((PropertyData) data).updateParent();

					data.serializedObject.ApplyModifiedProperties();
				}
				break;
			case SerializedPropertyType.Integer:
				int i;
				if (int.TryParse(input, out i) && m_SerializedProperty.intValue != i)
				{
					m_SerializedProperty.intValue = i;

					m_InputField.text = i.ToString();
					m_InputField.ForceUpdateLabel();

					data.serializedObject.ApplyModifiedProperties();
				}
				break;
			case SerializedPropertyType.Float:
				float f;
				if (float.TryParse(input, out f) && !Mathf.Approximately(m_SerializedProperty.floatValue, f))
				{
					m_SerializedProperty.floatValue = f;

					m_InputField.text = f.ToString();
					m_InputField.ForceUpdateLabel();

					data.serializedObject.ApplyModifiedProperties();
				}
				break;
		}
	}

	protected override object GetDropObject(Transform fieldBlock)
	{
		return m_InputField.text;
	}

	public override bool TestDrop(GameObject target, object droppedObject)
	{
		return droppedObject is string;
	}

	public override bool RecieveDrop(GameObject target, object droppedObject)
	{
		SetValue(droppedObject.ToString());
		return true;
	}
}