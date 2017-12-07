using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using EFT;

namespace TarkovHax
{
    public class TarkovHaxBehaviour : MonoBehaviour
    {
        public TarkovHaxBehaviour()
        {
        }

        private GameObject GameObjectHolder;

        private IEnumerable<Player> _players;
        private IEnumerable<LootableContainer> _lootableContainers;

        private float _playersNextUpdateTime;
        private float _weaponBoxesNextUpdateTime;
        private float _espUpdateInterval = 1f;

        private bool _isESPMenuActive;
        private bool _showPlayersESP;
        private bool _showWeaponBoxesESP;

        private float _maxDrawingDistance = 15000f;

        private string _magicAppDomainString = "mvhYQbbHjrHM";

        public void Load()
        {
            GameObjectHolder = new GameObject();
            GameObjectHolder.AddComponent<TarkovHaxBehaviour>();

            DontDestroyOnLoad(GameObjectHolder);
        }

        public void Unload()
        {
            Destroy(GameObjectHolder);
            Destroy(this);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.End))
            {
                Unload();
            }
            if (Input.GetKeyDown(KeyCode.Insert))
            {
                _isESPMenuActive = !_isESPMenuActive;
            }
            if (Input.GetKeyDown(KeyCode.Keypad0))
            {
                OpenDoors();
            }
            if (Input.GetKeyDown(KeyCode.Keypad1))
            {
                TeleportItemsToPlayer();
            }
            if (Input.GetKeyDown(KeyCode.KeypadPlus))
            {
                IncreaseFov();
            }
            if (Input.GetKeyDown(KeyCode.KeypadMinus))
            {
                DecreaseFov();
            }
        }

        private void IncreaseFov()
        {
            Camera.main.fieldOfView += 1f;
        }

        private void DecreaseFov()
        {
            Camera.main.fieldOfView -= 1f;
        }

        private void OpenDoors()
        {
            var doors = FindObjectsOfType<Door>();
            foreach (var door in doors.Where(d => d.DoorState == WorldInteractiveObject.EDoorState.Locked))
            {
                door.DoorState = WorldInteractiveObject.EDoorState.Shut;
            }
        }

        // TODO: draw in direction player is looking
        // TODO: investigate why not all corpses are lootable
        private void TeleportItemsToPlayer()
        {
            var lootItems = FindObjectsOfType<LootItem>();
            float xOffset = 0f;
            float yOffset = 0f;
            foreach (var lootItem in lootItems)
            {
                var newPosition = new Vector3(
                    Camera.main.transform.position.x + xOffset,
                    Camera.main.transform.position.y,
                    Camera.main.transform.position.z + yOffset);

                lootItem.transform.position = newPosition;

                xOffset += 0.1f;
                yOffset += 0.1f;
            }
        }

        private void OnGUI()
        {
            if (_isESPMenuActive)
            {
                DrawESPMenu();
            }

            GUI.color = Color.red;
            GUI.Label(new Rect(10f, 10f, 1000f, 500f), "tarkov h4x");

            if (_showPlayersESP && Time.time >= _playersNextUpdateTime)
            {
                _players = FindObjectsOfType<Player>();
                _playersNextUpdateTime = Time.time + _espUpdateInterval;
            }

            if (_showWeaponBoxesESP && Time.time >= _weaponBoxesNextUpdateTime)
            {
                _lootableContainers = FindObjectsOfType<LootableContainer>();
                _weaponBoxesNextUpdateTime = Time.time + _espUpdateInterval;
            }

            if (_showWeaponBoxesESP)
            {
                DrawLootableContainers();
            }

            if (_showPlayersESP)
            {
                DrawPlayers();
            }
        }

        private void DrawLootableContainers()
        {
            foreach (var lootableContainer in _lootableContainers.Where(lc => lc.name == "weapon_box_cover"))
            {
                float distanceToObject = Vector3.Distance(Camera.main.transform.position, lootableContainer.transform.position);
                var viewTransform = new Vector3(
                    Camera.main.WorldToScreenPoint(lootableContainer.transform.position).x, 
                    Camera.main.WorldToScreenPoint(lootableContainer.transform.position).y, 
                    Camera.main.WorldToScreenPoint(lootableContainer.transform.position).z);

                if (distanceToObject <= _maxDrawingDistance && viewTransform.z > 0.01)
                {
                    GUI.color = Color.cyan;
                    int distance = (int)distanceToObject;
                    string boxText = $"[x] {distance}m";

                    GUI.Label(new Rect(viewTransform.x - 50f, (float)Screen.height - viewTransform.y, 100f, 50f), boxText);
                }
            }
        }

        private void DrawPlayers()
        {
            foreach (var player in _players)
            {
                float distanceToObject = Vector3.Distance(Camera.main.transform.position, player.Transform.position);
                var playerBoundingVector = new Vector3(
                    Camera.main.WorldToScreenPoint(player.Transform.position).x,
                    Camera.main.WorldToScreenPoint(player.Transform.position).y,
                    Camera.main.WorldToScreenPoint(player.Transform.position).z);

                if (distanceToObject <= _maxDrawingDistance && playerBoundingVector.z > 0.01)
                { 
                    var playerHeadVector = new Vector3(
                        Camera.main.WorldToScreenPoint(player.PlayerBones.Head.position).x,
                        Camera.main.WorldToScreenPoint(player.PlayerBones.Head.position).y,
                        Camera.main.WorldToScreenPoint(player.PlayerBones.Head.position).z);

                    float boxXOffset = Camera.main.WorldToScreenPoint(player.Transform.position).x;
                    float boxYOffset = Camera.main.WorldToScreenPoint(player.PlayerBones.Head.position).y + 10f;
                    float boxHeight = Math.Abs(Camera.main.WorldToScreenPoint(player.PlayerBones.Head.position).y - Camera.main.WorldToScreenPoint(player.Transform.position).y) + 10f;
                    float boxWidth = boxHeight * 0.65f;

                    var playerColor = player.AIData != null && player.AIData.IsAI ? Color.cyan : Color.red;
                    var espColor = player.Profile.Health.IsAlive ? playerColor : Color.white;
                    GUI.color = espColor;

                    GuiHelper.DrawBox(boxXOffset - boxWidth / 2f, (float)Screen.height - boxYOffset, boxWidth, boxHeight, espColor);
                    GuiHelper.DrawLine(new Vector2(playerHeadVector.x - 2f, (float)Screen.height - playerHeadVector.y), new Vector2(playerHeadVector.x + 2f, (float)Screen.height - playerHeadVector.y), espColor);
                    GuiHelper.DrawLine(new Vector2(playerHeadVector.x, (float)Screen.height - playerHeadVector.y - 2f), new Vector2(playerHeadVector.x, (float)Screen.height - playerHeadVector.y + 2f), espColor);

                    string playerName = player.Profile.Health.IsAlive ? player.Profile.Info.Nickname : player.Profile.Info.Nickname + " (DEAD)";
                    float playerHealth = player.HealthController.SummaryHealth.CurrentValue / 435f * 100f;
                    int distance = (int)distanceToObject;
                    string playerText = $"[{(int)playerHealth}%] {playerName} [{distance}m]";

                    var playerTextVector = GUI.skin.GetStyle(playerText).CalcSize(new GUIContent(playerText));
                    GUI.Label(new Rect(playerBoundingVector.x - playerTextVector.x / 2f, (float)Screen.height - boxYOffset - 20f, 300f, 50f), playerText);
                }
            }
        }

        private void DrawESPMenu()
        {
            GUI.color = Color.black;
            GUI.Box(new Rect(100f, 100f, 190f, 190f), "");

            GUI.color = Color.white;
            GUI.Label(new Rect(180f, 110f, 50f, 20f), "ESP");

            _showPlayersESP = GUI.Toggle(new Rect(110f, 140f, 120f, 20f), _showPlayersESP, "  Players");
            _showWeaponBoxesESP = GUI.Toggle(new Rect(110f, 160f, 120f, 20f), _showWeaponBoxesESP, "  Weapon boxes");
        }

        private double GetDistance(double x1, double y1, double x2, double y2)
        {
            return Math.Sqrt(Math.Pow(x2 - x1, 2.0) + Math.Pow(y2 - y1, 2.0));
        }
    }
}
