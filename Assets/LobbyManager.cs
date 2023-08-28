using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class LobbyManager : MonoBehaviourPunCallbacks
{

    public Text LogText;
    public InputField NicknameInput;
    // Start is called before the first frame update
    void Start()
    {
        string nickName = PlayerPrefs.GetString("NickName", "Player " + Random.Range(1, 9999));
        PhotonNetwork.NickName = nickName;
        NicknameInput.text = nickName;
        Log("Ваш ник " + PhotonNetwork.NickName + " установлен.");
        PhotonNetwork.GameVersion = "1";
        PhotonNetwork.ConnectUsingSettings();
        PhotonNetwork.AutomaticallySyncScene = true;

    }

    public override void OnConnectedToMaster()
    {
        Log("Подключение с сервером установлено.");
    }

    public void CreateRoom()
    {
        PhotonNetwork.NickName = NicknameInput.text;
        PlayerPrefs.SetString("NickName", NicknameInput.text);
        PhotonNetwork.CreateRoom(null, new Photon.Realtime.RoomOptions { MaxPlayers = 20, CleanupCacheOnLeave = false});
    }

    public void JoinRoom()
    {
        PhotonNetwork.NickName = NicknameInput.text;
        PlayerPrefs.SetString("NickName", NicknameInput.text);
        PhotonNetwork.JoinRandomRoom();
    }

    public override void OnJoinedRoom()
    {
        Log("Connected To Room");
        PhotonNetwork.LoadLevel("Game");
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Log("Failed to create room: " + message);
    }
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Log("Failed to join room: " + message);
    }

    private void Log(string message)
    {
        Debug.Log(message);
        LogText.text += "\n";
        LogText.text += message;
    }
}