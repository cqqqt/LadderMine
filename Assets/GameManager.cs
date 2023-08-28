using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviourPunCallbacks
{
    public GameObject PlayerPrefab;
    public MapController MapController;
    public InputField NicknameInput;

    // Start is called before the first frame update
    void Start()
    {
        Vector3 pos = new Vector3(UnityEngine.Random.Range(1, 15), UnityEngine.Random.Range(1, 5));
        PhotonNetwork.Instantiate(PlayerPrefab.name, pos, Quaternion.identity);

        PhotonPeer.RegisterType(typeof(Vector2Int), 242, SerializeVector2Int, DeserializeVector2Int);
        PhotonPeer.RegisterType(typeof(SyncData), 243, SyncData.Serialize, SyncData.Deserialize);
    }

    // Update is called once per frame
    
    public void Leave()
    {
        PhotonNetwork.LeaveRoom();
    }
    public override void OnLeftRoom()
    {
        //  огда текущий игрок покидает комнату
        SceneManager.LoadScene(0);
    }
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if(PhotonNetwork.IsMasterClient)
        {
            MapController.SendSyncData(newPlayer);
        }

        Debug.LogFormat("Player {0} entered room", newPlayer.NickName);
    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        //PlayerControls Nickname = (PlayerControls)PhotonNetwork.NickName;
        //PlayerControls player = MapController.players.FirstOrDefault(p => p.Nickname.ToString().Contains(otherPlayer.NickName.ToString()));
        PlayerControls player = MapController.players.First(p => p.photonView.CreatorActorNr == otherPlayer.ActorNumber);
        Debug.LogFormat(player.ToString()); 
        Debug.LogFormat(otherPlayer.ToString());
        //PlayerControls player = MapController.players.First(p => p.photonView.Owner == null);

        //PlayerControls player = MapController.players.FirstOrDefault(p => p.photonView.Owner == null); 

        if (player != null) player.Kill();

        Debug.LogFormat("Player {0} left room", otherPlayer.NickName);
    }

    public static object DeserializeVector2Int(byte[] data)
    {
        Vector2Int result = new Vector2Int();

        result.x = BitConverter.ToInt32(data, 0);
        result.y = BitConverter.ToInt32(data, 4);

        return result;
    }

    public static byte[] SerializeVector2Int(object obj)
    {
        Vector2Int vector = (Vector2Int)obj;
        byte[] result = new byte[8];

        BitConverter.GetBytes(vector.x).CopyTo(result, 0);
        BitConverter.GetBytes(vector.y).CopyTo(result, 4);

        return result;
    }
}
