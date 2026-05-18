
using System;
using Unity.Netcode;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class PlayerNet : NetworkBehaviour
{

    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private float currHealth;
    [SerializeField] private Transform spawnedObjectPrefab;
    private Transform spawnedObjectTransform;
    
    private NetworkVariable<int> randomNumber = new NetworkVariable<int>(1,NetworkVariableReadPermission.Everyone,NetworkVariableWritePermission.Owner);

    private NetworkVariable<float> health = new NetworkVariable<float>(100, NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    public override void OnNetworkSpawn()
    {
        randomNumber.OnValueChanged += (int previousValue, int newValue) =>
        {
            Debug.Log(OwnerClientId+" newValue: " +  newValue);
        };

        health.OnValueChanged += HealthChange;
    }

    private void HealthChange(float previousValue, float newValue)
    {
        Debug.Log($"{OwnerClientId} health changed: {previousValue} -> {newValue}");
        currHealth = newValue;
        text.text = currHealth.ToString();
    }

    //在服务器端执行
    [ServerRpc]
    private void HealthServerRpc()
    {
        health.Value -= 10;
        
    }
    private void Update()
    {
        if(!IsOwner)return;
        
        //if(Input.GetKeyDown(KeyCode.Space)) randomNumber.Value = Random.Range(0, 100);

        if(Input.GetKeyDown(KeyCode.R))
            HealthServerRpc();


        
        TestSpawn();

        TestRpc();

        Move();
    }
    

    private void TestRpc()
    {
        if(Input.GetKeyDown(KeyCode.C)) TextServerRpc(new ServerRpcParams());
        if(Input.GetKeyDown(KeyCode.X)) TextClientRpc(new ClientRpcParams{Send = new ClientRpcSendParams{TargetClientIds = new List<ulong>{1}}});
    }

    private void TestSpawn()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        { 
            spawnedObjectTransform = Instantiate(spawnedObjectPrefab);
            spawnedObjectTransform.GetComponent<NetworkObject>().SpawnWithOwnership(OwnerClientId);
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            Destroy(spawnedObjectTransform.gameObject);
        }
    }

    private void Move()
    {
        Vector3 moveDir = new Vector3(0, 0, 0);
        if (Input.GetKey(KeyCode.W)) moveDir.z = 1f;
        if (Input.GetKey(KeyCode.S)) moveDir.z = -1f;
        if (Input.GetKey(KeyCode.A)) moveDir.x = -1f;
        if (Input.GetKey(KeyCode.D)) moveDir.x = 1f;
        
        float moveSpeed = 5f;
        var dir = moveDir * moveSpeed;
        transform.position += dir * Time.deltaTime;
    }


    //客户端访问服务端
    [ServerRpc]
    private void TextServerRpc(ServerRpcParams @params)
    {
        Debug.Log(OwnerClientId + " TextServerRpc  " + @params.Receive.SenderClientId);
    }

    //服务端访问客户端
    [ClientRpc]
    private void TextClientRpc(ClientRpcParams @params)
    {
        Debug.Log(OwnerClientId);
    }
}
