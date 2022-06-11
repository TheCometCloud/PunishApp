using System;
using System.Collections.Generic;
using System.Linq;

using PlayerSlayer;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;

public class GameManager : MonoBehaviourPunCallbacks, IOnEventCallback, IPunObservable
{
    #region Public Fields

    public Game Game;

    public const byte PlayCardEventCode = 1;

    #endregion

    #region Photon Callbacks

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(Game.User.Health);
            stream.SendNext(Game.User.Hand.Select(c => c.Priority).ToArray());

            stream.SendNext(Game.Guest.Health);
            stream.SendNext(Game.Guest.Hand.Select(c => c.Priority).ToArray());
        }
        else
        {
            // Network player, receive data
            Game.User.Health = (int)stream.ReceiveNext();
            Game.User.Hand = ((int[])stream.ReceiveNext()).Select(p => Game.Cards.Where(c => c.Priority == p).First()).ToList();

            Game.Guest.Health = (int)stream.ReceiveNext();
            Game.Guest.Hand = ((int[])stream.ReceiveNext()).Select(p => Game.Cards.Where(c => c.Priority == p).First()).ToList();
        }
    }

    private void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    private void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    /// <summary>
    /// Called when the local player left the room. We need to load the launcher scene.
    /// </summary>
    public override void OnLeftRoom()
    {
        SceneManager.LoadScene(0);
    }

    public List<Card> Hand => PhotonNetwork.IsMasterClient ? Game.User.Hand : Game.Guest.Hand;
    public List<Card> TheirHand => PhotonNetwork.IsMasterClient ? Game.Guest.Hand : Game.User.Hand;
    public int MyHealth => PhotonNetwork.IsMasterClient ? Game.User.Health : Game.Guest.Health;
    public int TheirHealth => PhotonNetwork.IsMasterClient ? Game.Guest.Health : Game.User.Health;
    public bool IAmMaster => PhotonNetwork.IsMasterClient;

    public void PlayCard(int index)
    {
        object[] content = new object[] { index };
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient };
        PhotonNetwork.RaiseEvent(PlayCardEventCode, content, raiseEventOptions, SendOptions.SendReliable); 
    }

    public void OnEvent(EventData photonEvent)
    {
        byte eventCode = photonEvent.Code;

        if (eventCode == PlayCardEventCode)
        {
            Debug.Log("It's happening.");
            object[] data = (object[])photonEvent.CustomData;

            int index = (int)data[0];
            switch(photonEvent.Sender)
            {
                default:
                    break;
                
                case 1:
                    Game.HostLock = index;
                    break;

                case 2:
                    Game.GuestLock = index;
                    break;
            }
        }
    }

    #endregion

    #region MonoBehavior Overrides

    void Start()
    {
        Game = new Game();
    }

    void Update()
    {
        if (!(Game.GuestLock is null || Game.HostLock is null))
        {
            Game.TriggerTurn();
        }
    }

    #endregion

    #region Private Methods

    void LoadArena()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            Debug.LogError("PhotonNetwork : Trying to Load a level but we are not the master Client");
        }
        Debug.LogFormat("PhotonNetwork : Loading Level : 1", PhotonNetwork.CurrentRoom.PlayerCount);
        PhotonNetwork.LoadLevel("Room for 1");
    }

    #endregion

    #region Public Methods

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    #endregion

    #region Photon Callbacks

    public override void OnPlayerEnteredRoom(Player other)
    {
        Debug.LogFormat("OnPlayerEnteredRoom() {0}", other.NickName); // not seen if you're the player connecting


        if (PhotonNetwork.IsMasterClient)
        {
            Debug.LogFormat("OnPlayerEnteredRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient); // called before OnPlayerLeftRoom


            LoadArena();
        }
    }

    public override void OnPlayerLeftRoom(Player other)
    {
        Debug.LogFormat("OnPlayerLeftRoom() {0}", other.NickName); // seen when other disconnects


        if (PhotonNetwork.IsMasterClient)
        {
            Debug.LogFormat("OnPlayerLeftRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient); // called before OnPlayerLeftRoom


            LoadArena();
        }
    }

    #endregion
}
