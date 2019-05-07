using UnityEngine;
using System.Collections;

public class NetworkManager : MonoBehaviour {

	private const string typeName = "Leap08051993";
	private const string gameName = "LeapRoom";

	private bool isRefreshingHostList = false;
	private HostData[] hostList;

	public GameObject playerPrefab;
	public GameObject glassBreakPrefab;
	public GameObject mainCamera;

	public GameObject player;
	
	private void StartServer()
	{
		Network.InitializeServer(4, 25000, !Network.HavePublicAddress());
		MasterServer.RegisterHost(typeName, gameName);
	}

	void OnServerInitialized()
	{
		SpawnPlayer();
	}

	void OnGUI()
	{
		if (!Network.isClient && !Network.isServer)
		{
			if (GUI.Button(new Rect(100, 100, 250, 100), "Start Server"))
				StartServer();
			
			if (GUI.Button(new Rect(100, 250, 250, 100), "Refresh Hosts"))
				RefreshHostList();
			
			if (hostList != null)
			{
				for (int i = 0; i < hostList.Length; i++)
				{
					if (GUI.Button(new Rect(400, 100 + (110 * i), 300, 100), hostList[i].gameName))
						JoinServer(hostList[i]);
				}
			}
		}
	}

	void Update()
	{
		if (isRefreshingHostList && MasterServer.PollHostList().Length > 0)
		{
			isRefreshingHostList = false;
			hostList = MasterServer.PollHostList();
		}
		DontDestroyOnLoad (player);
		DontDestroyOnLoad (gameObject);

	}

	private void RefreshHostList()
	{
		MasterServer.RequestHostList(typeName);
	}
	
	void OnMasterServerEvent(MasterServerEvent msEvent)
	{
		if (msEvent == MasterServerEvent.HostListReceived)
			hostList = MasterServer.PollHostList();
	}

	private void JoinServer(HostData hostData)
	{
		Network.Connect(hostData);
	}
	
	void OnConnectedToServer()
	{
		SpawnPlayer ();
	}

	private void SpawnPlayer()
	{
		if(Application.loadedLevelName == "Skyline") {
			Network.Instantiate(playerPrefab, new Vector3(-79.06138f, 17.56167f, 0f), Quaternion.identity, 0);
			Network.Instantiate(mainCamera, new Vector3(-79.06138f, 17.56167f, -10f), Quaternion.identity, 0);
		} else if(Application.loadedLevelName == "Desert City") {
			Network.Instantiate(playerPrefab, new Vector3(4.19f, 44.25f, 0f), Quaternion.identity, 0);
			Network.Instantiate(mainCamera, new Vector3(4.19f, 44.25f, -10f), Quaternion.identity, 0);
		}
		else if(Application.loadedLevelName == "insideShip") {
			player = Network.Instantiate(playerPrefab, new Vector3(0f, 3f, 0f), Quaternion.identity, 0) as GameObject;
		}
		
		((MonoBehaviour)playerPrefab.GetComponent("PlayerControl")).enabled = true;
	}

}
