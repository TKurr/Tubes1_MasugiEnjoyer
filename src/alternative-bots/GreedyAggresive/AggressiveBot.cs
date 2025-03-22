using System;
using System.Drawing;
using System.Threading;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

public class AggressiveBot : Bot
{
    public class TargetEvent
    {
        private double TargetX;
        private double TargetY;
        private int Forward;
        private AggressiveBot Bot;
        private double Radius;
        public TargetEvent(AggressiveBot Bot)
        {
            this.TargetX = -1;
            this.TargetY = -1;
            this.Bot = Bot;
            this.Forward = 1;
            this.Radius = 100;
        }
        public void LockTarget(double TargetX, double TargetY)
        {
            this.TargetX = TargetX;
            this.TargetY = TargetY;
            Bot.SetTurnGunLeft(Bot.GunBearingTo(this.TargetX, this.TargetY));
            if (Math.Abs(this.Bot.GunBearingTo(this.TargetX, this.TargetY)) < 7.5)
            {
                Bot.Fire(3 * Math.Exp(-0.01 * this.Distance()));
            }
            Bot.TurnRadar();
            double dx = this.Bot.X - this.TargetX;
            double dy = this.Bot.Y - this.TargetY;
            double baseAngle = Math.Atan2(dy, dx);
            double offsetDeg = this.Forward * 45.0;
            double offsetRad = offsetDeg * Math.PI / 180.0;
            double finalAngle1 = baseAngle + offsetRad;
            double CircleX = this.TargetX + (this.Radius * Math.Cos(finalAngle1));
            double CircleY = this.TargetY + (this.Radius * Math.Sin(finalAngle1));
            Bot.SetForward(10_000);
            double Bearing = Bot.NormalizeRelativeAngle(Bot.BearingTo(CircleX, CircleY));
            if (Math.Abs(Bearing) > 45)
            {
                Bot.MaxSpeed = 2;
            }
            else
            {
                Bot.MaxSpeed = 1000;
            }
            Bot.TurnRate = Bearing;
        }
        public void ChangeDirection()
        {
            this.Forward = -this.Forward;
        }
        public double Distance(double targetX, double targetY)
        {
            return Math.Sqrt(Math.Pow(targetX - this.Bot.X, 2) + Math.Pow(targetY - this.Bot.Y, 2));
        }
        public double Distance()
        {
            return Distance(this.TargetX, this.TargetY);
        }
    }
    TargetEvent targetEvent;
    static void Main(string[] args) => new AggressiveBot().Start();
    public bool clockwiseRadar;
    public void TurnRadar()
    {
        SetTurnRadarLeft(clockwiseRadar ? -360 : 360);
        clockwiseRadar = !clockwiseRadar;
    }
    public AggressiveBot() : base(BotInfo.FromFile("AggressiveBot.json"))
    {
        targetEvent = new TargetEvent(this);
    }
    public override void Run()
    {
        BodyColor = Color.Gray;
        clockwiseRadar = false;
        while (IsRunning)
        {
            TurnRadarLeft(360);
        }
    }
    public override void OnScannedBot(ScannedBotEvent e)
    {
        targetEvent.LockTarget(e.X, e.Y);
    }
    public override void OnHitBot(HitBotEvent e)
    {
        targetEvent.LockTarget(e.X, e.Y);
    }
    public override void OnHitWall(HitWallEvent e)
    {
        Back(100);
        targetEvent.ChangeDirection();
        Console.WriteLine("Ouch! I hit a wall, must turn back!");
    }
    public override void OnHitByBullet(HitByBulletEvent hitByBulletEvent)
    {
        targetEvent.ChangeDirection();
        Console.WriteLine("Ouch! a bullet hit me!");
    }
}
