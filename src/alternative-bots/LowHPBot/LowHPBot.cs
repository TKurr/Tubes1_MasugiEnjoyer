using System;
using System.Collections.Generic;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

public class MainBot : Bot
{
    private struct EnemyInfo
    {
        public int Id;
        public double Energy, X, Y;

        public EnemyInfo(int id, double energy, double x, double y)
        {
            Id = id;
            Energy = energy;
            X = x;
            Y = y;
        }
    }

    private readonly List<EnemyInfo> enemies = new();
    private const double LIMIT_TARGET_DISTANCE = 70;

    public MainBot() : base(BotInfo.FromFile("LowHPBot.json")) { }

    public override void Run()
    {
        enemies.Clear();
        AdjustRadarForBodyTurn = false;
        while (IsRunning)
        {
            TurnRadarRight(360);
            if (enemies.Count > 1)  
            {
                MoveToTarget();
            } else {
                EnemyInfo target = enemies[0];
                double distance = DistanceTo(target.X, target.Y);
                double bearing = BearingTo(target.X, target.Y);
                SetTurnRight(NormalizeBearing(bearing));

                if (distance > LIMIT_TARGET_DISTANCE)
                {
                    SetTurnLeft(NormalizeBearing(bearing));
                    SetForward(distance - LIMIT_TARGET_DISTANCE);
                }
                else if (distance < LIMIT_TARGET_DISTANCE / 2)
                {
                    SetBack(30);
                }

                if (bearing > -10 && bearing < 10 && distance < 300)
                {
                    Fire(3);
                }
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
                enemies[i] = new EnemyInfo(scannedId, e.Energy, e.X, e.Y);
                found = true;
                break;
            }
        }

        if (!found)
        {
            enemies.Add(new EnemyInfo(scannedId, e.Energy, e.X, e.Y));
        }
    }

    public override void OnBotDeath(BotDeathEvent e)
    {
        enemies.RemoveAll(enemy => enemy.Id == e.VictimId);
    }

    private void MoveToTarget()
    {
        if (enemies.Count == 0) return; 
        EnemyInfo lowestHPenemy = enemies[0];
        double minHP = lowestHPenemy.Energy;
        foreach (var enemy in enemies)
        {
            double HP = enemy.Energy;

            if (HP < minHP)
            {
                minHP = enemy.Energy;
                lowestHPenemy = enemy;
            }
        }

        double minDistance = DistanceTo(lowestHPenemy.X, lowestHPenemy.Y);
        double enemyBearing = BearingTo(lowestHPenemy.X, lowestHPenemy.Y);
        SetTurnLeft(NormalizeBearing(enemyBearing));

        if (minDistance > LIMIT_TARGET_DISTANCE)
        {
            SetForward(minDistance - LIMIT_TARGET_DISTANCE); 
        } else {
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
        new MainBot().Start();
    }
}
