using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public GameObject enemyPrefab;
    public GameObject bossPrefab;
    public GameObject[] powerupPrefabs;
    public GameObject[] resourcePrefabs;
    public int enemyCount;
    public int waveNumber = 1;
    public int spawnRange = 18;
    public int bosWaveInterval = 5;
    private int nextBosWave;
    public int bossAmount = 3;
    private int finalBossWave;
    public int maxEnemies = 5;
    public int powerupInterval = 2;
    private int nextPowerup;
    public int resourceInteval = 2;
    private int nextResource;

    public GameObject GameVictoryUI;
    public GameObject MainGUI;

    // Start is called before the first frame update
    void Start()
    {
        SpawnEnemyWave(waveNumber);
        nextBosWave = bosWaveInterval;
        finalBossWave = nextBosWave * bossAmount;
        nextPowerup = powerupInterval;
        nextResource = resourceInteval;
    }

    // Update is called once per frame
    void Update()
    {
        enemyCount = FindObjectsOfType<AIController>().Length;
        if(enemyCount == 0)
        {
            waveNumber++;
            if(waveNumber == ((bossAmount * bosWaveInterval) + 1))
            {
                GameVictoryUI.SetActive(true);
                MainGUI.SetActive(false);
                Time.timeScale = 0f;
            }
            if(waveNumber == nextResource)
            {
                SpawnResource();
                nextResource += resourceInteval;
            }
            if(waveNumber == nextPowerup)
            {
                SpawnPowerup();
                nextPowerup += powerupInterval;
            }
            if(waveNumber == nextBosWave)
            {
                SpawnBoss();
                nextBosWave += bosWaveInterval;
            }
            else
            {
                if(waveNumber < maxEnemies){
                    SpawnEnemyWave(waveNumber);
                }
                else
                {
                    SpawnEnemyWave(maxEnemies);
                }
            }
        }
        
    }

    void SpawnEnemyWave(int enemyAmount)
    {
        for (int i = 0; i < enemyAmount; i++)
        {
            Instantiate(enemyPrefab, GenerateSpawnPosition(), enemyPrefab.transform.rotation);
        }
    }

    void SpawnBoss()
    {
        Instantiate(bossPrefab, GenerateSpawnPosition(), bossPrefab.transform.rotation);
    }

    void SpawnPowerup()
    {
        int index = Random.Range(0, powerupPrefabs.Length);
        Instantiate(powerupPrefabs[index], GenerateSpawnPosition(), powerupPrefabs[index].transform.rotation);
    }

    void SpawnResource()
    {
        int index = Random.Range(0, resourcePrefabs.Length);
        Instantiate(resourcePrefabs[index], GenerateSpawnPosition(), resourcePrefabs[index].transform.rotation);
    }

    private Vector3 GenerateSpawnPosition()
    {
        float spawnPosX = Random.Range(-spawnRange, spawnRange);
        float spawnPosZ = Random.Range(-spawnRange, spawnRange);
        Vector3 randomPos = new Vector3(spawnPosX, 1, spawnPosZ);
        return randomPos;
    }
}
