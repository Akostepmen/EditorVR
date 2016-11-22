﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputNew;
using UnityEngine.VR.Actions;
using UnityEngine.VR.Tools;
using Object = UnityEngine.Object;

//[MainMenuItem("Cube", "Create", "Create cubes in the scene")]
[MainMenuItem(false)]
public class MakeCubeTool : MonoBehaviour, ITool, IStandardActionMap, IUsesRayOrigin, IActions
{
	class CubeToolAction : IAction
	{
		public Sprite icon { get; internal set; }
		public void ExecuteAction()
		{
		}
	}

	[SerializeField]
	Sprite m_Icon;

	readonly CubeToolAction m_CubeToolAction = new CubeToolAction();

	public List<IAction> actions { get; private set; }
	public Transform rayOrigin { get; set; }

	public Action<Object> addObjectToSpatialHash { get; set; }
	public Action<Object> removeObjectFromSpatialHash { get; set; }

	void Awake()
	{
		m_CubeToolAction.icon = m_Icon;
		actions = new List<IAction>() { m_CubeToolAction };
	}

	public void ProcessInput(ActionMapInput input, Action<InputControl> consumeControl)
	{
		var standardInput = (Standard)input;
		if (standardInput.action.wasJustPressed)
		{
			var cube = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
			if (rayOrigin)
				cube.position = rayOrigin.position + rayOrigin.forward * 5f;

			addObjectToSpatialHash(cube);

			consumeControl(standardInput.action);
		}
	}
}
