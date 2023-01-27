using Unt.Demo.Runtime;
using System.Collections.Generic;
using UnityEngine;

namespace Unt.Demo.Game
{
    public class Player : MonoBehaviour
    {
        [Header("Network")]
        public uint ClientId;
        public bool IsLocal;

        [Header("Move")]
        public float Speed = 5f;
        public Transform Target;

        private void Start()
        {
            Target ??= gameObject.transform;
        }

        private void Update()
        {
            if (IsLocal)
            {
                Vector2 input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")) * Speed * Time.deltaTime;
                Target.Translate(input.x, input.y, 0);
            }
            
        }

        private void FixedUpdate()
        {
            if (IsLocal)
            {
                Packet packet = Packet.New(DataId.Move);

                packet.AddUInt(ClientId);

                packet.AddFloat(Target.position.x);
                packet.AddFloat(Target.position.y);
                packet.AddFloat(Target.position.z);

                NetworkManager.Client.Send_RPC(packet, false);
            }
        }
    }
}