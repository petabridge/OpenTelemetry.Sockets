// -----------------------------------------------------------------------
// <copyright file="HelloActor.cs" company="Petabridge, LLC">
//      Copyright (C) 2025 - 2025 Petabridge, LLC <https://petabridge.com>
// </copyright>
// -----------------------------------------------------------------------

namespace Akka.IOListener;

public class HelloActor : ReceiveActor
{
    private readonly ILoggingAdapter _log = Context.GetLogger();
    private int _helloCounter;

    public HelloActor()
    {
        Receive<string>(message => { _log.Info("{0} {1}", message, _helloCounter++); });
    }
}