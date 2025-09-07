using System;
using System.Collections;
using System.Collections.Generic;
using CommonBase;
using UnityEngine;
using Random = UnityEngine.Random;

namespace FishCollection
{
    public class GameManager : MonoSingleton<GameManager>
    {
        [Header("Game Settings")] [SerializeField]
        private float fieldSize = 50f;

        [Header("Spawn Settings")] [SerializeField]
        private float minSpawnInterval = 0.5f;

        [SerializeField] private float maxSpawnInterval = 3f;
        [SerializeField] private int maxFishAtOnce = 50;
        [SerializeField] private float specialFishChance = 0.2f;

        [Header("Prefabs")] [SerializeField] private GameObject sharkPrefab;
        [SerializeField] private GameObject fishPrefab;

        private SharkController shark;
        private List<FishBoid> allFish = new List<FishBoid>();
        private GameObject field;
        private bool isGameActive;


        [Header("Game State")] public FSMSO gameFSMSO;
        private FiniteStateMachine gameFSM;
        public static int FishIdSeed = 0;

        public System.Action<int> OnFishCountUpdate;

        void Start()
        {
            OnLoadGame();
            SetupGame();
            StartGame();
        }

        public void OnLoadGame()
        {
            FishIdSeed = PlayerPrefs.GetInt("fishSeed", 1);
        }

        public void OnSaveGame()
        {
            PlayerPrefs.SetInt("fishSeed", FishIdSeed);
        }

        void SetupGame()
        {
            CreateShark();
            gameFSM = new FiniteStateMachine(gameFSMSO);
            gameFSM.Start();
        }

        void CreateShark()
        {
            GameObject sharkGO = Instantiate(sharkPrefab, Vector3.zero + Vector3.up * 0.5f, Quaternion.identity);

            shark = sharkGO.GetComponent<SharkController>();
            if (shark == null)
            {
                shark = sharkGO.AddComponent<SharkController>();
            }

            shark.OnFishEaten += OnFishEaten;
        }

        public void RegisterOnFishEaten(Action<SpecialFish> obj)
        {
            shark.HandleFishEaten += obj;
        }

        public void UnregisterOnFishEaten(Action<SpecialFish> obj)
        {
            shark.HandleFishEaten -= obj;
        }

        public void StartGame()
        {
            isGameActive = true;
            StartCoroutine(SpawnFishRoutine());
        }

        IEnumerator SpawnFishRoutine()
        {
            while (isGameActive)
            {
                if (allFish.Count < maxFishAtOnce)
                {
                    SpawnFish();
                }

                float waitTime = Random.Range(minSpawnInterval, maxSpawnInterval);
                yield return new WaitForSeconds(waitTime);
            }
        }

        void SpawnFish()
        {
            Vector3 spawnPosition = GetRandomSpawnPosition();
            GameObject fishGO;

            bool isSpecial = Random.Range(0f, 1f) < specialFishChance;

            if (fishPrefab != null)
            {
                fishGO = Instantiate(fishPrefab, spawnPosition, Quaternion.identity);
            }
            else
            {
                fishGO = new GameObject($"Fish_{allFish.Count}");
                fishGO.transform.position = spawnPosition;

                GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                sphere.transform.SetParent(fishGO.transform);
                sphere.transform.localPosition = Vector3.zero;
                sphere.transform.localScale = Vector3.one;
            }

            FishBoid fish = fishGO.GetComponent<FishBoid>();
            if (fish == null)
            {
                fish = fishGO.AddComponent<FishBoid>();
            }

            if (isSpecial)
            {
                SpecialFish special = fishGO.AddComponent<SpecialFish>();
            }

            fish.Initialize(Vector3.zero, fieldSize * 0.4f);
            allFish.Add(fish);

            OnFishCountUpdate?.Invoke(allFish.Count);
        }

        Vector3 GetRandomSpawnPosition()
        {
            float angle = Random.Range(0f, Mathf.PI * 2f);
            float radius = Random.Range(fieldSize * 0.2f, fieldSize * 0.4f);

            float x = Mathf.Cos(angle) * radius;
            float z = Mathf.Sin(angle) * radius;

            return new Vector3(x, 0.5f, z);
        }

        void OnFishEaten(int totalEaten)
        {
            allFish.RemoveAll(f => f.IsEaten);
            OnFishCountUpdate?.Invoke(allFish.Count);
        }

        private void Update()
        {
            gameFSM.Update();
        }

        private void OnDestroy()
        {
            this.OnSaveGame();
        }
    }
}