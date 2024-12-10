using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using RacingOnline.Race;
using UnityEngine;
using UnityEngine.InputSystem;

public class CarInput : NetworkBehaviour, INetworkRunnerCallbacks
{
	public struct NetworkInputData : INetworkInput
	{
		public const uint ButtonAccelerate = 1 << 0;
		public const uint ButtonReverse = 1 << 1;
		public const uint ButtonLookbehind = 1 << 3;
		public uint Buttons;
		private int _steer;
		public float Steer
		{
			get => _steer * .001f;
			set => _steer = (int)(value * 1000);
		}
		public bool IsDown(uint button) => (Buttons & button) == button;
		public bool IsAccelerate => IsDown(ButtonAccelerate);
		public bool IsReverse => IsDown(ButtonReverse);
	}

	[SerializeField] private InputAction accelerate;
	[SerializeField] private InputAction reverse;
	[SerializeField] private InputAction steer;
	[SerializeField] private InputAction lookBehind;

	private bool _useItemPressed;

	public override void Spawned()
	{
		base.Spawned();

		Runner.AddCallbacks(this);

		accelerate = accelerate.Clone();
		reverse = reverse.Clone();
		steer = steer.Clone();
		lookBehind = lookBehind.Clone();

		accelerate.Enable();
		reverse.Enable();
		steer.Enable();
		lookBehind.Enable();
	}

	public override void Despawned(NetworkRunner runner, bool hasState)
	{
		base.Despawned(runner, hasState);

		DisposeInputs();
		Runner.RemoveCallbacks(this);
	}

	private void OnDestroy()
	{
		DisposeInputs();
	}

	private void DisposeInputs()
	{
		accelerate.Dispose();
		reverse.Dispose();
		steer.Dispose();
		lookBehind.Dispose();
	}

	private static bool ReadBool(InputAction action) => action.ReadValue<float>() != 0;
	private static float ReadFloat(InputAction action) => action.ReadValue<float>();

	public void OnInput(NetworkRunner runner, NetworkInput input)
	{
		var userInput = new NetworkInputData();

		if (ReadBool(accelerate)) userInput.Buttons |= NetworkInputData.ButtonAccelerate;
		if (ReadBool(reverse)) userInput.Buttons |= NetworkInputData.ButtonReverse;
		if (ReadBool(lookBehind)) userInput.Buttons |= NetworkInputData.ButtonLookbehind;

		userInput.Steer = ReadFloat(steer);

		input.Set(userInput);

		_useItemPressed = false;
	}

	public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
	public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
	public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }
	public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
	public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
	public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
	public void OnConnectedToServer(NetworkRunner runner) { }
	public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
	public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
	public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
	public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
	public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
	public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
	public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
	public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
	public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
	public void OnSceneLoadDone(NetworkRunner runner) { }
	public void OnSceneLoadStart(NetworkRunner runner) { }
}