// -----------------------------------------------------------------------
// <copyright file="TimerActor.cs" company="Petabridge, LLC">
//      Copyright (C) 2025 - 2025 Petabridge, LLC <https://petabridge.com>
// </copyright>
// -----------------------------------------------------------------------

using Akka.Hosting;

namespace Akka.IOListener;

public class TimerActor : ReceiveActor, IWithTimers
{
    private readonly IActorRef _helloActor;

    public TimerActor(IRequiredActor<HelloActor> helloActor)
    {
        _helloActor = helloActor.ActorRef;
        Receive<string>(message => { _helloActor.Tell(message); });
    }

    public ITimerScheduler Timers { get; set; } = null!; // gets set by Akka.NET

    protected override void PreStart()
    {
        Timers.StartPeriodicTimer("hello-key", "hello", TimeSpan.FromSeconds(1));
    }
}