using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public GameObject enemyPrefab;
    public GameObject bossPrefab;
    public GameObject powerupPrefab;
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
    // Start is called before the first frame update
    void Start()
    {
        SpawnEnemyWave(waveNumber);
        nextBosWave = bosWaveInterval;
        finalBossWave = nextBosWave * bossAmount;
        nextPowerup = powerupInterval;
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
        Instantiate(powerupPrefab, GenerateSpawnPosition(), powerupPrefab.transform.rotation);
    }

    private Vector3 GenerateSpawnPosition()
    {
        float spawnPosX = Random.Range(-spawnRange, spawnRange);
        float spawnPosZ = Random.Range(-spawnRange, spawnRange);
        Vector3 randomPos = new Vector3(spawnPosX, 0, spawnPosZ);
        return randomPos;
    }

    // Update is called once per frame
    void Update()
    {
        enemyCount = FindObjectsOfType<AIController>().Length;
        if(enemyCount == 0)
        {   
            if(waveNumber == nextPowerup)
            {
                SpawnPowerup();
            }
            if(waveNumber == nextBosWave)
            {
                SpawnBoss();
                nextBosWave += bosWaveInterval;
                waveNumber++;
            }
            else
            {
                waveNumber++;
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
}
