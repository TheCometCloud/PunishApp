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

    public static Game Game;

    public const byte PlayCardEventCode = 1;
    public const byte RematchEventCode  = 2;

    public List<Card>   Hand            => PhotonNetwork.IsMasterClient ? Game.Host.Hand : Game.Guest.Hand;
    public List<Card>   TheirHand       => PhotonNetwork.IsMasterClient ? Game.Guest.Hand : Game.Host.Hand;
    public int          MyHealth        => PhotonNetwork.IsMasterClient ? Game.Host.Health : Game.Guest.Health;
    public int          TheirHealth     => PhotonNetwork.IsMasterClient ? Game.Guest.Health : Game.Host.Health;
    public bool         IAmMaster       => PhotonNetwork.IsMasterClient;
    public bool?        MyRematch       => PhotonNetwork.IsMasterClient ? Game.HostRematch : Game.GuestRematch;
    public bool?        TheirRematch    => PhotonNetwork.IsMasterClient ? Game.GuestRematch : Game.HostRematch;

    #endregion

    #region Photon Callbacks

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(Game.Host.Health);
            stream.SendNext(Game.Host.Hand.Select(c => c.Priority).ToArray());

            stream.SendNext(Game.Guest.Health);
            stream.SendNext(Game.Guest.Hand.Select(c => c.Priority).ToArray());
        }
        else
        {
            // Network player, receive data
            Game.Host.Health = (int)stream.ReceiveNext();
            Game.Host.Hand = ((int[])stream.ReceiveNext()).Select(p => Game.Cards.Where(c => c.Priority == p).First()).ToList();

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

    public void PlayCard(int index)
    {
        object[] content = new object[] { index };
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient };
        PhotonNetwork.RaiseEvent(PlayCardEventCode, content, raiseEventOptions, SendOptions.SendReliable); 
    }
    
    public void DecideRematch(bool rematch)
    {
        object[] content = new object[] { rematch };
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient };
        PhotonNetwork.RaiseEvent(RematchEventCode, content, raiseEventOptions, SendOptions.SendReliable); 
    }

    public void OnEvent(EventData photonEvent)
    {
        byte eventCode = photonEvent.Code;

        switch (eventCode)
        {
            case PlayCardEventCode:
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
                break;
            }

            case RematchEventCode:
            {
                Debug.Log("Running it back.");
                object[] data = (object[])photonEvent.CustomData;

                bool rematch = (bool)data[0];
                switch(photonEvent.Sender)
                {
                    default:
                        break;
                    
                    case 1:
                        Game.HostRematch = rematch;
                        break;

                    case 2:
                        Game.GuestRematch = rematch;
                        break;
                }
                break;
            }

            default:
                break;
        }
    }

    #endregion

    #region MonoBehavior Overrides

    void Start()
    {
        if (Game is null)
            Game = new Game();
    }

    void Update()
    {
        if (!(Game.GuestLock is null || Game.HostLock is null))
        {
            Game.TriggerTurn();
        }

        if (Game.GameOver)
        {
            PhotonNetwork.LoadLevel("Match Result");
            Game.GameOver = false;
        }

        if (Game.GuestRematch is null || Game.HostRematch is null)
        {
            // pass
        }
        else if ((bool)Game.GuestRematch && (bool)Game.HostRematch)
        {
            PhotonNetwork.LoadLevel("Room for 1");
            Game = new Game();
        }
        else
        {
            LeaveRoom();
        }
    }

    #endregion

    #region Private Methods

    void LoadArena()
    {
        PhotonNetwork.LoadLevel("Room for 1");
    }

    #endregion

    #region Public Methods

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    public void NewMatch()
    {
        LoadArena();
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
