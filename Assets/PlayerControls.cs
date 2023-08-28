using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerControls : MonoBehaviour, IPunObservable
{
    public PhotonView photonView;
    private SpriteRenderer spriteRenderer;
    //public PlayerControls Nickname;

    public Vector2Int Direction;
    public Vector2Int GamePosition;

    public Sprite OtherPlayerSprite;
    public Sprite DeadPlayerSprite;
    public Transform Ladder;
    public TextMeshPro NicknameText;

    public bool IsDead;
    public int Score = 0;

    public void SetLadderLength(int length)
    {
        for (int i = 0; i < Ladder.childCount; i++)
        {
            Ladder.GetChild(i).gameObject.SetActive(i < length);
        }
        while (Ladder.childCount < length)
        {
            Transform lastTile = Ladder.GetChild(Ladder.childCount - 1);
            Instantiate(lastTile, lastTile.position + Vector3.down, Quaternion.identity, Ladder);
        }
    }

    public void Kill()
    {
        IsDead = true;
        spriteRenderer.sprite = DeadPlayerSprite;
        SetLadderLength(0);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(Direction);
        }
        else
        {
            Direction = (Vector2Int)stream.ReceiveNext();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        photonView = GetComponent<PhotonView> ();
        spriteRenderer = GetComponent<SpriteRenderer>();

        GamePosition = new Vector2Int((int)transform.position.x, (int)transform.position.y);
        FindObjectOfType<MapController>().AddPlayer(this);

        NicknameText.SetText(photonView.Owner.NickName);
        if (!photonView.IsMine)
        {
            spriteRenderer.sprite = OtherPlayerSprite;
            NicknameText.color = Color.green;
        }
    }

    public static explicit operator PlayerControls(string v)
    {
        throw new NotImplementedException();
    }

    // Update is called once per frame
    void Update()
    {

        if (photonView.IsMine && !IsDead)
        {
            if (Input.GetKey(KeyCode.LeftArrow)) Direction = Vector2Int.left;
            if (Input.GetKey(KeyCode.RightArrow)) Direction = Vector2Int.right;
            if (Input.GetKey(KeyCode.UpArrow)) Direction = Vector2Int.up;
            if (Input.GetKey(KeyCode.DownArrow)) Direction = Vector2Int.down;
        }
        if (Direction == Vector2Int.left) spriteRenderer.flipX = false;
        if (Direction == Vector2Int.right) spriteRenderer.flipX = true;

        transform.position = Vector3.Lerp(transform.position, (Vector2)GamePosition, Time.deltaTime * 3);
    }
}
