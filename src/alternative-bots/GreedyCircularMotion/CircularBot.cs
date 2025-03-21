using System;
using System.Drawing;
using System.Threading;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

public class CircularBot : Bot
{
    public class LockInEvent
    {
        private long turns;
        private bool lockIn;
        private ResetEvent resetEvent;
        private CircularBot bot;

        public LockInEvent(ResetEvent resetEvent, CircularBot bot)
        {
            this.resetEvent = resetEvent;
            this.bot = bot;
            this.turns = 0;
            this.lockIn = false;
        }

        public void increment() => this.turns++;

        public bool check()
        {
            if (this.lockIn)
            {
                if (this.turns > 100)
                {
                    this.turns = 0;
                    this.lockIn = false;
                    this.resetEvent.run();
                    return true;
                }
                else
                {
                    increment();
                    return false;
                }
            }
            return false;
        }

        public void set(double X, double Y)
        {
            this.turns = 0;
            this.lockIn = true;
            this.bot.targetX = X;
            this.bot.targetY = Y;
        }
    }

    public class ResetEvent
    {
        private CircularBot bot;
        private bool resetting;
        public ResetEvent(CircularBot bot)
        {
            this.bot = bot;
            this.resetting = false;
        }

        public void run(bool onStart = false)
        {
            resetting = true;
            bot.targetX = bot.ArenaWidth / 2;
            bot.targetY = bot.ArenaHeight / 2;
            bot.clockwise = true;

            if (onStart)
            {
                bot.TurnGunLeft(90);
                bot.TurnRadarLeft(360);
                double desiredTurn = bot.BearingTo(bot.targetX, bot.targetY) - 90;
                bot.TurnLeft(bot.NormalizeRelativeAngle(desiredTurn));
                done();
            }
            else
            {
                bot.SetTurnRadarLeft(360);
                double desiredTurn = bot.BearingTo(bot.targetX, bot.targetY) - 90;
                bot.SetTurnLeft(bot.NormalizeRelativeAngle(desiredTurn));
                Condition RadarDone = new Condition("RadarDone", () => bot.RadarTurnRemaining == 0);
                bot.AddCustomEvent(RadarDone);
                Console.WriteLine("Turning my radar...!");
            }
        }

        public void done()
        {
            this.resetting = false;
        }

        public bool isResetting() => resetting;
    }

    ResetEvent resetEvent;
    LockInEvent lockInEvent;
    public double targetX;
    public double targetY;
    public bool clockwise;

    static void Main(string[] args) => new CircularBot().Start();

    public CircularBot() : base(BotInfo.FromFile("CircularBot.json"))
    {
        resetEvent = new ResetEvent(this);
        lockInEvent = new LockInEvent(resetEvent, this);
    }

    public override void Run()
    {
        BodyColor = Color.Gray;
        resetEvent.run(true);
        while (IsRunning)
        {
            if (!resetEvent.isResetting())
            {
                Forward(clockwise ? 10_000 : -10_000);
            }
            else
            {
                Forward(0);
            }
        }
    }

    public double shrinkRate = 25;
    public override void OnTick(TickEvent tickEvent)
    {

        if (resetEvent.isResetting() || lockInEvent.check())
        {
            return;
        }

        double dx = targetX - tickEvent.BotState.X;
        double dy = targetY - tickEvent.BotState.Y;
        double radius = Math.Sqrt(dx * dx + dy * dy);
        radius = Math.Max(1, radius - shrinkRate);
        TurnRate = (180 * Speed) / (Math.PI * radius);
    }

    public override void OnScannedBot(ScannedBotEvent e)
    {
        lockInEvent.set(e.X, e.Y);
        if (!resetEvent.isResetting())
        {
            SmartFire(e.X, e.Y);
        }
    }

    public void SmartFire(double enemyX, double enemyY)
    {
        double dx = this.X - enemyX;
        double dy = this.Y - enemyY;
        double distance = Math.Sqrt(dx * dx + dy * dy);
        double bulletPower = 3 * Math.Exp(-0.01 * distance);
        Fire(bulletPower);
    }

    public override void OnHitWall(HitWallEvent e)
    {
        if (resetEvent.isResetting())
            return;

        Console.WriteLine("Ouch! I hit a wall, must turn back!");
        clockwise = !clockwise;
    }

    public override void OnHitByBullet(HitByBulletEvent hitByBulletEvent)
    {
        if (resetEvent.isResetting())
            return;
        clockwise = !clockwise;
    }

    public override void OnHitBot(HitBotEvent e)
    {

        if (resetEvent.isResetting())
            return;
        lockInEvent.set(e.X, e.Y);
        Console.WriteLine("I hit a bot at " + e.X + ", " + e.Y);
        clockwise = !clockwise;
    }

    public override void OnCustomEvent(CustomEvent e)
    {
        if (resetEvent.isResetting())
        {
            if (e.Condition.Name == "TurnDone")
            {
                Console.WriteLine("Im done turning!");
                resetEvent.done();
            }
            else if (e.Condition.Name == "RadarDone")
            {
                double bearing = GunDirection - RadarDirection;
                bearing = NormalizeRelativeAngle(bearing);
                TurnRadarLeft(bearing);
                Console.WriteLine("Done turning my radar!");
                Console.WriteLine("Turning my body...!");
                double desiredTurn = BearingTo(targetX, targetY) - 90;
                TurnLeft(NormalizeRelativeAngle(desiredTurn));
                Console.WriteLine("Done turning my body!");
                resetEvent.done();
            }
        }
        RemoveCustomEvent(e.Condition);

    }
}
