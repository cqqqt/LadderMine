using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class MapController : MonoBehaviour, IOnEventCallback
{
    public GameObject CellPrefab;

    public PlayersTop Top;

    private GameObject[,] cells;
    public List<PlayerControls> players = new List<PlayerControls>();
    private double lastTickTime;

    public void AddPlayer(PlayerControls player)
    {
        players.Add(player);

        cells[player.GamePosition.x, player.GamePosition.y].SetActive(false);
    }

    // Start is called before the first frame update
    void Start()
    {
        cells = new GameObject[20, 10];
        for (int x = 0; x < cells.GetLength(0); x++)
        {
            for (int y = 0; y < cells.GetLength(1); y++)
            {
                cells[x, y] = Instantiate(CellPrefab, new Vector3(x,y), Quaternion.identity, transform);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(PhotonNetwork.Time > lastTickTime + 1 && 
            PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom.PlayerCount >= 2)
        {
            // Разослать всем событие
            Vector2Int[] directions = players
                .Where(p=>!p.IsDead)
                .OrderBy(p => p.photonView.Owner.ActorNumber)
                .Select(p => p.Direction)
                .ToArray();

            RaiseEventOptions options = new RaiseEventOptions { Receivers = ReceiverGroup.Others };
            SendOptions sendOptions = new SendOptions { Reliability = true };
            PhotonNetwork.RaiseEvent(42, directions, options, sendOptions);
            // Сделать шаг игры
            PerformTick(directions);
        }
    }

    public void SendSyncData (Player player)
    {
        // Создать объект data, содержащий все данные для синхронизации состояния мира
        SyncData data = new SyncData();

        // Заполнить положения и счета игроков в data
        data.Positions = new Vector2Int[players.Count];
        data.Scores = new int[players.Count];

        PlayerControls[] sortedPlayers = players
                .Where(p => !p.IsDead)
                .OrderBy(p => p.photonView.Owner.ActorNumber)
                .ToArray();
        for (int i = 0; i < sortedPlayers.Length; i++)
        {
            data.Positions[i] = sortedPlayers[i].GamePosition;
            data.Scores[i] = sortedPlayers[i].Score;
        }

        // Заполнить MapData состояниями всех блоков (выкопан/не выпокан)
        data.MapData = new BitArray(20 * 10);
        for (int x = 0; x < cells.GetLength(0); x++)
        {
            for (int y = 0; y < cells.GetLength(1); y++)
            {
                data.MapData.Set(x + y * cells.GetLength(0), cells[x,y].activeSelf);
            }
        }
        RaiseEventOptions options = new RaiseEventOptions { TargetActors = new[] { player.ActorNumber } };
        SendOptions sendOptions = new SendOptions { Reliability = true };
        PhotonNetwork.RaiseEvent(43, data, options, sendOptions);
    }

    public void OnEvent(EventData photonEvent)
    {
        switch (photonEvent.Code)
        {
            case 42:
                Vector2Int[] directions = (Vector2Int[])photonEvent.CustomData;

                PerformTick(directions);

                break;
            case 43:
                SyncData data = (SyncData)photonEvent.CustomData;

                StartCoroutine(OnSyncDataReceived(data));

                break;
        }
    }

    private IEnumerator OnSyncDataReceived(SyncData data)
    {
        PlayerControls[] sortedPlayers;
        do
        {
            yield return null;
            sortedPlayers = players
                    .Where(p => !p.IsDead)
                    .Where(p => !p.photonView.IsMine)
                    .OrderBy(p => p.photonView.Owner.ActorNumber)
                    .ToArray();
        } while (sortedPlayers.Length != data.Positions.Length);
        for (int i = 0; i < sortedPlayers.Length; i++)
        {
            sortedPlayers[i].GamePosition = data.Positions[i];
            sortedPlayers[i].Score = data.Scores[i];

            sortedPlayers[i].transform.position = (Vector2)sortedPlayers[i].GamePosition;
        }

        for (int x = 0; x < cells.GetLength(0); x++)
        {
            for (int y = 0; y < cells.GetLength(1); y++) 
            {
                bool cellActive = data.MapData.Get(x + y * cells.GetLength(0));
                if (!cellActive) cells[x, y].SetActive(false);
            }
        }
    }

    private void PerformTick(Vector2Int[] directions)
    {
        if (players.Count != directions.Length) return;

        PlayerControls[] sortedPlayers = players
            .Where(p => !p.IsDead)
            .OrderBy(p => p.photonView.Owner.ActorNumber)
            .ToArray();

        int i = 0;
        foreach (var player in sortedPlayers)
        {
            player.Direction = directions[i++];
            MinePlayerBlock(player);
        }

        foreach (var player in sortedPlayers)
        {
            MovePlayer(player);
        }

        foreach (var player in players.Where(p => p.IsDead))
        {
            Vector2Int pos = player.GamePosition;
            while (pos.y > 0 && !cells[pos.x, pos.y - 1].activeSelf)
            {
                pos.y--;
            }
            player.GamePosition = pos;
        }

        Top.SetTexts(players);
        lastTickTime = PhotonNetwork.Time;
    }

    private void MinePlayerBlock(PlayerControls player)
    {
        if (player.Direction == Vector2Int.zero) return;

        Vector2Int targetPosition = player.GamePosition + player.Direction;

        // Копаем блок:
        if (targetPosition.x < 0) return;
        if (targetPosition.y < 0) return;
        if (targetPosition.x >= cells.GetLength(0)) return;
        if (targetPosition.y >= cells.GetLength(1)) return;

        if (cells[targetPosition.x, targetPosition.y].activeSelf) {
            cells[targetPosition.x, targetPosition.y].SetActive(false);
            player.Score++;
        }
        // Проверяем, не убило ли нас этим копанием:

        Vector2Int pos = targetPosition;
        PlayerControls minePlayer = players.First(p => p.photonView.IsMine);
        if(minePlayer != player)
            while (pos.y < cells.GetLength(1) && !cells[pos.x, pos.y].activeSelf)
            {
                if (pos == minePlayer.GamePosition)
                {
                    PhotonNetwork.LeaveRoom();
                    break;
                }
                pos.y++;
            }
    }
    private void MovePlayer(PlayerControls player)
    {
        player.GamePosition += player.Direction;

        if (player.GamePosition.x < 0) player.GamePosition.x = 0;
        if (player.GamePosition.y < 0) player.GamePosition.y = 0;
        if (player.GamePosition.x >= cells.GetLength(0)) player.GamePosition.x = cells.GetLength(0) - 1;
        if (player.GamePosition.y >= cells.GetLength(1)) player.GamePosition.y = cells.GetLength(1) - 1;

        int ladderLength = 0;
        Vector2Int pos = player.GamePosition;
        while (pos.y > 0 && !cells[pos.x, pos.y - 1].activeSelf)
        {
            ladderLength++;
            pos.y--;
        }
        player.SetLadderLength(ladderLength);
    }
    public void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }
    public void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }
}
