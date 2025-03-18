using System;
using System.Collections.Generic;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

public class MainBot : Bot
{
    private struct EnemyInfo
    {
        public int Id;
        public double X, Y;

        public EnemyInfo(int id, double x, double y)
        {
            Id = id;
            X = x;
            Y = y;
        }
    }

    private readonly List<EnemyInfo> enemies = new();
    private const double TARGET_DISTANCE = 100;
    private int bulletWallHitStreak = 0;  // Track consecutive wall hits
    private const int WALL_HIT_LIMIT = 3; // Max streak before switching target
    private bool stopScanning = false;

    public MainBot() : base(BotInfo.FromFile("MainBot.json")) { }

    public override void Run()
    {
        while (IsRunning)
        {
            if (!stopScanning)
            {
                TurnRadarRight(360); // Scan surroundings if allowed
            }
            MoveToNearestEnemy();
        }
    }

    public override void OnScannedBot(ScannedBotEvent e)
    {
        if (stopScanning) return;
        int scannedId = e.ScannedBotId;
        bool found = false;

        // Update enemy position if already in the list
        for (int i = 0; i < enemies.Count; i++)
        {
            if (enemies[i].Id == scannedId)
            {
                enemies[i] = new EnemyInfo(scannedId, e.X, e.Y);
                found = true;
                break;
            }
        }

        // If enemy is new, add it
        if (!found)
        {
            enemies.Add(new EnemyInfo(scannedId, e.X, e.Y));
        }
    }

    public override void OnBotDeath(BotDeathEvent e)
    {
        // Remove destroyed bot from the list
        enemies.RemoveAll(enemy => enemy.Id == e.VictimId);
    }

    private void MoveToNearestEnemy()
    {
        if (enemies.Count == 0)
        {
            TurnRadarRight(360); // Rescan if no enemies
            return;
        }

        // Find the nearest enemy
        EnemyInfo nearestEnemy = enemies[0];
        double minDistance = DistanceTo(nearestEnemy.X, nearestEnemy.Y);

        foreach (var enemy in enemies)
        {
            double distance = DistanceTo(enemy.X, enemy.Y);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearestEnemy = enemy;
            }
        }

        double enemyBearing = BearingTo(nearestEnemy.X, nearestEnemy.Y);
        TurnLeft(enemyBearing);

        Fire(1);

        if (minDistance > TARGET_DISTANCE)
        {
            Forward(minDistance - TARGET_DISTANCE); // Move closer
        }

        Fire(2);
    }

    // public override void OnBulletHitWall(BulletHitWallEvent e)
    // {
    //     bulletWallHitStreak++;
    
    //     if (bulletWallHitStreak >= WALL_HIT_LIMIT)
    //     {
    //         ChangeTarget();
    //     }
    // }

    // public override void OnBulletHit(BulletHitBotEvent e)
    // {
    //     stopScanning = false;
    //     bulletWallHitStreak = 0;  // Reset streak if we hit a bot
    // }

    // private void ChangeTarget()
    // {
    //     if (enemies.Count > 1)
    //     {
    //         stopScanning = true;
    //         EnemyInfo currentTarget = enemies[0]; // Get current target
    //         enemies.RemoveAt(0); // Remove it from the list
    //         enemies.Add(currentTarget); // Move it to the back

    //         // Reset streak and re-engage new target
    //         bulletWallHitStreak = 0;
    //         MoveToNearestEnemy();
    //     }
    // }



    public override void OnTick(TickEvent e)
    {
        // Ensure we keep scanning to find new enemies
        if (enemies.Count == 0)
        {
            TurnRadarRight(360);
        }
    }

    public static void Main()
    {
        new MainBot().Start();
    }
}
