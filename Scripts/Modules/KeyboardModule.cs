﻿#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor.Experimental.EditorVR.Core;
using UnityEditor.Experimental.EditorVR.Helpers;
using UnityEditor.Experimental.EditorVR.UI;
using UnityEditor.Experimental.EditorVR.Utilities;
using UnityEngine;

namespace UnityEditor.Experimental.EditorVR.Modules
{
	sealed class KeyboardModule : MonoBehaviour, ICustomRay, IForEachRayOrigin, IConnectInterfaces
	{
		[SerializeField]
		KeyboardMallet m_KeyboardMalletPrefab;

		[SerializeField]
		KeyboardUI m_NumericKeyboardPrefab;

		[SerializeField]
		KeyboardUI m_StandardKeyboardPrefab;

		readonly Dictionary<Transform, KeyboardMallet> m_KeyboardMallets = new Dictionary<Transform, KeyboardMallet>();
		KeyboardUI m_NumericKeyboard;
		KeyboardUI m_StandardKeyboard;

		public Action<ForEachRayOriginCallback> forEachRayOrigin { private get; set; }
		public ConnectInterfacesDelegate connectInterfaces { private get; set; }

		public KeyboardUI SpawnNumericKeyboard()
		{
			if (m_StandardKeyboard != null)
				m_StandardKeyboard.gameObject.SetActive(false);

			// Check if the prefab has already been instantiated
			if (m_NumericKeyboard == null)
			{
				m_NumericKeyboard = ObjectUtils.Instantiate(m_NumericKeyboardPrefab.gameObject, CameraUtils.GetCameraRig()).GetComponent<KeyboardUI>();
				var smoothMotions = m_NumericKeyboard.GetComponentsInChildren<SmoothMotion>(true);
				foreach (var smoothMotion in smoothMotions)
				{
					connectInterfaces(smoothMotion);
				}
			}

			return m_NumericKeyboard;
		}

		public KeyboardUI SpawnAlphaNumericKeyboard()
		{
			if (m_NumericKeyboard != null)
				m_NumericKeyboard.gameObject.SetActive(false);

			// Check if the prefab has already been instantiated
			if (m_StandardKeyboard == null)
			{
				m_StandardKeyboard = ObjectUtils.Instantiate(m_StandardKeyboardPrefab.gameObject, CameraUtils.GetCameraRig()).GetComponent<KeyboardUI>();
				var smoothMotions = m_StandardKeyboard.GetComponentsInChildren<SmoothMotion>(true);
				foreach (var smoothMotion in smoothMotions)
				{
					connectInterfaces(smoothMotion);
				}
			}

			return m_StandardKeyboard;
		}

		public void SpawnKeyboardMallet(Transform rayOrigin)
		{
			var malletTransform = ObjectUtils.Instantiate(m_KeyboardMalletPrefab.gameObject, rayOrigin).transform;
			malletTransform.position = rayOrigin.position;
			malletTransform.rotation = rayOrigin.rotation;
			var mallet = malletTransform.GetComponent<KeyboardMallet>();
			mallet.gameObject.SetActive(false);
			m_KeyboardMallets.Add(rayOrigin, mallet);
		}

		public void UpdateKeyboardMallets()
		{
			forEachRayOrigin(rayOrigin =>
			{
				var malletVisible = true;
				var numericKeyboardNull = false;
				var standardKeyboardNull = false;

				if (m_NumericKeyboard != null)
					malletVisible = m_NumericKeyboard.ShouldShowMallet(rayOrigin);
				else
					numericKeyboardNull = true;

				if (m_StandardKeyboard != null)
					malletVisible = malletVisible || m_StandardKeyboard.ShouldShowMallet(rayOrigin);
				else
					standardKeyboardNull = true;

				if (numericKeyboardNull && standardKeyboardNull)
					malletVisible = false;

				var mallet = m_KeyboardMallets[rayOrigin];

				if (mallet.visible != malletVisible)
				{
					mallet.visible = malletVisible;
					if (malletVisible)
						this.HideDefaultRay(rayOrigin);
					else
						this.ShowDefaultRay(rayOrigin);
				}

				// TODO remove this after physics is in
				mallet.CheckForKeyCollision();
			});
		}
	}
}
#endif
