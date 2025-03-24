using System;
using System.Collections.Generic;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

public class mainBot : Bot
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
    private const double LIMIT_TARGET_DISTANCE = 70;

    public mainBot() : base(BotInfo.FromFile("mainBot.json")) { }

    public override void Run()
    {
        enemies.Clear();
        AdjustRadarForBodyTurn = false;
        while (IsRunning)
        {
            TurnRadarRight(360);
            if (enemies.Count > 0)
            {
                MoveToNearestEnemy();
            }
        }
    }

    public override void OnScannedBot(ScannedBotEvent e)
    {
        var distance = DistanceTo(e.X, e.Y);
        var bearing = BearingTo(e.X, e.Y);
        if (bearing > -10 && bearing < 10 && distance < 300)
        {
            Fire(3);
        }

        int scannedId = e.ScannedBotId;
        bool found = false;

        for (int i = 0; i < enemies.Count; i++)
        {
            if (enemies[i].Id == scannedId)
            {
                enemies[i] = new EnemyInfo(scannedId, e.X, e.Y);
                found = true;
                break;
            }
        }

        if (!found)
        {
            enemies.Add(new EnemyInfo(scannedId, e.X, e.Y));
        }
    }

    public override void OnBotDeath(BotDeathEvent e)
    {
        enemies.RemoveAll(enemy => enemy.Id == e.VictimId);
    }

    private void MoveToNearestEnemy()
    {
        if (enemies.Count == 0) return;
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
        SetTurnLeft(NormalizeBearing(enemyBearing));

        if (minDistance > LIMIT_TARGET_DISTANCE)
        {
            SetForward(minDistance - LIMIT_TARGET_DISTANCE);
        }
        else
        {
            SetBack(LIMIT_TARGET_DISTANCE - minDistance);
        }
    }

    private double NormalizeBearing(double angle)
    {
        while (angle > 180) angle -= 360;
        while (angle < -180) angle += 360;
        return angle;
    }

    public override void OnHitBot(HitBotEvent e)
    {
        var bearing = BearingTo(e.X, e.Y);
        if (bearing > -10 && bearing < 10)
        {
            Fire(3);
        }
        if (e.IsRammed)
        {
            TurnLeft(10);
        }
    }

    public static void Main()
    {
        new mainBot().Start();
    }
}
