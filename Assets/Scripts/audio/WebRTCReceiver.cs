using System;
using System.Collections;
using UnityEngine;
using SocketIOClient;
using Unity.WebRTC;

public class WebRTCReceiver : MonoBehaviour
{
    [Header("Audio Source")]
    public AudioSource audioSource;

    public string serverUrl = "http://localhost:3001";

    private RTCPeerConnection pc;
    private SocketIOClient.SocketIO client;

    IEnumerator Start()
    {
        WebRTC.Initialize();

        var config = new RTCConfiguration
        {
            iceServers = new[]
            {
                new RTCIceServer { urls = new[] { "stun:stun.l.google.com:19302" } }
            }
        };
        pc = new RTCPeerConnection(ref config);

        pc.OnTrack = e =>
        {
            if (e.Track is AudioStreamTrack audioTrack)
            {
                var clip = audioTrack.ToAudioClip();
                audioSource.clip = clip;
                audioSource.loop = true;
                audioSource.Play();
            }
        };

        client = new SocketIOClient.SocketIO(serverUrl, new SocketIOOptions
        {
            Transport = SocketIOClient.Transport.TransportProtocol.WebSocket
        });

        client.OnConnected += async (s, e) =>
        {
            Debug.Log("✅ Socket.IO connecté");
        };

        client.OnDisconnected += (s, e) =>
        {
            Debug.Log("⚠️ Socket.IO déconnecté");
        };

        client.On("webrtc-offer", async response =>
        {
            var sdpInit = response.GetValue<RTCSessionDescriptionInit>();
            var offer = new RTCSessionDescription
            {
                type = RTCSdpType.Offer,
                sdp = sdpInit.sdp
            };

            await pc.SetRemoteDescription(ref offer);
            var answer = await pc.CreateAnswer();
            await pc.SetLocalDescription(ref answer);

            await client.EmitAsync("webrtc-answer", new
            {
                type = "answer",
                sdp = answer.sdp
            });
        });

        client.On("webrtc-candidate", async response =>
        {
            var candidate = response.GetValue<RTCIceCandidateInit>();
            await pc.AddIceCandidate(candidate);
        });

        pc.OnIceCandidate = candidate =>
        {
            if (candidate != null)
                _ = client.EmitAsync("webrtc-candidate", candidate);
        };

        var connectTask = client.ConnectAsync();
        while (!connectTask.IsCompleted) yield return null;
    }

    private async void OnApplicationQuit()
    {
        pc.Close();
        WebRTC.Dispose();

        if (client != null && client.Connected)
        {
            try { await client.DisconnectAsync(); } catch { }
        }
    }
}