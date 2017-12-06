using System;
using EFT;
using UnityEngine;

namespace TarkovHax
{
    public class TarkovHaxBehaviour : MonoBehaviour
    {
        public GameObject GameObjectHolder;

        private float _playerNextUpdateTime;
        private float _lootNextUpdateTime;
        private float _lootContainerNextUpdateTime;

        private UnityEngine.Object[] _playerObjects;
        private UnityEngine.Object[] _lootableObjects;
        private UnityEngine.Object[] _lootableContainerObjects;

        private bool _isESPMenuActive;
        private bool _showPlayersESP;
        private bool _showLootESP;
        private bool _showLootableContainersESP;

        private float _maxDrawingDistance = 15000f;

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
                ToggleNightVision();
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
            var doors = FindObjectsOfType(typeof(Door));
            foreach (Door door in doors)
            {
                if (door.DoorState == WorldInteractiveObject.EDoorState.Locked)
                {
                    door.DoorState = WorldInteractiveObject.EDoorState.Shut;
                }
            }
        }

        private bool _nightVisionOn = false;
        private void ToggleNightVision()
        {
            //var camera = (PlayerCameraController)FindObjectOfType(typeof(PlayerCameraController));
            //if (_nightVisionOn)
            //{
            //    var component = camera.gameObject.AddComponent<NightVision>();
            //    if (component != null)
            //    {
            //        DestroyImmediate(component);
            //    }
            //}
            //else
            //{
            //    camera.gameObject.AddComponent<NightVision>();
            //}

            //_nightVisionOn = !_nightVisionOn;
        }

        private void OnGUI()
        {
            if (_isESPMenuActive)
            {
                DrawESPMenu();
            }

            GUI.color = Color.red;
            GUI.Label(new Rect(10f, 10f, 100f, 50f), "tarkov h4x");

            if (Time.time >= _playerNextUpdateTime)
            {
                _playerObjects = FindObjectsOfType(typeof(Player));
                _playerNextUpdateTime = Time.time + 0.5f;
            }

            if (_showLootESP && Time.time >= _lootNextUpdateTime)
            {
                _lootableObjects = FindObjectsOfType(typeof(LootItem));
                _lootNextUpdateTime = Time.time + 0.01f;
            }

            if (_showLootableContainersESP && Time.time >= _lootContainerNextUpdateTime)
            {
                _lootableContainerObjects = FindObjectsOfType(typeof(LootableContainer));
                _lootContainerNextUpdateTime = Time.time + 0.01f;
            }

            if (_showLootESP)
            {
                DrawLoot();
            }

            if (_showLootableContainersESP)
            {
                DrawLootableContainers();
            }

            if (_showPlayersESP)
            {
                DrawPlayers();
            }
        }

        private void DrawLoot()
        {
            foreach (LootItem lootItem in _lootableObjects)
            {
                float distanceToObject = Vector3.Distance(Camera.main.transform.position, lootItem.transform.position);
                var viewTransform = new Vector3(
                    Camera.main.WorldToScreenPoint(lootItem.transform.position).x, 
                    Camera.main.WorldToScreenPoint(lootItem.transform.position).y, 
                    Camera.main.WorldToScreenPoint(lootItem.transform.position).z);

                if (distanceToObject <= _maxDrawingDistance && viewTransform.z > 0.01f)
                {
                    GUI.color = Color.green;
                    GUI.Label(new Rect(viewTransform.x - 50f, (float)Screen.height - viewTransform.y, 100f, 50f), lootItem.name);
                }
            }
        }

        private void DrawLootableContainers()
        {
            foreach (LootableContainer lootableContainer in _lootableContainerObjects)
            {
                float distanceToObject = Vector3.Distance(Camera.main.transform.position, lootableContainer.transform.position);
                var viewTransform = new Vector3(
                    Camera.main.WorldToScreenPoint(lootableContainer.transform.position).x, 
                    Camera.main.WorldToScreenPoint(lootableContainer.transform.position).y, 
                    Camera.main.WorldToScreenPoint(lootableContainer.transform.position).z);

                if (distanceToObject <= _maxDrawingDistance && viewTransform.z > 0.01f)
                {
                    GUI.color = Color.cyan;
                    GUI.Label(new Rect(viewTransform.x - 50f, (float)Screen.height - viewTransform.y, 100f, 50f), lootableContainer.name);
                }
            }
        }

        private void DrawPlayers()
        {
            foreach (Player player in _playerObjects)
            {
                var playerBoundingVector = new Vector3(
                    Camera.main.WorldToScreenPoint(player.Transform.position).x,
                    Camera.main.WorldToScreenPoint(player.Transform.position).y,
                    Camera.main.WorldToScreenPoint(player.Transform.position).z);

                var playerHeadVector = new Vector3(
                    Camera.main.WorldToScreenPoint(player.PlayerBones.Head.position).x,
                    Camera.main.WorldToScreenPoint(player.PlayerBones.Head.position).y,
                    Camera.main.WorldToScreenPoint(player.PlayerBones.Head.position).z);

                float distanceToObject = Vector3.Distance(Camera.main.transform.position, player.Transform.position);
                float boxXOffset = Camera.main.WorldToScreenPoint(player.Transform.position).x;
                float boxYOffset = Camera.main.WorldToScreenPoint(player.PlayerBones.Head.position).y + 10f;
                float boxHeight = Math.Abs(Camera.main.WorldToScreenPoint(player.PlayerBones.Head.position).y - Camera.main.WorldToScreenPoint(player.Transform.position).y) + 10f;
                float boxWidth = boxHeight * 0.65f;

                if (distanceToObject <= _maxDrawingDistance && playerBoundingVector.z > 0.01)
                {
                    var uiColor = player.Profile.Health.IsAlive ? Color.red : Color.white;
                    GUI.color = uiColor;

                    GuiHelper.DrawBox(boxXOffset - boxWidth / 2f, (float)Screen.height - boxYOffset, boxWidth, boxHeight, uiColor);
                    GuiHelper.DrawLine(new Vector2(playerHeadVector.x - 2f, (float)Screen.height - playerHeadVector.y), new Vector2(playerHeadVector.x + 2f, (float)Screen.height - playerHeadVector.y), uiColor);
                    GuiHelper.DrawLine(new Vector2(playerHeadVector.x, (float)Screen.height - playerHeadVector.y - 2f), new Vector2(playerHeadVector.x, (float)Screen.height - playerHeadVector.y + 2f), uiColor);

                    string playerName = player.Profile.Health.IsAlive ? player.Profile.Info.Nickname : player.Profile.Info.Nickname + " (DEAD)";
                    int playerHealth = (int)player.HealthController.SummaryHealth.CurrentValue / 435 * 100;
                    int distance = (int)distanceToObject;
                    string playerText = $"[{playerHealth}%] {playerName} [{distance}m]";

                    var playerTextVector = GUI.skin.GetStyle(playerText).CalcSize(new GUIContent(playerText));
                    GUI.Label(new Rect(playerBoundingVector.x - playerTextVector.x / 2f, (float)Screen.height - boxYOffset - 20f, 300f, 50f), playerText);

                    var weaponTextVector = GUI.skin.GetStyle(player.Weapon.Template.ShortName).CalcSize(new GUIContent(player.Weapon.Template.ShortName));
                    GUI.Label(new Rect(playerBoundingVector.x - weaponTextVector.x / 2f, (float)Screen.height - playerBoundingVector.y + 2f, 100f, 20f), player.Weapon.Template.ShortName);
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
            _showLootESP = GUI.Toggle(new Rect(110f, 160f, 120f, 20f), _showLootESP, "  Loot");
            _showLootableContainersESP = GUI.Toggle(new Rect(110f, 180f, 120f, 20f), _showLootableContainersESP, "  Lootables");
        }

        private double GetDistance(double x1, double y1, double x2, double y2)
        {
            return Math.Sqrt(Math.Pow(x2 - x1, 2.0) + Math.Pow(y2 - y1, 2.0));
        }
    }
}
